using BookFast.Common.Domain;
using MassTransit;

namespace BookFast.Booking.Integration
{
    [EntityName("accommodation-bookable-changed-event")]
    public record AccommodationBookableChanged : IntegrationEvent
    {
        public Guid AccommodationId { get; init; }
        public bool Bookable { get; init; }
        public Money Rate { get; init; }
    }
}
