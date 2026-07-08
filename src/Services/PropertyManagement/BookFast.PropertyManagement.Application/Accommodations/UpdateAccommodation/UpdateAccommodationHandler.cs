using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Integration;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation
{
    public class UpdateAccommodationHandler : ICommandHandler<UpdateAccommodationCommand>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;
        private readonly IIntegrationEventPublisher eventPublisher;

        public UpdateAccommodationHandler(IDbContext dbContext,
                                          ISecurityContext securityContext,
                                          IIntegrationEventPublisher eventPublisher)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
            this.eventPublisher = eventPublisher;
        }

        public async Task<Result> Handle(UpdateAccommodationCommand request, CancellationToken cancellationToken)
        {
            return await dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var accommodation = await dbContext.Accommodations.FirstOrDefaultAsync(
                    a => a.Id == request.AccommodationId && a.TenantId == securityContext.GetCurrentTenant(),
                    ct);
                if (accommodation == null)
                {
                    return ErrorCodes.AccommodationNotFound(request.AccommodationId);
                }

                accommodation.Update(
                    request.Name,
                    request.Description,
                    request.Bedrooms,
                    request.Images,
                    request.Quantity,
                    request.PriceRange,
                    request.Facilities);

                await dbContext.SaveChangesAsync(ct);

                await eventPublisher.PublishAsync(new AccommodationUpdatedEvent
                {
                    TenantId = securityContext.GetCurrentTenant(),
                    AccommodationId = accommodation.Id,
                    PropertyId = accommodation.PropertyId,
                    Name = accommodation.Name,
                    Description = accommodation.Description,
                    Bedrooms = accommodation.Bedrooms,
                    Images = accommodation.Images,
                    Quantity = accommodation.Quantity,
                    PriceRange = accommodation.PriceRange.IsEmpty ? null : accommodation.PriceRange,
                    Facilities = accommodation.Facilities.Select(facility => facility.ToString()).ToArray()
                }, ct);

                return Result.Success();
            }, cancellationToken);
        }
    }
}
