namespace BookFast.Facility.Core.Commands.CreateAccommodation
{
    public class CreateAccommodationHandler : IRequestHandler<CreateAccommodationCommand, int>
    {
        private readonly IDbContext dbContext;

        public CreateAccommodationHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<int> Handle(CreateAccommodationCommand request, CancellationToken cancellationToken)
        {
            if (!await dbContext.Facilities.AnyAsync(facility => facility.Id == request.FacilityId, cancellationToken: cancellationToken))
            {
                throw new NotFoundException("Facility", request.FacilityId);
            }

            var accommodation = Accommodation.NewAccommodation(
                request.FacilityId,
                request.Name,
                request.Description,
                request.RoomCount,
                request.Images);

            await dbContext.Accommodations.AddAsync(accommodation, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return accommodation.Id;
        }
    }
}
