using BookFast.Facility.Core.Queries.Representations;

namespace BookFast.Facility.Core.Queries.GetAccommodation
{
    public class GetAccommodationHandler : IRequestHandler<GetAccommodationQuery, AccommodationRepresentation>
    {
        private readonly IDbContext dbContext;

        public GetAccommodationHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<AccommodationRepresentation> Handle(GetAccommodationQuery request, CancellationToken cancellationToken)
        {
            var accommodation = await dbContext.Accommodations.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.Id);
            return accommodation?.ToRepresentation();
        }
    }
}
