using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookFast.Identity.Infrastructure
{
    /// <summary>
    /// A design time factory that is used by EF tools (see https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli).
    /// 
    /// We don't need a connection string to add/remove migrations and generate SQL script.
    /// 
    /// dotnet ef migrations add Identity_XXX
    /// dotnet ef migrations script -i -o ../identity.sql
    /// </summary>
    internal class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
    {
        public IdentityContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionsBuilder.UseSqlServer("");

            return new IdentityContext(optionsBuilder.Options);
        }
    }
}
