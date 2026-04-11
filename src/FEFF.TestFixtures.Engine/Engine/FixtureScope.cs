using FEFF.Extensions.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Engine;

internal sealed class FixtureScope : IAsyncDisposable, IFixtureScope
{
    private readonly AsyncServiceScope _serviceScope;
    private bool _isDisposed;

    public FixtureScope(ServiceProvider sp)
    {
        _serviceScope = sp.CreateAsyncScope();
    }

    public ValueTask DisposeAsync()
    {
        if(_isDisposed)
            return ValueTask.CompletedTask;

        _isDisposed = true;

//TODO: remove in version 11+
        return ScopeDisposeWorkaround.DisposeAsync(_serviceScope);
        //return _serviceScope.DisposeAsync();
    }

    public T GetFixture<T>()
    where T : notnull
    {
        return _serviceScope.ServiceProvider.GetRequiredService<T>();
    }
}

//ServiceProviderEngineScope should aggregate exceptions in Dispose rather than throwing on the first
//[MERGED to main (dotnet-11-preview)]
//https://github.com/dotnet/runtime/pull/123342
internal static class ScopeDisposeWorkaround
{
#if NET11_0_OR_GREATER
    public static ValueTask DisposeAsync(AsyncServiceScope scope)
    {
        return scope.DisposeAsync();
    }
#else
    private static readonly Lazy<bool> __isVersion11OrGreater = new ( () =>
    {   
        var assembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .SingleOrDefault(x => x.GetName().Name == "Microsoft.Extensions.DependencyInjection")
            ;

        var ver = assembly?.GetName().Version;
        if(ver == null)
            return false;

        return ver.Major >= 11;
    });

    public static ValueTask DisposeAsync(AsyncServiceScope scope)
    {
        var disposables = GetDisposables(scope);

        // use default disposing method
        if (disposables == null || disposables.Count <= 0)
            return scope.DisposeAsync();

        if(disposables.Count > 1)
            disposables.Reverse();

        return DisposeHelper.DisposeAsync(disposables);
    }

    private static List<object>? GetDisposables(AsyncServiceScope scope)
    {
        // if version >= 11 do not override disposing method
        if(__isVersion11OrGreater.Value)
            return null;

        return scope
            .TryGetPrivateInstanceFieldValue<IServiceScope>("_serviceScope")
            ?.TryGetPrivateInstancePropertyValue<List<object>>("Disposables");
    }
#endif
}