namespace FEFF.TestFixtures.AspNetCore.Randomness.Tests;

public class ThrowNormalizationOutOfBoundsTests
{
    protected FakeRandom Rand { get; } = new()
    {
        NormalizationStrategy = new ThrowNormalization(),
    };

    #region int-32

    [Theory]
    [InlineData(-5)]
    [InlineData(int.MaxValue)]
    public void NextI32__should_throw_when_out_of_bounds(int value)
    {
        Rand.Int32Next = FixedNextStrategy.From(value);
        Rand.Invoking(r => r.Next())
            .Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(150)]
    public void NextI32_WithMaxValue__should_throw_when_out_of_bounds(int value)
    {
        Rand.Int32Next = FixedNextStrategy.From(value);
        Rand.Invoking(r => r.Next(100))
            .Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(-200)]
    [InlineData(-101)]
    [InlineData(100)]
    [InlineData(150)]
    public void NextI32_WithMinMax__should_throw_when_out_of_bounds(int value)
    {
        Rand.Int32Next = FixedNextStrategy.From(value);
        Rand.Invoking(r => r.Next(-100, 100))
            .Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region int-64

    [Theory]
    [InlineData(-5L)]
    [InlineData(-1L)]
    [InlineData(long.MaxValue)]
    public void NextI64__should_throw_when_out_of_bounds(long value)
    {
        Rand.Int64Next = FixedNextStrategy.From(value);
        Rand.Invoking(r => r.NextInt64())
            .Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(-100L)]
    [InlineData(-1L)]
    [InlineData(100L)]
    [InlineData(150L)]
    public void NextI64_WithMaxValue__should_throw_when_out_of_bounds(long value)
    {
        Rand.Int64Next = FixedNextStrategy.From(value);
        Rand.Invoking(r => r.NextInt64(100))
            .Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(-200L)]
    [InlineData(-101L)]
    [InlineData(100L)]
    [InlineData(150L)]
    public void NextI64_WithMinMax__should_throw_when_out_of_bounds(long value)
    {
        Rand.Int64Next = FixedNextStrategy.From(value);
        Rand.Invoking(r => r.NextInt64(-100, 100))
            .Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-0.5f)]
    [InlineData(1.0f)]
    [InlineData(2.0f)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(float.NaN)]
    public void NextSingle__should_throw_when_out_of_bounds(float value)
    {
        Rand.SingleNext = FixedNextStrategy.From(value);
        Rand.Invoking(r => r.NextSingle())
            .Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region double

    [Theory]
    [InlineData(-0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.NaN)]
    public void NextDouble__should_throw_when_out_of_bounds(double value)
    {
        Rand.DoubleNext = FixedNextStrategy.From(value);
        Rand.Invoking(r => r.NextDouble())
            .Should().Throw<InvalidOperationException>();
    }

    #endregion
}
