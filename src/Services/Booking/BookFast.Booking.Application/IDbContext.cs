using BookFast.Booking.Domain;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Application
{
    public interface IDbContext
    {
        DbSet<Accommodation> Accommodations { get; set; }

        DbSet<Reservation> Reservations { get; set; }

        DbSet<PaymentAttempt> PaymentAttempts { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> operation, CancellationToken cancellationToken = default);
        Task<Result<TResponse>> ExecuteInTransactionAsync<TResponse>(Func<CancellationToken, Task<Result<TResponse>>> operation, CancellationToken cancellationToken = default);
        Task<Result<TResponse>> ExecuteInTransactionWithConcurrencyRetryAsync<TResponse>(Func<CancellationToken, Task<Result<TResponse>>> operation, CancellationToken cancellationToken = default);
    }
}
