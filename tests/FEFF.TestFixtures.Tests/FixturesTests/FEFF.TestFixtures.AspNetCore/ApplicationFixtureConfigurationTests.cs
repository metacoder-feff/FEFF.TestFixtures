using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class ApplicationFixtureConfigurationTests
{
    protected ITestApplicationFixture App = TestContext.Current.GetFeffFixture<TestApplicationFixture<Program>>();
    protected IAppClientFixture Client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("cloudy")]
    [InlineData("warm")]
    public async Task SetEnvVar__should_make_api_to_respond_with(string? envVarValue)
    {
        // change TestApp before it starts
        if(envVarValue != null)
            App.Configuration.UseSetting("summary", envVarValue);

        var resp = await Client.LazyValue.GetAsync("/weatherforecast/env", TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // null or string
        var summaryJson = JsonSerializer.Serialize(envVarValue);
        JToken.Parse(body)
            .Should().BeEquivalentTo(
            $$"""
            [
                {
                    "date": "2000-01-01",
                    "temperatureC": 20,
                    "summary": {{summaryJson}}
                }
            ]
            """);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(AspEnvironment.Development)]
    [InlineData(AspEnvironment.Production)]
    public async Task SetAspEnv__should_make_api_to_respond_with(AspEnvironment? envVarValue)
    {
        // change TestApp before it starts
        if(envVarValue != null)
            App.Configuration.UseAspEnvironment(envVarValue.Value);

        var resp = await Client.LazyValue.GetAsync("/asp-env", TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        if(envVarValue == AspEnvironment.Production)
        {
            JToken.Parse(body)
                .Should().BeEquivalentTo(
                """
                {
                    "applicationName": "WebApiTestSubject",
                    "environmentName": "Production",
                    "isDevelopment": false
                }
                """);
        }
        else
        {
            JToken.Parse(body)
                .Should().BeEquivalentTo(
                """
                {
                    "applicationName": "WebApiTestSubject",
                    "environmentName": "Development",
                    "isDevelopment": true
                }
                """);
        }
    }

    [Fact]
    public void Configuration_attempt__after_app_is_built__should_throw()
    {
        // Build an applixation
        _ = App.LazyApplication;

        var act = () => App.Configuration.UseAspEnvironment(AspEnvironment.Production);
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Can't use 'IApplicationConfigurator' after application is created.")
            ;
    }
}