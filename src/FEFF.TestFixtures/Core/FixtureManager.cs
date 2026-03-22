using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Core;

public interface IFixtureScope
{
    T GetFixture<T>() where T : notnull;
}

internal interface IFixtureManagerOptions
{
    ServiceProvider BuildServiceProvider();    
}

/// <summary>
/// This class creates, memoizes and disoses <see cref="IFixtureScope"/>.<br/>
/// User can destroy <see cref="IFixtureScope"/> either by calling <see cref="RemoveScopeAsync"/> or by <see cref="DisposeAsync"/> that disposes all resources including all cached fuxture-scopes.
/// </summary>
/// <remarks>
/// Use <see cref="FixtureManagerBuilder"/> for instance construction.
/// </remarks>
public sealed class FixtureManager : IAsyncDisposable
{
    private readonly ServiceProvider _provider;
    private readonly Dictionary<string, FixtureScope> _scopes = [];

#if NET9_0_OR_GREATER
    private readonly Lock _lock = new(); 
#else
    private readonly Object _lock = new(); 
#endif
    private bool _isDisposed;

    internal FixtureManager(IFixtureManagerOptions options)
    {
        _provider = options.BuildServiceProvider();
    }

    public IFixtureScope GetScope(string id)
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

            var res = CreateScope();
            _scopes[id] = res;
            return res; 
        }
    }

    private FixtureScope CreateScope()
    {
        return new FixtureScope(_provider);
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
