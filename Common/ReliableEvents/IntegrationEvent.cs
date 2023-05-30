using System.Text.Json.Serialization;

namespace ReliableEvents
{
    public record IntegrationEvent
    {
        public IntegrationEvent()
        {
            EventId = Guid.NewGuid().ToString().ToLowerInvariant();
            OccurredAt = DateTimeOffset.UtcNow;
        }

        [JsonConstructor]
        public IntegrationEvent(string id, DateTime createDate)
        {
            EventId = id;
            OccurredAt = createDate;
        }

        [JsonInclude]
        public string EventId { get; init; }

        [JsonInclude]
        public DateTimeOffset OccurredAt { get; init; }
    }
}
