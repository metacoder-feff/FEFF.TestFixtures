# Options Pattern

Fixtures that implement `IFixtureRegistrar` can expose configuration options through the standard `Microsoft.Extensions.Options` pattern. This article demonstrates how to add configuration support to a fixture using `TmpDirectoryFixture` as an example.

## Overview

When you need to customize fixture behavior without modifying test code, configure the fixture through **Environment variables**.

## Example: TmpDirectoryFixture Configuration

The `TmpDirectoryFixture` provides a unique temporary directory for each test scope. By implementing configuration options, users can control:

- Whether the directory is deleted after disposal (useful for debugging or CI optimization)
- A custom prefix for the directory name (for easier identification in temp folders)

### Step 1: Define Configuration Options

Create an `Options` class and any supporting enums:

```csharp
/// <summary>
/// Specifies the behavior when the fixture is disposed.
/// </summary>
public enum DisposeType
{
    /// <summary>
    /// Deletes the temporary directory and its contents on disposal.
    /// </summary>
    Delete,

    /// <summary>
    /// Skips deletion of the temporary directory on disposal.
    /// </summary>
    /// <remarks>
    /// Can be used for optimization in CI environments.
    /// </remarks>
    Skip
}

/// <summary>
/// Configuration options for TmpDirectoryFixture.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets or sets whether the temporary directory should be deleted on disposal.
    /// Defaults to DisposeType.Delete.
    /// </summary>
    public DisposeType DisposeType { get; set; } = DisposeType.Delete;

    /// <summary>
    /// Gets or sets the prefix for the temporary directory name.
    /// </summary>
    public string? Prefix { get; set; }
}
```

### Step 2: Register Options with DI

Implement the `RegisterFixture` method to register the options:

> [!TIP]
> The `IFixtureRegistrar` can be implemented on any public type including a fixture class.

```csharp
[Fixture]
public sealed class TmpDirectoryFixture : IFixtureRegistrar
{
    /// <summary>
    /// Registers TmpDirectoryFixture configuration options with the service collection,
    /// binding from the TmpDirectoryFixture configuration section.
    /// </summary>
    public static void RegisterFixture(IServiceCollection services)
    {
        services
            .AddOptions<Options>()
            .BindConfiguration(nameof(TmpDirectoryFixture))
            ;
    }
}
```

This enables:
- Binding from configuration sections (e.g., environment variables with `__` separator)
- Type-safe access via `IOptions<Options>`

### Step 3: Inject and Use Options

Inject `IOptions<Options>` into the fixture constructor:

```csharp
private readonly Options _opts;
public string Path { get; }

public TmpDirectoryFixture(IOptions<Options> opts)
{
    _opts = opts.Value;
    Path = Directory.CreateTempSubdirectory(_opts.Prefix).FullName;
}

public void Dispose()
{
    if (_opts.DisposeType != DisposeType.Delete)
        return;

    try
    {
        Directory.Delete(Path, true);
    }
    catch (DirectoryNotFoundException)
    {
    }
}
```

## Configuration Methods

### Environment Variables

Use environment variables with the `__` (double underscore) separator for hierarchy:

```bash
# Windows
set TmpDirectoryFixture__DisposeType=Skip
set TmpDirectoryFixture__Prefix=mytest_

# Linux/macOS
export TmpDirectoryFixture__DisposeType=Skip
export TmpDirectoryFixture__Prefix=mytest_
```

### Programmatic Configuration

Configure options directly in code during fixture manager setup:

```csharp
var manager = new FixtureManagerBuilder()
    .ConfigureServices(services =>
    {
        services.Configure<TmpDirectoryFixture.Options>(options =>
        {
            options.DisposeType = TmpDirectoryFixture.DisposeType.Skip;
            options.Prefix = "custom_";
        });
    })
    .Build();
```

## Best Practices

1. **Provide sensible defaults** - Always set default values in the `Options` class properties
2. **Use clear naming** - Prefix option properties with the fixture name in configuration
3. **Document all options** - Include XML documentation comments for IntelliSense support
4. **Validate configuration** - Consider adding validation logic in the fixture constructor
5. **Make options optional** - Allow fixtures to work without explicit configuration

## See Also

| Resource | Description |
|----------|-------------|
| [TmpDirectoryFixture.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures/Fixtures/TmpDirectoryFixture.cs) | Complete source code example |
| [TmpDirectoryFixtureTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/Fixtures/TmpDirectoryFixtureTests.cs) | Unit tests for TmpDirectoryFixture |
| [OptionsConfigurationTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/EngineTests/OptionsConfigurationTests.cs) | Tests for options configuration patterns |
| [Parameterization Pattern](parameterization-pattern.md) | Alternative generic type-based configuration |
| [Selecting Configuration Method](selecting-configuration-method.md) | Comparison of configuration approaches |
