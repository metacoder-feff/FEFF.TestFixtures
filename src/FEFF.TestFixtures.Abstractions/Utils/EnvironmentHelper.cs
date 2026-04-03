using System.Collections;
using System.Collections.Frozen;

namespace FEFF.Extensions;

//TODO: link nuget

internal static class EnvironmentHelper
{
    /// <summary>
    /// Typed version of Environment.GetEnvironmentVariables().
    /// Skips env records when key or value is null, that typically should not occur.
    /// </summary>
    public static FrozenDictionary<string, string> GetEnvironmentVariables()
    {
        return Environment
                    .GetEnvironmentVariables()
                    .Cast<DictionaryEntry>()
                    .Select(TryMakeTyped<string, string>)
                    .WhereNotNull()
                    .ToFrozenDictionary();
    }

    private static KeyValuePair<TKey, TVal>? TryMakeTyped<TKey, TVal>(DictionaryEntry src)
    {
        if(src.Key is TKey k == false)
            return null;

        if(src.Value is TVal v == false)
            return null;

        return new KeyValuePair<TKey, TVal>(k, v);
    }
}