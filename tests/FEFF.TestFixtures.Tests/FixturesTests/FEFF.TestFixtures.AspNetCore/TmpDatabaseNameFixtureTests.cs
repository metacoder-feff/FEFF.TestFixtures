using System.Data.Common;
using FEFF.TestFixtures.Engine;
using FEFF.TestFixtures.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class TmpDatabaseNameFixtureTests
{
    [Fixture]
    internal class TestingFxOptions : ITmpDatabaseNameFixtureOptions
    {
        public IReadOnlyCollection<string> ConnectionStringNames => [Program.ConnectionStringName/*, cs222*/];
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Fixture__should_make_api_to_respond_with_replaced_db_name__when(bool replace)
    {
        var client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

        if (replace == true)
            _ = TestContext.Current.GetFeffFixture<TmpDatabaseNameFixture<Program, TestingFxOptions>>();

        var resp = await client.LazyValue.GetAsync("/db-info", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        // {
        //     "name": "postgres",
        // }
        var name = JToken.Parse(body)["name"]?.Value<string>();

        if (replace == false)
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
        _ = s1.GetFixture<TmpDatabaseNameFixture<Program, TestingFxOptions>>();
        _ = s2.GetFixture<TmpDatabaseNameFixture<Program, TestingFxOptions>>();

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