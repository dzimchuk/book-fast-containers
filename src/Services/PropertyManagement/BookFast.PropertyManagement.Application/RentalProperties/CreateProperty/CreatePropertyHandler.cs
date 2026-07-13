using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Integration;

namespace BookFast.PropertyManagement.Application.RentalProperties.CreateProperty
{
    public class CreatePropertyHandler : ICommandHandler<CreatePropertyCommand, Guid>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;
        private readonly IIntegrationEventPublisher eventPublisher;

        public CreatePropertyHandler(IDbContext dbContext,
                                     ISecurityContext securityContext,
                                     IIntegrationEventPublisher eventPublisher)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
            this.eventPublisher = eventPublisher;
        }

        public async Task<Result<Guid>> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
        {
            var tenantId = securityContext.GetCurrentTenant();

            return await dbContext.ExecuteInTransactionAsync<Guid>(async ct =>
            {
                var property = Property.NewProperty(
                    tenantId,
                    request.Name,
                    request.Description,
                    request.Address,
                    request.Location,
                    request.Images,
                    request.Facilities);

                property.Id = Guid.CreateVersion7();

                dbContext.Properties.Add(property);

                await dbContext.SaveChangesAsync(ct);

                await eventPublisher.PublishAsync(new PropertyCreatedEvent
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

                return property.Id;
            }, cancellationToken);
        }
    }
}
