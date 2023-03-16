namespace BookFast.Facility.Core.Models
{
    public class Facility
    {
        public int Id { get; set; }

        public string Owner { get; private set; }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string StreetAddress { get; private set; }
        public string[] Images { get; private set; }

        public Location Location { get; private set; }

        public static Facility NewFacility(string owner,
            string name,
            string description,
            string streetAddress,
            double? latitude,
            double? longitude,
            string[] images)
        {
            var facility = new Facility
            {
                Owner = owner ?? throw new ArgumentNullException(nameof(owner)),
                Name = name ?? throw new ArgumentNullException(nameof(name)),
                Description = description,
                StreetAddress = streetAddress ?? throw new ArgumentNullException(streetAddress),
                Location = latitude != null && longitude != null ? new Location(latitude.Value, longitude.Value) : null,
                Images = ImagePathHelper.CleanUp(images)
            };

            return facility;
        }

        public void Update(
            string name,
            string description,
            string streetAddress,
            double? latitude,
            double? longitude,
            string[] images)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            StreetAddress = streetAddress ?? throw new ArgumentNullException(streetAddress);
            Location = latitude != null && longitude != null ? new Location(latitude.Value, longitude.Value) : null;
            Images = ImagePathHelper.Merge(Images, images);
        }
    }
}