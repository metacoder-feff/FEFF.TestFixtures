using FEFF.Extensions.Testing;
using FEFF.TestFixtures.Engine.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

// removes postgres-Db without EfCore.DbContext
internal class PgDbRemover : IAsyncDisposable
{
    private IWebApplicationFactory _app;
    private string _connectionStringName;
//TODO: remove IWebApplicationFactory
    public PgDbRemover(IWebApplicationFactory app, string connectionStringName)
    {
        _app = app;
        _connectionStringName = connectionStringName;
    }

    public async ValueTask DisposeAsync()
    {
        await PgRemoveDatabaseAsync(_app, _connectionStringName);
    }

    private async Task PgRemoveDatabaseAsync(IWebApplicationFactory app, string connectionStringName)
    {
        var config = app.Services.GetRequiredService<IConfiguration>();
        //TODO: GetRequiredConnectionString
        var cs = ThrowHelper.EnsureNotNull(
            config.GetConnectionString(connectionStringName)
        );
        await EnsureDeletedAsync(cs);
    }

    private static async Task EnsureDeletedAsync(string connectionString)
    {
        var csb = new NpgsqlConnectionStringBuilder(connectionString);
        var dbName = ThrowHelper.EnsureNotNull(csb.Database);

        // CreateAdminConnection
        //     Database = npgsqlOptions.AdminDatabase ?? "postgres",
        csb.Database = "postgres";
        csb.Pooling = false;
        csb.Multiplexing = false;

        using var conn = new NpgsqlConnection(csb.ToString());
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{dbName}\" WITH (FORCE);", conn);
        await cmd.ExecuteNonQueryAsync();

        // using (var masterConnection = connection.CreateAdminConnection())
        // {
        //     Dependencies.MigrationCommandExecutor
        //         .ExecuteNonQuery(CreateDropCommands(), masterConnection);
        // }
    }
}

// removes Db using DbContext.Database.EnsureDeleted()
internal sealed class CtxDbRemover<T> : IAsyncDisposable
where T: DbContext
{
    private readonly T _ctx;

    public CtxDbRemover(T ctx)
    {
        _ctx = ctx;
    }

    public async ValueTask DisposeAsync()
    {
        await _ctx.Database.EnsureDeletedAsync();
    }
}

public class AspPipelineTests
{
    // Fixtures should provide data/services and behaviour for tests:
    internal record FixtureData(
        FakeTimeProvider Time, 
        FakeRandom Rand, 
        FakeLogCollector Collector, 
        IServiceProvider Sp, 
        ApplicationDbContext Ctx, 
        HttpClient Client
    );
    // Fixtures should provide only behaviour (no-data) for tests:
    // (order of fixtures matter)
    // appBuilder.UseDatabaseNamePostfix
    // CtxDbRemover
    // ctx.Database.EnsureCreatedAsync
    // SeedDBAsync

    [Fact]
    public async Task Typical_test_pipeline_without_asp_fixtures()
    {
        var scopeIdFx = TestContext.Current.GetFeffFixture<TmpScopeIdFixture>();

        var time = new FakeTimeProvider();
        var rand = new FakeRandom();
        using var logP = new FakeLoggerProvider();

        var appBuilder = new TestApplicationBuilder<Program>();
        appBuilder.UseTimeProvider(time);
        appBuilder.UseRandom(rand);
        appBuilder.UseLoggerProvider(logP);
        appBuilder.UseTmpDatabaseName(scopeIdFx, Program.ConnectionStringName/*, cs222*/);

        await using var app = appBuilder.Build();
        app.StartServer();

        // await using var _ = new PgDbRemover(app, Program.ConnectionStringName);

        using var client = app.CreateClient();
        await using var scope = app.Services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var ctx = sp.GetRequiredService<ApplicationDbContext>();
        await using var ctxDbRemover = new CtxDbRemover<ApplicationDbContext>(ctx);

        // await ctx.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        await ctx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        await SeedDBAsync(TestContext.Current.CancellationToken);

        var fd = new FixtureData(time, rand, logP.Collector, sp, ctx, client);
        await RunTest(fd);

//TODO: add ability to 
// reconfigure appBuilder at start of test
    }

    [Fact]
    public async Task With_disposables()
    {
        var scopeIdFx = TestContext.Current.GetFeffFixture<TmpScopeIdFixture>();
        var appBuilder = new TestApplicationBuilder<Program>();

        var disposables = new List<object>();
        var dispTesting = new DisposableFixture();
        disposables.Add(dispTesting);
        dispTesting.IsDisposed.Should().BeFalse();

        var time = new FakeTimeProvider();
        appBuilder.UseTimeProvider(time);

        var rand = new FakeRandom();
        appBuilder.UseRandom(rand);

        var logP = new FakeLoggerProvider();
        disposables.Add(logP);
        appBuilder.UseLoggerProvider(logP);

        appBuilder.UseTmpDatabaseName(scopeIdFx, Program.ConnectionStringName/*, cs222*/);

        var app = appBuilder.Build();
        disposables.Add(app);

        app.StartServer();

        // await using var _ = new PgDbRemover(app, Program.ConnectionStringName);

        var client = app.CreateClient();
        disposables.Add(client);

        var scope = app.Services.CreateAsyncScope();
        disposables.Add(scope);

        //!!! auto resolve scope ???
        var sp = scope.ServiceProvider;
        var ctx = sp.GetRequiredService<ApplicationDbContext>();
        var ctxDbRemover = new CtxDbRemover<ApplicationDbContext>(ctx);
        disposables.Add(ctxDbRemover);

        // await ctx.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        await ctx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        await SeedDBAsync(TestContext.Current.CancellationToken);

        var fd = new FixtureData(time, rand, logP.Collector, sp, ctx, client);
        disposables.Reverse();
        try
        {
            await RunTest(fd);
        }
        finally
        {
            await DisposeHelper.DisposeAsync(disposables);
        }

        dispTesting.IsDisposed.Should().BeTrue();

//TODO: add ability to 
// reconfigure appBuilder at start of test
    }

    private async Task SeedDBAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1);
    }

    private async Task RunTest(FixtureData fd)
    {
        var resp = await fd.Client.PostAsync("/user", null, TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
    }
}