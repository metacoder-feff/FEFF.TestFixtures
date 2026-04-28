# AppManagerFixture

> **Assembly**: `FEFF.TestFixtures.AspNetCore.dll`  
> **Namespace**: `FEFF.TestFixtures.AspNetCore`  
> **Source**: [`AppManagerFixture.cs`](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore/Fixtures/AppManagerFixture/AppManagerFixture.cs)

## Overview

`AppManagerFixture<TEntryPoint>` manages the lifecycle of an ASP.NET Core test application using `WebApplicationFactory<TEntryPoint>`. It provides a complete test host environment that can be configured before startup, making it ideal for integration testing of web APIs, middleware, and dependency injection configurations.

Key features:

- **Central** element for testing ASP.NET Core applications allowing to extend the test pipeline
- **Lazy initialization**: The application starts on first access to `LazyApplication`
- **Configuration support**: Via `ConfigurationBuilder` to modify `IWebHostBuilder` before startup
- **Full lifecycle management**: Automatic cleanup via `IAsyncDisposable`
- **Test isolation**: Each test scope gets its own application instance based on the configured fixture scope

## Basic Usage

First, define your test application entry point:

```csharp
// ASP.NET Core test application entry point (Program.cs)
public class Program
{
    public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/const", () =>
        {
            return new WeatherForecast(DateOnly.Parse("2000-01-01"), 20, "normal");
        });

        app.Run();
    }
}
```

Then use the fixture in your tests:

```csharp
using FEFF.TestFixtures.AspNetCore;

public class ApiTests
{
    protected AppManagerFixture<Program> AppManagerFx { get; } = 
        TestContext.Current.GetFeffFixture<AppManagerFixture<Program>>();

    [Fact]
    public async Task Example1__Client__should__get_response()
    {
        using var client = AppManagerFx.LazyApplication.CreateClient();
        var resp = await client.GetAsync("/const", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

        JToken.Parse(body)
            .Should().BeEquivalentTo(
            """
            {
                "date": "2000-01-01",
                "temperatureC": 20,
                "summary": "normal"
            }
            """);
    }
}
```

## Key Members

| Property | Type | Description |
|----------|------|-------------|
| `ConfigurationBuilder` | `IAppConfigurator` | Provides access to configure the web host before startup |
| `LazyApplication` | `ITestApplication` | Lazily-started test application instance |
| `IsStarted` | `bool` | Indicates whether the application has been started |

### IAppConfigurator Key Members

| Method | Returns | Description |
|--------|------|-------------|
| `ConfigureWebHost` | `void` | Adds a configuration action to be applied to the `IWebHostBuilder`, accepts `Action<IWebHostBuilder>` as an argument|

### ITestApplication Key Members

| Property | Type | Description |
|----------|------|-------------|
| `Services` | `IServiceProvider` | Gets the service provider for the test application |
| `Server` | `TestServer` | Gets the `TestServer` for the test application (useful for SignalR testing) |

| Method | Returns | Description |
|--------|------|-------------|
| `CreateClient` | `HttpClient` | Creates an `HttpClient` configured to communicate with the test application |

## Configuration Before Startup

You can configure the application before it starts using `ConfigurationBuilder`. Here's how to modify configuration values:

```csharp
// ASP.NET Core test application entry point (Program.cs)
// ...
app.MapGet("/weatherforecast/env", (IConfiguration c) =>
{
    var summary = c.GetValue<string>("summary");
    return new WeatherForecast(DateOnly.Parse("2000-01-01"), 20, summary);
});
// ...
```

```csharp
using FEFF.TestFixtures.AspNetCore;

public class ConfigurationTests
{
    protected AppManagerFixture<Program> AppManagerFx { get; } = 
        TestContext.Current.GetFeffFixture<AppManagerFixture<Program>>();

    [Theory]
    [InlineData("cloudy")]
    [InlineData("warm")]
    public async Task SetEnvVar__should_affect_api_response(string summaryValue)
    {
        // Configure before application starts
        AppManagerFx.ConfigurationBuilder
            .UseSetting("summary", summaryValue);

        // First request to 'LazyApplication' triggers application startup
        using var client = AppManagerFx.LazyApplication.CreateClient();
        var resp = await client.GetAsync("/weatherforecast/env", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

        // Assert the JSON response contains the summary value we configured
        JToken.Parse(body)
            .Should().BeEquivalentTo(
            $$"""
            {
                "date": "2000-01-01",
                "temperatureC": 20,
                "summary": "{{summaryValue}}"
            }
            """);
    }
}
```

> [!TIP]
> `UseSetting` is an extension method for `IAppConfigurator`. You can also modify application's `ServiceCollection`.

## See Also

| Resource | Description |
|----------|-------------|
| [API: AppManagerFixture](xref:FEFF.TestFixtures.AspNetCore.AppManagerFixture`1) | API reference |
| @FEFF.TestFixtures.AspNetCore.AppConfiguratorExtensions | Extension methods for `IAppConfigurator` |
| [Basic Tests](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures.AspNetCore/AppManagerFixture.BasicTests.cs) | Core functionality test coverage |
| [Configuration Tests](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures.AspNetCore/AppManagerFixture.ConfigurationTests.cs) | Configuration options test coverage |
| [API Integration Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs) | Full integration test example with xUnit v3 and Asp.Net Core |
