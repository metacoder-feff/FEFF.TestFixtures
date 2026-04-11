namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Contract for configuration options for <see cref="TmpDatabaseNameFixture{TEntryPoint, TOptionsFixture}"/>.
/// </summary>
public interface ITmpDatabaseNameFixtureOptions
{
    /// <summary>
    /// Gets the names of connection strings that should be patched with unique database names.
    /// </summary>
    IReadOnlyCollection<string> ConnectionStringNames { get; }
}

/// <summary>
/// A fixture that appends a unique test identifier to connection string database names,
/// ensuring each test runs against an isolated database.
/// </summary>
/// <typeparam name="TEntryPoint">The application entry point type.</typeparam>
/// <typeparam name="TOptionsFixture">The options type implementing <see cref="ITmpDatabaseNameFixtureOptions"/>.</typeparam>
[Fixture]
public class TmpDatabaseNameFixture<TEntryPoint, TOptionsFixture>
where TEntryPoint : class
where TOptionsFixture : ITmpDatabaseNameFixtureOptions
{
    /// <summary>
    /// Creates a new <see cref="TmpDatabaseNameFixture{TEntryPoint, TOptionsFixture}"/> and configures
    /// the application to use a unique database name for the test.
    /// </summary>
    /// <param name="app">The application manager fixture.</param>
    /// <param name="testId">The unique test scope identifier.</param>
    /// <param name="opts">The database name fixture options.</param>
    public TmpDatabaseNameFixture(AppManagerFixture<TEntryPoint> app, TmpScopeIdFixture testId, TOptionsFixture opts)
    {
        ThrowHelper.Argument.ThrowIfNullOrEmpty(opts.ConnectionStringNames);

        app.ConfigurationBuilder.UseDatabaseNamePostfix($"test-{testId.Value}", opts.ConnectionStringNames);
    }
}