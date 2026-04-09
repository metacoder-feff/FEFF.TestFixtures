using FEFF.Extensions.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

internal static class Exxx
{
}

internal interface IAppFixture
{
    FakeTimeProvider Time { get; }
    FakeRandom Random { get; }
    IApplicationConfigurator Config { get; }
    HttpClient LazyClient { get; }
    IServiceProvider LazyServices { get; }
    ApplicationDbContext LazyDbCtx  { get; }
}

[Fixture]
internal class AppTestFixtureV1 : IAppFixture
{
    private readonly TestApplicationFixture<Program> appFx;
    private readonly AppClientFixture<Program> clientFx;
    private readonly AppServicesFixture<Program> servicessFx;
    private readonly FakeTimeFixture<Program> timeFx;
    private readonly FakeRandomFixture<Program> randomFx;

    public AppTestFixtureV1(
        TestApplicationFixture<Program> appFx,
        AppClientFixture<Program> clientFx,
        AppServicesFixture<Program> servicessFx,
        FakeTimeFixture<Program> timeFx,
        FakeRandomFixture<Program> randomFx,
        TmpScopeIdFixture scopeIdFx,
        // this fixture is not directly used
        // it only auto attaches itself to 'appFx' when it is constructed
        EnsureDbContextFixture<Program, ApplicationDbContext> ensureCtxFx
)
    {
        this.appFx = appFx;
        this.clientFx = clientFx;
        this.servicessFx = servicessFx;
        this.timeFx = timeFx;
        this.randomFx = randomFx;
        appFx.Configuration.UseTmpDatabaseName(scopeIdFx, Program.ConnectionStringName/*, cs222*/);
    }

    public FakeRandom Random => randomFx.Value;
    public FakeTimeProvider Time => timeFx.Value;
    public IApplicationConfigurator Config => appFx.Configuration;
    public HttpClient LazyClient => clientFx.LazyValue;
    public IServiceProvider LazyServices => servicessFx.LazyServiceProvider;
    public ApplicationDbContext LazyDbCtx => servicessFx.LazyServiceProvider.GetRequiredService<ApplicationDbContext>();
}

[Fixture]
internal class AppTestFixtureV2: IAppFixture
{
    public FakeTimeProvider Time { get; } = new();
    public FakeRandom Random { get; } = new();
    public IApplicationConfigurator Config => appFx.Configuration;
    public HttpClient LazyClient => clientFx.LazyValue;
    public IServiceProvider LazyServices => servicessFx.LazyServiceProvider;
    public ApplicationDbContext LazyDbCtx => servicessFx.LazyServiceProvider.GetRequiredService<ApplicationDbContext>();

    private readonly TestApplicationFixture<Program> appFx;
    private readonly AppClientFixture<Program> clientFx;
    private readonly AppServicesFixture<Program> servicessFx;

    public AppTestFixtureV2(
        TestApplicationFixture<Program> appFx,
        AppClientFixture<Program> clientFx,
        AppServicesFixture<Program> servicessFx,
        TmpScopeIdFixture scopeIdFx,
        // this fixture is not directly used
        // it only auto attaches itself to 'appFx' when it is constructed
        EnsureDbContextFixture<Program, ApplicationDbContext> ensureCtxFx
    )
    {
        this.appFx = appFx;
        this.clientFx = clientFx;
        this.servicessFx = servicessFx;
        
        appFx.Configuration.UseRandom(Random);
        appFx.Configuration.UseTimeProvider(Time);
        appFx.Configuration.UseTmpDatabaseName(scopeIdFx, Program.ConnectionStringName/*, cs222*/);
    }
}

public class FullExampleTests
{
    // internal AppTestFixtureV1 AppFx { get; } = TestContext.Current.GetFeffFixture<AppTestFixtureV1>();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task User__should_be_created(int arg)
    {
        var AppFx = arg switch
        {
            1 =>  (IAppFixture) TestContext.Current.GetFeffFixture<AppTestFixtureV1>(),
            2 =>  (IAppFixture) TestContext.Current.GetFeffFixture<AppTestFixtureV2>(),
            _ => throw new InvalidOperationException(),
        };

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