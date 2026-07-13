using BookFast.Common.Domain;
using MassTransit;

namespace BookFast.PropertyManagement.Integration
{
    [EntityName("property-created-event")]
    public record PropertyCreatedEvent : IntegrationEvent
    {
        public string TenantId { get; init; }
        public Guid PropertyId { get; init; }

        public string Name { get; init; }
        public string Description { get; init; }
        public AddressDTO Address { get; init; }
        public LocationDTO Location { get; init; }
        public string[] Facilities { get; init; }
        public string[] Images { get; init; }
    }
}
