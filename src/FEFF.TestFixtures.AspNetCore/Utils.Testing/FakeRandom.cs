namespace FEFF.Extentions.Testing;

public interface IRandomStrategy<T>
{
    T Next();
}
/// <summary>
/// The default strategy is "Constant seed"
/// Additional strategies are:
/// - ConstNext
/// - AutoIncrementNext (TODO)
/// - ListRoundRobin    (TODO)
/// For types:
/// - int
/// - long      (not tested)
/// - float     (not tested)
/// - double    (not tested)
/// - byte[]    (TODO)
/// </summary>
public class FakeRandom : Random
{
    public IRandomStrategy<int>?    IntStrategy     { get; set; }
    public IRandomStrategy<float>?  SingleStrategy  { get; set; }
    public IRandomStrategy<double>? DoubleStrategy  { get; set; }

    public FakeRandom() : base(1)
    {
    }

    #region  int
    public override int Next()
    {
        var s = IntStrategy;
        if(s == null)
            return base.Next();

        return s.Next();
    }

    public override int Next(int maxValue)
    {
        //assert params
        _ = base.Next(maxValue);

        return this.Next() % maxValue;
    }

    public override int Next(int minValue, int maxValue)
    {
        //assert params
        _ = base.Next(minValue, maxValue);

        if (minValue == maxValue)
            return minValue;

        var d = maxValue - minValue;
        var r = minValue + Next(d);
        return r;
    }
    #endregion

    #region  long
    public override long NextInt64()
    {
        var s = IntStrategy;
        if(s == null)
            return base.NextInt64();

        return s.Next();
    }

    public override long NextInt64(long maxValue)
    {
        //assert params
        _ = base.NextInt64(maxValue);

        return this.NextInt64() % maxValue;
    }

    public override long NextInt64(long minValue, long maxValue)
    {
        //assert params
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
    // public static IRandomStrategy<float>? DefaultSingletrategy => null;
    // public static IRandomStrategy<double>? DefaultDoubleStrategy => null;

    public static ConstRandomStrategy<T> ConstStrategy<T>(T value)
    {
        return new ConstRandomStrategy<T>(value);
    }
}

public class ConstRandomStrategy<T> : IRandomStrategy<T>
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