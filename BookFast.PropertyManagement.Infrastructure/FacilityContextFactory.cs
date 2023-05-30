using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookFast.PropertyManagement.Infrastructure
{
    /// dotnet ef migrations add PropertyManagement_XXX
    /// dotnet ef migrations script -i -o ../propertyManagement.sql
    internal class FacilityContextFactory : IDesignTimeDbContextFactory<FacilityContext>
    {
        public FacilityContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FacilityContext>()
                .UseSqlServer(""/*ConfigurationHelper.GetConnectionString("BookFast.PropertyManagement")*/);

            return new FacilityContext(optionsBuilder.Options);
        }
    }
}