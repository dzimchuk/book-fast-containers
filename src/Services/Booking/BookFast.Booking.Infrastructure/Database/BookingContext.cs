using BookFast.Booking.Application;
using BookFast.Booking.Domain;
using BookFast.Common.SeedWork;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Infrastructure.Database
{
    internal class BookingContext(DbContextOptions<BookingContext> options) : DbContext(options), IDbContext
    {
        private const int MaxConcurrencyRetries = 3;

        public DbSet<Accommodation> Accommodations { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<PaymentAttempt> PaymentAttempts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schemas.Booking);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingContext).Assembly);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }

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

        public async Task<Result<TResponse>> ExecuteInTransactionWithConcurrencyRetryAsync<TResponse>(
            Func<CancellationToken, Task<Result<TResponse>>> operation, CancellationToken cancellationToken = default)
        {
            for (var attempt = 1; attempt <= MaxConcurrencyRetries; attempt++)
            {
                try
                {
                    return await ExecuteInTransactionAsync(operation, cancellationToken);
                }
                catch (DbUpdateConcurrencyException) when (attempt < MaxConcurrencyRetries)
                {
                    ChangeTracker.Clear();
                }
            }

            return Result.Failure<TResponse>(Error.Conflict("Concurrency.TooManyConflicts", "Too many concurrent updates. Please try again."));
        }
    }
}
