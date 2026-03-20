using BookFast.Common.Domain;

namespace BookFast.PropertyManagement.Domain
{
    public class Accommodation : Entity<Guid>
    {
        public string TenantId { get; private set; }

        public Guid PropertyId { get; private set; }

        public string Name { get; private set; }
        public string Description { get; private set; }

        public int RoomCount { get; private set; }
        public string[] Images { get; private set; }

        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        public bool IsActive { get; private set; }

        public static Accommodation NewAccommodation(
            string tenantId,
            Guid propertyId,
            string name,
            string description,
            int roomCount,
            string[] images,
            int quantity,
            decimal price)
        {
            var accommodation = new Accommodation
            {
                TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId)),
                PropertyId = propertyId,
                Name = name ?? throw new ArgumentNullException(nameof(name)),
                Description = description,
                RoomCount = roomCount,
                Images = ImagePathHelper.CleanUp(images),
                Quantity = quantity,
                Price = price,
                IsActive = true
            };

            return accommodation;
        }

        public void Update(
            string name,
            string description,
            int roomCount,
            string[] images,
            int quantity,
            decimal price)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            RoomCount = roomCount;
            Images = ImagePathHelper.Merge(Images, images);
            Quantity = quantity;
            Price = price;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}