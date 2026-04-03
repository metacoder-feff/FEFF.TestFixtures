using System.Runtime.ExceptionServices;

namespace FEFF.Extensions;

//TODO: link nuget

internal static class DisposeHelper
{
    public static void Dispose(IReadOnlyList<IDisposable> disposables)
    {
        ArgumentNullException.ThrowIfNull(disposables);

        if(disposables.Count <= 0)
            return;

        if(disposables.Count == 1)
        {
            disposables[0].Dispose();
            return;
        }

        var errorCtx = new ErrorContext();

        foreach (var d in disposables)
        {
            try
            {
                d.Dispose();
            }
            catch (Exception e)
            {
                errorCtx.Add(e);
            }
        }

        errorCtx.RethrowIfAny();
    }

    public static ValueTask DisposeAsync(IReadOnlyList<IAsyncDisposable> disposables)
    {
        return InternalDisposeAsync<Disposer, IAsyncDisposable>(disposables);
    }

    // disposables may be either 'IDisposable' or 'IAsyncDisposable'
    public static ValueTask DisposeAsync(IReadOnlyList<object> disposables)
    {
        return InternalDisposeAsync<Disposer, object>(disposables);
    }

    /// <remarks>
    /// DRY and optimization:
    /// Polymorphic over 'T' algorithm. The <see cref="Disposer"/> class provides an abstraction over non-polymorphic static methods.
    /// </remarks>
    private static ValueTask InternalDisposeAsync<TDisposer,T>(IReadOnlyList<T> disposables)
    where TDisposer : IDisposer<T>
    {
        // see also (optimizations)
        // https://github.com/dotnet/runtime/pull/123342
        // PR: ServiceProviderEngineScope should aggregate exceptions in Dispose rather than throwing on the first

        ArgumentNullException.ThrowIfNull(disposables);

        if(disposables.Count <= 0)
            return ValueTask.CompletedTask;

        if(disposables.Count == 1)
            return TDisposer.DisposeAsync(disposables[0]);

        var errorCtx = new ErrorContext();

        var tasks = BeginDispose<TDisposer, T>(disposables, ref errorCtx);

        // if all tasks finished sync
        if(tasks.Count <= 0)
        {
            errorCtx.RethrowIfAny();
            return ValueTask.CompletedTask;
        }

        // some of tasks needs to be awaited...

        // WARNING: "Async methods cannot have ref, in or out parameters",
        // we can't use 'ref errorCtx'
        // but 'errorCtx' would not be used anymore
        // therefore just copy it
        return AwaitTasksAndRethrow(tasks, errorCtx);
    }

    // WARNING: "Async methods cannot have ref, in or out parameters",
    private static async ValueTask AwaitTasksAndRethrow(List<ValueTask> tasks, ErrorContext errorCtx)
    {
        foreach (var t in tasks)
        {
            try
            {
                await t.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                errorCtx.Add(e);
            }
        }

        errorCtx.RethrowIfAny();
    }

    private static List<ValueTask> BeginDispose<TDisposer,T>(IReadOnlyList<T> disposables, ref ErrorContext errorCtx)
    where TDisposer : IDisposer<T>
    {
        var res = new List<ValueTask>(disposables.Count);

        foreach (var d in disposables)
        {
            try
            {
                var vt = TDisposer.DisposeAsync(d);
                if(vt.IsCompleted == false)
                    res.Add(vt);
            }
            catch (Exception e)
            {
                errorCtx.Add(e);
            }
        }

        return res;
    }

    // Rethrows nothing if no exceptions were handled.
    // Rethrows source exception if only one was handled.
    // Rethrows AggregateException if more than one exception was handled.
    // DRY: used in multiple places
    private struct ErrorContext
    {
        private ExceptionDispatchInfo? _first;
        private List<Exception>? _others;

        public void Add(Exception e)
        {
            if (_first == null)
            {
                _first = ExceptionDispatchInfo.Capture(e);
            }
            else
            {
                _others ??= new(2); // reserve a slot for first exception
                _others.Add(e);
            }
        }

        public readonly void RethrowIfAny()
        {
            if (_others != null)
            {
                if (_first != null) // guard, 'other != null && first == null' should not occur
                {
                    _others.Add(_first.SourceException);
                    // _first = null;
                }
                throw new AggregateException("Multiple errors at .Dispose[Async]().", _others);
            }
            else if (_first != null)
                _first.Throw();
        }
    }

    // Optimizations for polymorphic 'DisposeAsync' call.
    // Using a direct generic static call instead of a vcall or delegate.
    internal interface IDisposer<T>
    {
        static abstract ValueTask DisposeAsync(T disposable);
    }

    // A static class cannot implement an interface; therefore, a non-static nested class is created.
    private class Disposer : IDisposer<object>, IDisposer<IAsyncDisposable>
    {
        static ValueTask IDisposer<object>.DisposeAsync(object disposable)
        {
            if(disposable is IAsyncDisposable ad)
                return ad.DisposeAsync();

            else if(disposable is IDisposable d)
                d.Dispose();

            return ValueTask.CompletedTask;
        }

        static ValueTask IDisposer<IAsyncDisposable>.DisposeAsync(IAsyncDisposable disposable)
        {
            return disposable.DisposeAsync();
        }
    }
}