namespace FEFF.Extensions.Testing;

public interface IFakeRandomStrategy<T>
{
    T Next();
}
/// <summary>
/// The default strategy is "Constant seed"
/// Additional strategies are:
/// - Const
/// - AutoIncrement     (TODO)
/// - ListRoundRobin    (TODO)
/// For types:
/// - int
/// - long
/// - float (single)
/// - double
/// - byte[]            (TODO)
/// </summary>
public class FakeRandom : Random
{
    public IFakeRandomStrategy<int>?    IntStrategy     { get; set; }
    public IFakeRandomStrategy<long>?   Int64Strategy   { get; set; }
    public IFakeRandomStrategy<float>?  SingleStrategy  { get; set; }
    public IFakeRandomStrategy<double>? DoubleStrategy  { get; set; }

    public FakeRandom() : base(1)
    {
    }

    #region int
    public override int Next()
    {
        var s = IntStrategy;
        if(s == null)
            return base.Next();

        return s.Next();
    }

    public override int Next(int maxValue)
    {
        // Assert arguments
        _ = base.Next(maxValue);

        return this.Next() % maxValue;
    }

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
    public override long NextInt64()
    {
        var s = Int64Strategy;
        if(s == null)
            return base.NextInt64();

        return s.Next();
    }

    public override long NextInt64(long maxValue)
    {
        // Assert arguments
        _ = base.NextInt64(maxValue);

        return this.NextInt64() % maxValue;
    }

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
    public override float NextSingle()
    {
        var s = SingleStrategy;
        if(s == null)
            return base.NextSingle();

        return s.Next();
    }

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

public class ConstRandomStrategy<T> : IFakeRandomStrategy<T>
{
    private readonly T _value;

    public ConstRandomStrategy(T value)
    {
        _value = value;
    }

    public T Next()
    {
        return _value;
    }
}

public static class ConstRandomStrategy
{
    public static ConstRandomStrategy<T> From<T>(T value)
    {
        return new ConstRandomStrategy<T>(value);
    }
}