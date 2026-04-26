# FakeTimeFixture

**Assembly**: `FEFF.TestFixtures.AspNetCore.dll`  
**Namespace**: `FEFF.TestFixtures.AspNetCore`  
**Source**: [FakeTimeFixture.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore/Fixtures/FakeTimeFixture.cs)

## Overview

The `FakeTimeFixture` is an extension to [`AppManagerFixture`](AppManagerFixture.md).   
It replaces the `TimeProvider` service with a `FakeTimeProvider` singleton in the application under test, providing deterministic control over time-dependent behavior in your tests.

### Key Features

- **Deterministic Time**: Start time at a fixed point (2000-01-01 00:00:00 UTC) for reproducible tests
- **Dynamic Time Control**: Adjust the current time during test execution using `SetUtcNow()`
- **Automatic Registration**: Automatically registers the fake time provider with the application's DI container
- **Integration with AppManagerFixture**: Works seamlessly with other ASP.NET Core fixtures

## Basic Usage

First, define your test application entry point:

```csharp
// ASP.NET Core test application entry point (Program.cs)
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton((_) => TimeProvider.System);
        var app = builder.Build();

        // Example endpoint that uses TimeProvider
        app.MapGet("/current-time", (TimeProvider timeProvider) =>
        {
            return timeProvider.GetUtcNow().ToString("yyyy-MM-dd");
        });

        app.Run();
    }
}
```

Note: `TimeProvider` should be registered as a singleton:

```csharp
builder.Services.AddSingleton((_) => TimeProvider.System);
```

Then use the fixture in your tests:

```csharp
using FEFF.TestFixtures.AspNetCore;
using Microsoft.Extensions.Time.Testing;

public class TimeTests
{
    protected IAppClientFixture Client = 
        TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    protected FakeTimeFixture<Program> FakeTimeFx = 
        TestContext.Current.GetFeffFixture<FakeTimeFixture<Program>>();

    protected FakeTimeProvider FakeTime => FakeTimeFx.Value;

    [Fact]
    public async Task CurrentTime__should_respond_with_default_date()
    {
        // Default time is 2000-01-01 00:00:00 UTC
        var resp = await Client.LazyValue.GetAsync("/current-time", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Be("\"2000-01-01\"");
    }

    [Theory]
    [InlineData("2006-01-05")]
    [InlineData("2150-11-15")]
    public async Task CurrentTime__should_respond_with_custom_date(string date)
    {
        // Set the fake time to a specific date
        FakeTime.SetUtcNow(DateTimeOffset.Parse($"{date}T05:05:05Z"));

        var resp = await Client.LazyValue.GetAsync("/current-time", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Be($@"""{date}""");
    }
}
```

## Key Members

| Member | Type | Description |
|--------|------|-------------|
| `Value` | `FakeTimeProvider` | Gets the `FakeTimeProvider` instance. Can be used to control time during tests using methods like `SetUtcNow()` and `AdvanceDays()`. |

## Time Control Methods

The `FakeTimeProvider` provides several methods to control time:

| Method | Description |
|--------|-------------|
| `SetUtcNow(DateTimeOffset)` | Sets the current UTC time to a specific value |
| `AdvanceDays(int)` | Advances the current time by the specified number of days |
| `Advance(TimeSpan)` | Advances the current time by the specified time span |

> [!TIP]
> The `FakeTimeProvider` instance can be manipulated at any point during a test: before and after the application under test is started.

## See Also

| Link | Description |
|------|-------------|
| [FakeTimeFixtureTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures.AspNetCore/FakeTimeFixtureTests.cs) | Unit tests for `FakeTimeFixture` |
| [API Integration Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs) | Integration test examples using `FakeTimeFixture` |
| [Microsoft.Extensions.Time.Testing Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.time.testing.faketimeprovider) | Official documentation for `FakeTimeProvider` |
