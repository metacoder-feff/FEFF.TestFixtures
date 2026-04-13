namespace FEFF.TestFixtures.AspNetCore.Randomness;

/// <summary>
/// A configurable random number generator for testing.
/// <para>
/// The default behavior uses a constant seed. Additional strategies such as
/// fixed values can be applied via <see cref="Int32Next"/>, <see cref="Int64Next"/>,
/// <see cref="SingleNext"/>, and <see cref="DoubleNext"/>.
/// </para>
/// </summary>
/// <remarks>
/// Additional 'Next' strategies are:  <br/>
/// - Fixed                     <br/>
/// - AutoIncrement     (TODO)  <br/>
/// - ListRoundRobin    (TODO)  <br/>
/// For types:                  <br/>
/// - int                       <br/>
/// - long                      <br/>
/// - float (single)            <br/>
/// - double                    <br/>
/// - byte[]
/// When 'Next' strategy returns value that is out of bounds of a defined method contract (e.g. Next(min,max))
/// 'NormalizationStrategy' can fix this.
/// The default strategy 'NormalizationStrategy' throws InvalidOperationException.<br/>
/// Available 'NormalizationStrategy' implementations:<br/>
/// - <see cref="ThrowNormalization"/> — throws on out-of-range values<br/>
/// - <see cref="ReturnAsIsNormalization"/> — returns the value unchanged<br/>
/// - AutoFitNormalization (TODO)
/// </remarks>
public class FakeRandom : Random
{
    /// <summary>
    /// Gets or sets the strategy for generating <see cref="int"/> values, or <c>null</c> to use the default.
    /// </summary>
    public INextStrategy<int>?    Int32Next     { get; set; }
    /// <summary>
    /// Gets or sets the strategy for generating <see cref="long"/> values, or <c>null</c> to use the default.
    /// </summary>
    public INextStrategy<long>?   Int64Next   { get; set; }
    /// <summary>
    /// Gets or sets the strategy for generating <see cref="float"/> values, or <c>null</c> to use the default.
    /// </summary>
    public INextStrategy<float>?  SingleNext  { get; set; }
    /// <summary>
    /// Gets or sets the strategy for generating <see cref="double"/> values, or <c>null</c> to use the default.
    /// </summary>
    public INextStrategy<double>? DoubleNext  { get; set; }
    /// <summary>
    /// Gets or sets the strategy for generating <see cref="byte"/> values, or <c>null</c> to use the default.
    /// </summary>
    public INextStrategy<byte>? ByteNext  { get; set; }

    private INormalizationStrategy _normalizationStrategy = new ThrowNormalization();

    /// <summary>
    /// Gets or sets the normalization strategy used when a generated value falls outside the valid
    /// range of the target <see cref="Random"/> method.
    /// </summary>
    /// <remarks>
    /// The default is <see cref="ThrowNormalization"/>, which throws <see cref="InvalidOperationException"/>
    /// on out-of-range values.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when setting the property to <c>null</c>.</exception>
    public INormalizationStrategy NormalizationStrategy
    {
        get => _normalizationStrategy;
        set => _normalizationStrategy = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Creates a new <see cref="FakeRandom"/> with a constant seed of 1.
    /// </summary>
    public FakeRandom() : base(1)
    {
    }

    #region int

    // int32Next is not null here
    private int NextI32Internal(INextStrategy<int> int32Next, int min, int max)
    {
        if(min == max)
            return min;

        var next = int32Next.Next();

        if(next >= min && next < max)
            return next;

        return NormalizationStrategy.NormalizeI32(next, min, max);
    }

    /// <inheritdoc/>
    public override int Next()
    {
        // just do as other "Assert arguments" pattern
        var def = base.Next();

        // avoid race condition
        var strategy = Int32Next;

        if (strategy == null)
            return def;

        // int32Next is not null here
        return NextI32Internal(strategy, 0, int.MaxValue);
    }

    /// <inheritdoc/>
    public override int Next(int maxValue)
    {
        // Assert arguments
        var def = base.Next(maxValue);

        var strategy = Int32Next;
        if (strategy == null)
            return def;

        return NextI32Internal(strategy, 0, maxValue);
    }

    /// <inheritdoc/>
    public override int Next(int minValue, int maxValue)
    {
        // Assert arguments
        var def = base.Next(minValue, maxValue);
        var strategy = Int32Next;
        if (strategy == null)
            return def;

        return NextI32Internal(strategy, minValue, maxValue);
    }
    #endregion

    #region long

    // int64Next is not null here
    private long NextI64Internal(INextStrategy<long> int64Next, long min, long max)
    {
        if(min == max)
            return min;

        var next = int64Next.Next();

        if(next >= min && next < max)
            return next;

        return NormalizationStrategy.NormalizeI64(next, min, max);
    }

    /// <inheritdoc/>
    public override long NextInt64()
    {
        var def = base.NextInt64();
        var strategy = Int64Next;
        if (strategy == null)
            return def;

        return NextI64Internal(strategy, 0, long.MaxValue);
    }

    /// <inheritdoc/>
    public override long NextInt64(long maxValue)
    {
        // Assert arguments
        var def = base.NextInt64(maxValue);
        var strategy = Int64Next;
        if (strategy == null)
            return def;

        return NextI64Internal(strategy, 0, maxValue);
    }

    /// <inheritdoc/>
    public override long NextInt64(long minValue, long maxValue)
    {
        // Assert arguments
        var def = base.NextInt64(minValue, maxValue);
        var strategy = Int64Next;
        if (strategy == null)
            return def;

        return NextI64Internal(strategy, minValue, maxValue);
    }
    #endregion

    #region float/double

    // singleNext is not null here
    private float NextSingleInternal(INextStrategy<float> singleNext)
    {
        var next = singleNext.Next();

        if(next >= 0f && next < 1f)
            return next;

        return NormalizationStrategy.NormalizeSingle(next);
    }

    /// <inheritdoc/>
    public override float NextSingle()
    {
        var def = base.NextSingle();
        var strategy = SingleNext;
        if (strategy == null)
            return def;

        return NextSingleInternal(strategy);
    }

    // doubleNext is not null here
    private double NextDoubleInternal(INextStrategy<double> doubleNext)
    {
        var next = doubleNext.Next();

        if(next >= 0.0 && next < 1.0)
            return next;

        return NormalizationStrategy.NormalizeDouble(next);
    }

    /// <inheritdoc/>
    public override double NextDouble()
    {
        var def = base.NextDouble();
        var strategy = DoubleNext;
        if (strategy == null)
            return def;

        return NextDoubleInternal(strategy);
    }
    #endregion

    #region  byte[]

    /// <inheritdoc/>
    public override void NextBytes(byte[] buffer)
    {
        this.NextBytes((Span<byte>) buffer);
    }

    /// <inheritdoc/>
    public override void NextBytes(Span<byte> buffer)
    {
        // Assert arguments
        base.NextBytes(buffer);

        var strategy = ByteNext;
        if(strategy != null)
        {
            for(int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = strategy.Next();
            }
        }
    }
    #endregion

// TODO: methods to override ??
    // protected override double Sample() => throw new NotSupportedException();

    // public static IRandomStrategy<int>? DefaultIntStrategy => null;
    // public static IRandomStrategy<float>? DefaultSingleStrategy => null;
    // public static IRandomStrategy<double>? DefaultDoubleStrategy => null;
}
