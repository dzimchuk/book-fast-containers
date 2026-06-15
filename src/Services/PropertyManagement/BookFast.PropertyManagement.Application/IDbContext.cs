using BookFast.PropertyManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application
{
    public interface IDbContext
    {
        DbSet<Property> Properties { get; set; }
        DbSet<Accommodation> Accommodations { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default);
    }
}
