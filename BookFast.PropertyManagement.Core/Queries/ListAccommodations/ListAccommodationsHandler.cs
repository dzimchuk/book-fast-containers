using BookFast.PropertyManagement.Core.Queries.Representations;

namespace BookFast.PropertyManagement.Core.Queries.ListAccommodations
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
            var accommodations = await dbContext.Accommodations.AsNoTracking().Where(item => item.PropertyId == request.PropertyId)
                .ToListAsync(cancellationToken: cancellationToken);

            return accommodations.Select(item => item.ToRepresentation()).ToList();
        }
    }
}
