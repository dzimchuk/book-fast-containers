using BookFast.Common.SeedWork;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Identity.Core
{
    public interface IDbContext
    {
        DbSet<Tenant> Tenants { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<IdentityUserRole<string>> UserRoles { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> operation, CancellationToken cancellationToken = default);
        Task<Result<TResponse>> ExecuteInTransactionAsync<TResponse>(Func<CancellationToken, Task<Result<TResponse>>> operation, CancellationToken cancellationToken = default);
    }
}
