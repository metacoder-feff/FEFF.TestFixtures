using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.AspNetCore.EF;

/// <summary>
/// Defines the contract for managing database lifecycle during tests.
/// </summary>
public interface IDatabaseLifecycleFixture
{
    /// <summary>
    /// Ensures that the database for the context exists and is created.
    /// </summary>
    /// <param name="token">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Starts the application under test if not already running.
    /// </remarks>
    Task EnsureCreatedAsync(CancellationToken token);
}

/// <inheritdoc/>
public interface IDatabaseLifecycleFixture<TContext> : IDatabaseLifecycleFixture
where TContext : DbContext
{
    /// <summary>
    /// Gets the <typeparamref name="TContext" /> instance resolved from the service provider.
    /// </summary>
    /// <remarks>
    /// Accessing this property starts the application under test if not already running.
    /// </remarks>
    TContext LazyDbContext { get; }
}

//TODO: skip options/properties + tests
//TODO: doc/rethrow: EnsureDeleted cannot remove an admin database. Consider using TmpDbNameFixture

/// <summary>
/// A fixture that manages Entity Framework Core database creation and deletion
/// for the lifetime of a test scope. The database is deleted on <see cref="DisposeAsync"/>.
/// </summary>
/// <remarks>
/// Note: EnsureDeleted may fail for admin databases. Consider using TmpDbNameFixture for such cases.
/// </remarks>
/// <typeparam name="TEntryPoint">The application entry point type.</typeparam>
/// <typeparam name="TContext">The <see cref="DbContext"/> type to manage.</typeparam>
[Fixture]
public sealed class DatabaseLifecycleFixture<TEntryPoint, TContext> : IAsyncDisposable, IDatabaseLifecycleFixture<TContext>
where TEntryPoint : class
where TContext : DbContext
{
    private readonly AppServicesFixture<TEntryPoint> _servicesFx;
    private readonly AppManagerFixture<TEntryPoint> _app;

    /// <inheritdoc/>
    public TContext LazyDbContext => _servicesFx.LazyServiceProvider.GetRequiredService<TContext>();

    /// <summary>
    /// Creates a new database lifecycle management fixture.
    /// </summary>
    /// <param name="app">The application manager fixture.</param>
    /// <param name="services">The application services fixture.</param>
    public DatabaseLifecycleFixture(AppManagerFixture<TEntryPoint> app, AppServicesFixture<TEntryPoint> services)
    {
        _servicesFx = services;
        _app = app;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_app.IsStarted == false)
            return;

        await LazyDbContext.Database.EnsureDeletedAsync().ConfigureAwait(false);
// TODO:
            // Log warning but do not throw to prevent test failures during cleanup
            // This can happen when database is locked or inaccessible
            // System.Console.Error.WriteLine($"[DatabaseLifecycleFixture] Warning: Failed to delete database: {ex.Message}");
    }

    /// <inheritdoc/>
    public async Task EnsureCreatedAsync(CancellationToken token)
    {
        await LazyDbContext.Database.EnsureCreatedAsync(token).ConfigureAwait(false);
    }
}
