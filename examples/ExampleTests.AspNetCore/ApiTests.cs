// ============================================================================
// Packages to be installed
// ============================================================================
/*
    dotnet add package AwesomeAssertions
    dotnet add package AwesomeAssertions.Json

    dotnet add package FEFF.TestFixtures.Xunit
    dotnet add package FEFF.TestFixtures
    dotnet add package FEFF.TestFixtures.AspNetCore
*/

using System.Net;
using AwesomeAssertions;
using AwesomeAssertions.Json; // Required for proper JSON assertions
using FEFF.TestFixtures;
using FEFF.TestFixtures.AspNetCore;
using FEFF.TestFixtures.AspNetCore.Randomness;
using FEFF.TestFixtures.AspNetCore.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;
using Xunit.v3;

// Register the FEFF TestFixtures extension with xUnit v3
[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]

//TODO: add example of AuthorizedAppClientFixture
//TODO: add example of FakeLoggerFixture (extended example)
//TODO: add example of SignalrClientFixture + dotnet add package (extended example)

namespace ExampleTests.AspNetCore;

// ============================================================================
// FIXTURE CONFIGURATION
// ============================================================================

/// <summary>
/// This fixture defines configuration options for other fixtures
/// (the TmpDatabaseNameFixture in the example).
/// The ConnectionStringNames property specifies which connection strings should
/// be redirected to temporary databases.
/// </summary>
[Fixture]
public class OptionsFixture : ITmpDatabaseNameFixtureOptions
{
    public IReadOnlyCollection<string> ConnectionStringNames => [Program.ConnectionStringName];
}

/// <summary>
/// FixtureSet is a record that bundles all the fixtures required for testing the API.
/// This is a core concept in FEFF TestFixtures - instead of inheriting from base classes,
/// we compose test infrastructure as fixtures and request them as a single grouped unit.
/// 
/// Each fixture serves a specific purpose:
/// - AppManagerFixture: Manages the ASP.NET Core TestApplication lifecycle and configuration
/// - FakeRandomFixture: Provides deterministic random number generation for reproducible tests
/// - FakeTimeFixture: Allows control over time-dependent behavior
/// - AppClientFixture: Provides an HttpClient for making requests to the test application
/// - DatabaseLifecycleFixture: Ensures the database is deleted after tests, can optionally create the DB during/before the tests
/// - TmpDatabaseNameFixture: Sets unique temporary database names for test isolation
/// </summary>
[Fixture]
public record FixtureSet(
    AppManagerFixture<Program> AppManagerFx,                 // Allows to configure the test web application before start
    FakeRandomFixture<Program> FakeRandomFx,                 // Deterministic randomness
    FakeTimeFixture<Program> FakeTimeFx,                     // Controllable time provider
    AppClientFixture<Program> ClientFx,                      // HTTP client for API requests
    DatabaseLifecycleFixture<Program, ApplicationDbContext> DbFx,  // Database EnsureCreated/EnsureDeleted using EfCore.DbContext
    TmpDatabaseNameFixture<Program, OptionsFixture> TmpDbNameFx    // Temp database naming
);

/// <summary>
/// Main test class containing API integration tests.
/// Tests demonstrate various fixture capabilities: HTTP responses, environment variable configuration,
/// time manipulation, and database interactions.
/// </summary>
public class ApiTests
{
    /// <summary>
    /// Retrieves the FixtureSet for the current test context with all dependencies.
    /// GetFeffFixture&lt;T&gt; is an extension method that resolves fixtures from the context.
    /// </summary>
    protected FixtureSet FixtureSet { get; } = TestContext.Current.GetFeffFixture<FixtureSet>();

    #region properties for fast access
    // ============================================================================
    // CONVENIENCE PROPERTIES
    // These properties provide quick access to commonly-used fixture components,
    // reducing verbosity in test methods.
    // ============================================================================

