using FEFF.TestFixtures.AspNetCore.Randomness;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class FakeRandomFixtureTests
{
    protected IAppClientFixture Client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    // FakeRandomFixture is injected into AppManagerFixture.ConfigurationBuilder
    protected FakeRandomFixture<Program> FakeRandomFx = TestContext.Current.GetFeffFixture<FakeRandomFixture<Program>>();
    protected FakeRandom FakeRandom => FakeRandomFx.Value;

    [Theory]
    [InlineData(11)]
    [InlineData(22)]
    public async Task Fixture__should_make_api_to_respond__with(int randValue)
    {
        // FakeRandom singleton object can be updated at any moment of test
        FakeRandom.Int32Next = FixedNextStrategy.From(randValue);

        var resp = await Client.LazyValue.GetAsync("/weatherforecast/random", TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        JToken.Parse(body)
            .Should().BeEquivalentTo(
            $$"""
            [
                {
                    "date": "2000-01-01",
                    "temperatureC": {{randValue}},
                    "summary": "normal"
                }
            ]
            """);
    }
}
