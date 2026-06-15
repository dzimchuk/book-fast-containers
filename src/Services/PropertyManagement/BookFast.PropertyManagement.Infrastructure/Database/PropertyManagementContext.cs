using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Application;
using BookFast.PropertyManagement.Domain;
using MassTransit;
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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PropertyManagementContext).Assembly);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlServer("Data Source=(localdb)\\ProjectsV13;Initial Catalog=BookFast;Trusted_Connection=True;MultipleActiveResultSets=true");
        //    }
        //}

        public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
            => ExecuteInTransactionAsync<bool>(
                async ct =>
                {
                    await operation(ct);
                    return true;
                },
                cancellationToken);

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
        {
            // https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
            var executionStrategy = Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

                TResult result = await operation(cancellationToken);

                if (ShouldCommitTransaction(result))
                {
                    await SaveChangesAsync(cancellationToken); // make sure to persist outbox messages
                    await transaction.CommitAsync(cancellationToken); 
                }

                return result;
            });
        }

        private static bool ShouldCommitTransaction<TResult>(TResult result)
        {
            if (result is Result r)
            {
                return r.IsSuccess;
            }

            return true;
        }
    }
}