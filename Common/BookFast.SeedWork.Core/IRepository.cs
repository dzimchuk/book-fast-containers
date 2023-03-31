using BookFast.SeedWork.Modeling;
using System.Linq.Expressions;

namespace BookFast.SeedWork.Core
{
    public interface IRepository<TEntity, TKey> where TEntity : Entity<TKey>, IAggregateRoot
    {
        Task<TKey> AddAsync(TEntity entity);
        
        Task<TEntity> FindAsync(TKey id);
        
        Task<bool> AnyAsync(TKey id);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter);
        
        void Delete(TKey id);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
