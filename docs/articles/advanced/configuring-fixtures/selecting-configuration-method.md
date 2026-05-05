# Selecting Configuration Method

The library provides two distinct ways to configure fixtures. Choose based on whether the configuration varies between tests or is controlled externally.

| Aspect | [Parameterization Pattern](parameterization-pattern.md) | [Options Pattern](options-pattern.md) |
|--------|----------------------------------------|---------------------------------------------------------------------|
| **When to use** | Different tests or test suites need different behavior for the same fixture | Global or environment-specific settings (CI, debugging, local overrides) |
| **Value requirement** | Fixture explicitly requires a value | Fixture should have default value |
| **Mechanism** | Generic type parameter + options fixture interface | `Microsoft.Extensions.Options` bound to configuration (e.g., environment variables) |
| **Discovery** | Compile-time via generic constraint | Runtime via `IOptions<T>` and `IFixtureRegistrar` |
| **Flexibility** | Per-test-suite variation by swapping the options fixture type | Global change without recompiling tests |
| **Example scenario** | One test suite uses database `A`, another uses database `B`, via `TmpDatabaseNameFixture<SuiteAOptions>` vs `TmpDatabaseNameFixture<SuiteBOptions>` | Skipping temp directory cleanup on CI by setting `TmpDirectoryFixture__DisposeType=Skip` |

## Decision Guide

- **Use parameterization** when the fixture must behave differently across test classes or collections and the decision is made at design time. This keeps configuration type-safe and visible in test code.
- **Use the options pattern** when you need operators or CI pipelines to adjust behavior without changing source code, or when a setting applies broadly across all tests using that fixture.

The two patterns can coexist: a fixture may accept an options fixture for structural choices (which connection strings to redirect) while also using `IOptions<T>` for operational tuning (cleanup behavior, timeouts).

## See Also

| Resource | Description |
|----------|-------------|
| [Parameterization Pattern](parameterization-pattern.md) | Generic type-based configuration |
| [Options Pattern](options-pattern.md) | Environment variable-based configuration |
