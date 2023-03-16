namespace BookFast.Facility.Core.Models
{
    public class Accommodation
    {
        public int Id { get; set; }

        public int FacilityId { get; private set; }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public int RoomCount { get; private set; }
        public string[] Images { get; private set; }

        public static Accommodation NewAccommodation(
            int facilityId,
            string name,
            string description,
            int roomCount,
            string[] images)
        {
            var accommodation = new Accommodation
            {
                FacilityId = facilityId,
                Name = name ?? throw new ArgumentNullException(nameof(name)),
                Description = description,
                RoomCount = roomCount,
                Images = ImagePathHelper.CleanUp(images)
            };

            return accommodation;
        }

        public void Update(
            string name,
            string description,
            int roomCount,
            string[] images)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            RoomCount = roomCount;
            Images = ImagePathHelper.Merge(Images, images);
        }
    }
}