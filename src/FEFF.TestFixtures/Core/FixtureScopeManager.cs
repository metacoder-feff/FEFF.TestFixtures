namespace FEFF.TestFixtures.Core;

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

    public ValueTask DisposeAsync()
    {
        List<IAsyncDisposable> disposables;
        lock (_lock)
        {
            _isDisposed = true;
            disposables = new(_scopes.Count + 1); // reserve a slot for _provider
            disposables.AddRange(_scopes.Values);
        }

        disposables.Add(_provider);

        return DisposeHelper.DisposeAsync(disposables);
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
