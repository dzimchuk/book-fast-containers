using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Integration;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.Replay
{
    public class ReplayCatalogueHandler : ICommandHandler<ReplayCatalogueCommand>
    {
        private const int BatchSize = 25;
        private static readonly TimeSpan BatchDelay = TimeSpan.FromMilliseconds(250);

        private readonly IDbContext dbContext;
        private readonly IIntegrationEventPublisher eventPublisher;
        private readonly ISecurityContext securityContext;

        public ReplayCatalogueHandler(IDbContext dbContext, IIntegrationEventPublisher eventPublisher, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.eventPublisher = eventPublisher;
            this.securityContext = securityContext;
        }

        public async Task<Result> Handle(ReplayCatalogueCommand request, CancellationToken cancellationToken)
        {
            var tenantId = securityContext.GetCurrentTenant();

            await ReplayPropertiesAsync(tenantId, cancellationToken);
            await ReplayAccommodationsAsync(tenantId, cancellationToken);

            return Result.Success();
        }

        private async Task ReplayPropertiesAsync(string tenantId, CancellationToken cancellationToken)
        {
            var properties = await dbContext.Properties.AsNoTracking()
                .Where(property => property.TenantId == tenantId)
                .OrderBy(property => property.Id)
                .ToListAsync(cancellationToken);

            foreach (var batch in properties.Chunk(BatchSize))
            {
                await dbContext.ExecuteInTransactionAsync(async ct =>
                {
                    foreach (var property in batch)
                    {
                        await eventPublisher.PublishAsync(ToEvent(property), ct);
                    }

                    return Result.Success();
                }, cancellationToken);

                await Task.Delay(BatchDelay, cancellationToken);
            }
        }

        private async Task ReplayAccommodationsAsync(string tenantId, CancellationToken cancellationToken)
        {
            var accommodations = await dbContext.Accommodations.AsNoTracking()
                .Where(accommodation => accommodation.TenantId == tenantId)
                .OrderBy(accommodation => accommodation.Id)
                .ToListAsync(cancellationToken);

            foreach (var batch in accommodations.Chunk(BatchSize))
            {
                await dbContext.ExecuteInTransactionAsync(async ct =>
                {
                    foreach (var accommodation in batch)
                    {
                        await eventPublisher.PublishAsync(ToEvent(accommodation), ct);
                    }

                    return Result.Success();
                }, cancellationToken);

                await Task.Delay(BatchDelay, cancellationToken);
            }
        }

        private static PropertyCreatedEvent ToEvent(Property property) => new()
        {
            TenantId = property.TenantId,
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
        };

        private static AccommodationCreatedEvent ToEvent(Accommodation accommodation) => new()
        {
            TenantId = accommodation.TenantId,
            AccommodationId = accommodation.Id,
            PropertyId = accommodation.PropertyId,
            Name = accommodation.Name,
            Description = accommodation.Description,
            Bedrooms = accommodation.Bedrooms,
            Images = accommodation.Images,
            Quantity = accommodation.Quantity,
            PriceRange = accommodation.PriceRange.IsEmpty ? null : accommodation.PriceRange,
            Facilities = accommodation.Facilities.Select(facility => facility.ToString()).ToArray()
        };
    }
}
