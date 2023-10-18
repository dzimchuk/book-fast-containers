namespace BookFast.PropertyManagement.Core.Commands.CreateAccommodation
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
            if (!await dbContext.Properties.AnyAsync(facility => facility.Id == request.PropertyId, cancellationToken: cancellationToken))
            {
                throw new NotFoundException("Facility", request.PropertyId);
            }

            var accommodation = Accommodation.NewAccommodation(
                request.PropertyId,
                request.Name,
                request.Description,
                request.RoomCount,
                request.Images,
                request.Quantity,
                request.Price);

            await dbContext.Accommodations.AddAsync(accommodation, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return accommodation.Id;
        }
    }
}
