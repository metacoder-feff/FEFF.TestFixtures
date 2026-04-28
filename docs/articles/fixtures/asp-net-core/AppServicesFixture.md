# AppServicesFixture

**Assembly**: `FEFF.TestFixtures.AspNetCore.dll`  
**Namespace**: `FEFF.TestFixtures.AspNetCore`  
**Source**: [AppServicesFixture.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore/Fixtures/AppServicesFixture.cs)

## Overview

The `AppServicesFixture` is an extension to [`AppManagerFixture`](AppManagerFixture.md).  
It provides access to the application's service provider, enabling resolution of services with scoped lifetime from the application under test.  
The fixture automatically creates and disposes an `AsyncServiceScope` from the application under test.

### Key Features

- **Lazy Initialization**: The application under test starts only when `LazyServiceProvider` is first accessed
- **Scoped Service Resolution**: Allows resolution of scoped services from the application's DI container
- **Automatic Disposal**: The service scope is automatically disposed when the fixture is disposed

## Basic Usage

First, define your test application entry point:

```csharp
// ASP.NET Core test application entry point (Program.cs)
public class SomeService
{
    public string Data { get; } = "123";
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Register a scoped service
        builder.Services.AddScoped<SomeService>();
        
        var app = builder.Build();
        app.Run();
    }
}
```

Then use the fixture in your tests:

```csharp
using FEFF.TestFixtures.AspNetCore;

public class ServicesTests
{
    protected IAppServicesFixture ServicesFx = 
        TestContext.Current.GetFeffFixture<AppServicesFixture<Program>>();

    [Fact]
    public void AppServices_should_be_resolved()
    {
        // Resolve scoped services from the service provider
        var svc = ServicesFx.LazyServiceProvider.GetRequiredService<SomeService>();

        svc.Data.Should().Be("123");
    }
}
```

Note: the `SomeService` instance can also be accessed directly via `AppManagerFixture`, but it requires more boilerplate:

```csharp
using var scope = AppManagerFx.LazyApplication.Services.CreateAsyncScope();
var svc = scope.ServiceProvider.GetRequiredService<SomeService>();
```

## Key Members

| Member | Type | Description |
|--------|------|-------------|
| `LazyServiceProvider` | `IServiceProvider` | Gets the service provider from a lazily-created `AsyncServiceScope`. Starts the application under test on first access if not already running. |

## See Also

| Link | Description |
|------|-------------|
| [API: AppServicesFixture](xref:FEFF.TestFixtures.AspNetCore.AppServicesFixture`1) | API reference |
| [AppServicesFixtureTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures.AspNetCore/AppServicesFixtureTests.cs) | Unit tests for `AppServicesFixture` |
| [API Integration Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs) | Integration test examples using `AppServicesFixture` |
