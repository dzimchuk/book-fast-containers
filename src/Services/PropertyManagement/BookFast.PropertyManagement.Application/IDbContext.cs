using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application
{
    public interface IDbContext
    {
        DbSet<Property> Properties { get; set; }
        DbSet<Accommodation> Accommodations { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> operation, CancellationToken cancellationToken = default);
        Task<Result<TResponse>> ExecuteInTransactionAsync<TResponse>(Func<CancellationToken, Task<Result<TResponse>>> operation, CancellationToken cancellationToken = default);
    }
}
