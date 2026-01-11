using System.Diagnostics.CodeAnalysis;

namespace Backend.Tools;

public static class CollectionTools
{
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] ICollection<T>? collection)
    {
        return collection == null || collection.Count == 0;
    }
}
