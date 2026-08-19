using BookFast.Booking.Domain;
using BookFast.Common.Application.Clock;
using BookFast.Common.Application.Security;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Domain;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Application.Reservations.CreateReservation
{
    public class CreateReservationCommandHandler(IDbContext dbContext, ISecurityContext securityContext, IDateTimeProvider dateTimeProvider)
        : ICommandHandler<CreateReservationCommand, ReservationRepresentation>
    {
        private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(15);

        public async Task<Result<ReservationRepresentation>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            var guestId = securityContext.GetCurrentUser();
            var stay = new Stay(request.CheckIn, request.CheckOut);

            var result = await dbContext.ExecuteInTransactionWithConcurrencyRetryAsync(ct => TryCreateAsync(request, guestId, stay, ct), cancellationToken);

            if (!result.IsSuccess)
            {
                return Result.Failure<ReservationRepresentation>(result.Error);
            }

            var asOf = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

            return Result.Success(ReservationRepresentation.Map(result.Value, asOf));
        }

        private async Task<Result<Reservation>> TryCreateAsync(CreateReservationCommand request, string guestId, Stay stay, CancellationToken cancellationToken)
        {
            var accommodation = await dbContext.Accommodations.FirstOrDefaultAsync(a => a.Id == request.AccommodationId, cancellationToken);

            if (accommodation is null || !accommodation.Bookable)
            {
                return Result.Failure<Reservation>(ErrorCodes.AccommodationNotBookable(request.AccommodationId));
            }

            var overlapping = await dbContext.Reservations
                .Where(r => r.AccommodationId == request.AccommodationId
                         && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
                         && r.Stay.CheckIn < stay.CheckOut && stay.CheckIn < r.Stay.CheckOut)
                .Select(r => new { r.Stay.CheckIn, r.Stay.CheckOut, r.Units })
                .ToListAsync(cancellationToken);

            if (!HasCapacityForEveryNight(accommodation.Quantity, overlapping.Select(r => (r.CheckIn, r.CheckOut, r.Units)), stay, request.Units))
            {
                return Result.Failure<Reservation>(ErrorCodes.InsufficientAvailability());
            }

            // copied, because the it's already tracked with the accommodation
            var frozenRate = new Money(accommodation.Rate.Amount, accommodation.Rate.Currency);

            var reservation = Reservation.NewReservation(
                Guid.CreateVersion7(),
                guestId,
                accommodation.Id,
                accommodation.TenantId,
                stay,
                request.Units,
                frozenRate,
                new DateTimeOffset(dateTimeProvider.UtcNow).Add(HoldDuration));

            dbContext.Reservations.Add(reservation);
            accommodation.Book();

            return Result.Success(reservation);
        }

        private static bool HasCapacityForEveryNight(
            int quantity, IEnumerable<(DateOnly CheckIn, DateOnly CheckOut, int Units)> overlapping, Stay stay, int requestedUnits)
        {
            var overlappingList = overlapping.ToList();

            for (var night = stay.CheckIn; night < stay.CheckOut; night = night.AddDays(1))
            {
                var heldUnits = overlappingList.Where(r => r.CheckIn <= night && night < r.CheckOut).Sum(r => r.Units);

                if (quantity - heldUnits < requestedUnits)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
