using BookFast.Common.Domain;
using MassTransit;

namespace BookFast.PropertyManagement.Integration
{
    [EntityName("property-deactivated-event")]
    public record PropertyDeactivatedEvent : IntegrationEvent
    {
        public string TenantId { get; init; }
        public Guid PropertyId { get; init; }
    }
}
