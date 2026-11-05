namespace BookFast.Common.Domain
{
    public record IntegrationEvent : Event
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
