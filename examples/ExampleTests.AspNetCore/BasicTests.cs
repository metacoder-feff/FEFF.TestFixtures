using System.Data.Common;
using System.Net;
using AwesomeAssertions;
using AwesomeAssertions.Json; // Required for proper JSON assertions
using FEFF.TestFixtures.AspNetCore.Randomness;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;

namespace ExampleTests.AspNetCore.Basic;

internal class WebApp : WebApplicationFactory<Program>
{
    public FakeTimeProvider FakeTime { get; } = new();
    public FakeRandom FakeRandom { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        
        builder.ConfigureServices((ctx, _) =>
        {
            ctx.Configuration.AddSuffixToConnectionString("PgDb", Guid.NewGuid().ToString());
        });

        builder.ConfigureServices(services =>
            services.TryReplaceSingleton<TimeProvider>(FakeTime)
        );
        
        builder.ConfigureServices(services =>
            services.TryReplaceSingleton<Random>(FakeRandom)
        );

        builder.UseSetting("summary", "sunny");
    }
}

public sealed class BasicApiTests : IAsyncLifetime
{
    internal WebApp App { get; }
    internal HttpClient Client { get; }
    internal AsyncServiceScope Scope { get; }

    internal FakeTimeProvider AppTime => App.FakeTime;
    internal FakeRandom AppRandom => App.FakeRandom;
    internal ApplicationDbContext AppDbCtx => Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    public BasicApiTests()
    {
        App = new();
        Client = App.CreateClient();

        // application starts here
        Scope = App.Services.CreateAsyncScope();
    }

    public async ValueTask DisposeAsync()
    {
        await AppDbCtx.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        await Scope.DisposeAsync();
        Client.Dispose();
        await App.DisposeAsync();
    }

    public async ValueTask InitializeAsync()
    {
        await AppDbCtx.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }
    #region  tutorial: ASP.NET Core Application Testing

    /// <summary>
    /// Test: POST /weatherforecast/generate creates a forecast using time, random, and env var,
    /// persists it to the database, and GET /weatherforecast/today returns it.
    ///
    /// This test verifies the full integration flow:
    /// 1. Configure fake time, fake random, and environment variable
    /// 2. POST to /weatherforecast/generate
    /// 3. Query the database directly to verify persistence
    /// 4. GET /weatherforecast/today to verify the API returns the persisted record
    /// </summary>
    [Fact]
    public async Task Example_Tutorial_Asp__Api__should_persist_and_return()
    {
        var expectedDate = "2025-06-15";
        var expectedTemperature = 42;
        var expectedSummary = "sunny";

        AppTime.SetUtcNow(DateTimeOffset.Parse($"{expectedDate}T12:00:00Z"));
        AppRandom.Int32Next = FixedNextStrategy.From(expectedTemperature);
        
        // !!!
        // This is set at ConfigureWebHost
        // AppConfigurationBuilder.UseSetting("summary", expectedSummary);

        // !!!
        // This is called at InitializeAsync
        //await DbFx.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        await PostAsync(Client, "/weatherforecast/generate", null);

        var forecastEntities = await AppDbCtx.WeatherForecasts.ToListAsync(TestContext.Current.CancellationToken);
        var forecasts = forecastEntities.Select(x => x.Data).ToList();
        // Assert the WeatherForecasts table contains exactly one record with expected properties
        JToken.FromObject(forecasts)
            .Should().BeEquivalentTo($$"""
            [
                {
                    "Date": "{{expectedDate}}",
                    "TemperatureC": {{expectedTemperature}},
                    "Summary": "{{expectedSummary}}",
                }
            ]
            """);

        var response = await GetAsync(Client, "/weatherforecast/today");

        response
            .Should().BeEquivalentTo(
            $$"""
            {
                "date": "{{expectedDate}}",
                "temperatureC": {{expectedTemperature}},
                "summary": "{{expectedSummary}}"
            }
            """);
    }
    #endregion

    # region helpers

    private static async Task<JToken> GetAsync(HttpClient client, string url)
    {
        var getResp = await client.GetAsync(url, TestContext.Current.CancellationToken);
        var getBody = await getResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        getResp.StatusCode.Should().Be(HttpStatusCode.OK, getBody);
        return JToken.Parse(getBody);
    }

    private static async Task PostAsync(HttpClient client, string url, string? data)
    {
        StringContent? sc = null;
        if(data != null)
            sc = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            
        var resp = await client.PostAsync(url, sc, TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
    }
    #endregion
}

internal static class HelperExtensions
{
    public static IServiceCollection TryReplaceSingleton<TService>(this IServiceCollection services, TService instance)
        where TService : class
    {
        var srcType = typeof(TService);
        var oldD = services.SingleOrDefault(d => d.ServiceType == srcType);
        if (oldD == null)
            return services;

        if(oldD.Lifetime != ServiceLifetime.Singleton)
            throw new InvalidOperationException();

        var sdNew = new ServiceDescriptor(srcType, instance);
        services.Replace(sdNew);

        return services;
    }
    
    internal static void AddSuffixToConnectionString(this IConfiguration config, string connectionStringName, string suffix)
    {
        var key = "ConnectionStrings:" + connectionStringName;
        var cs = config[key];
        var csb = new DbConnectionStringBuilder
        {
            ConnectionString = cs
        };
        csb["Database"] = $"{csb["Database"]}-{suffix}";
        var newCs = csb.ConnectionString;
        config[key] = newCs;
    }
}
