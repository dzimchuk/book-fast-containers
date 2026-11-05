using BookFast.Common.Domain;
using MassTransit;

namespace BookFast.PropertyManagement.Integration
{
    [EntityName("accommodation-deleted-event")]
    public record AccommodationDeletedEvent : IntegrationEvent
    {
        public string TenantId { get; init; }
        public Guid PropertyId { get; init; }
        public Guid AccommodationId { get; init; }
    }
}
