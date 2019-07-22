using BookFast.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookFast.Facility.Data
{
    internal class DesignTimeFacilityContextFactory : IDesignTimeDbContextFactory<FacilityContext>
    {
        public FacilityContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FacilityContext>()
                .UseSqlServer(ConfigurationHelper.GetConnectionString("BookFast.Facility"));

            return new FacilityContext(optionsBuilder.Options);
        }
    }
}