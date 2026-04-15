using FEFF.TestFixtures.AspNetCore.Randomness;
using FEFF.TestFixtures.AspNetCore.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Preview.Tests;

[Fixture]
internal class FixtureOptions : ITmpDatabaseNameFixtureOptions
{
    public IReadOnlyCollection<string> ConnectionStringNames => [Program.ConnectionStringName/*, cs222*/];
}

internal interface IAppFixture
{
    FakeTimeProvider Time { get; }
    FakeRandom Random { get; }
    IAppConfigurator Config { get; }
    HttpClient LazyClient { get; }
    IServiceProvider LazyServices { get; }
    ApplicationDbContext LazyDbCtx { get; }
    IDatabaseLifecycleFixture EnsureFx { get; }
}

[Fixture]
internal class AppTestFixtureV1 : IAppFixture
{
    private readonly AppManagerFixture<Program> appFx;
    private readonly AppClientFixture<Program> clientFx;
    private readonly AppServicesFixture<Program> servicesFx;
    private readonly FakeTimeFixture<Program> timeFx;
    private readonly FakeRandomFixture<Program> randomFx;

    public IDatabaseLifecycleFixture EnsureFx { get; }

    public AppTestFixtureV1(
        AppManagerFixture<Program> appFx,
        AppClientFixture<Program> clientFx,
        AppServicesFixture<Program> servicesFx,
        FakeTimeFixture<Program> timeFx,
        FakeRandomFixture<Program> randomFx,
        DatabaseLifecycleFixture<Program, ApplicationDbContext> ensureCtxFx,
        // this fixture is not directly used
        // it only auto attaches itself to 'appFx' when it is constructed
        TmpDatabaseNameFixture<Program, FixtureOptions> tmpDb
)
    {
        this.appFx = appFx;
        this.clientFx = clientFx;
        this.servicesFx = servicesFx;
        this.timeFx = timeFx;
        this.randomFx = randomFx;
        EnsureFx = ensureCtxFx;
    }

    public FakeRandom Random => randomFx.Value;
    public FakeTimeProvider Time => timeFx.Value;
    public IAppConfigurator Config => appFx.ConfigurationBuilder;
    public HttpClient LazyClient => clientFx.LazyValue;
    public IServiceProvider LazyServices => servicesFx.LazyServiceProvider;
    public ApplicationDbContext LazyDbCtx => servicesFx.LazyServiceProvider.GetRequiredService<ApplicationDbContext>();
}

[Fixture]
internal record AppTestFixtureV2(
        AppManagerFixture<Program> AppFx,
        AppClientFixture<Program> ClientFx,
        AppServicesFixture<Program> ServicesFx,
        FakeTimeFixture<Program> TimeFx,
        FakeRandomFixture<Program> RandomFx,
        DatabaseLifecycleFixture<Program, ApplicationDbContext> EnsureFx,
        // this fixture is not directly used
        // it only auto attaches itself to 'appFx' when it is constructed
        TmpDatabaseNameFixture<Program, FixtureOptions> TmpDb
) : IAppFixture
{
    public FakeRandom Random => RandomFx.Value;
    public FakeTimeProvider Time => TimeFx.Value;
    public IAppConfigurator Config => AppFx.ConfigurationBuilder;
    public HttpClient LazyClient => ClientFx.LazyValue;
    public IServiceProvider LazyServices => ServicesFx.LazyServiceProvider;
    public ApplicationDbContext LazyDbCtx => ServicesFx.LazyServiceProvider.GetRequiredService<ApplicationDbContext>();
    IDatabaseLifecycleFixture IAppFixture.EnsureFx => EnsureFx;
}


[Fixture]
internal class AppTestFixtureV3(
        AppManagerFixture<Program> appFx,
        AppClientFixture<Program> clientFx,
        AppServicesFixture<Program> servicesFx,
        FakeTimeFixture<Program> timeFx,
        FakeRandomFixture<Program> randomFx,
        DatabaseLifecycleFixture<Program, ApplicationDbContext> ensureFx,
#pragma warning disable CS9113
        // this fixture is not directly used
        // it only auto attaches itself to 'appFx' when it is constructed
        TmpDatabaseNameFixture<Program, FixtureOptions> tmpDb
#pragma warning restore CS9113
) : IAppFixture
{
    public FakeRandom Random => randomFx.Value;
    public FakeTimeProvider Time => timeFx.Value;
    public IAppConfigurator Config => appFx.ConfigurationBuilder;
    public HttpClient LazyClient => clientFx.LazyValue;
    public IServiceProvider LazyServices => servicesFx.LazyServiceProvider;
    public ApplicationDbContext LazyDbCtx => servicesFx.LazyServiceProvider.GetRequiredService<ApplicationDbContext>();
    public IDatabaseLifecycleFixture EnsureFx => ensureFx;
}

public class FullExampleTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task User__should_be_created(int arg)
    {
        var AppFx = arg switch
        {
            1 => (IAppFixture)TestContext.Current.GetFeffFixture<AppTestFixtureV1>(),
            2 => (IAppFixture)TestContext.Current.GetFeffFixture<AppTestFixtureV2>(),
            3 => (IAppFixture)TestContext.Current.GetFeffFixture<AppTestFixtureV3>(),
            _ => throw new InvalidOperationException(),
        };

        await AppFx.EnsureFx.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var resp = await AppFx.LazyClient.PostAsync("/user", null, TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var users = await AppFx.LazyDbCtx.Users.ToListAsync(TestContext.Current.CancellationToken);
        JToken.FromObject(users)
            .Should().BeEquivalentTo("""
            [
                {
                    "Id": 1,
                    "Name": "test",
                    "Age": 100
                }
            ]
            """);
    }
}