    /// <summary>
    /// Provides a FakeRandom instance that replaces the application's Random service.
    /// Allows configuration of random strategies (e.g., ConstRandomStrategy) to produce
    /// deterministic, predictable values for int, long, float, and double generation.
    /// Useful for testing code that depends on random number generation.
    /// </summary>
    protected FakeRandom AppRandom => FixtureSet.FakeRandomFx.Value;

    /// <summary>
    /// Provides a fake time provider that can be manipulated to return a specific instant
    /// to the tested application.
    /// </summary>
    protected FakeTimeProvider AppTime => FixtureSet.FakeTimeFx.Value;

    /// <summary>
    /// Allows configuration of the test application before it starts.
    /// Can be used to set environment variables, update DI container, etc.
    /// </summary>
    protected IAppConfigurator AppConfigurationBuilder => FixtureSet.AppManagerFx.ConfigurationBuilder;

    /// <summary>
    /// Interface for ensuring the database is created.
    /// Call EnsureCreatedAsync() before or during tests that require a fresh database.
    /// The database would be deleted automatically after the test.
    /// </summary>
    protected IDatabaseLifecycleFixture DbFx => FixtureSet.DbFx;

    /// <summary>
    /// HTTP client for making requests to the test application's API endpoints.
    /// </summary>
    protected HttpClient Client => FixtureSet.ClientFx.LazyValue;

    /// <summary>
    /// Direct access to the application's DbContext for database assertions/manipulations.
    /// </summary>
    protected ApplicationDbContext AppDbCtx => FixtureSet.DbFx.LazyDbContext;
    #endregion

