# FakeRandomFixture

**Assembly**: `FEFF.TestFixtures.AspNetCore.dll`  
**Namespace**: `FEFF.TestFixtures.AspNetCore`  
**Source**: [FakeRandomFixture.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore/Fixtures/FakeRandomFixture.cs)

## Overview

The `FakeRandomFixture` is an extension to [`AppManagerFixture`](AppManagerFixture.md).  
It replaces the `Random` service with a `FakeRandom` singleton in the application under test, providing deterministic control over random number generation in your tests.

### Key Features

- **Deterministic Randomness**: Default seed of `1` for reproducible tests
- **Configurable Strategies**: Override default random generation with custom strategies for different data types
- **Thread-Safe**: All public methods are thread-safe via `lock()`
- **Automatic Registration**: Automatically registers the fake random service with the application's DI container
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
        builder.Services.AddSingleton((_) => Random.Shared);
        var app = builder.Build();

        // Example endpoint that uses Random
        app.MapGet("/random-number", (Random random) =>
        {
            return random.Next(1, 100);
        });

        app.Run();
    }
}
```

Note: `Random` should be registered as a singleton:

```csharp
builder.Services.AddSingleton((_) => Random.Shared);
```

Then use the fixture in your tests:

```csharp
using FEFF.TestFixtures.AspNetCore;
using FEFF.TestFixtures.AspNetCore.Randomness;

public class RandomTests
{
    protected IAppClientFixture Client { get; } = 
        TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    protected FakeRandomFixture<Program> FakeRandomFx { get; } = 
        TestContext.Current.GetFeffFixture<FakeRandomFixture<Program>>();

    protected FakeRandom FakeRandom => FakeRandomFx.Value;

