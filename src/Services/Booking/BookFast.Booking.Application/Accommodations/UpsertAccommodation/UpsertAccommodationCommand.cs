using BookFast.Common.Application.Messaging;
using BookFast.Common.Domain;

namespace BookFast.Booking.Application.Accommodations.UpsertAccommodation
{
    public abstract class UpsertAccommodationCommand : ICommand
    {
        public string TenantId { get; init; }
        public Guid PropertyId { get; init; }
        public Guid AccommodationId { get; init; }
        
        public int Quantity { get; init; }
        public PriceRange PriceRange { get; init; }

        public DateTimeOffset OccurredAt { get; set; }
    }

    public class CreateAccommodationCommand : UpsertAccommodationCommand
    {
    }

    public class UpdateAccommodationCommand : UpsertAccommodationCommand
    {
    }
}
