using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookFast.Facility.Infrastructure
{
    /// dotnet ef migrations add FacilityService_XXX
    /// dotnet ef migrations script -i -o ../facility.sql
    internal class FacilityContextFactory : IDesignTimeDbContextFactory<FacilityContext>
    {
        public FacilityContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FacilityContext>()
                .UseSqlServer(""/*ConfigurationHelper.GetConnectionString("BookFast.Facility")*/);

            return new FacilityContext(optionsBuilder.Options);
        }
    }
}