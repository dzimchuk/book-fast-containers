using BookFast.Common.Domain;
using MassTransit;

namespace BookFast.PropertyManagement.Integration
{
    [EntityName("accommodation-updated-event")]
    public record AccommodationUpdatedEvent
    {
        public string TenantId { get; init; }
        public Guid PropertyId { get; init; }
        public Guid AccommodationId { get; init; }

        public string Name { get; init; }
        public string Description { get; init; }
        public int? Bedrooms { get; init; }
        public string[] Images { get; init; }

        public int Quantity { get; init; }
        public PriceRange PriceRange { get; init; }

        public string[] Facilities { get; init; }
    }
}
