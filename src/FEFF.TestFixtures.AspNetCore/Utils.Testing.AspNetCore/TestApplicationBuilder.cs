using Microsoft.AspNetCore.Mvc.Testing;

namespace FEFF.Extentions.Testing.AspNetCore;

internal class TestApplicationBuilder
{
    internal static ITestApplication Build<TEntryPoint>(IEnumerable<Action<IWebHostBuilder>> configureActions)
    where TEntryPoint: class
    {
        return new OverridenWebApplicationFactory<TEntryPoint>(configureActions);
    }
}

internal class OverridenWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, ITestApplication
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
}