using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.AspNetCore;

public interface IDatabaseLifecycleFixture
{
    Task EnsureCreatedAsync(CancellationToken token);
}

//TODO: skip options/properties + tests
//TODO: doc/rethrow: EnsureDeleted cannot remove an admin database. Consider using TmpDbNameFixture
// Drop
//preview
[Fixture]
public sealed class DatabaseLifecycleFixture<TEntryPoint, TContext> : IAsyncDisposable, IDatabaseLifecycleFixture 
where TEntryPoint : class
where TContext : DbContext
{
    private readonly AppServicesFixture<TEntryPoint> _servicesFx;
    private readonly AppManagerFixture<TEntryPoint> _app;

    public DatabaseLifecycleFixture(AppManagerFixture<TEntryPoint> app, AppServicesFixture<TEntryPoint> services)
    {
        _servicesFx = services;
        _app = app;
    }

    public async ValueTask DisposeAsync()
    {
        if (_app.IsStarted == false)
            return;

        var ctx = _servicesFx.LazyServiceProvider.GetRequiredService<TContext>();
        await ctx.Database.EnsureDeletedAsync().ConfigureAwait(false);
        System.Diagnostics.Debugger.Break();
    }

    public async Task EnsureCreatedAsync(CancellationToken token)
    {
        var ctx = _servicesFx.LazyServiceProvider.GetRequiredService<TContext>();
        //ctx.Database.EnsureDeleted();
        await ctx.Database.EnsureCreatedAsync(token);
    }
}