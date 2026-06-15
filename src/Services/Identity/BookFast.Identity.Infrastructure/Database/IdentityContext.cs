using BookFast.Common.SeedWork;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace BookFast.Identity.Infrastructure.Database
{
    internal class IdentityContext : IdentityDbContext<User, Role, string>, IDbContext
    {
        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Register the entity sets needed by OpenIddict.
            // Note: use the generic overload if you need
            // to replace the default OpenIddict entities.
            builder.UseOpenIddict();

            builder.HasDefaultSchema(Schemas.Identity);

            builder.ApplyConfigurationsFromAssembly(typeof(IdentityContext).Assembly);

            builder.AddInboxStateEntity();
            builder.AddOutboxMessageEntity();
            builder.AddOutboxStateEntity();

            builder.RewriteIdentityTableNames();
            builder.RewriteOpenIddictTableNames();
        }

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
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                                                       new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                                                       TransactionScopeAsyncFlowOption.Enabled);

                TResult result = await operation(cancellationToken);

                if (ShouldCommitTransaction(result))
                {
                    await SaveChangesAsync(cancellationToken); // make sure to persist outbox messages
                    scope.Complete();
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