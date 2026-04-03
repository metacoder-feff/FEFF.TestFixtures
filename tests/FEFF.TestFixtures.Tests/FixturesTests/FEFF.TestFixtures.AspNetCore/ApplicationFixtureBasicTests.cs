using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class ApplicationFixtureBasicTests
{
    protected ITestApplicationFixture App = TestContext.Current.GetFeffFixture<TestApplicationFixture<Program>>();

    [Fact]
    public async Task Api__should_respond()
    {
        /// The fixture starts the app and creates clients.
        /// The user has to dispose these clients manually.
        /// See <see cref="AppClientFixture{}"/> for automation.
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
        /// The fixture starts the app and creates a service scope.
        /// The user has to dispose the service scope manually.
        /// See <see cref="AppServicesFixture"/> for automation.
        using var serviceScope = App.LazyApplication.Services.CreateScope();

        var svc = serviceScope.ServiceProvider.GetRequiredService<SomeService>();

        svc.Data.Should().Be("123");
    }
}