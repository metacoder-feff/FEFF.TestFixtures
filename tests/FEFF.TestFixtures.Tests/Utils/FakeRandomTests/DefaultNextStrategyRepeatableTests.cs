namespace FEFF.TestFixtures.AspNetCore.Randomness.Tests;

public class DefaultNextStrategyRepeatableTests
{
    protected FakeRandom Rand { get; } = new();

    #region int-32

    [Fact]
    public void Next__should_be_repeatable()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select(_ => Rand.Next())
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select(_ => rand2.Next())
            .ToList();

        r1.Should().BeEquivalentTo(r2);
    }

    [Fact]
    public void Next_with_maxValue__should_be_repeatable()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select(_ => Rand.Next(100))
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select(_ => rand2.Next(100))
            .ToList();

        r1.Should().BeEquivalentTo(r2);
    }

    [Fact]
    public void Next_with_min_and_maxValue__should_be_repeatable()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select(_ => Rand.Next(10, 100))
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select(_ => rand2.Next(10, 100))
            .ToList();

        r1.Should().BeEquivalentTo(r2);
    }
    #endregion

    #region int-64

    [Fact]
    public void NextInt64__should_be_repeatable()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select(_ => Rand.NextInt64())
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select(_ => rand2.NextInt64())
            .ToList();

        r1.Should().BeEquivalentTo(r2);
    }

    [Fact]
    public void NextInt64_with_maxValue__should_be_repeatable()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select(_ => Rand.NextInt64(100))
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select(_ => rand2.NextInt64(100))
            .ToList();

        r1.Should().BeEquivalentTo(r2);
    }

    [Fact]
    public void NextInt64_with_min_and_maxValue__should_be_repeatable()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select(_ => Rand.NextInt64(10, 100))
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select(_ => rand2.NextInt64(10, 100))
            .ToList();

        r1.Should().BeEquivalentTo(r2);
    }
    #endregion

    #region float/double

    [Fact]
    public void NextSingle__should_be_repeatable()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select(_ => Rand.NextSingle())
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select(_ => rand2.NextSingle())
            .ToList();

        r1.Should().BeEquivalentTo(r2);
    }

    [Fact]
    public void NextDouble__should_be_repeatable()
    {
        var r1 = Enumerable.Range(1, 10)
            .Select(_ => Rand.NextDouble())
            .ToList();

        var rand2 = new FakeRandom();
        var r2 = Enumerable.Range(1, 10)
            .Select(_ => rand2.NextDouble())
            .ToList();

        r1.Should().BeEquivalentTo(r2);
    }
    #endregion

    #region byte[]

    [Fact]
    public void NextBytes_array__should_be_repeatable()
    {
        var r1 = new byte[16];
        Rand.NextBytes(r1);

        var rand2 = new FakeRandom();
        var r2 = new byte[16];
        rand2.NextBytes(r2);

        r1.Should().BeEquivalentTo(r2);
    }

    [Fact]
    public void NextBytes_span__should_be_repeatable()
    {
        var r1 = new byte[16];
        Rand.NextBytes((Span<byte>)r1);

        var rand2 = new FakeRandom();
        var r2 = new byte[16];
        rand2.NextBytes((Span<byte>)r2);

        r1.Should().BeEquivalentTo(r2);
    }
    #endregion
}
