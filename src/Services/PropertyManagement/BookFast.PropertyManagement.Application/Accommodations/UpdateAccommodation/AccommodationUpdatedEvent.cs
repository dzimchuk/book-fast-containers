namespace BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation
{
    public class AccommodationUpdatedEvent
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int RoomCount { get; set; }
        public string[] Images { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
