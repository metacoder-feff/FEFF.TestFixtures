# AppClientFixture

**Assembly**: `FEFF.TestFixtures.AspNetCore.dll`  
**Namespace**: `FEFF.TestFixtures.AspNetCore`  
**Source**: [AppClientFixture.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore/Fixtures/AppClientFixture.cs)

## Overview

The `AppClientFixture` is an extension to [`AppManagerFixture`](AppManagerFixture.md).  
It automates the creation and disposal of an `HttpClient` connected to the test application.

### Key Features

- **Lazy Initialization**: The application starts only when `LazyValue` is first accessed
- **Automatic `HttpClient` Lifecycle Management**: `AppClientFixture` leverages the fixture disposal mechanism
- **Can be used instead of `AppManagerFixture`**: `AppClientFixture` internally requests `AppManagerFixture` as a dependency to access the test application

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
    protected IAppClientFixture ClientFx { get; } = 
        TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    protected HttpClient Client => ClientFx.LazyValue;

    [Fact]
    public async Task Example1__Client__should__get_response()
    {
        var resp = await Client.GetAsync("/const", TestContext.Current.CancellationToken);
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

Note: in this example, access to the `HttpClient` is:

```csharp
protected HttpClient Client => ClientFx.LazyValue;
```

This differs from direct usage of `AppManagerFixture`:

```csharp
using var client = AppManagerFx.LazyApplication.CreateClient();
```

## Key Members

| Member | Type | Description |
|--------|------|-------------|
| `LazyValue` | `HttpClient` | Gets the lazily-created `HttpClient`. Starts the application under test on first access if not already running. |

## See Also

| Link | Description |
|------|-------------|
| [API: AppClientFixture](xref:FEFF.TestFixtures.AspNetCore.AppClientFixture`1) | API reference |
| [AppClientFixtureTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures.AspNetCore/AppClientFixtureTests.cs) | Unit tests for `AppClientFixture` |
| [API Integration Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs) | Integration test examples using `AppClientFixture` |