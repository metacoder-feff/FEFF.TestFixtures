//TODO: link nuget

namespace FEFF.Extensions;

internal class EnumMatchException : InvalidOperationException
{
    public static EnumMatchException From<T>(T value)
    where T : struct, Enum
    {
        var n = typeof(T).Name;
        var i = value.ToString("D"); // print as underlying TInteger 
        var v = value.ToString("F"); // print all flags delimited by ','
        var s = value.ToString("G"); // print default (number if multiple flags)

        var isDefined = Enum.IsDefined(value);
        var hasMultipleFlags = v != s;

        var postfix = (isDefined, hasMultipleFlags) switch
        {
            (_, true) => " - value contains multiple flags",
            (false, _) => " - value is not defined in the enum type",
            _ => "",
        };

        return new EnumMatchException($"Enum match error: Got enum type '{n}' with value '{v}' ({i}){postfix}.");
    }

    public EnumMatchException(string? message) : base(message)
    {
    }
}
