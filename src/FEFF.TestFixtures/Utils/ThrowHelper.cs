using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FEFF.Extentions;

//TODO: link nuget

[DebuggerNonUserCode]
internal static class ThrowHelper
{
    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if assertion fails.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static void Assert(
        [DoesNotReturnIf(false)]
            bool argument,
        [CallerArgumentExpression(nameof(argument))]
            string? argumentExpression = null)
    {
        if (argument == false)
        {
            throw new InvalidOperationException($"Assertion violated: '{argumentExpression}'");
        }
    }

    /// <summary>
    /// Returns argument if it is not null.<br/>
    /// Throws <see cref="InvalidOperationException"/> otherwise.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static T GuardNotNull<T>(
        [NotNull] T? argument, 
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        where T : notnull
    {
        return argument 
            ?? throw new InvalidOperationException($"Value cannot be null. (Expression '{paramName}')");
    }

    public static class Argument
    {
        public static void ThrowIfNullOrEmpty<T>(
            [NotNull]
                IEnumerable<T>? argument,
            [CallerArgumentExpression(nameof(argument))]
                string? paramName = null)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);

            if(argument.Any() == false)
                throw new ArgumentException("The value cannot be an empty collection.", paramName);
        }

        // The overload to avoid confusion because string is IEnumerable<char>
        public static void ThrowIfNullOrEmpty(
            [NotNull]
                string? argument,
            [CallerArgumentExpression(nameof(argument))]
                string? paramName = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
        }
    }

/* Alternative  'extensible' way
    public interface IArgumentExceptionFactory
    {
        [DoesNotReturn]
        void Throw(string? message, string? paramName);
    }

    public class ArgumentExceptionFactory : IArgumentExceptionFactory
    {
        [DoesNotReturn]
        public void Throw(string? message, string? paramName)
        {
            throw new ArgumentException(message, paramName);
        }
    }

    public static readonly ArgumentExceptionFactory Argument = new ();

    public static void ThrowIfNullOrEmpty<T>(this IArgumentExceptionFactory src,
        [NotNull]
            IEnumerable<T>? argument,
        [CallerArgumentExpression(nameof(argument))]
            string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(argument, paramName);

        if(argument.Any() == false)
            src.Throw("The value cannot be an empty collection.", paramName);
    }
    //*/
}
