using BookFast.Booking.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Booking.Infrastructure
{
    public static class MigrationExtensions
    {
        public static void ApplyMigration(IServiceScope scope)
        {
            using var context = scope.ServiceProvider.GetRequiredService<BookingContext>();

            context.Database.Migrate();
        }
    }
}
