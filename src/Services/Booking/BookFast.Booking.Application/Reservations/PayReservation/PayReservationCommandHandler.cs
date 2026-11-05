using BookFast.Booking.Application.Payments;
using BookFast.Booking.Domain;
using BookFast.Common.Application.Clock;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Application.Reservations.PayReservation
{
    public class PayReservationCommandHandler(
        IDbContext dbContext, ISecurityContext securityContext, IPaymentGateway paymentGateway, IDateTimeProvider dateTimeProvider)
        : ICommandHandler<PayReservationCommand, ReservationRepresentation>
    {
        public Task<Result<ReservationRepresentation>> Handle(PayReservationCommand request, CancellationToken cancellationToken)
        {
            var guestId = securityContext.GetCurrentUser();

            return dbContext.ExecuteInTransactionAsync(ct => PayAsync(request, guestId, ct), cancellationToken);
        }

        private async Task<Result<ReservationRepresentation>> PayAsync(PayReservationCommand request, string guestId, CancellationToken cancellationToken)
        {
            var reservation = await dbContext.Reservations
                .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.GuestId == guestId, cancellationToken);

            if (reservation is null)
            {
                return Result.Failure<ReservationRepresentation>(ErrorCodes.ReservationNotFound(request.ReservationId));
            }

            if (reservation.Status != ReservationStatus.Pending)
            {
                return Result.Failure<ReservationRepresentation>(ErrorCodes.ReservationNotPending());
            }

            if (request.Outcome == PaymentOutcome.Settle)
            {
                var attemptInProgress = await dbContext.PaymentAttempts.AnyAsync(a => a.ReservationId == reservation.Id, cancellationToken);

                if (attemptInProgress)
                {
                    return Result.Failure<ReservationRepresentation>(ErrorCodes.PaymentAlreadyInProgress());
                }

                await paymentGateway.ChargeAsync(reservation.Id, cancellationToken);
            }

            // Decline is the guest's own out-of-band choice - the gateway is simply never invoked.
            var asOf = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

            return Result.Success(ReservationRepresentation.Map(reservation, asOf));
        }
    }
}
