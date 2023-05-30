namespace BookFast.PropertyManagement.Core
{
    public interface IDbContext
    {
        DbSet<Property> Properties { get; set; }
        DbSet<Accommodation> Accommodations { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
