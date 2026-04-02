
namespace FEFF.Extentions.Testing.Tests;

public class FakeRandomTests
{
    private FakeRandom Rand { get; } = new();

    #region int

    [Fact]
    public void Int_Default_returns_same()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select( _ => Rand.Next())
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select( _ => rand2.Next())
            .ToList();
        
        r1.Should().BeEquivalentTo(r2);
    }

    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    public void Int_Const(int value)
    {
        Rand.IntStrategy = FakeRandom.ConstStrategy(value);
        Rand.Next()
            .Should().Be(value);
        Rand.Next()
            .Should().Be(value);
    }

    [Theory]
    [InlineData(11 , 11)]
    [InlineData(15 , 15)]
    [InlineData(150, 50)]
    public void Int_Const_max(int value, int res)
    {
        Rand.IntStrategy = FakeRandom.ConstStrategy(value);
        Rand.Next(100)
            .Should().Be(res);
        Rand.Next(100)
            .Should().Be(res);
    }

    [Theory]
    [InlineData(11 , 21)]
    [InlineData(15 , 25)]
    [InlineData(5  , 15)]
    [InlineData(150, 70)]
    public void Int_Const_min_max(int value, int res)
    {
        Rand.IntStrategy = FakeRandom.ConstStrategy(value);
        Rand.Next(10, 100)
            .Should().Be(res);
        Rand.Next(10, 100)
            .Should().Be(res);
    }

    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    public void Int_Const_min_eq_max(int value)
    {
        Rand.IntStrategy = FakeRandom.ConstStrategy(value);
        Rand.Next(value, value)
            .Should().Be(value);
    }
    #endregion

    #region int64

    [Fact]
    public void Int64_Default_returns_same()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select( _ => Rand.NextInt64())
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select( _ => rand2.NextInt64())
            .ToList();
        
        r1.Should().BeEquivalentTo(r2);
    }

    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    public void Int64_Const(long value)
    {
        Rand.Int64Strategy = FakeRandom.ConstStrategy(value);
        Rand.NextInt64()
            .Should().Be(value);
        Rand.NextInt64()
            .Should().Be(value);
    }

    [Theory]
    [InlineData(11 , 11)]
    [InlineData(15 , 15)]
    [InlineData(150, 50)]
    public void Int64_Const_max(long value, long res)
    {
        Rand.Int64Strategy = FakeRandom.ConstStrategy(value);
        Rand.NextInt64(100)
            .Should().Be(res);
        Rand.NextInt64(100)
            .Should().Be(res);
    }

    [Theory]
    [InlineData(11 , 21)]
    [InlineData(15 , 25)]
    [InlineData(5  , 15)]
    [InlineData(150, 70)]
    public void Int64_Const_min_max(long value, long res)
    {
        Rand.Int64Strategy = FakeRandom.ConstStrategy(value);
        Rand.NextInt64(10, 100)
            .Should().Be(res);
        Rand.NextInt64(10, 100)
            .Should().Be(res);
    }

    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    public void Int64_Const_min_eq_max(long value)
    {
        Rand.Int64Strategy = FakeRandom.ConstStrategy(value);
        Rand.NextInt64(value, value)
            .Should().Be(value);
    }
    #endregion

    #region Single

    [Fact]
    public void Single_Default_returns_same()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select( _ => Rand.NextSingle())
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select( _ => rand2.NextSingle())
            .ToList();
        
        r1.Should().BeEquivalentTo(r2);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(0.6)]
    public void Single_Const(float value)
    {
        Rand.SingleStrategy = FakeRandom.ConstStrategy(value);
        Rand.NextSingle()
            .Should().Be(value);
        Rand.NextSingle()
            .Should().Be(value);
    }
    #endregion

    #region Double

    [Fact]
    public void Double_Default_returns_same()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select( _ => Rand.NextDouble())
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select( _ => rand2.NextDouble())
            .ToList();
        
        r1.Should().BeEquivalentTo(r2);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(0.6)]
    public void Double_Const(double value)
    {
        Rand.DoubleStrategy = FakeRandom.ConstStrategy(value);
        Rand.NextDouble()
            .Should().Be(value);
        Rand.NextDouble()
            .Should().Be(value);
    }
    #endregion
}