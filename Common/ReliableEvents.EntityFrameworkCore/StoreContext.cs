using Microsoft.EntityFrameworkCore;
using ReliableEvents.Persistence;

namespace ReliableEvents.EntityFrameworkCore
{
    internal class StoreContext : DbContext
    {
        public DbSet<EventEntry> Events { get; set; }
    }
}
