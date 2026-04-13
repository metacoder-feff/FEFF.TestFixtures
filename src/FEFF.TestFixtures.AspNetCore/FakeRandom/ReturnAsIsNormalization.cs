namespace FEFF.TestFixtures.AspNetCore.Randomness;

/// <summary>
/// A normalization strategy that returns the generated value unchanged,
/// regardless of whether it falls inside or outside the valid range for the target method.
/// </summary>
/// <remarks>
/// Use this strategy when you want FakeRandom to produce exact preconfigured values
/// without any clamping, mapping, or validation — even if those values would be
/// considered invalid under the normal <see cref="Random"/> contract.
/// </remarks>
public class ReturnAsIsNormalization : INormalizationStrategy
{
    /// <inheritdoc/>
    public int NormalizeI32(int next, int min, int max) => next;

    /// <inheritdoc/>
    public long NormalizeI64(long next, long min, long max) => next;

    /// <inheritdoc/>
    public float NormalizeSingle(float next) => next;

    /// <inheritdoc/>
    public double NormalizeDouble(double next) => next;
}
