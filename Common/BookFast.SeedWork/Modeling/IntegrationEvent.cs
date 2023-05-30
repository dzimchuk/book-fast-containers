namespace BookFast.SeedWork.Modeling
{
    public class IntegrationEvent : Event
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
