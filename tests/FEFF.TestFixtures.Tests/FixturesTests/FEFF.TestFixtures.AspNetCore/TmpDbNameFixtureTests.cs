using System.Data.Common;
using FEFF.Extensions.Testing;
using FEFF.TestFixtures.Engine;
using FEFF.TestFixtures.Tests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

//preview
[Fixture]
internal class MyTmpDbNameFixture : TmpDbNameFixtureBase
{
    public MyTmpDbNameFixture(TestApplicationFixture<Program> app, TmpScopeIdFixture testId)
    : base(app, testId, Program.ConnectionStringName)
    {
    }
}

[Fixture]
internal class MyAppFixture(
    TestApplicationFixture<Program> app,
    AppClientFixture<Program> client,
    FakeRandomFixture<Program> fakeRandomFx,
    FakeTimeFixture<Program> fakeTimeFx,
#pragma warning disable CS9113 // Parameter 'tmpDbName' is unread.
    // just construct the fixture
    MyTmpDbNameFixture tmpDbName
#pragma warning restore CS9113 // Parameter 'tmpDbName' is unread.
    )
{
    IApplicationConfigurator Configuration => app.Configuration;
    FakeRandom Random => fakeRandomFx.Value; 
    FakeTimeProvider Time => fakeTimeFx.Value;
    HttpClient LazyClient => client.LazyValue;
}

// internal static class TmpDbNameFixtureExtensions
// {
//     public static void SetDbNamePostfix(ITestApplicationFixture app, string connectionStringName, string dbPostfix)
//     {
//         ArgumentException.ThrowIfNullOrEmpty(connectionStringName);
//         ArgumentException.ThrowIfNullOrEmpty(dbPostfix);
//     }
// }

internal class TmpDbNameFixtureBase
{
    private readonly string _testTag;
//TODO: list    
    private readonly string _connectionStringName;
    private string? _oldCs;
    private string? _newCs;

    public TmpDbNameFixtureBase(ITestApplicationFixture app, TmpScopeIdFixture testId, string connectionStringName)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionStringName);

        _testTag = testId.Value;
        _connectionStringName = Program.ConnectionStringName;

        app.Configuration.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(WebHostBuilderContext ctx, IServiceCollection _)
    {
        var config = (ConfigurationManager)ctx.Configuration;
        var key = "ConnectionStrings:" + _connectionStringName;
        ChangeDbName(config, key);
    }
    
    private void ChangeDbName(ConfigurationManager config, string key)
    {
        var cs = config[key];
        var csb = new DbConnectionStringBuilder
        {
            ConnectionString = cs
        };
        csb["Database"] = $"{csb["Database"]}-test-{_testTag}";
        var newCs = csb.ConnectionString;
        config[key] = newCs;

        _oldCs = cs;
        _newCs = newCs;
    }
}

public class TmpDbNameFixtureTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Fixture__should_make_api_to_respond_with_replaced_db_name__when(bool replace)
    {
        var client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

        if(replace == true)
            _ = TestContext.Current.GetFeffFixture<MyTmpDbNameFixture>();

        var resp = await client.LazyValue.GetAsync("/db-info", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        // {
        //     "name": "postgres",
        // }
        var name = JToken.Parse(body)["name"]?.Value<string>();

        if(replace == false)
            name.Should().Be("postgres");
        else
        {
            name.Should()
                .NotBe("postgres")
                .And
                .NotBeNullOrEmpty()
                ;
        }
    }

    [Fact]
    public void DbName__should_be_unique_for_each_scope()
    {
        var fm = TestContext.Current.GetFeffFixture<FixtureHelper>();

        var s0 = fm.Scope;
        var s1 = fm.FixtureManager.GetScope("1");
        var s2 = fm.FixtureManager.GetScope("3");

        // name in s0 stays unchanged
        _ = s1.GetFixture<MyTmpDbNameFixture>();
        _ = s2.GetFixture<MyTmpDbNameFixture>();

        var name0 = GetDbName(s0);
        var name1 = GetDbName(s1);
        var name2 = GetDbName(s2);

        name0.Should().Be("postgres");

        name1.Should()
            .NotBe(name0)
            .And
            .NotBeNullOrEmpty()
            ;

        name2.Should()
            .NotBe(name0)
            .And
            .NotBe(name1)
            .And
            .NotBeNullOrEmpty()
            ;
    }

    private static string GetDbName(IFixtureScope scope)
    {
        var connString = scope
            .GetFixture<AppServicesFixture<Program>>()
            .LazyServiceProvider
            .GetRequiredService<IConfiguration>()
            .GetConnectionString(Program.ConnectionStringName)
            ;

        var csb = new DbConnectionStringBuilder
        {
            ConnectionString = connString
        };

        return (string)csb["Database"];
    }
}