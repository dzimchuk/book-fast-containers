using BookFast.Common.Domain;

namespace BookFast.PropertyManagement.Domain
{
    public class Property : Entity<int>
    {
        public string TenantId { get; private set; }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string[] Images { get; private set; }

        public Address Address { get; private set; }
        public Location Location { get; private set; }

        public bool IsActive { get; private set; }

        public static Property NewProperty(string tenantId,
            string name,
            string description,
            Address address,
            Location location,
            string[] images)
        {
            var facility = new Property
            {
                TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId)),
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