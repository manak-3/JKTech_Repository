using System.Linq.Expressions;
using DocumentManagementSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementSystem.Infrastructure.Data;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
	public async Task<T> GetByIdAsync(string id)
	{
		var keyProperty = _dbSet.EntityType.FindPrimaryKey().Properties.First();
		var keyType = keyProperty.ClrType;

		object convertedId;

		try
		{
			if (keyType == typeof(Guid))
			{
				convertedId = Guid.Parse(id);
			}
			else
			{
				convertedId = Convert.ChangeType(id, keyType);
			}
		}
		catch (Exception ex)
		{
			throw new InvalidCastException($"Cannot convert id '{id}' to type '{keyType.Name}'", ex);
		}

		return await _dbSet.FindAsync(convertedId);
	}

	public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(
     Expression<Func<T, bool>> predicate,
     Func<IQueryable<T>, IQueryable<T>> include = null)
    {
        IQueryable<T> query = _dbSet;

        if (include != null)
        {
            query = include(query);
        }

        return await query.Where(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}