namespace BookFast.PropertyManagement.Core.Commands.UpdateAccommodation
{
    public class UpdateAccommodationHandler : IRequestHandler<UpdateAccommodationCommand>
    {
        private readonly IDbContext dbContext;

        public UpdateAccommodationHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Handle(UpdateAccommodationCommand request, CancellationToken cancellationToken)
        {
            var accommodation = await dbContext.Accommodations.FindAsync(request.AccommodationId);
            if (accommodation == null)
            {
                throw new NotFoundException("Accommodation", request.AccommodationId);
            }

            accommodation.Update(
                request.Name,
                request.Description,
                request.RoomCount,
                request.Images,
                request.Quantity,
                request.Price);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
