namespace BookFast.Facility.Core
{
    public interface IDbContext
    {
        DbSet<Models.Facility> Facilities { get; set; }
        DbSet<Accommodation> Accommodations { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
