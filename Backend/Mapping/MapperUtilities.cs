using System.Reflection;

namespace Backend.Mapping;

public class MapperUtilities
{
    public virtual List<TTarget> Map<TSource, TTarget>(List<TSource> sources)
    {
        MethodInfo method = GetMappingMethod<TSource, TTarget>();
        object? methodParentInstance = Activator.CreateInstance(method.DeclaringType!);

        return [.. sources.Select(s => MapBase<TSource, TTarget>(method, methodParentInstance, s))];
    }

    public virtual IEnumerable<TTarget> Map<TSource, TTarget>(IEnumerable<TSource> sources)
    {
        MethodInfo method = GetMappingMethod<TSource, TTarget>();
        object? methodParentInstance = Activator.CreateInstance(method.DeclaringType!);

        return sources.Select(s => MapBase<TSource, TTarget>(method, methodParentInstance, s));
    }

    public virtual TTarget[] Map<TSource, TTarget>(TSource[] sources)
    {
        MethodInfo method = GetMappingMethod<TSource, TTarget>();
        object? methodParentInstance = Activator.CreateInstance(method.DeclaringType!);

        return [.. sources.Select(s => MapBase<TSource, TTarget>(method, methodParentInstance, s))];
    }

    public virtual TTarget Map<TSource, TTarget>(TSource source)
    {
        MethodInfo method = GetMappingMethod<TSource, TTarget>();
        object? methodParentInstance = Activator.CreateInstance(method.DeclaringType!);

        return MapBase<TSource, TTarget>(method, methodParentInstance, source);
    }

    //By doing the core mapping in a separate method
    //We minimize the amount of reflection calls needed, as we only need to find the mapping method once per mapping operation, and then we can reuse it for each source object
    protected virtual TTarget MapBase<TSource, TTarget>(MethodInfo method, object? methodParentInstance, TSource source)
    {
        //Invoke the method found method using an instance of the class that contains it, passing the source object as a parameter
        TTarget? result = (TTarget?) method.Invoke(methodParentInstance, [source]);
        return result ?? throw new InvalidOperationException("Mapping method returned null.");
    }

    protected virtual MethodInfo GetMappingMethod<TSource, TTarget>()
    {
        //Get all types in the current assembly that are subclasses of MapperUtilities
        Type parentType = typeof(MapperUtilities);
        Assembly currentAssembly = parentType.Assembly;
        IEnumerable<Type> mappers = currentAssembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(parentType));

        //Iterate through the mappers and find a method that has the correct signature for mapping from TSource to TTarget
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

            if (foundMethod != null)
            {
                return foundMethod;
            }
        }

        throw new InvalidOperationException("No mapping method found for the given source and target types.");
    }
}
