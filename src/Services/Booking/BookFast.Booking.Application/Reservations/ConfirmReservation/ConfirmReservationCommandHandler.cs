using BookFast.Booking.Domain;
using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Application.Reservations.ConfirmReservation
{
    public class ConfirmReservationCommandHandler(IDbContext dbContext) : ICommandHandler<ConfirmReservationCommand>
    {
        public async Task<Result> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
        {
            var result = await dbContext.ExecuteInTransactionWithConcurrencyRetryAsync(
                ct => ConfirmAsync(request.ReservationId, ct), cancellationToken);

            return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
        }

        private async Task<Result<bool>> ConfirmAsync(Guid reservationId, CancellationToken cancellationToken)
        {
            var reservation = await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken);

            if (reservation is not null && reservation.Status == ReservationStatus.Pending)
            {
                reservation.Confirm();
            }

            return Result.Success(true);
        }
    }
}
