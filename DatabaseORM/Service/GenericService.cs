using DatabaseORM.Mapping;
using DatabaseORM.Repository;

namespace DatabaseORM.Service;

/// <summary>
/// Base class of the services. Services return plain values (null or an empty list when nothing matches)
/// and throw a <see cref="Communication.Exceptions.ServiceException"/> subclass for outcomes the user should read.
/// </summary>
public abstract class GenericService<TEntity>(GenericRepository<TEntity> repository) where TEntity : class
{
    protected readonly GenericRepository<TEntity> repository = repository;

    protected static List<TResource> MapList<TData, TResource>(List<TData> data)
    {
        return new MapperUtilities().Map<TData, TResource>(data);
    }
}
