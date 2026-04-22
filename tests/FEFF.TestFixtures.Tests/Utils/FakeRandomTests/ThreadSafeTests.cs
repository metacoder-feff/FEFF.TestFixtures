using System.Collections.Concurrent;

namespace FEFF.TestFixtures.AspNetCore.Randomness.Tests;

public class ThreadSafeTests
{
    private class RaceStrategy<T>(T a, T b) : INextStrategy<T>
    {
        private volatile int _counter = 0;
        public T Next()
        {
            var current = Interlocked.Increment(ref _counter);
            if(current == 1)
            {
                Thread.Sleep(1000);
                return a;
            }
            return b;
        }
    }

    private static RaceStrategy<T> CreateRaceStrategyFrom<T>(T a, T b) => new(a,b);

    private const int ThreadCount = 2;

    protected FakeRandom Rand { get; } = new();

    #region int-32
    
    [Fact]
    public void Next__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int32Next = CreateRaceStrategyFrom(1, 2);
        var results = new ConcurrentQueue<int>();

        Parallel.For(0, ThreadCount, _ =>
        {
            var value = Rand.Next();
            results.Enqueue(value);
        });

        results.ToList()
            .Should().BeEquivalentTo(
            [ 1, 2 ],
            options => options.WithStrictOrdering()
        );

        // without lock it would be: [2, 1]
        // because '1' has a delay
    }

    [Fact]
    public void Next_max__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int32Next = CreateRaceStrategyFrom(10, 20);
        var results = new ConcurrentQueue<int>();

        Parallel.For(0, ThreadCount, _ =>
        {
            var value = Rand.Next(100);
            results.Enqueue(value);
        });

        results.ToList()
            .Should().BeEquivalentTo(
            [ 10, 20 ],
            options => options.WithStrictOrdering()
        );
    }

    [Fact]
    public void Next_min_max__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int32Next = CreateRaceStrategyFrom(100, 200);
        var results = new ConcurrentQueue<int>();

        Parallel.For(0, ThreadCount, _ =>
        {
            var value = Rand.Next(50, 300);
            results.Enqueue(value);
        });

        results.ToList()
            .Should().BeEquivalentTo(
            [ 100, 200 ],
            options => options.WithStrictOrdering()
        );
    }
    #endregion

    #region int-64

    [Fact]
    public void Next64__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int64Next = CreateRaceStrategyFrom(1L, 2L);
        var results = new ConcurrentQueue<long>();

        Parallel.For(0, ThreadCount, _ =>
        {
            var value = Rand.NextInt64();
            results.Enqueue(value);
        });

        results.ToList()
            .Should().BeEquivalentTo(
            [ 1L, 2L ],
            options => options.WithStrictOrdering()
        );
    }

    [Fact]
    public void Next64_max__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int64Next = CreateRaceStrategyFrom(10L, 20L);
        var results = new ConcurrentQueue<long>();

        Parallel.For(0, ThreadCount, _ =>
        {
            var value = Rand.NextInt64(100L);
            results.Enqueue(value);
        });

        results.ToList()
            .Should().BeEquivalentTo(
            [ 10L, 20L ],
            options => options.WithStrictOrdering()
        );
    }

    [Fact]
    public void Next64_min_max__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int64Next = CreateRaceStrategyFrom(100L, 200L);
        var results = new ConcurrentQueue<long>();

        Parallel.For(0, ThreadCount, _ =>
        {
            var value = Rand.NextInt64(50L, 300L);
            results.Enqueue(value);
        });

        results.ToList()
            .Should().BeEquivalentTo(
            [ 100L, 200L ],
            options => options.WithStrictOrdering()
        );
    }
    #endregion

    #region float/double

    [Fact]
    public void NextSingle__when_multi_threaded__should_not_mix_values()
    {
        Rand.SingleNext = CreateRaceStrategyFrom(0.1f, 0.2f);
        var results = new ConcurrentQueue<float>();

        Parallel.For(0, ThreadCount, _ =>
        {
            var value = Rand.NextSingle();
            results.Enqueue(value);
        });

        results.ToList()
            .Should().BeEquivalentTo(
            [ 0.1f, 0.2f ],
            options => options.WithStrictOrdering()
        );
    }

    [Fact]
    public void NextDouble__when_multi_threaded__should_not_mix_values()
    {
        Rand.DoubleNext = CreateRaceStrategyFrom(0.1, 0.2);
        var results = new ConcurrentQueue<double>();

        Parallel.For(0, ThreadCount, _ =>
        {
            var value = Rand.NextDouble();
            results.Enqueue(value);
        });

        results.ToList()
            .Should().BeEquivalentTo(
            [ 0.1, 0.2 ],
            options => options.WithStrictOrdering()
        );
    }
    #endregion

    #region byte[]

    [Fact]
    public void NextBytes__when_multi_threaded__should_not_mix_values()
    {
        Rand.ByteNext = CreateRaceStrategyFrom((byte)1, (byte)2);
        var results = new ConcurrentQueue<byte>();

        Parallel.For(0, ThreadCount, _ =>
        {
            var buffer = new byte[1];
            Rand.NextBytes(buffer);
            results.Enqueue(buffer[0]);
        });
        results.ToList()
            .Should().BeEquivalentTo(
            [ (byte)1, (byte)2 ],
            options => options.WithStrictOrdering()
        );
    }

    [Fact]
    public void NextBytes_span__when_multi_threaded__should_not_mix_values()
    {
        Rand.ByteNext = CreateRaceStrategyFrom((byte)1, (byte)2);
        var results = new ConcurrentQueue<byte>();

        Parallel.For(0, ThreadCount, _ =>
        {
            Span<byte> buffer = new byte[1];
            Rand.NextBytes(buffer);
            results.Enqueue(buffer[0]);
        });
        results.ToList()
            .Should().BeEquivalentTo(
            [ (byte)1, (byte)2 ],
            options => options.WithStrictOrdering()
        );
    }
    #endregion
}