using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookFast.Booking.Infrastructure.Database
{
    /// dotnet ef migrations add _Name_ -o Database/Migrations
    /// dotnet ef migrations script -i -o ../Booking.sql
    internal class BookingContextFactory : IDesignTimeDbContextFactory<BookingContext>
    {
        public BookingContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BookingContext>();
            optionsBuilder.UseSqlServer(
                    "",
                    sqlServerOptions => sqlServerOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Booking))
                    .UseSnakeCaseNamingConvention();

            return new BookingContext(optionsBuilder.Options);
        }
    }
}
