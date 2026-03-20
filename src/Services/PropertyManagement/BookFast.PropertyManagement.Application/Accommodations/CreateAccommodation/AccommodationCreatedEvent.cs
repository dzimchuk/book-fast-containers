using BookFast.PropertyManagement.Domain;

namespace BookFast.PropertyManagement.Application.Accommodations.CreateAccommodation
{
    public class AccommodationCreatedEvent
    {
        private readonly Accommodation accommodation;
        private Guid? id;

        public AccommodationCreatedEvent()
        {
        }

        public AccommodationCreatedEvent(Accommodation accommodation)
        {
            this.accommodation = accommodation;

            PropertyId = accommodation.PropertyId;
            Name = accommodation.Name;
            Description = accommodation.Description;
            RoomCount = accommodation.RoomCount;
            Images = accommodation.Images;
            Quantity = accommodation.Quantity;
            Price = accommodation.Price;
        }

        public Guid Id
        {
            get => id ?? accommodation.Id;
            set => id = value;
        }

        public Guid PropertyId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int RoomCount { get; set; }
        public string[] Images { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
