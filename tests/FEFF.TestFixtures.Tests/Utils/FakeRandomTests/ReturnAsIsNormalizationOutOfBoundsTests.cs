namespace FEFF.TestFixtures.AspNetCore.Randomness.Tests;

public class ReturnAsIsNormalizationOutOfBoundsTests
{
    protected FakeRandom Rand { get; } = new()
    {
        NormalizationStrategy = new ReturnAsIsNormalization(),  
    };

    #region int-32

    [Theory]
    [InlineData(-5)]
    [InlineData(int.MaxValue)]
    public void NextI32__should_return_without_normalization(int value)
    {
        Rand.Int32Next = FixedNextStrategy.From(value);
        Rand.Next()
            .Should().Be(value);
        Rand.Next()
            .Should().Be(value);
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(150)]
    public void NextI32_WithMaxValue__should_return_without_normalization(int value)
    {
        Rand.Int32Next = FixedNextStrategy.From(value);
        Rand.Next(100)
            .Should().Be(value);
    }

    [Theory]
    [InlineData(-200)]
    [InlineData(-101)]
    [InlineData(100)]
    [InlineData(150)]
    public void NextI32_WithMinMax__should_return_without_normalization(int value)
    {
        Rand.Int32Next = FixedNextStrategy.From(value);
        Rand.Next(-100, 100)
            .Should().Be(value);
    }

    #endregion

    #region int-64

    [Theory]
    [InlineData(-5L)]
    [InlineData(-1L)]
    [InlineData(long.MaxValue)]
    public void NextI64__should_return_without_normalization(long value)
    {
        Rand.Int64Next = FixedNextStrategy.From(value);
        Rand.NextInt64()
            .Should().Be(value);
        Rand.NextInt64()
            .Should().Be(value);
    }

    [Theory]
    [InlineData(-100L)]
    [InlineData(-1L)]
    [InlineData(100L)]
    [InlineData(150L)]
    public void NextI64_WithMaxValue__should_return_without_normalization(long value)
    {
        Rand.Int64Next = FixedNextStrategy.From(value);
        Rand.NextInt64(100)
            .Should().Be(value);
    }

    [Theory]
    [InlineData(-200L)]
    [InlineData(-101L)]
    [InlineData(100L)]
    [InlineData(150L)]
    public void NextI64_WithMinMax__should_return_without_normalization(long value)
    {
        Rand.Int64Next = FixedNextStrategy.From(value);
        Rand.NextInt64(-100, 100)
            .Should().Be(value);
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
    public void NextSingle__should_return_without_normalization(float value)
    {
        Rand.SingleNext = FixedNextStrategy.From(value);
        Rand.NextSingle()
            .Should().Be(value);
        Rand.NextSingle()
            .Should().Be(value);
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
    public void NextDouble__should_return_without_normalization(double value)
    {
        Rand.DoubleNext = FixedNextStrategy.From(value);
        Rand.NextDouble()
            .Should().Be(value);
        Rand.NextDouble()
            .Should().Be(value);
    }

    #endregion
}
