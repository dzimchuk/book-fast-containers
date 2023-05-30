using ReliableEvents.Persistence;
using System.Data.Common;

namespace ReliableEvents
{
    public class IntegrationEventsService
    {
        private readonly IEventEntryStore eventEntryStore;

        public Task PersistAsync(IntegrationEvent @event, string transactionId, DbTransaction transaction)
        {
            return eventEntryStore.PersistAsync(EventEntry.FromIntegrationEvent(@event), transaction);
        }

        public Task PublishPersistedEvents(string transactionId)
        {
            return Task.CompletedTask;
        }
    }
}