    [Fact]
    public async Task RandomNumber__should_respond_with_default_value()
    {
        // Default behavior uses seed 1
        var resp = await Client.LazyValue.GetAsync("/random-number", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Be("42"); // Example expected value
    }

    [Theory]
    [InlineData(11)]
    [InlineData(22)]
    [InlineData(99)]
    public async Task RandomNumber__should_respond_with_custom_value(int expectedValue)
    {
        // Set the fake random to always return a specific value
        FakeRandom.Int32Next = FixedNextStrategy.From(expectedValue);

        var resp = await Client.LazyValue.GetAsync("/random-number", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Be($$"""{{expectedValue}}""");
    }
}
```

## Key Members

### FakeRandomFixture<TEntryPoint>

| Property | Type | Description |
|--------|------|-------------|
| `Value` | `FakeRandom` | Gets the `FakeRandom` instance, which can be configured to control random value generation during tests. |

### FakeRandom

| Property | Type | Description |
|--------|------|-------------|
| `Int32Next` | `INextStrategy<int>?` | Gets or sets the strategy for generating `int` values. |
| `Int64Next` | `INextStrategy<long>?` | Gets or sets the strategy for generating `long` values. |
| `SingleNext` | `INextStrategy<float>?` | Gets or sets the strategy for generating `float` values. |
| `DoubleNext` | `INextStrategy<double>?` | Gets or sets the strategy for generating `double` values. |
| `ByteNext` | `INextStrategy<byte>?` | Gets or sets the strategy for generating `byte` values. |
| `NormalizationStrategy` | `INormalizationStrategy` | Gets or sets the normalization strategy for out-of-range values. |

## FakeRandom Deep Dive

The `FakeRandom` class is a configurable random number generator that extends `System.Random` and provides fine-grained control over random value generation.

### Supported Types

`FakeRandom` supports the following random value types:

- `int` (32-bit integer)
- `long` (64-bit integer)
- `float` (single-precision)
- `double` (double-precision)
- `byte[]` (byte arrays)

### Default Generation Strategies

By default, `FakeRandom` uses the standard `Next()` methods of `System.Random` initialized with a seed value of `1`.

### Custom Generation Strategies

You can override the default random behavior by setting custom strategies for each type:

```csharp
// Example: Configure custom strategies for different types
FakeRandom.Int32Next = FixedNextStrategy.From(42);
FakeRandom.DoubleNext = FixedNextStrategy.From(0.5);
FakeRandom.ByteNext = FixedNextStrategy.From((byte)255);
```

| Property | Type | Description |
|----------|------|-------------|
| `Int32Next` | `INextStrategy<int>?` | Strategy for generating `int` values. |
| `Int64Next` | `INextStrategy<long>?` | Strategy for generating `long` values. |
| `SingleNext` | `INextStrategy<float>?` | Strategy for generating `float` values. |
| `DoubleNext` | `INextStrategy<double>?` | Strategy for generating `double` values. |
| `ByteNext` | `INextStrategy<byte>?` | Strategy for generating `byte` values. |

When a property is `null`, the default `Random` behavior is used.

#### INextStrategy<T>

The `INextStrategy<T>` interface defines a strategy for generating fake values:

```csharp
public interface INextStrategy<T>
{
    T Next();
}
```

### Built-in Strategies

#### FixedNextStrategy<T>

Returns a constant fixed value every time:

```csharp
using FEFF.TestFixtures.AspNetCore.Randomness;

// Create a strategy that always returns 42
FakeRandom.Int32Next = FixedNextStrategy.From(42);

// Or create directly
FakeRandom.Int32Next = new FixedNextStrategy<int>(100);
```

Factory method `FixedNextStrategy.From(value)` is the recommended way to create fixed strategies.

### Normalization Strategies

When a configured strategy returns a value that violates the method contract (e.g., `Next(min, max)` where the value is outside the expected range), a normalization strategy handles the situation.

```csharp
public interface INormalizationStrategy
{
    int NormalizeI32(int next, int min, int max);
    long NormalizeI64(long next, long min, long max);
    float NormalizeSingle(float next);
    double NormalizeDouble(double next);
}
```

#### Available Normalization Strategies

| Strategy | Behavior |
|----------|----------|
| `ThrowNormalization` (default) | Throws `InvalidOperationException` on out-of-range values. |
| `ReturnAsIsNormalization` | Returns the value unchanged. |

#### Using Normalization Strategies

```csharp
using FEFF.TestFixtures.AspNetCore.Randomness;

// Use default (throws on out-of-range values)
FakeRandom.NormalizationStrategy = new ThrowNormalization();

// Return values as-is without validation
FakeRandom.NormalizationStrategy = new ReturnAsIsNormalization();
```

### Thread Safety

All public methods in `FakeRandom` are thread-safe via `lock()`, making it safe to use in parallel test scenarios.

## Advanced Usage

### Multiple Value Types

You can configure strategies for different types independently:

```csharp
// Configure int generation
FakeRandom.Int32Next = FixedNextStrategy.From(42);

// Configure double generation  
FakeRandom.DoubleNext = FixedNextStrategy.From(0.5);

// Configure byte array generation
FakeRandom.ByteNext = FixedNextStrategy.From((byte)255);
```

### Using ReturnAsIsNormalization

When you need exact control over values without validation:

```csharp
using FEFF.TestFixtures.AspNetCore.Randomness;

// Allow any value to be returned, even if out of range
FakeRandom.NormalizationStrategy = new ReturnAsIsNormalization();

// This will return 150 even if Next(0, 100) is called
FakeRandom.Int32Next = FixedNextStrategy.From(150);
```

> [!TIP]
> You can use extension points and create needed implementations of `INextStrategy` or `INormalizationStrategy`, for example:
> - `RoundRobinListNextStrategy`
> - `AutoIncrementedNextStrategy`
> - `ModuloNormalizationStrategy`


## See Also

| Link | Description |
|------|-------------|
| [API: FakeRandomFixture](xref:FEFF.TestFixtures.AspNetCore.FakeRandomFixture`1) | API reference |
| [FakeRandomFixtureTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures.AspNetCore/FakeRandomFixtureTests.cs) | Unit tests for `FakeRandomFixture` |
| [FakeRandomTests](https://github.com/metacoder-feff/FEFF.TestFixtures/tree/main/tests/FEFF.TestFixtures.Tests/Utils/FakeRandomTests) | Unit tests for `FakeRandom` |
| [API Integration Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs) | Integration test examples using `FakeRandomFixture` |