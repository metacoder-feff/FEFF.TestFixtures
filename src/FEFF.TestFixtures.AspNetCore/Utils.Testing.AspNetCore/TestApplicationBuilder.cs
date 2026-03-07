using System.Collections.Frozen;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace FEFF.Extentions.Testing.AspNetCore;

/// <summary>
/// This abstraction layer is needed to reuse and combinate TestFixtures - 'TestApp extentions'.
/// </summary>
public interface ITestApplicationBuilder
{
    void ConfigureWebHost(Action<IWebHostBuilder> action);
    ITestApplication Build(); 
}

public interface ITestApplication : IAsyncDisposable
{
    IServiceProvider Services { get; }
    TestServer Server { get; }

    HttpClient CreateClient();

//TODO: async
    void StartServer();
}

public class TestApplicationBuilder<TEntryPoint> : ITestApplicationBuilder
where TEntryPoint: class
{
    private readonly List<Action<IWebHostBuilder>> _builderOverrides = [];
    private bool _isBuilt = false;

    public void ConfigureWebHost(Action<IWebHostBuilder> action)
    {
        if(_isBuilt)
            throw new InvalidOperationException("Configuration can't be changed after the app is started.");

        _builderOverrides.Add(action);
    }

    public ITestApplication Build()
    {
        _isBuilt = true;
        return new OverridenWebApplicationFactory<TEntryPoint>(_builderOverrides.ToFrozenSet());
    }
}

internal class OverridenWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, ITestApplication
where TEntryPoint: class
{
    private readonly FrozenSet<Action<IWebHostBuilder>> _builderOverrides;

    public OverridenWebApplicationFactory(FrozenSet<Action<IWebHostBuilder>> builderOverrides)
    {
        _builderOverrides = builderOverrides;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        foreach(var configureAction in _builderOverrides)
            configureAction(builder);
    }
}