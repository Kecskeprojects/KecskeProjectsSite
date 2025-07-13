using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Backend.Database.Repository;

public class GenericRepository<TEntity>(KecskeDatabaseContext context) where TEntity : class
{
    protected readonly KecskeDatabaseContext context = context;

    public ValueTask<TEntity?> FindByIdAsync(int id)
    {
        return context.Set<TEntity>().FindAsync(id);
    }

    public Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        IQueryable<TEntity> dbSet = context.Set<TEntity>();

        dbSet = AddIncludes(dbSet, includes);

        return dbSet.FirstOrDefaultAsync(predicate);
    }

    public Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        IQueryable<TEntity> dbSet = context.Set<TEntity>();

        if (predicate is not null)
        {
            dbSet = dbSet.Where(predicate);
        }

        dbSet = AddIncludes(dbSet, includes);
        dbSet = OrderBy(dbSet, orderBy, ascending);

        return dbSet.ToListAsync();
    }

    public Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate)
    {
        return context.Set<TEntity>().AnyAsync(predicate);
    }

    public Task<int> SaveChangesAsync()
    {
        return context.SaveChangesAsync();
    }

    #region Add
    public async Task<int> AddAsync(TEntity item, bool saveChanges = true)
    {
        context.Set<TEntity>().Add(item);

        return saveChanges ? await context.SaveChangesAsync() : 0;
    }

    public async Task<int> AddAsync(ICollection<TEntity> items, bool saveChanges = true)
    {
        context.Set<TEntity>().AddRange(items);

        return saveChanges ? await context.SaveChangesAsync() : 0;
    }
    #endregion

    #region Remove
    public async Task<int> RemoveAsync(TEntity item, bool saveChanges = true)
    {
        context.Set<TEntity>().Remove(item);

        return saveChanges ? await context.SaveChangesAsync() : 0;
    }

    public async Task<int> RemoveAsync(ICollection<TEntity> items, bool saveChanges = true)
    {
        context.Set<TEntity>().RemoveRange(items);

        return saveChanges ? await context.SaveChangesAsync() : 0;
    }
    #endregion

    #region Update
    public async Task<int> UpdateAsync(TEntity item, bool saveChanges = true)
    {
        context.Set<TEntity>().Update(item);

        return saveChanges ? await context.SaveChangesAsync() : 0;
    }

    public async Task<int> UpdateAsync(ICollection<TEntity> items, bool saveChanges = true)
    {
        context.Set<TEntity>().UpdateRange(items);

        return saveChanges ? await context.SaveChangesAsync() : 0;
    }
    #endregion

    #region Helper Methods
    public static IQueryable<TEntity> AddIncludes(
        IQueryable<TEntity> dbSet,
        Expression<Func<TEntity, object>>[]? includes)
    {
        if (includes is not null)
        {
            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                dbSet = dbSet.Include(include);
            }
        }

        return dbSet;
    }

    private static IQueryable<TEntity> OrderBy(
        IQueryable<TEntity> dbSet, Expression<Func<TEntity, object>>? orderBy,
        bool ascending = true)
    {
        if (orderBy is not null)
        {
            dbSet = ascending
                ? dbSet.OrderBy(orderBy)
                : dbSet.OrderByDescending(orderBy);
        }

        return dbSet;
    }
    #endregion
}
