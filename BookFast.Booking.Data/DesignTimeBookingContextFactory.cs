using BookFast.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookFast.Booking.Data
{
    internal class DesignTimeBookingContextFactory : IDesignTimeDbContextFactory<BookingContext>
    {
        public BookingContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BookingContext>()
                .UseSqlServer(ConfigurationHelper.GetConnectionString("BookFast.Booking"));

            return new BookingContext(optionsBuilder.Options);
        }
    }
}
