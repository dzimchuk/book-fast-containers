namespace ReliableEvents.Persistence
{
    public class EventEntry
    {
        public string TransactionId { get; set; }
        public string EventId { get; set; }

        public string EventName { get; set; }
        public DateTimeOffset OccurredAt { get; set; }

        public string Payload { get; set; }
        public string Metadata { get; set; }

        public static EventEntry FromIntegrationEvent(IntegrationEvent @event)
        {
            return null;
        }
    }
}