    /// <summary>
    /// Test: Basic HTTP GET request validation.
    /// 
    /// This test verifies that the /weatherforecast/const endpoint:
    /// 1. Returns a successful HTTP 200 OK response
    /// 2. Returns a predictable JSON response with hardcoded values
    /// 
    /// Assertions use AwesomeAssertions.Json (FluentAssertions fork) for readable assertions
    /// and JSON equivalence checking.
    /// </summary>
    [Fact]
    public async Task Example1__Client__should__get_response()
    {
        var resp = await Client.GetAsync("/weatherforecast/const", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

        // Parse the JSON response and assert it matches the expected structure
        // The response should contain an array with a single weather forecast object
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

    /// <summary>
    /// Test: Environment variable configuration affects API response.
    /// 
    /// This parameterized test demonstrates how to modify the application's
    /// configuration before it starts. It verifies that the "summary" configuration
    /// setting controls the weather forecast summary returned by the /weatherforecast/env endpoint.
    /// 
    /// Key concept: <see cref="AppManagerFixture{T}.ConfigurationBuilder"/> allows tests to inject configuration
    /// values before the application is built, enabling testing of different configuration scenarios.
    /// 
    /// Application is built and started on first access to <see cref="AppManagerFixture{T}.LazyApplication"/>, 
    /// <see cref="AppClientFixture{T}.LazyValue"/> and so on.
    /// 
    /// Parameters:
    /// - "cloudy": Tests that the API returns "cloudy" as the summary
    /// - "warm": Tests that the API returns "warm" as the summary
    /// </summary>
    [Theory]
    [InlineData("cloudy")]
    [InlineData("warm")]
    public async Task Example2__SetEnvVar__should_make_api_to_respond_with(string envVarValue)
    {
        // Change TestApp configuration before it starts
        // UseSetting modifies the appsettings configuration with the specified key-value pair
        // UseSetting is an extension method
        if (envVarValue != null)
            AppConfigurationBuilder.UseSetting("summary", envVarValue);

        // Making the first request builds and starts the application
        var resp = await Client.GetAsync("/weatherforecast/env", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

        // Assert the JSON response contains the summary value we configured
        JToken.Parse(body)
            .Should().BeEquivalentTo(
            $$"""
            [
                {
                    "date": "2000-01-01",
                    "temperatureC": 20,
                    "summary": "{{envVarValue}}"
                }
            ]
            """);
    }

    /// <summary>
    /// Test: FakeTimeFixture controls time-dependent API behavior.
    ///
    /// This parameterized test demonstrates the FakeTimeFixture capability.
    /// The test sets the system's "current" time to different dates and verifies
    /// that the /weatherforecast/time endpoint returns the configured date.
    ///
    /// Key concept: Time.SetUtcNow() allows tests to control what TimeProvider.GetUtcNow() returns,
    /// enabling deterministic testing of time-dependent code without race conditions
    /// or waiting for specific dates.
    ///
    /// Parameters:
    /// - "2006-01-05": Tests that the API returns this specific date
    /// - "2150-11-15": Tests that the API works with future dates as well
    /// </summary>
    [Theory]
    [InlineData("2006-01-05")]
    [InlineData("2150-11-15")]
    public async Task Example3__FakeTimeFixture__should_make_api_to_respond__with(string date)
    {
        // Set the fake time provider's "current" UTC time to the specified date at 05:05:05
        // This makes TimeProvider.GetUtcNow() return this value throughout the test
        AppTime.SetUtcNow(DateTimeOffset.Parse($"{date}T05:05:05Z"));

        // Making the first request builds and starts the application
        var resp = await Client.GetAsync("/weatherforecast/time", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

        // Assert the JSON response contains the date we set via the FakeTimeFixture
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

    /// <summary>
    /// Test: FakeRandomFixture controls random-dependent API behavior.
    ///
    /// This parameterized test demonstrates the FakeRandomFixture capability.
    /// The test configures the random number generator to return a specific value
    /// and verifies that the /weatherforecast/random endpoint returns that value as temperatureC.
    ///
    /// Key concept: FakeRandomFixture replaces the application's Random service with a FakeRandom
    /// instance that can be controlled through strategies like ConstRandomStrategy,
    /// enabling deterministic testing of random-dependent code.
    ///
    /// Parameters:
    /// - 42: Tests that the API returns 42 as the temperature
    /// - 77: Tests that the API returns 77 as the temperature
    /// </summary>
    [Theory]
    [InlineData(42)]
    [InlineData(77)]
    public async Task Example4__FakeRandomFixture__should_make_api_to_respond__with(int temperature)
    {
        // Configure the fake random number generator to always return the specified temperature
        // This makes Random.Next() return this value throughout the test
        AppRandom.Int32Next = FixedNextStrategy.From(temperature);

        // Making the first request builds and starts the application
        var resp = await Client.GetAsync("/weatherforecast/random", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

        // Assert the JSON response contains the temperature we set via the FakeRandomFixture
        JToken.Parse(body)
            .Should().BeEquivalentTo(
            $$"""
            [
                {
                    "date": "2000-01-01",
                    "temperatureC": {{temperature}},
                    "summary": "normal"
                }
            ]
            """);
    }

    /// <summary>
    /// Test: POST operation creates a user record in the database.
    ///
    /// This test verifies the full integration flow:
    /// 1. POST request to create a user
    /// 2. Database query to verify the user was actually persisted
    ///
    /// Key concepts demonstrated:
    /// - EnsureDbFx.EnsureCreatedAsync(): Creates (and fast migrates) the database before the test
    /// - Direct DbContext access for database-level assertions
    /// - POST request testing (currently sends null body, which the API handles by creating a default user)
    /// </summary>
    [Fact]
    public async Task Example5__Post_user__should_create_record_in_db()
    {
        // Ensure the database is created and all migrations are applied
        // This gives us a clean database state for the test
        // Note that we work with a unique database due to the TmpDatabaseNameFixture
        await DbFx.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        // Send a POST request to the /user endpoint to create a new user
        // The API creates a default user when the request body is null
        var resp = await Client.PostAsync("/user", null, TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

        // Query the database directly to verify the user was actually created
        // This proves the API didn't just return a success response, but actually persisted the data
        var users = await AppDbCtx.Users.ToListAsync(TestContext.Current.CancellationToken);

        // Assert the users table contains exactly one user with expected properties
        JToken.FromObject(users)
            .Should().BeEquivalentTo("""
            [
                {
                    "Id": 1,
                    "Name": "test",
                    "Age": 100
                }
            ]
            """);
    }
}
