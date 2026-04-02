using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class BasicApplicationFixtureTests
{
    protected ITestApplicationFixture App = TestContext.Current.GetFeffFixture<TestApplicationFixture<Program>>();

    [Fact]
    public async Task Api__should_respond()
    {
        /// fixture starts and creates clients
        /// user has to dispose these clients manually
        /// <see cref="AppClientFixture{}"/> for automation.
        using var client = App.LazyApplication.CreateClient();

        var resp = await client.GetAsync("/weatherforecast/const", TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        JToken.Parse(body)
            .Should().BeEquivalentTo(
            """
            [
                {
                    "date": "2000-01-01",
                    "temperatureC": 20,
                    "summary": "normal"
                }
            ]
            """);
    }

    [Fact]
    public async Task AppServices_should_be_resolved()
    {
        /// fixture starts app and creates serviceScope
        /// user has to dispose the serviceScope manually
        /// <see cref="AppServicesFixture"/> for automation.
        using var serviceScope = App.LazyApplication.Services.CreateScope();

        var svc = serviceScope.ServiceProvider.GetRequiredService<SomeService>();

        svc.Data.Should().Be("123");
    }
}