using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.Accommodations.GetAccommodation
{
    public class GetAccommodationHandler : IQueryHandler<GetAccommodationQuery, AccommodationRepresentation>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public GetAccommodationHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<Result<AccommodationRepresentation>> Handle(GetAccommodationQuery request, CancellationToken cancellationToken)
        {
            var accommodation = await dbContext.Accommodations.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == request.Id && item.TenantId == securityContext.GetCurrentTenant(), cancellationToken: cancellationToken);
            
            if (accommodation == null)
            {
                return ErrorCodes.AccommodationNotFound(request.Id);
            }

            return new AccommodationRepresentation
            {
                Id = accommodation.Id,
                PropertyId = accommodation.PropertyId,
                Name = accommodation.Name,
                Description = accommodation.Description,
                Bedrooms = accommodation.Bedrooms,
                Images = accommodation.Images,
                Quantity = accommodation.Quantity,
                PriceRange = PriceRangeRepresentation.Map(accommodation.PriceRange),
                Facilities = accommodation.Facilities,
                IsActive = accommodation.IsActive
            };
        }
    }
}
