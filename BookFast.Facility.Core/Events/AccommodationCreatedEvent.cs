using BookFast.Facility.Core.Models;

namespace BookFast.Facility.Core.Events
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

            FacilityId = accommodation.FacilityId;
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
