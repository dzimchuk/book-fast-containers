using BookFast.Booking.Domain;
using BookFast.Common.Application.Clock;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Application.Reservations.CancelReservation
{
    public class CancelReservationCommandHandler(IDbContext dbContext, ISecurityContext securityContext, IDateTimeProvider dateTimeProvider)
        : ICommandHandler<CancelReservationCommand, ReservationRepresentation>
    {
        public Task<Result<ReservationRepresentation>> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
        {
            var guestId = securityContext.GetCurrentUser();

            return dbContext.ExecuteInTransactionAsync(ct => CancelAsync(request, guestId, ct), cancellationToken);
        }

        private async Task<Result<ReservationRepresentation>> CancelAsync(CancelReservationCommand request, string guestId, CancellationToken cancellationToken)
        {
            var reservation = await dbContext.Reservations
                .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.GuestId == guestId, cancellationToken);

            if (reservation is null)
            {
                return Result.Failure<ReservationRepresentation>(ErrorCodes.ReservationNotFound(request.ReservationId));
            }

            var asOf = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

            if (reservation.GetDisplayStatus(asOf) != ReservationStatus.Confirmed)
            {
                return Result.Failure<ReservationRepresentation>(ErrorCodes.ReservationNotCancellable());
            }

            reservation.Cancel();

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(ReservationRepresentation.Map(reservation, asOf));
        }
    }
}
