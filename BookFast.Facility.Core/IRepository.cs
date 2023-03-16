namespace BookFast.Facility.Core
{
    public interface IRepository<TEntity, TKey>
    {
        Task<TKey> AddAsync(TEntity entity);
        Task<TEntity> FindAsync(TKey id);
        Task<bool> AnyAsync(TKey id);
        void Delete(TKey id);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
