namespace FEFF.Extensions.Testing;

/// <summary>
/// Defines a strategy for generating fake values for a specific type in <see cref="FakeRandom"/>.
/// </summary>
/// <typeparam name="T">The type of value to generate.</typeparam>
public interface IFakeRandomStrategy<T>
{
    /// <summary>
    /// Returns the next fake value.
    /// </summary>
    T Next();
}

/// <summary>
/// A configurable random number generator for testing.
/// <para>
/// The default behavior uses a constant seed. Additional strategies such as
/// constant values can be applied via <see cref="IntStrategy"/>, <see cref="Int64Strategy"/>,
/// <see cref="SingleStrategy"/>, and <see cref="DoubleStrategy"/>.
/// </para>
/// </summary>
/// <remarks>
/// Additional strategies are:  <br/>
/// - Const                     <br/>
/// - AutoIncrement     (TODO)  <br/>
/// - ListRoundRobin    (TODO)  <br/>
/// For types:                  <br/>
/// - int                       <br/>
/// - long                      <br/>
/// - float (single)            <br/>
/// - double                    <br/>
/// - byte[]            (TODO)
/// </remarks>
public class FakeRandom : Random
{
    /// <summary>
    /// Gets or sets the strategy for generating <see cref="int"/> values, or <c>null</c> to use the default.
    /// </summary>
    public IFakeRandomStrategy<int>?    IntStrategy     { get; set; }
    /// <summary>
    /// Gets or sets the strategy for generating <see cref="long"/> values, or <c>null</c> to use the default.
    /// </summary>
    public IFakeRandomStrategy<long>?   Int64Strategy   { get; set; }
    /// <summary>
    /// Gets or sets the strategy for generating <see cref="float"/> values, or <c>null</c> to use the default.
    /// </summary>
    public IFakeRandomStrategy<float>?  SingleStrategy  { get; set; }
    /// <summary>
    /// Gets or sets the strategy for generating <see cref="double"/> values, or <c>null</c> to use the default.
    /// </summary>
    public IFakeRandomStrategy<double>? DoubleStrategy  { get; set; }

    /// <summary>
    /// Creates a new <see cref="FakeRandom"/> with a constant seed of 1.
    /// </summary>
    public FakeRandom() : base(1)
    {
    }

    #region int
    /// <inheritdoc/>
    public override int Next()
    {
        var s = IntStrategy;
        if(s == null)
            return base.Next();

        return s.Next();
    }

    /// <inheritdoc/>
    public override int Next(int maxValue)
    {
        // Assert arguments
        _ = base.Next(maxValue);

        return this.Next() % maxValue;
    }

    /// <inheritdoc/>
    public override int Next(int minValue, int maxValue)
    {
        // Assert arguments
        _ = base.Next(minValue, maxValue);

        if (minValue == maxValue)
            return minValue;

        ThrowHelper.Assert(maxValue > minValue);

        var d = maxValue - minValue;
        var r = minValue + Next(d);
        return r;
    }
    #endregion

    #region long
    /// <inheritdoc/>
    public override long NextInt64()
    {
        var s = Int64Strategy;
        if(s == null)
            return base.NextInt64();

        return s.Next();
    }

    /// <inheritdoc/>
    public override long NextInt64(long maxValue)
    {
        // Assert arguments
        _ = base.NextInt64(maxValue);

        return this.NextInt64() % maxValue;
    }

    /// <inheritdoc/>
    public override long NextInt64(long minValue, long maxValue)
    {
        // Assert arguments
        _ = base.NextInt64(minValue, maxValue);

        if (minValue == maxValue)
            return minValue;

        var d = maxValue - minValue;
        var r = minValue + NextInt64(d);
        return r;
    }
    #endregion

    #region float/double
    /// <inheritdoc/>
    public override float NextSingle()
    {
        var s = SingleStrategy;
        if(s == null)
            return base.NextSingle();

        return s.Next();
    }

    /// <inheritdoc/>
    public override double NextDouble()
    {
        var s = DoubleStrategy;
        if(s == null)
            return base.NextDouble();

        return s.Next();
    }
    #endregion

    // List of methods to override
    // public override void NextBytes(byte[] buffer) => throw new NotSupportedException();
    // public override void NextBytes(Span<byte> buffer) => throw new NotSupportedException();
    // protected override double Sample() => throw new NotSupportedException();

    // public static IRandomStrategy<int>? DefaultIntStrategy => null;
    // public static IRandomStrategy<float>? DefaultSingleStrategy => null;
    // public static IRandomStrategy<double>? DefaultDoubleStrategy => null;
}

/// <summary>
/// A random strategy that always returns the same constant value.
/// </summary>
/// <typeparam name="T">The type of the constant value.</typeparam>
public class ConstRandomStrategy<T> : IFakeRandomStrategy<T>
{
    private readonly T _value;

    /// <summary>
    /// Creates a new <see cref="ConstRandomStrategy{T}"/> that always returns the specified value.
    /// </summary>
    /// <param name="value">The constant value to return.</param>
    public ConstRandomStrategy(T value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public T Next()
    {
        return _value;
    }
}

/// <summary>
/// Factory methods for creating <see cref="ConstRandomStrategy{T}"/> instances.
/// </summary>
public static class ConstRandomStrategy
{
    /// <summary>
    /// Creates a new <see cref="ConstRandomStrategy{T}"/> that always returns the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the constant value.</typeparam>
    /// <param name="value">The constant value to return.</param>
    /// <returns>A new <see cref="ConstRandomStrategy{T}"/>.</returns>
    public static ConstRandomStrategy<T> From<T>(T value)
    {
        return new ConstRandomStrategy<T>(value);
    }
}