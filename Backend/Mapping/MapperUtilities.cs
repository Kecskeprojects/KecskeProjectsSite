using System.Reflection;

namespace Backend.Mapping;

public class MapperUtilities
{
    public virtual List<TTarget> Map<TSource, TTarget>(List<TSource> sources)
    {
        return [.. sources.Select(Map<TSource, TTarget>)];
    }

    public virtual IEnumerable<TTarget> Map<TSource, TTarget>(IEnumerable<TSource> sources)
    {
        return sources.Select(Map<TSource, TTarget>);
    }

    public virtual TTarget[] Map<TSource, TTarget>(TSource[] sources)
    {
        return [.. sources.Select(Map<TSource, TTarget>)];
    }

    public virtual TTarget Map<TSource, TTarget>(TSource source)
    {
        MethodInfo method = GetMappingMethod<TSource, TTarget>();

        TTarget? result = (TTarget?) method.Invoke(this, [source]);
        return result ?? throw new InvalidOperationException("Mapping method returned null.");
    }

    protected virtual MethodInfo GetMappingMethod<TSource, TTarget>()
    {
        Type parentType = typeof(MapperUtilities);
        Assembly currentAssembly = parentType.Assembly;
        IEnumerable<Type> mappers = currentAssembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(parentType));

        foreach (Type type in mappers)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            MethodInfo? foundMethod =
                methods
                .FirstOrDefault(m =>
                    m.ReturnType == typeof(TTarget) &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(TSource) &&
                    m.Name != nameof(Map));

            if(foundMethod != null)
            {
                return foundMethod;
            }
        }

        throw new InvalidOperationException("No mapping method found for the given source and target types.");
    }
}
