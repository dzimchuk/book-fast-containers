using BookFast.Common.Application.Messaging;

namespace BookFast.Booking.Application.Accommodations.MarkNotBookable
{
    public class MarkAccommodationNotBookableCommand : ICommand
    {
        public string TenantId { get; init; }
        public Guid PropertyId { get; init; }
        public Guid AccommodationId { get; init; }

        public DateTimeOffset OccurredAt { get; set; }
    }
}
