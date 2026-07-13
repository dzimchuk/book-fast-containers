using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Integration;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.RentalProperties.UpdateProperty
{
    public class UpdatePropertyHandler : ICommandHandler<UpdatePropertyCommand>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;
        private readonly IIntegrationEventPublisher eventPublisher;

        public UpdatePropertyHandler(IDbContext dbContext,
                                     ISecurityContext securityContext,
                                     IIntegrationEventPublisher eventPublisher)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
            this.eventPublisher = eventPublisher;
        }

        public async Task<Result> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
        {
            var tenantId = securityContext.GetCurrentTenant();

            return await dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var property = await dbContext.Properties.FirstOrDefaultAsync(
                    p => p.Id == request.PropertyId && p.TenantId == tenantId,
                    ct);
                if (property == null)
                {
                    return ErrorCodes.PropertyNotFound(request.PropertyId);
                }

                property.Update(
                    request.Name,
                    request.Description,
                    request.Address,
                    request.Location,
                    request.Images,
                    request.Facilities);

                await dbContext.SaveChangesAsync(ct);

                await eventPublisher.PublishAsync(new PropertyUpdatedEvent
                {
                    TenantId = tenantId,
                    PropertyId = property.Id,
                    Name = property.Name,
                    Description = property.Description,
                    Address = property.Address is null ? null : new AddressDTO(
                        property.Address.Country, property.Address.State, property.Address.City,
                        property.Address.Street, property.Address.ZipCode),
                    Location = property.Location is null ? null : new LocationDTO(
                        property.Location.Latitude, property.Location.Longitude),
                    Facilities = property.Facilities.Select(facility => facility.ToString()).ToArray(),
                    Images = property.Images
                }, ct);

                return Result.Success();
            }, cancellationToken);
        }
    }
}
