namespace BookFast.PropertyManagement.Core.Commands.UpdateAccommodation
{
    public class AccommodationUpdatedEvent
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int RoomCount { get; set; }
        public string[] Images { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
