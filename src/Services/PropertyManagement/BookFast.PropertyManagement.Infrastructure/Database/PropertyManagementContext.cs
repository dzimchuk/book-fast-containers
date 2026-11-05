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

        public async Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> operation, CancellationToken cancellationToken = default)
            => await ExecuteInTransactionAsync<int>(
                async ct =>
                {
                    var result = await operation(ct);
                    return result.IsSuccess
                        ? Result.Success(0)
                        : Result.Failure<int>(result.Error);
                },
                cancellationToken);

        public async Task<Result<TResponse>> ExecuteInTransactionAsync<TResponse>(Func<CancellationToken, Task<Result<TResponse>>> operation, CancellationToken cancellationToken = default)
        {
            // https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
            var executionStrategy = Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

                var result = await operation(cancellationToken);

                if (result.IsSuccess)
                {
                    await SaveChangesAsync(cancellationToken); // make sure to persist outbox messages
                    await transaction.CommitAsync(cancellationToken);
                }

                return result;
            });
        }
    }
}