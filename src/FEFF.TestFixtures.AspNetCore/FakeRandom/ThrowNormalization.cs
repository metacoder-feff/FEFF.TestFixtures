namespace FEFF.TestFixtures.AspNetCore.Randomness;

/// <summary>
/// A normalization strategy that throws <see cref="InvalidOperationException"/> when a generated
/// value falls outside the valid range for the target method.
/// </summary>
/// <remarks>
/// Use this strategy when you want FakeRandom to fail loudly if a configured
/// <see cref="FixedNextStrategy{T}"/> produces a value that violates the method contract
/// (for example, a value greater than or equal to <c>maxValue</c> in <see cref="Random.Next(int)"/>).
/// </remarks>
public class ThrowNormalization : INormalizationStrategy
{
    /// <inheritdoc/>
    public int NormalizeI32(int next, int min, int max) =>
        throw new InvalidOperationException(CreateMsg(next, min, max));

    /// <inheritdoc/>
    public long NormalizeI64(long next, long min, long max) =>
        throw new InvalidOperationException(CreateMsg(next, min, max));

    /// <inheritdoc/>
    public float NormalizeSingle(float next) =>
        throw new InvalidOperationException(CreateMsg(next));

    /// <inheritdoc/>
    public double NormalizeDouble(double next) =>
        throw new InvalidOperationException(CreateMsg(next));

    private static string CreateMsg<T>(T next, T min, T max) =>
        $"Next fake random value '{next}' is out of range: '[{min}, {max})'. Consider using another 'Next fake random value' or 'NormalizationStrategy'.";

    private static string CreateMsg<T>(T next) =>
        $"Next fake random value '{next}' is out of range: '[0.0, 1.0)'. Consider using another 'Next fake random value' or 'NormalizationStrategy'.";
}
