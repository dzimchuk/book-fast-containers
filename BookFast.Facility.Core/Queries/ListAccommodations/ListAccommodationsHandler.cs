using BookFast.Facility.Core.Queries.Representations;

namespace BookFast.Facility.Core.Queries.ListAccommodations
{
    public class ListAccommodationsHandler : IRequestHandler<ListAccommodationsQuery, IEnumerable<AccommodationRepresentation>>
    {
        private readonly IDbContext dbContext;

        public ListAccommodationsHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<AccommodationRepresentation>> Handle(ListAccommodationsQuery request, CancellationToken cancellationToken)
        {
            var accommodations = await dbContext.Accommodations.AsNoTracking().Where(item => item.FacilityId == request.FacilityId).ToListAsync();
            return accommodations.Select(item => item.ToRepresentation()).ToList();
        }
    }
}
