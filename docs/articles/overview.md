# Overview

FEFF.TestFixtures is a testing library extension for .NET that replaces traditional setup/teardown methods or test-class "Disposable pattern" with reusable, composable fixtures.

## What is a Fixture?

A **fixture** is a reusable component that manages resources for testing purposes. Each fixture has three parts:

| Part | Purpose | Example |
|------|---------|---------|
| **Setup** | Initialize resources | Create a temp directory, open a database connection |
| **State** | Expose data to tests | File path, connection string, test data |
| **Teardown** | Clean up resources | Delete files, close connections, rollback transactions |

## Goals

FEFF.TestFixtures aims to:

✅ **Eliminate boilerplate** - Replace repetitive setup/teardown methods  
✅ **Enable composition** - Build complex fixtures from simpler ones  
✅ **Simplify ASP.NET Core testing** - Built-in fixtures for web applications  
✅ **Maintain isolation** - Each test gets clean, predictable state  

Additionaly:  
✅ **Support multiple frameworks** - Works with xUnit v3 and TUnit  

## Key Concepts

### Fixtures

Fixtures are classes marked with the `[Fixture]` attribute. The framework discovers and manages their lifecycle automatically.

```csharp
[Fixture]
public class MyFixture : IDisposable
{
    public string Resource { get; }

    public MyFixture()
    {
        Resource = "initialized";
    }

    public void Dispose()
    {
        // Cleanup
    }
}
```

### Scopes

The **scope** of a fixture defines its lifetime. Within a scope, each fixture is created only once (lazily on demand) and destroyed at the end of the scope. If the fixture implements Dispose() or DisposeAsync(), those methods are called.

The available scopes are defined by the test framework used. For **Xunit Integration**, they are:  
`TestCase` , `Class`, `Collection`, `Assembly`

For **TUnit Integration**, they are:  
`TestCase` , `Class`, `Assembly`, `Session`

### Fixture Dependencies

Fixtures can depend on other fixtures through constructor injection:

```csharp
[Fixture]
public class ComplexFixture
{
    public ComplexFixture(SimpleFixture1 f1, SimpleFixture2 f2)
    {
        // Dependencies automatically resolved
    }
}
```

All dependencies share the same scope as the parent fixture.

## Package Structure

The library is distributed as multiple NuGet packages:

| Package | Purpose |
|---------|---------|
| `FEFF.TestFixtures.XunitV3` | Integration with xUnit v3 |
| `FEFF.TestFixtures.TUnit` | Integration with TUnit |
| `FEFF.TestFixtures` | Core fixtures (temp directory, environment, etc.) |
| `FEFF.TestFixtures.AspNetCore` | ASP.NET Core testing fixtures |
| `FEFF.TestFixtures.AspNetCore.EF` | EF Core database lifecycle |
| `FEFF.TestFixtures.AspNetCore.SignalR` | SignalR client for testing |

## Supported Platforms

- **.NET**: 8.0, 10.0
- **Test Frameworks**: xUnit v3, TUnit

## Resources

- [GitHub Repository](https://github.com/metacoder-feff/FEFF.TestFixtures)
- [NuGet Packages](https://www.nuget.org/packages/FEFF.TestFixtures)
- [Examples](https://github.com/metacoder-feff/FEFF.TestFixtures/tree/main/examples)
- [Issue Tracker](https://github.com/metacoder-feff/FEFF.TestFixtures/issues)

## What's Next?

Choose your path based on your needs:

| Your Goal | Next Step |
|-----------|-----------|
| Quick setup with xUnit v3 | [Quick Start (xUnit v3)](quick-start-xunit.md) |
| Quick setup with TUnit | [Quick Start (TUnit)](quick-start-tunit.md) |
| Create your own fixture | [Creating Custom Fixtures](creating-custom-fixtures.md) |
| Combine fixtures | [Fixture Dependencies](fixture-dependencies.md) |
| Explore builtin fixtures | [Fixture List](built-in-fixtures.md) |
