using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

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

    // fast set by index by counter
    private class ConcurrentList<T>(int capacity)
    {
        private readonly T[] _list = new T[capacity];
        private volatile int _nextIdx = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            var current = Interlocked.Increment(ref _nextIdx);

            if(current >= capacity)
               throw new InvalidOperationException("List is full");

            _list[current] = item;
        }

        internal List<T> ToList()
        {
            var n = _nextIdx;
            if(n < 0)
                return [];
            return _list.Take(n + 1).ToList();
        }       
    }

    private static RaceStrategy<T> CreateRaceStrategyFrom<T>(T a, T b) => new(a,b);

    private const int ThreadCount = 2;
    private readonly Barrier _barrier = new(ThreadCount);

    protected FakeRandom Rand { get; } = new();

    #region int-32
    
    [Fact]
    public void Next__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int32Next = CreateRaceStrategyFrom(1, 2);
        var results = new ConcurrentList<int>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            _barrier.SignalAndWait();
            var value = Rand.Next();
            results.Add(value);
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
        var results = new ConcurrentList<int>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            _barrier.SignalAndWait();
            var value = Rand.Next(100);
            results.Add(value);
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
        var results = new ConcurrentList<int>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            _barrier.SignalAndWait();
            var value = Rand.Next(50, 300);
            results.Add(value);
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
        var results = new ConcurrentList<long>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            _barrier.SignalAndWait();
            var value = Rand.NextInt64();
            results.Add(value);
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
        var results = new ConcurrentList<long>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            _barrier.SignalAndWait();
            var value = Rand.NextInt64(100L);
            results.Add(value);
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
        var results = new ConcurrentList<long>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            _barrier.SignalAndWait();
            var value = Rand.NextInt64(50L, 300L);
            results.Add(value);
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
        var results = new ConcurrentList<float>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            _barrier.SignalAndWait();
            var value = Rand.NextSingle();
            results.Add(value);
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
        var results = new ConcurrentList<double>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            _barrier.SignalAndWait();
            var value = Rand.NextDouble();
            results.Add(value);
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
        var results = new ConcurrentList<byte>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            var buffer = new byte[1];
            _barrier.SignalAndWait();
            Rand.NextBytes(buffer);
            results.Add(buffer[0]);
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
        var results = new ConcurrentList<byte>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            Span<byte> buffer = new byte[1];
            _barrier.SignalAndWait();
            Rand.NextBytes(buffer);
            results.Add(buffer[0]);
        });
        results.ToList()
            .Should().BeEquivalentTo(
            [ (byte)1, (byte)2 ],
            options => options.WithStrictOrdering()
        );
    }
    #endregion
}