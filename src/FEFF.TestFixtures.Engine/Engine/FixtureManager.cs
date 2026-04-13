using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Engine;

/// <summary>
/// Provides access to fixtures within a specific scope.
/// </summary>
public interface IFixtureScope
{
    /// <summary>
    /// Resolves the fixture of type <typeparamref name="T"/> from the current scope.
    /// </summary>
    /// <typeparam name="T">The type of fixture to resolve.</typeparam>
    /// <returns>The resolved fixture instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no fixture of type <typeparamref name="T"/> is registered.</exception>
    T GetFixture<T>() where T : notnull;
}

internal interface IFixtureManagerOptions
{
    ServiceProvider BuildServiceProvider();
}

/// <summary>
/// This class creates, memoizes, and disposes <see cref="IFixtureScope"/>.<br/>
/// A user can destroy <see cref="IFixtureScope"/> either by calling <see cref="RemoveScopeAsync"/> or by <see cref="DisposeAsync"/> that disposes all resources including all cached fixture-scopes.
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

    //TODO: public ctor
    /// <remarks>
    /// Use <see cref="FixtureManagerBuilder"/> for instance construction.
    /// </remarks>
    internal FixtureManager(IFixtureManagerOptions options)
    {
        _provider = options.BuildServiceProvider();
    }

    /// <summary>
    /// Gets or creates a scoped fixture container for the specified identifier.
    /// </summary>
    /// <param name="id">A unique identifier for the scope.</param>
    /// <returns>An <see cref="IFixtureScope"/> for the given scope.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the manager has been disposed.</exception>
    public IFixtureScope GetScope(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        // double-check: optimization
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        lock (_lock)
        {
            // double-check: guard
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            if (_scopes.ContainsKey(id))
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

    /// <summary>
    /// Disposes the manager and all cached fixture scopes asynchronously.
    /// </summary>
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

    /// <summary>
    /// Disposes and removes a specific fixture scope by its identifier.
    /// </summary>
    /// <param name="scopeId">The identifier of the scope to remove.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous disposal operation.</returns>
    public ValueTask RemoveScopeAsync(string scopeId)
    {
        FixtureScope scope;
        lock (_lock)
        {
            //TODO: optimize
            if (_scopes.ContainsKey(scopeId) == false)
                return ValueTask.CompletedTask;

            scope = _scopes[scopeId];
            _scopes.Remove(scopeId);
        }

        return scope.DisposeAsync();
    }
}
