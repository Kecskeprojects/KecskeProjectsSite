using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Backend.Database.Repository;

public class GenericRepository<TEntity>(KecskeDatabaseContext context) where TEntity : class
{
    protected readonly KecskeDatabaseContext context = context;
    //Todo: Potentially can be improved with memory caching: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-9.0

    public ValueTask<TEntity?> FindByIdAsync(int id)
    {
        return context.Set<TEntity>().FindAsync(id);
    }

    public Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        IQueryable<TEntity> dbSet = context.Set<TEntity>();
        dbSet = AddWhere(dbSet, predicate);
        dbSet = AddIncludes(dbSet, includes);

        _ = CheckQueryString(dbSet);
        return dbSet.FirstOrDefaultAsync();
    }

    public Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        IQueryable<TEntity> dbSet = context.Set<TEntity>();
        dbSet = AddWhere(dbSet, predicate);
        dbSet = AddIncludes(dbSet, includes);
        dbSet = AddOrderBy(dbSet, orderBy, ascending);

        _ = CheckQueryString(dbSet);
        return dbSet.ToListAsync();
    }

    public Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate)
    {
        IQueryable<TEntity> dbSet = context.Set<TEntity>();
        dbSet = AddWhere(dbSet, predicate);

        return dbSet.AnyAsync();
    }

    public async Task<int> SaveChangesAsync(bool saveChanges = true)
    {
        return saveChanges ? await context.SaveChangesAsync() : 0;
    }

    #region Add
    public async Task<int> AddAsync(TEntity item, bool saveChanges = true)
    {
        _ = context.Set<TEntity>().Add(item);

        return await SaveChangesAsync(saveChanges);
    }

    public async Task<int> AddAsync(ICollection<TEntity> items, bool saveChanges = true)
    {
        context.Set<TEntity>().AddRange(items);

        return await SaveChangesAsync(saveChanges);
    }
    #endregion

    #region Remove
    public async Task<int> RemoveAsync(TEntity item, bool saveChanges = true)
    {
        _ = context.Set<TEntity>().Remove(item);

        return await SaveChangesAsync(saveChanges);
    }

    public async Task<int> RemoveAsync(ICollection<TEntity> items, bool saveChanges = true)
    {
        context.Set<TEntity>().RemoveRange(items);

        return await SaveChangesAsync(saveChanges);
    }

    public async Task<int> RemoveAsync(Expression<Func<TEntity, bool>> predicate, bool saveChanges = true)
    {
        List<TEntity> query = await GetListAsync(predicate);

        context.Set<TEntity>().RemoveRange(query);

        return await SaveChangesAsync(saveChanges);
    }
    #endregion

    #region Update
    public async Task<int> UpdateAsync(TEntity item, bool saveChanges = true)
    {
        _ = context.Set<TEntity>().Update(item);

        return await SaveChangesAsync(saveChanges);
    }

    public async Task<int> UpdateAsync(ICollection<TEntity> items, bool saveChanges = true)
    {
        context.Set<TEntity>().UpdateRange(items);

        return await SaveChangesAsync(saveChanges);
    }
    #endregion

    #region Helper Methods
    public static IQueryable<TEntity> AddWhere(
        IQueryable<TEntity> dbSet,
        Expression<Func<TEntity, bool>>? predicate)
    {
        if (predicate is not null)
        {
            dbSet = dbSet.Where(predicate);
        }

        return dbSet;
    }

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

    private static IQueryable<TEntity> AddOrderBy(
        IQueryable<TEntity> dbSet,
        Expression<Func<TEntity, object>>? orderBy,
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

    private static string CheckQueryString(IQueryable<TEntity> dbSet)
    {
#if DEBUG
        string sqlString = dbSet.ToQueryString();
        return sqlString;
#else
        return string.Empty;
#endif
    }
    #endregion
}
