using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;

namespace BookFast.PropertyManagement.Core.Accommodations.UpdateAccommodation
{
    public class UpdateAccommodationHandler : ICommandHandler<UpdateAccommodationCommand>
    {
        private readonly IDbContext dbContext;

        public UpdateAccommodationHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Result> Handle(UpdateAccommodationCommand request, CancellationToken cancellationToken)
        {
            var accommodation = await dbContext.Accommodations.FindAsync(request.AccommodationId);
            if (accommodation == null)
            {
                return ErrorCodes.AccommodationNotFound(request.AccommodationId);
            }

            accommodation.Update(
                request.Name,
                request.Description,
                request.RoomCount,
                request.Images,
                request.Quantity,
                request.Price);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
