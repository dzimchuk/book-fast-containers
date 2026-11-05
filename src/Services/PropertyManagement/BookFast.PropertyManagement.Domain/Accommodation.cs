using BookFast.Common.Domain;

namespace BookFast.PropertyManagement.Domain
{
    public class Accommodation : Entity<Guid>
    {
        public string TenantId { get; private set; }

        public Guid PropertyId { get; private set; }

        public string Name { get; private set; }
        public string Description { get; private set; }

        public int? Bedrooms { get; private set; }
        public string[] Images { get; private set; }

        public int Quantity { get; private set; }
        public PriceRange PriceRange { get; private set; }

        public Facility[] Facilities { get; private set; }

        public bool IsActive { get; private set; }

        public static Accommodation NewAccommodation(
            string tenantId,
            Guid propertyId,
            string name,
            string description,
            int? bedrooms,
            string[] images,
            int quantity,
            PriceRange priceRange,
            Facility[] facilities)
        {
            var accommodation = new Accommodation
            {
                TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId)),
                PropertyId = propertyId,
                Name = name ?? throw new ArgumentNullException(nameof(name)),
                Description = description,
                Bedrooms = bedrooms,
                Images = ImagePathHelper.CleanUp(images),
                Quantity = quantity,
                PriceRange = new PriceRange(priceRange?.MinPrice, priceRange?.MaxPrice),
                Facilities = facilities?.Distinct().ToArray() ?? [],
                IsActive = true
            };

            return accommodation;
        }

        public void Update(
            string name,
            string description,
            int? bedrooms,
            string[] images,
            int quantity,
            PriceRange priceRange,
            Facility[] facilities)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            Bedrooms = bedrooms;
            Images = ImagePathHelper.Merge(Images, images);
            Quantity = quantity;
            PriceRange = new PriceRange(priceRange?.MinPrice, priceRange?.MaxPrice);
            Facilities = facilities?.Distinct().ToArray() ?? [];
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}