using BookFast.PropertyManagement.Core;

namespace BookFast.PropertyManagement.Core.Commands.DeleteAccommodation
{
    public class DeleteAccommodationHandler : IRequestHandler<DeleteAccommodationCommand>
    {
        private readonly IDbContext dbContext;

        public DeleteAccommodationHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Handle(DeleteAccommodationCommand request, CancellationToken cancellationToken)
        {
            var accommodation = await dbContext.Accommodations.FindAsync(new object[] { request.AccommodationId }, cancellationToken: cancellationToken);
            if (accommodation == null)
            {
                throw new NotFoundException("Accommodation", request.AccommodationId);
            }

            dbContext.Accommodations.Remove(accommodation);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
