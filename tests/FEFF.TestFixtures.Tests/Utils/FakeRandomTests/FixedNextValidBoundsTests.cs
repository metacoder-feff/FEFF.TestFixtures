namespace FEFF.TestFixtures.AspNetCore.Randomness.Tests;

public class FixedNextValidBoundsTests
{
    protected FakeRandom Rand { get; } = new();

    #region int-32

    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    public void NextI32__should_return_fixed(int value)
    {
        Rand.Int32Next = FixedNextStrategy.From(value);
        Rand.Next()
            .Should().Be(value);
        Rand.Next()
            .Should().Be(value);
    }

    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    public void NextI32_WithMaxValue__should_return_fixed(int value)
    {
        Rand.Int32Next = FixedNextStrategy.From(value);
        Rand.Next(100)
            .Should().Be(value);
    }

    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    public void NextI32_WithMinMax__should_return_fixed(int value)
    {
        Rand.Int32Next = FixedNextStrategy.From(value);
        Rand.Next(-100, 100)
            .Should().Be(value);
    }

    #endregion

    #region int-64

    [Theory]
    [InlineData(11L)]
    [InlineData(15L)]
    public void NextI64__should_return_fixed(long value)
    {
        Rand.Int64Next = FixedNextStrategy.From(value);
        Rand.NextInt64()
            .Should().Be(value);
        Rand.NextInt64()
            .Should().Be(value);
    }

    [Theory]
    [InlineData(5L)]
    [InlineData(10L)]
    public void NextI64_WithMaxValue__should_return_fixed(long value)
    {
        Rand.Int64Next = FixedNextStrategy.From(value);
        Rand.NextInt64(100)
            .Should().Be(value);
    }

    [Theory]
    [InlineData(5L)]
    [InlineData(15L)]
    public void NextI64_WithMinMax__should_return_fixed(long value)
    {
        Rand.Int64Next = FixedNextStrategy.From(value);
        Rand.NextInt64(-20, 20)
            .Should().Be(value);
    }

    #endregion

    #region float

    [Theory]
    [InlineData(0.5f)]
    [InlineData(0.75f)]
    public void NextSingle__should_return_fixed(float value)
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
    [InlineData(0.5)]
    [InlineData(0.75)]
    public void NextDouble__should_return_fixed(double value)
    {
        Rand.DoubleNext = FixedNextStrategy.From(value);
        Rand.NextDouble()
            .Should().Be(value);
        Rand.NextDouble()
            .Should().Be(value);
    }

    #endregion

    #region byte[]

    [Theory]
    [InlineData(42)]
    [InlineData(255)]
    public void NextBytes_Array__should_fill_with_fixed(byte value)
    {
        Rand.ByteNext = FixedNextStrategy.From(value);
        var buffer = new byte[5];
        Rand.NextBytes(buffer);
        buffer.Should().AllBeEquivalentTo(value);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(255)]
    public void NextBytes_Span__should_fill_with_fixed(byte value)
    {
        Rand.ByteNext = FixedNextStrategy.From(value);
        var buffer = new byte[5];
        var span = new Span<byte>(buffer);
        Rand.NextBytes(span);
        buffer.Should().AllBeEquivalentTo(value);
    }

    #endregion
}
