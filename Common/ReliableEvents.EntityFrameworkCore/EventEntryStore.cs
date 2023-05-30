using Microsoft.EntityFrameworkCore;
using ReliableEvents.Persistence;
using System.Data.Common;

namespace ReliableEvents.EntityFrameworkCore
{
    internal class EventEntryStore : IEventEntryStore
    {
        private readonly StoreContext storeContext;

        public EventEntryStore(StoreContext storeContext)
        {
            this.storeContext = storeContext;
        }

        public Task PersistAsync(EventEntry eventEntry, DbTransaction transaction)
        {
            storeContext.Database.UseTransaction(transaction);

            storeContext.Events.Add(eventEntry);

            return storeContext.SaveChangesAsync();
        }
    }
}
