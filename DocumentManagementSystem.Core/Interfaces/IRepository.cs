using System.Linq.Expressions;
namespace DocumentManagementSystem.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(string stringId);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>> include = null);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    IQueryable<T> GetQueryable();
}