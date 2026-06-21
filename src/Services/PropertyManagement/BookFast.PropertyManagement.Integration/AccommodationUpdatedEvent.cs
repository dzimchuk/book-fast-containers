namespace BookFast.PropertyManagement.Integration
{
    public record AccommodationUpdatedEvent
    {
        public string TenantId { get; init; }
        public Guid PropertyId { get; init; }
        public Guid AccommodationId { get; init; }

        public string Name { get; init; }
        public string Description { get; init; }
        public int RoomCount { get; init; }
        public string[] Images { get; init; }

        public int Quantity { get; init; }
        public decimal Price { get; init; }
    }
}
