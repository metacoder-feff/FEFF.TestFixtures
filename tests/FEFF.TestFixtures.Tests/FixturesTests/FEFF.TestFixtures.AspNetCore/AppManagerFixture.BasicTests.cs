using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class AppManagerFixtureBasicTests
{
    protected IAppManagerFixture App = TestContext.Current.GetFeffFixture<AppManagerFixture<Program>>();

    [Fact]
    public async Task Fixture__after_access_to_LazyApplication__should_be_started()
    {
        // Pre-assert
        App.IsStarted.Should().BeFalse();
        
        // Act
        _ = App.LazyApplication;
        
        // Assert
        App.IsStarted.Should().BeTrue();
    }

    [Fact]
    public async Task Client__should__be_created_and_get_response()
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
}