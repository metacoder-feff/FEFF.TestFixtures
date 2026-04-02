using Microsoft.AspNetCore.TestHost;

namespace FEFF.Extensions.Testing.AspNetCore;

public interface IApplicationConfigurator
{
    void ConfigureWebHost(Action<IWebHostBuilder> action);
}

public interface ITestApplication : IAsyncDisposable
{
    IServiceProvider Services { get; }
    TestServer Server { get; }

    HttpClient CreateClient();

//TODO: async
    void StartServer();
}