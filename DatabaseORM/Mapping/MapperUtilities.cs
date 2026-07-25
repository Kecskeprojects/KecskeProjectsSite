using Mapster;

namespace DatabaseORM.Mapping;

public class MapperUtilities
{
    public virtual List<TTarget> Map<TSource, TTarget>(List<TSource> sources)
    {
        return sources.Adapt<List<TTarget>>();
    }

    public virtual IEnumerable<TTarget> Map<TSource, TTarget>(IEnumerable<TSource> sources)
    {
        return sources.Adapt<IEnumerable<TTarget>>();
    }

    public virtual TTarget[] Map<TSource, TTarget>(TSource[] sources)
    {
        return sources.Adapt<TTarget[]>();
    }

    public virtual TTarget? Map<TSource, TTarget>(TSource source)
    {
        return source.Adapt<TTarget>();
    }
}
