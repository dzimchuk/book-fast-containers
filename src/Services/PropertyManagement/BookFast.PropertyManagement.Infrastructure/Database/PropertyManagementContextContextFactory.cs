using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookFast.PropertyManagement.Infrastructure.Database
{
    /// dotnet ef migrations add _Name_ -o Database/Migrations
    /// dotnet ef migrations script -i -o ../PropertyManagement.sql
    internal class PropertyManagementContextContextFactory : IDesignTimeDbContextFactory<PropertyManagementContext>
    {
        public PropertyManagementContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PropertyManagementContext>();
            optionsBuilder.UseSqlServer(
                    "",
                    sqlServerOptions => sqlServerOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.PropertyManagement))
                    .UseSnakeCaseNamingConvention();

            return new PropertyManagementContext(optionsBuilder.Options);
        }
    }
}