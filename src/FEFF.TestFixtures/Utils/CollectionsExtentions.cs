using System.Diagnostics.CodeAnalysis;

//TODO: link nuget

// namespace System.Collections.Generic;
namespace System; // Array

internal static class CollectionsExtentions
{
//TODO: single method?
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

//TODO: test
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence)
    where T : notnull
    {
        // return enumerable.Where(e => e != null).Select(e => e!);
        foreach (var item in sequence)
        {
            if (item == null)
                continue;
            yield return item;
        }
    }
    
//TODO: test
    // returns 'null' only when not found. Only allowed when 'TVal : notnull'.
    public static TVal? TryGetOrNull<TKey, TVal>(this IDictionary<TKey, TVal>src, TKey key)
    where TVal : notnull
    {
        var b = src.TryGetValue(key, out var value);
        if (b == false)
            return default;
        return value;
    }

//TODO: test
    // returns 'null' whether not found or found null
    public static TVal? TryGetBothNull<TKey, TVal>(this IDictionary<TKey, TVal?>src, TKey key)
    where TVal : notnull
    {
        var b = src.TryGetValue(key, out var value);
        if (b == false)
            return default;
        return value;
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IReadOnlyCollection<T>? src)
    {
        if (src == null)
            return true;

        return src.Count <= 0;
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this T[]? src)
    {
        if (src == null)
            return true;

        return src.Length <= 0;
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this List<T>? src)
    {
        if (src == null)
            return true;

        return src.Count <= 0;
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ICollection<T>? src)
    {
        if (src == null)
            return true;

        return src.Count <= 0;
    }
}