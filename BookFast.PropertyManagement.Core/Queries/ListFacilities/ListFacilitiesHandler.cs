using BookFast.PropertyManagement.Core.Queries.Representations;
using BookFast.Security;

namespace BookFast.PropertyManagement.Core.Queries.ListFacilities
{
    public class ListFacilitiesHandler : IRequestHandler<ListFacilitiesQuery, IEnumerable<FacilityRepresentation>>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public ListFacilitiesHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<IEnumerable<FacilityRepresentation>> Handle(ListFacilitiesQuery request, CancellationToken cancellationToken)
        {
            var facilities = await (from item in dbContext.Properties.AsNoTracking()
                                    where item.Owner == securityContext.GetCurrentTenant()
                                    select new
                                    {
                                        item.Id,
                                        item.Name,
                                        item.Description,
                                        item.StreetAddress,
                                        item.Location.Latitude,
                                        item.Location.Longitude,
                                        item.Images
                                    }).ToListAsync();

            return (from facility in facilities
                    select new FacilityRepresentation
                    {
                        Id = facility.Id,
                        Name = facility.Name,
                        Description = facility.Description,
                        StreetAddress = facility.StreetAddress,
                        Latitude = facility.Latitude,
                        Longitude = facility.Longitude,
                        Images = facility.Images
                    }).ToList();
        }
    }
}
