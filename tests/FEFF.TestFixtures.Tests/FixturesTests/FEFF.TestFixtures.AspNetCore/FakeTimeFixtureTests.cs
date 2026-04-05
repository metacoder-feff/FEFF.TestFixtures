using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class FakeTimeFixtureTests
{
    protected IAppClientFixture Client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    // FakeTimeFixture is injected into TestApplicationFixture.ApplicationBuilder
    protected FakeTimeFixture<Program> FakeTimeFx = TestContext.Current.GetFeffFixture<FakeTimeFixture<Program>>();
    protected FakeTimeProvider FakeTime => FakeTimeFx.Value;

    [Theory]
    [InlineData("2006-01-05")]
    [InlineData("2150-11-15")]
    public async Task Fixture__should_make_api_to_respond__with(string date)
    {
        // FakeTime singleton object can be updated at any moment of test
        FakeTime.SetUtcNow(DateTimeOffset.Parse($"{date}T05:05:05Z"));

        var resp = await Client.LazyValue.GetAsync("/weatherforecast/time", TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        JToken.Parse(body)
            .Should().BeEquivalentTo(
            $$"""
            [
                {
                    "date": "{{date}}",
                    "temperatureC": 20,
                    "summary": "normal"
                }
            ]
            """);
    }
}