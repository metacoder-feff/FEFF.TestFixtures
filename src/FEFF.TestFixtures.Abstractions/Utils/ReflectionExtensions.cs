using System.Reflection;

namespace FEFF.Extensions.Reflection;

//TODO: link nuget

internal static class ReflectionExtensions
{
    public static T? TryGetPrivateInstaceFieldValue<T>(this object obj, string fieldName)
    where T : class
    {
        return obj
            .GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(obj)
            as T;
    }

    public static T? TryGetPrivateInstacePropertyValue<T>(this object obj, string fieldName)
    where T : class
    {
        return obj
            .GetType()
            .GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(obj)
            as T;
    }
}