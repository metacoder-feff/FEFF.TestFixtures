using System.Runtime.ExceptionServices;

namespace FEFF.Extentions;

//TODO: link nuget

public static class DisposeHelper
{
    public static async ValueTask DisposeAsync(List<IAsyncDisposable> disposables)
    {
        // see also (optimizations)
        // https://github.com/dotnet/runtime/pull/123342
        // PR: ServiceProviderEngineScope should aggregate exceptions in Dispose rather than throwing on the first

//TODO: optimize await ValueTask

        ExceptionDispatchInfo? first = null;
        List<Exception>? other = null;

        foreach(var d in disposables)
        {
            try
            {
                await d.DisposeAsync().ConfigureAwait(false);
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
}