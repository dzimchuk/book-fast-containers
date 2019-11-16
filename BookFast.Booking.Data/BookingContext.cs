using BookFast.Booking.Data.Configurations;
using BookFast.Booking.Data.Models;
using BookFast.ReliableEvents;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Data
{
    internal class BookingContext : DbContext
    {
        public BookingContext(DbContextOptions<BookingContext> options) : base(options)
        {
        }

        public DbSet<BookingRecord> BookingRecords { get; set; }
        public DbSet<ReliableEvent> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookingRecordConfiguration());
            modelBuilder.ApplyConfiguration(new ReliableEventConfiguration());
        }
    }
}
