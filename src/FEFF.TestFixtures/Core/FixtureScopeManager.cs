namespace FEFF.TestFixtures;

public interface IFixtureScope
{
    T GetFixture<T>() where T : notnull;
}

/// <summary>
/// This class creates, memoizes and disoses <see cref="IFixtureScope"/>.<br/>
/// User can destroy <see cref="IFixtureScope"/> either by calling <see cref="RemoveScopeAsync"/> or by <see cref="DisposeAsync"/> that disposes all resources including all cached fuxture-scopes.
/// </summary>
public sealed class FixtureScopeManager : IAsyncDisposable
{
    private readonly FixtureScopeFactory _provider = new();
    private readonly Dictionary<string, FixtureScope> _scopes = [];
    private readonly Lock _lock = new(); 
    private bool _isDisposed;
    
    public FixtureScope GetScope(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        // double-check: optimization
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        lock(_lock)
        {
            // double-check: guard
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            if(_scopes.ContainsKey(id))
                return _scopes[id];

            var res = _provider.CreateScope();
            _scopes[id] = res;
            return res; 
        }
    }

    public async ValueTask DisposeAsync()
    {
        // see also (optimizations)
        // https://github.com/dotnet/runtime/pull/123342
        // PR: ServiceProviderEngineScope should aggregate exceptions in Dispose rather than throwing on the first

        List<IAsyncDisposable> disposables;
        lock(_lock)
        {
            _isDisposed = true;
            disposables = new (_scopes.Count + 1); // reserve slot for _provider
            disposables.AddRange(_scopes.Values);
        }

        disposables.Add(_provider);

        var exx = await DisposeAsync(disposables);

        if(exx.Count > 0)
        {
            throw new AggregateException("Multiple errors at .Dispose[Async]().", exx);
        }
    }

    private static async Task<List<Exception>> DisposeAsync(List<IAsyncDisposable> disposables)
    {
        var res = new List<Exception>();
        foreach(var d in disposables)
        {
            try
            {
                await d.DisposeAsync();
            }
            catch(Exception e)
            {
                res.Add(e);
            }
        }
        return res;
    }

    public ValueTask RemoveScopeAsync(string scopeId)
    {
        FixtureScope scope;
        lock(_lock)
        {
//TODO: optimize
            if(_scopes.ContainsKey(scopeId) == false)
                return ValueTask.CompletedTask;
                
            scope = _scopes[scopeId];
            _scopes.Remove(scopeId);
        }

        return scope.DisposeAsync();
    }
}
