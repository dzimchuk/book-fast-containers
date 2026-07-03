using MassTransit;

namespace BookFast.PropertyManagement.Integration
{
    [EntityName("accommodation-deleted-event")]
    public record AccommodationDeletedEvent
    {
        public string TenantId { get; init; }
        public Guid PropertyId { get; init; }
        public Guid AccommodationId { get; init; }
    }
}
