using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.Accommodations.GetAccommodation
{
    public class GetAccommodationHandler : IQueryHandler<GetAccommodationQuery, AccommodationRepresentation>
    {
        private readonly IDbContext dbContext;

        public GetAccommodationHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Result<AccommodationRepresentation>> Handle(GetAccommodationQuery request, CancellationToken cancellationToken)
        {
            var accommodation = await dbContext.Accommodations.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken: cancellationToken);
            
            if (accommodation == null)
            {
                return ErrorCodes.AccommodationNotFound(request.Id);
            }

            return new AccommodationRepresentation
            {
                Id = accommodation.Id,
                PropertyId = accommodation.PropertyId,
                Name = accommodation.Name,
                Description = accommodation.Description,
                RoomCount = accommodation.RoomCount,
                Images = accommodation.Images,
                Quantity = accommodation.Quantity,
                Price = accommodation.Price,
                IsActive = accommodation.IsActive
            };
        }
    }
}
