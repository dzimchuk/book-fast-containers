using BookFast.SeedWork.Modeling;

namespace BookFast.PropertyManagement.Core.Models
{
    public class Property : Entity<int>
    {
        public string Owner { get; private set; }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string[] Images { get; private set; }

        public Address Address { get; private set; }
        public Location Location { get; private set; }

        public bool IsActive { get; private set; }

        public static Property NewProperty(string owner,
            string name,
            string description,
            Address address,
            Location location,
            string[] images)
        {
            var facility = new Property
            {
                Owner = owner ?? throw new ArgumentNullException(nameof(owner)),
                Name = name ?? throw new ArgumentNullException(nameof(name)),
                Description = description,
                Address = address,
                Location = location,
                Images = ImagePathHelper.CleanUp(images),
                IsActive = true
            };

            return facility;
        }

        public void Update(
            string name,
            string description,
            Address address,
            Location location,
            string[] images)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            Address = address;
            Location = location;
            Images = ImagePathHelper.Merge(Images, images);
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}