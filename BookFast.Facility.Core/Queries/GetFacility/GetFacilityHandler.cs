using BookFast.Facility.Core.Queries.Representations;

namespace BookFast.Facility.Core.Queries.GetFacility
{
    public class GetFacilityHandler : IRequestHandler<GetFacilityQuery, FacilityRepresentation>
    {
        private readonly IDbContext dbContext;

        public GetFacilityHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<FacilityRepresentation> Handle(GetFacilityQuery request, CancellationToken cancellationToken)
        {
            var facility = await(from item in dbContext.Facilities.AsNoTracking()
                                 where item.Id == request.Id
                                 select new
                                 {
                                     item.Id,
                                     item.Name,
                                     item.Description,
                                     item.StreetAddress,
                                     item.Location.Latitude,
                                     item.Location.Longitude,
                                     item.Images
                                 }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            return facility != null
                ? new FacilityRepresentation
                {
                    Id = facility.Id,
                    Name = facility.Name,
                    Description = facility.Description,
                    StreetAddress = facility.StreetAddress,
                    Latitude = facility.Latitude,
                    Longitude = facility.Longitude,
                    Images = facility.Images
                }
                : null;
        }
    }
}
