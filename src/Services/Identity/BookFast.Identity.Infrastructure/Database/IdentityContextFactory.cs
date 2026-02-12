using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookFast.Identity.Infrastructure.Database
{
    /// <summary>
    /// A design time factory that is used by EF tools (see https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli).
    /// 
    /// We don't need a connection string to add/remove migrations and generate SQL script.
    /// 
    /// dotnet ef migrations add _Name_ -o Database/Migrations
    /// dotnet ef migrations script -i -o ../Identity.sql
    /// </summary>
    internal class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
    {
        public IdentityContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionsBuilder.UseSqlServer(
                    "",
                    sqlServerOptions => sqlServerOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                    .UseSnakeCaseNamingConvention();

            return new IdentityContext(optionsBuilder.Options);
        }
    }
}
