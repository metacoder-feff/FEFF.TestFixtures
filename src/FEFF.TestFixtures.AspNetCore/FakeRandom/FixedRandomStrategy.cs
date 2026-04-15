namespace FEFF.TestFixtures.AspNetCore.Randomness;

/// <summary>
/// A random strategy that always returns the same fixed value.
/// </summary>
/// <typeparam name="T">The type of the fixed value.</typeparam>
public class FixedNextStrategy<T> : INextStrategy<T>
{
    /// <summary>
    /// Gets or sets the fixed value to return.
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Creates a new <see cref="FixedNextStrategy{T}"/> that always returns the specified value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    public FixedNextStrategy(T value)
    {
        Value = value;
    }

    /// <inheritdoc/>
    public T Next()
    {
        return Value;
    }
}

/// <summary>
/// Factory methods for creating <see cref="FixedNextStrategy{T}"/> instances.
/// </summary>
public static class FixedNextStrategy
{
    /// <summary>
    /// Creates a new <see cref="FixedNextStrategy{T}"/> that always returns the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the constant value.</typeparam>
    /// <param name="value">The constant value to return.</param>
    /// <returns>A new <see cref="FixedNextStrategy{T}"/>.</returns>
    public static FixedNextStrategy<T> From<T>(T value)
    {
        return new FixedNextStrategy<T>(value);
    }
}
