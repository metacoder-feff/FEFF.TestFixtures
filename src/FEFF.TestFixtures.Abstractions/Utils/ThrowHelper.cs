using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FEFF.Extensions;

//TODO: link nuget

[DebuggerStepThrough] // Break on exceptions only in an outer context when debugging
[StackTraceHidden]    // Do not show these methods in the trace of the test outputs (for AwesomeAssertions)
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
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="argument">The value to check for null.</param>
    /// <param name="paramName">The name of the parameter (auto-populated).</param>
    /// <returns>The argument if not null.</returns>
    /// <remarks>
    /// It is convenient to wrap a complex expression with <see cref="EnsureNotNull"/>:
    /// <example>
    /// <code>
    /// var result = ThrowHelper.EnsureNotNull(anObject?.Nested?.Nested?.Value);
    /// </code>
    /// </example>
    /// Here:
    /// <list type="number">
    /// <item>The whole expression is serialized into error message, if expression is null.</item>
    /// <item>The expression is calculated only once and is returned to consumer, if expression is NOT null.</item>
    /// </list>
    /// </remarks>
    /// <exception cref="InvalidOperationException"></exception>
    public static T EnsureNotNull<T>(
        [NotNull] T? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        where T : notnull
    {
        return argument
            ?? throw new InvalidOperationException($"Value cannot be null. (Expression '{paramName}')");
    }


    [DebuggerStepThrough] // Break on exceptions only in an outer context when debugging
    [StackTraceHidden]    // Do not show these methods in the trace of the test outputs (for AwesomeAssertions)
    public static class Argument
    {
        public static void ThrowIfNullOrEmpty<T>(
            [NotNull]
                IEnumerable<T>? argument,
            [CallerArgumentExpression(nameof(argument))]
                string? paramName = null)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);

            if (argument.Any() == false)
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

    /* Better Alternative  'extensible' way:
            private static void Example1()
            {
                ThrowHelper.Throw<ArgumentException, ArgExFactory>("123");
            }

            [DoesNotReturn]
            public static void Throw<TException, TFactory>(string? msg)
            where TException : Exception
            where TFactory : IExceptionFactory<TException>
            {
                throw TFactory.Create(msg);
            }

            public interface IExceptionFactory<T>
            where T : Exception
            {
                public static abstract T Create(string? msg);
            }

            class ArgExFactory : IExceptionFactory<ArgumentException>
            {
                public static ArgumentException Create(string? msg) => new(msg);
            }
    //*/

    /* Alternative  'extensible' way
            private static void Example2()
            {
                ThrowHelper.Argument2.ThrowIfNullOrEmpty("");
            }

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

            public static readonly ArgumentExceptionFactory Argument2 = new ();

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
