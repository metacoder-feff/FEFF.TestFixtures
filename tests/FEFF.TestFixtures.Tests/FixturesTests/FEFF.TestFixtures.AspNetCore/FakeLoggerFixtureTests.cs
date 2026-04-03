using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class FakeLoggerFixtureTests
{
    protected IAppClientFixture Client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    // FakeLoggerFixture is injected into TestApplicationFixture.ApplicationBuilder
    protected FakeLoggerFixture FakeLoggerFx = TestContext.Current.GetFeffFixture<FakeLoggerFixture<Program>>();

    [Fact]
    public async Task Fixture__should_collect_logs()
    {
        var resp = await Client.LazyValue.PostAsync("/logging", null, TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var snap = FakeLoggerFx.Collector
            .GetSnapshot()
            // .Where(x => x.Category == "WebApiTestSubject.Program.WeatherForecast")
            ;
//TODO: Enum serialization
        JToken.FromObject(snap)
            .Should().ContainSubtree(
            """
            [
            {
                "Level": 2,
                "Exception": null,
                "Message": "log-1",
                "Scopes": [
                [
                    {
                    "Key": "RequestId",
                    },
                    {
                    "Key": "RequestPath",
                    "Value": "/logging"
                    }
                ],
                "message-scope-1"
                ],
                "Category": "WebApiTestSubject.SomeService",
                "LevelEnabled": true,
                // "Timestamp": "2000-01-01T01:01:01.2953625+00:00"
            },
            {
                "Level": 3,
                "Exception": null,
                "Message": "log-2",
                "Scopes": [
                //...
                ],
                "Category": "WebApiTestSubject.SomeService",
                "LevelEnabled": true,
                // "Timestamp": "2000-01-01T01:01:01.2953625+00:00"
            }
            ]
            """);
    }
}