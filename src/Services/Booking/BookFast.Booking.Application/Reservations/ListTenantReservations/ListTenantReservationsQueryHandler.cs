using BookFast.Common.Application.Clock;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Application.Reservations.ListTenantReservations
{
    public class ListTenantReservationsQueryHandler(IDbContext dbContext, ISecurityContext securityContext, IDateTimeProvider dateTimeProvider)
        : IQueryHandler<ListTenantReservationsQuery, IReadOnlyList<ReservationRepresentation>>
    {
        public async Task<Result<IReadOnlyList<ReservationRepresentation>>> Handle(ListTenantReservationsQuery request, CancellationToken cancellationToken)
        {
            var tenantId = securityContext.GetCurrentTenant();

            var reservations = await dbContext.Reservations.AsNoTracking()
                .Where(r => r.TenantId == tenantId)
                .OrderByDescending(r => r.Stay.CheckIn)
                .ToListAsync(cancellationToken);

            var asOf = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

            IReadOnlyList<ReservationRepresentation> representations =
                reservations.Select(r => ReservationRepresentation.Map(r, asOf)).ToList();

            return Result.Success(representations);
        }
    }
}
