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
    Task EnsureCreatedAsync(CancellationToken token);
}

//TODO: skip options/properties + tests
//TODO: doc/rethrow: EnsureDeleted cannot remove an admin database. Consider using TmpDbNameFixture
// Drop
//preview

/// <summary>
/// A fixture that manages Entity Framework Core database creation and deletion
/// for the lifetime of a test scope. The database is deleted on <see cref="DisposeAsync"/>.
/// </summary>
/// <typeparam name="TEntryPoint">The application entry point type.</typeparam>
/// <typeparam name="TContext">The <see cref="DbContext"/> type to manage.</typeparam>
[Fixture]
public sealed class DatabaseLifecycleFixture<TEntryPoint, TContext> : IAsyncDisposable, IDatabaseLifecycleFixture
where TEntryPoint : class
where TContext : DbContext
{
    private readonly AppServicesFixture<TEntryPoint> _servicesFx;
    private readonly AppManagerFixture<TEntryPoint> _app;

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

        var ctx = _servicesFx.LazyServiceProvider.GetRequiredService<TContext>();
        await ctx.Database.EnsureDeletedAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task EnsureCreatedAsync(CancellationToken token)
    {
        var ctx = _servicesFx.LazyServiceProvider.GetRequiredService<TContext>();
        //ctx.Database.EnsureDeleted();
        await ctx.Database.EnsureCreatedAsync(token);
    }
}
