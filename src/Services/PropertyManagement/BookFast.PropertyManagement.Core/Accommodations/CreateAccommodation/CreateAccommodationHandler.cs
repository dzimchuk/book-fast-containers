using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Core.Accommodations.CreateAccommodation
{
    public class CreateAccommodationHandler : ICommandHandler<CreateAccommodationCommand, int>
    {
        private readonly IDbContext dbContext;

        public CreateAccommodationHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Result<int>> Handle(CreateAccommodationCommand request, CancellationToken cancellationToken)
        {
            if (!await dbContext.Properties.AnyAsync(facility => facility.Id == request.PropertyId, cancellationToken: cancellationToken))
            {
                return Result.Failure<int>(ErrorCodes.PropertyNotFound(request.PropertyId));
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
