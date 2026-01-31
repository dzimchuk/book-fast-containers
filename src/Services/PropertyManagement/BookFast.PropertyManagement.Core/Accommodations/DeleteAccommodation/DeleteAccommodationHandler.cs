using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;

namespace BookFast.PropertyManagement.Core.Accommodations.DeleteAccommodation
{
    public class DeleteAccommodationHandler : ICommandHandler<DeleteAccommodationCommand>
    {
        private readonly IDbContext dbContext;

        public DeleteAccommodationHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Result> Handle(DeleteAccommodationCommand request, CancellationToken cancellationToken)
        {
            var accommodation = await dbContext.Accommodations.FindAsync([request.AccommodationId], cancellationToken: cancellationToken);
            if (accommodation == null)
            {
                return ErrorCodes.AccommodationNotFound(request.AccommodationId);
            }

            dbContext.Accommodations.Remove(accommodation);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
