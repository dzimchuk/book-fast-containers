using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Integration;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.Accommodations.CreateAccommodation
{
    public class CreateAccommodationHandler : ICommandHandler<CreateAccommodationCommand, Guid>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;
        private readonly IIntegrationEventPublisher eventPublisher;

        public CreateAccommodationHandler(IDbContext dbContext,
                                          ISecurityContext securityContext,
                                          IIntegrationEventPublisher eventPublisher)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
            this.eventPublisher = eventPublisher;
        }

        public async Task<Result<Guid>> Handle(CreateAccommodationCommand request, CancellationToken cancellationToken)
        {
            var tenantId = securityContext.GetCurrentTenant();

            if (!await dbContext.Properties.AnyAsync(facility => facility.Id == request.PropertyId && facility.TenantId == tenantId, cancellationToken: cancellationToken))
            {
                return Result.Failure<Guid>(ErrorCodes.PropertyNotFound(request.PropertyId));
            }

            return await dbContext.ExecuteInTransactionAsync<Guid>(async ct =>
            {
                var accommodation = Accommodation.NewAccommodation(
                    tenantId,
                    request.PropertyId,
                    request.Name,
                    request.Description,
                    request.RoomCount,
                    request.Images,
                    request.Quantity,
                    request.Price);

                accommodation.Id = Guid.CreateVersion7();

                await dbContext.Accommodations.AddAsync(accommodation, ct);

                await dbContext.SaveChangesAsync(ct);

                await eventPublisher.PublishAsync(new AccommodationCreatedEvent
                {
                    TenantId = tenantId,
                    AccommodationId = accommodation.Id,
                    PropertyId = accommodation.PropertyId,
                    Name = accommodation.Name,
                    Description = accommodation.Description,
                    RoomCount = accommodation.RoomCount,
                    Images = accommodation.Images,
                    Quantity = accommodation.Quantity,
                    Price = accommodation.Price
                }, ct);

                return accommodation.Id;
            }, cancellationToken);
        }
    }
}
