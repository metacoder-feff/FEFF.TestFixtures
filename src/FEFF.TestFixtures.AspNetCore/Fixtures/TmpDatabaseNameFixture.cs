namespace FEFF.TestFixtures.AspNetCore;

public interface ITmpDatabaseNameFixtureOptions
{
    IReadOnlyCollection<string> ConnectionStringNames { get; }
}

[Fixture]
public class TmpDatabaseNameFixture<TEntryPoint, TOptionsFixture>
where TEntryPoint : class
where TOptionsFixture : ITmpDatabaseNameFixtureOptions
{
    public TmpDatabaseNameFixture(AppManagerFixture<TEntryPoint> app, TmpScopeIdFixture testId, TOptionsFixture opts)
    {
        ThrowHelper.Argument.ThrowIfNullOrEmpty(opts.ConnectionStringNames);

        app.ConfigurationBuilder.UseDatabaseNamePostfix($"test-{testId.Value}", opts.ConnectionStringNames);
    }
}