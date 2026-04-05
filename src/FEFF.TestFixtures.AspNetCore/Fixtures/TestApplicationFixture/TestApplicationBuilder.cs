using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FEFF.TestFixtures.AspNetCore;

internal interface IWebApplicationFactory : IAsyncDisposable
{
    IServiceProvider Services { get; }

    HttpClient CreateClient();

    void StartServer();
    // TestServer Server { get; }
    // ...
}

internal class TestApplicationBuilder<TEntryPoint>: IApplicationConfigurator
where TEntryPoint: class
{
    private readonly List<Action<IWebHostBuilder>> _builderOverrides = [];

    public void ConfigureWebHost(Action<IWebHostBuilder> action)
    {
        _builderOverrides.Add(action);
    }

    public IWebApplicationFactory Build()
    {
        return new OverridenWebApplicationFactory<TEntryPoint>(_builderOverrides.ToImmutableArray());
    }
}

internal class OverridenWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IWebApplicationFactory
where TEntryPoint: class
{
    private readonly IEnumerable<Action<IWebHostBuilder>> _builderOverrides;

    internal OverridenWebApplicationFactory(IEnumerable<Action<IWebHostBuilder>> builderOverrides)
    {
        _builderOverrides = builderOverrides;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        foreach(var configureAction in _builderOverrides)
            configureAction(builder);
    }

#if ! NET10_0_OR_GREATER
    public void StartServer()
    {
        _ = Server;
    }
#endif
}