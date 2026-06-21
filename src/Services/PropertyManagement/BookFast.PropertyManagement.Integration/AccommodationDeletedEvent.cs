namespace BookFast.PropertyManagement.Integration
{
    public record AccommodationDeletedEvent
    {
        public string TenantId { get; init; }
        public Guid PropertyId { get; init; }
        public Guid AccommodationId { get; init; }
    }
}
