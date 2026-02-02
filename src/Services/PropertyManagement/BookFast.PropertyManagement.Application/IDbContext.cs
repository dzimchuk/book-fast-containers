using BookFast.PropertyManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application
{
    public interface IDbContext
    {
        DbSet<Property> Properties { get; set; }
        DbSet<Accommodation> Accommodations { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
