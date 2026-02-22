using AutoMapper;
using Backend.Communication.Internal;
using Backend.Database.Repository;
using Backend.Enums;
using System.Linq.Expressions;

namespace Backend.Database.Service;

public class GenericService<TEntity>(GenericRepository<TEntity> repository, IMapper mapper) where TEntity : class
{
    protected readonly GenericRepository<TEntity> repository = repository;

    #region Simple Queries
    public async Task<DatabaseActionResult<TEntity?>> FindByIdAsync(int id)
    {
        TEntity? result = await repository.FindByIdAsync(id);

        return CreateResult(
            result is not null
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.NotFound,
            result);
    }

    public async Task<DatabaseActionResult<TEntity?>> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        TEntity? result = await repository.FirstOrDefaultAsync(predicate, includes);

        return CreateResult(
            result is not null
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.NotFound,
            result);
    }

    public async Task<DatabaseActionResult<List<TEntity>?>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        List<TEntity> result = await repository.GetListAsync(predicate, orderBy, ascending, includes);

        return CreateResult(
            result.Count > 0
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.NotFound,
            result);
    }

    public async Task<DatabaseActionResult<bool>> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate)
    {
        bool result = await repository.ExistsAsync(predicate);

        return CreateResult(
            result
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.NotFound,
            result);
    }

    public async Task<DatabaseActionResult<int>> SaveChangesAsync()
    {
        int result = await repository.SaveChangesAsync();

        return CreateResult(
            result > 0
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.Failure,
            result);
    }

    public async Task<DatabaseActionResult<int>> AddAsync(TEntity item, bool saveChanges = true)
    {
        int result = await repository.AddAsync(item, saveChanges);

        return CreateResult(
            result > 0
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.Failure,
            result);
    }

    public async Task<DatabaseActionResult<int>> AddAsync(ICollection<TEntity> items, bool saveChanges = true)
    {
        int result = await repository.AddAsync(items, saveChanges);

        return CreateResult(
            result > 0
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.Failure,
            result);
    }

    public async Task<DatabaseActionResult<int>> RemoveAsync(TEntity item, bool saveChanges = true)
    {
        int result = await repository.RemoveAsync(item, saveChanges);

        return CreateResult(
            result > 0
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.Failure,
            result);
    }

    public async Task<DatabaseActionResult<int>> RemoveAsync(ICollection<TEntity> items, bool saveChanges = true)
    {
        int result = await repository.RemoveAsync(items, saveChanges);

        return CreateResult(
            result > 0
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.Failure,
            result);
    }

    public async Task<DatabaseActionResult<int>> UpdateAsync(TEntity item, bool saveChanges = true)
    {
        int result = await repository.UpdateAsync(item, saveChanges);

        return CreateResult(
            result > 0
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.Failure,
            result);
    }

    public async Task<DatabaseActionResult<int>> UpdateAsync(ICollection<TEntity> items, bool saveChanges = true)
    {
        int result = await repository.UpdateAsync(items, saveChanges);

        return CreateResult(result > 0
            ? DatabaseActionResultEnum.Success
            : DatabaseActionResultEnum.Failure,
            result);
    }
    #endregion

    public DatabaseActionResult<TData?> CreateResult<TData>(DatabaseActionResultEnum result, TData? data = default, string? message = null)
    {
        return new DatabaseActionResult<TData?>(result, data, message);
    }

    public DatabaseActionResult<TResource?> CreateMappedResult<TData, TResource>(DatabaseActionResultEnum result, TData? data = default, string? message = null)
    {
        TResource? mappedData = data is not null ? mapper.Map<TData, TResource>(data) : default;
        return new DatabaseActionResult<TResource?>(result, mappedData, message);
    }
}
