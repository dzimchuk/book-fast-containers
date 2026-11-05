using BookFast.Common.Application.Clock;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Application.Reservations.GetReservation
{
    public class GetReservationQueryHandler(IDbContext dbContext, ISecurityContext securityContext, IDateTimeProvider dateTimeProvider)
        : IQueryHandler<GetReservationQuery, ReservationRepresentation>
    {
        public async Task<Result<ReservationRepresentation>> Handle(GetReservationQuery request, CancellationToken cancellationToken)
        {
            var guestId = securityContext.GetCurrentUser();

            var reservation = await dbContext.Reservations.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == request.Id && r.GuestId == guestId, cancellationToken);

            if (reservation is null)
            {
                return Result.Failure<ReservationRepresentation>(ErrorCodes.ReservationNotFound(request.Id));
            }

            var asOf = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

            return ReservationRepresentation.Map(reservation, asOf);
        }
    }
}
