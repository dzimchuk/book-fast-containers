namespace BookFast.Facility.Core.Events
{
    public class AccommodationUpdatedEvent
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int RoomCount { get; set; }
        public string[] Images { get; set; }
    }
}
