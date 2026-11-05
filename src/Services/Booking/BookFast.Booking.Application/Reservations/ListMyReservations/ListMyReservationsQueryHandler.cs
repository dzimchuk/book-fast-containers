using BookFast.Common.Application.Clock;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Application.Reservations.ListMyReservations
{
    public class ListMyReservationsQueryHandler(IDbContext dbContext, ISecurityContext securityContext, IDateTimeProvider dateTimeProvider)
        : IQueryHandler<ListMyReservationsQuery, IReadOnlyList<ReservationRepresentation>>
    {
        public async Task<Result<IReadOnlyList<ReservationRepresentation>>> Handle(ListMyReservationsQuery request, CancellationToken cancellationToken)
        {
            var guestId = securityContext.GetCurrentUser();

            var reservations = await dbContext.Reservations.AsNoTracking()
                .Where(r => r.GuestId == guestId)
                .OrderByDescending(r => r.Stay.CheckIn)
                .ToListAsync(cancellationToken);

            var asOf = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

            IReadOnlyList<ReservationRepresentation> representations =
                reservations.Select(r => ReservationRepresentation.Map(r, asOf)).ToList();

            return Result.Success(representations);
        }
    }
}
