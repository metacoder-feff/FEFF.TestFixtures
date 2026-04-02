using System.Text.Json;
using FEFF.Extentions.Testing;
using FEFF.Extentions.Testing.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
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

public class AppClientFixtureTests
{
    /// <remarks>
    /// fixture can be requested in initializer
    /// application starts on first <see cref="AppClientFixture.Value"/> access.
    /// fixture is disposed automatically
    /// </remarks>
    protected IAppClientFixture Client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    [Fact]
    public async Task Api__should_respond()
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

public class ConfiguredApplicationFixtureTests
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
//TODO: rename App.Configuration
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

public class FakeRandomFixtureTests
{
    protected IAppClientFixture Client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    // FakeRandomFixture is injected into TestApplicationFixture.ApplicationBuilder
    protected FakeRandomFixture FakeRandomFx = TestContext.Current.GetFeffFixture<FakeRandomFixture>();
    protected FakeRandom FakeRandom => FakeRandomFx.Value;

    [Theory]
    [InlineData(11)]
    [InlineData(22)]
    public async Task FakeRandomFixture__should_make_api_to_respond_with(int randValue)
    {
        // FakeRandom singletone object can be updated at any moment of test
        FakeRandom.IntStrategy = FakeRandom.ConstStrategy(randValue);

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

public class FakeTimeFixtureTests
{
    protected IAppClientFixture Client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    // FakeTimeFixture is injected into TestApplicationFixture.ApplicationBuilder
    protected FakeTimeFixture FakeTimeFx = TestContext.Current.GetFeffFixture<FakeTimeFixture>();
    protected FakeTimeProvider FakeTime => FakeTimeFx.Value;

    [Theory]
    [InlineData("2006-01-05")]
    [InlineData("2150-11-15")]
    public async Task FakeTimeFixture__should_make_api_to_respond_with(string date)
    {
        // FakeTime singletone object can be updated at any moment of test
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

public class FakeLoggerFixtureTests
{
    protected IAppClientFixture Client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    // FakeLoggerFixture is injected into TestApplicationFixture.ApplicationBuilder
    protected FakeLoggerFixture FakeLoggerFx = TestContext.Current.GetFeffFixture<FakeLoggerFixture>();

    [Fact]
    public async Task FakeLoggerFixture__should_collect_logs()
    {
        var resp = await Client.LazyValue.PostAsync("/logging", null, TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var snap = FakeLoggerFx.Collector
            .GetSnapshot()
            // .Where(x => x.Category == "WebApiTestSubject.Program.WeatherForecast")
            ;

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


public class AppServicesFixtureTests
{
    protected IAppServicesFixture ServicesFx = TestContext.Current.GetFeffFixture<AppServicesFixture<Program>>();

    protected FakeLoggerFixture FakeLoggerFx = TestContext.Current.GetFeffFixture<FakeLoggerFixture>();

    [Fact]
    public async Task AppServices_should_be_resolved()
    {
        var svc = ServicesFx.LazyServiceProvider.GetRequiredService<SomeService>();

        svc.Data.Should().Be("123");
    }
}