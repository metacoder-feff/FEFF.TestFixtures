using System.Runtime.CompilerServices;

namespace FEFF.TestFixtures.AspNetCore.Randomness.Tests;

public class ThreadSafeTests
{
    // Delays are : (1000, 500)
    // 1: 1000 - if lock is NOT implemented:
    //      creates a race condition in Next()
    // 2: 500  - if lock is implemented 
    //      we need to add a little delay after first lock is released
    //      to avoid another race condition during test 
    //      when adding result to list
    private class RaceStrategy<T>(T a, T b) : INextStrategy<T>
    {
        private volatile int _counter = 0;
        public T Next()
        {
            var current = Interlocked.Increment(ref _counter);
            if(current == 1)
            {
                Thread.Sleep(1000); // ensure order: (b,a) if NOT locked
                return a;
            }

            Thread.Sleep(500); // ensure order: (a,b) if locked
            return b;
        }
    }

    // Add: fast set by index by counter
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

    private const int ThreadCount = 2;
    private readonly Barrier _barrier = new(ThreadCount);
    protected FakeRandom Rand { get; } = new();

    #region helper methods

    private static RaceStrategy<T> CreateRaceStrategyFrom<T>(T a, T b) => new(a,b);

    private List<T> RunParallel<T>(Func<T> func)
    {
        var results = new ConcurrentList<T>(ThreadCount);

        Parallel.For(0, ThreadCount, _ =>
        {
            _barrier.SignalAndWait();
            var value = func();
            results.Add(value);
        });

        return results.ToList();
    }
    #endregion

    #region int-32
    
    [Fact]
    public void Next__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int32Next = CreateRaceStrategyFrom(1, 2);

        var results = RunParallel(() => 
            Rand.Next()
        );

        results
            .Should().BeEquivalentTo(
            [ 1, 2 ],
            options => options.WithStrictOrdering()
        );

        // without lock it would be: [2, 1]
        // because '1' has bigger delay
    }

    [Fact]
    public void Next_max__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int32Next = CreateRaceStrategyFrom(10, 20);

        var results = RunParallel(() => 
            Rand.Next(100)
        );

        results
            .Should().BeEquivalentTo(
            [ 10, 20 ],
            options => options.WithStrictOrdering()
        );
    }

    [Fact]
    public void Next_min_max__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int32Next = CreateRaceStrategyFrom(100, 200);
        
        var results = RunParallel(() => 
            Rand.Next(50, 300)
        );

        results
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
        
        var results = RunParallel(() => 
            Rand.NextInt64()
        );

        results
            .Should().BeEquivalentTo(
            [ 1L, 2L ],
            options => options.WithStrictOrdering()
        );
    }

    [Fact]
    public void Next64_max__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int64Next = CreateRaceStrategyFrom(10L, 20L);
        
        var results = RunParallel(() => 
            Rand.NextInt64(100L)
        );

        results
            .Should().BeEquivalentTo(
            [ 10L, 20L ],
            options => options.WithStrictOrdering()
        );
    }

    [Fact]
    public void Next64_min_max__when_multi_threaded__should_not_mix_values()
    {
        Rand.Int64Next = CreateRaceStrategyFrom(100L, 200L);
        
        var results = RunParallel(() => 
            Rand.NextInt64(50L, 300L)
        );

        results
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
        
        var results = RunParallel(() => 
            Rand.NextSingle()
        );

        results
            .Should().BeEquivalentTo(
            [ 0.1f, 0.2f ],
            options => options.WithStrictOrdering()
        );
    }

    [Fact]
    public void NextDouble__when_multi_threaded__should_not_mix_values()
    {
        Rand.DoubleNext = CreateRaceStrategyFrom(0.1, 0.2);
        
        var results = RunParallel(() => 
            Rand.NextDouble()
        );

        results
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
            _barrier.SignalAndWait(); // barrier strictly before Next()

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
            _barrier.SignalAndWait(); // barrier strictly before Next()

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