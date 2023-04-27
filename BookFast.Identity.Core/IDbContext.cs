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
    }
}
