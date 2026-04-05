using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.AspNetCore;

//TODO: skip options/properies + tests
//TODO: rename: AppDbContextEnsuredFixture
//TODO: doc/rethrow: EnsureDeleted cant remove admin db. Consider using TmpDbNameFixture
// Drop 
//preview
[Fixture]
public sealed class EnsureDbContextFixture<TEntryPoint, TContext> : IAsyncDisposable
where TEntryPoint: class
where TContext: DbContext
{
    private readonly AppServicesFixture<TEntryPoint> _servicesFx;
    private readonly TestApplicationFixture<TEntryPoint> _app;

    public EnsureDbContextFixture(TestApplicationFixture<TEntryPoint> app, AppServicesFixture<TEntryPoint> services)
    {
        app.ApplicationStarted += OnAppStarted;
        _servicesFx = services;
        _app = app;
    }

    public async ValueTask DisposeAsync()
    {
        if(_app.IsStarted == false)
            return;

        var ctx = _servicesFx.LazyServiceProvider.GetRequiredService<TContext>();
        await ctx.Database.EnsureDeletedAsync().ConfigureAwait(false);
        System.Diagnostics.Debugger.Break();
    }

    private void OnAppStarted()
    {
        // OnStarted is invoked only once
        // help GC: remove circular ref
        _app.ApplicationStarted -= OnAppStarted;

        var ctx = _servicesFx.LazyServiceProvider.GetRequiredService<TContext>();
        //ctx.Database.EnsureDeleted();
        ctx.Database.EnsureCreated();
    }
}