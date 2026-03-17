//TODO: link nuget

namespace System.Collections.Generic;

internal static class CollectionsExtentions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence)
    where T : struct
    {
        // return enumerable.Where(e => e != null).Select(e => e!);
        foreach (var item in sequence)
        {
            if (item == null)
                continue;
            yield return item.Value;
        }
    }
    
    // returns 'null' only when not found. Only allowed when 'TVal : notnull'.
    public static TVal? TryGetOrNull<TKey, TVal>(this IDictionary<TKey, TVal>src, TKey key)
    where TVal : notnull
    {
        var b = src.TryGetValue(key, out var value);
        if (b == false)
            return default;
        return value;
    }
}