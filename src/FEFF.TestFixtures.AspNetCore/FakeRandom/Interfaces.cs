namespace FEFF.TestFixtures.AspNetCore.Randomness;

/// <summary>
/// Defines a strategy for generating fake values for a specific type in <see cref="FakeRandom"/>.
/// </summary>
/// <typeparam name="T">The type of value to generate.</typeparam>
public interface INextStrategy<T>
{
    /// <summary>
    /// Returns the next fake random value.
    /// </summary>
    /// <returns>A fake random value of type <typeparamref name="T"/>.</returns>
    T Next();
}

/// <summary>
/// Defines how <see cref="FakeRandom"/> should handle generated values that fall
/// outside the valid range of the target <see cref="Random"/> method.
/// </summary>
public interface INormalizationStrategy
{
    /// <summary>
    /// Normalizes an out-of-range <see cref="int"/> value to fit within <paramref name="min"/> (inclusive) and <paramref name="max"/> (exclusive).
    /// </summary>
    /// <param name="next">The raw generated value.</param>
    /// <param name="min">The inclusive lower bound of the method contract.</param>
    /// <param name="max">The exclusive upper bound of the method contract.</param>
    /// <returns>A normalized value that the method should return.</returns>
    int NormalizeI32(int next, int min, int max);

    /// <summary>
    /// Normalizes an out-of-range <see cref="long"/> value to fit within <paramref name="min"/> (inclusive) and <paramref name="max"/> (exclusive).
    /// </summary>
    /// <param name="next">The raw generated value.</param>
    /// <param name="min">The inclusive lower bound of the method contract.</param>
    /// <param name="max">The exclusive upper bound of the method contract.</param>
    /// <returns>A normalized value that the method should return.</returns>
    long NormalizeI64(long next, long min, long max);

    /// <summary>
    /// Normalizes an out-of-range <see cref="float"/> value to fit within the valid range <c>[0.0, 1.0)</c>.
    /// </summary>
    /// <param name="next">The raw generated value.</param>
    /// <returns>A normalized value that <see cref="Random.NextSingle"/> should return.</returns>
    float NormalizeSingle(float next);

    /// <summary>
    /// Normalizes an out-of-range <see cref="double"/> value to fit within the valid range <c>[0.0, 1.0)</c>.
    /// </summary>
    /// <param name="next">The raw generated value.</param>
    /// <returns>A normalized value that <see cref="Random.NextDouble"/> should return.</returns>
    double NormalizeDouble(double next);
}
