using System.Data.Common;

namespace ReliableEvents.Persistence
{
    public interface IEventEntryStore
    {
        Task PersistAsync(EventEntry eventEntry, DbTransaction transaction);
    }
}
