using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class AppClientFixtureTests
{
    /// <remarks>
    /// fixture can be requested in initializer
    /// application starts on first <see cref="AppClientFixture.Value"/> access.
    /// fixture is disposed automatically
    /// </remarks>
    protected IAppClientFixture Client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    [Fact]
    public async Task Client__should__get_response()
    {
        var resp = await Client.LazyValue.GetAsync("/weatherforecast/const", TestContext.Current.CancellationToken);
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