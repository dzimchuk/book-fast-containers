using BookFast.Facility.Core;
using BookFast.Facility.Core.Models;
using BookFast.Facility.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Facility.Infrastructure
{
    internal class FacilityContext : DbContext, IDbContext
    {
        public FacilityContext(DbContextOptions<FacilityContext> options) : base(options)
        {
        }

        // needed for tooling, alternatively one can implement IDesignTimeDbContextFactory<TContext>
        //public FacilityContext()
        //{
        //}

        public DbSet<Core.Models.Facility> Facilities { get; set; }
        public DbSet<Accommodation> Accommodations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("facility");

            modelBuilder.HasSequence(FacilityConfiguration.SequenceName).IncrementsBy(1);
            modelBuilder.HasSequence(AccommodationConfiguration.SequenceName).IncrementsBy(1);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FacilityContext).Assembly);
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlServer("Data Source=(localdb)\\ProjectsV13;Initial Catalog=BookFast;Trusted_Connection=True;MultipleActiveResultSets=true");
        //    }
        //}
    }
}