using BookFast.PropertyManagement.Application;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Infrastructure.Database
{
    internal class PropertyManagementContext : DbContext, IDbContext
    {
        public PropertyManagementContext(DbContextOptions<PropertyManagementContext> options) : base(options)
        {
        }

        // needed for tooling, alternatively one can implement IDesignTimeDbContextFactory<TContext>
        //public PropertyManagementContext()
        //{
        //}

        public DbSet<Property> Properties { get; set; }
        public DbSet<Accommodation> Accommodations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schemas.PropertyManagement);

            modelBuilder.HasSequence(PropertyConfiguration.SequenceName).IncrementsBy(1);
            modelBuilder.HasSequence(AccommodationConfiguration.SequenceName).IncrementsBy(1);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PropertyManagementContext).Assembly);
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