using System.Runtime.ExceptionServices;

namespace FEFF.Extentions;

//TODO: link nuget

internal interface IDisposer<T>
{
    static abstract ValueTask DisposeAsync(T disposable);
}

internal static class DisposeHelper
{
    public static ValueTask DisposeAsync(IReadOnlyList<IAsyncDisposable> disposables)
    {
        return InternalDisposeAsync<Disposer, IAsyncDisposable>(disposables);
    }

    public static ValueTask DisposeAsync(IReadOnlyList<object> disposables)
    {
        return InternalDisposeAsync<Disposer, object>(disposables);
    }

    /// <remarks>
    /// Polymorfic over 'T' algorithm. The <see cref="Disposer"/> class defines abstraction over non-polymorphic static methods.
    /// </remarks>
    private static async ValueTask InternalDisposeAsync<TDisposer,T>(IReadOnlyList<T> disposables)
    where TDisposer : IDisposer<T>
    {
        // see also (optimizations)
        // https://github.com/dotnet/runtime/pull/123342
        // PR: ServiceProviderEngineScope should aggregate exceptions in Dispose rather than throwing on the first

        ExceptionDispatchInfo? first = null;
        List<Exception>? other = null;

        foreach(var d in disposables)
        {
            try
            {
//TODO: optimize
                await TDisposer.DisposeAsync(d).ConfigureAwait(false);
                // not working
                // var vt = DisposeObject(d);
                // if(vt.IsCompleted == false)
                //     await vt.ConfigureAwait(false);
            }
            catch(Exception e)
            {
                if(first == null)
                {
                    first = ExceptionDispatchInfo.Capture(e);
                }
                else
                {
                    other ??= new (2); // reserve a slot for first exception
                    other.Add(e);
                }
            }
        }

        if (other != null)
        {
            if (first != null) // guard, 'other != null && first == null' should not occur
                other.Add(first.SourceException);
            throw new AggregateException("Multiple errors at .Dispose[Async]().", other);
        }
        else if (first != null)
            first.Throw();
    }

    // static class can not implement interface therefore create non-static nested class
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