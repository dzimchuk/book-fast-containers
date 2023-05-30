using BookFast.PropertyManagement.Core.Models;

namespace BookFast.PropertyManagement.Core.Commands.CreateAccommodation
{
    public class AccommodationCreatedEvent
    {
        private readonly Accommodation accommodation;
        private int? id;

        public AccommodationCreatedEvent()
        {
        }

        public AccommodationCreatedEvent(Accommodation accommodation)
        {
            this.accommodation = accommodation;

            FacilityId = accommodation.PropertyId;
            Name = accommodation.Name;
            Description = accommodation.Description;
            RoomCount = accommodation.RoomCount;
            Images = accommodation.Images;
        }

        public int Id
        {
            get => id ?? accommodation.Id;
            set => id = value;
        }

        public int FacilityId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int RoomCount { get; set; }
        public string[] Images { get; set; }
    }
}
