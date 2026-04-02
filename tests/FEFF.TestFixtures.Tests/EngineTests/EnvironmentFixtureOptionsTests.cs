using FEFF.TestFixtures.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FEFF.TestFixtures.Engine.Tests;

[Collection(EnvironmentCollection)]
public class EnvironmentFixtureOptionsTests : FixtureTestBase
{
    // Auto restore ENV after test
    // Use regular fuxture integration for EnvironmentFixture used here as a helper
    protected EnvironmentFixture Env {get;} = TestContext.Current.GetFeffFixture<EnvironmentFixture>();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Value-1")]
    [InlineData("Value-2")]
    public void Directory__after_dispose__should_exist__when_option(string? value)
    {
        var k = "FixtureWithOptions__Prefix";
        // Arrange
        Env.SetEnvironmentVariable(k, value);
        Environment.GetEnvironmentVariable(k)
            .Should().Be(value);

        // Force reread Configuration.Environment
        // (need when IConfiguration has already been materialized for eny fixture from FixtureManager)
        // var c = Helper.GetFixture<IConfigurationRoot>();
        // c!.Reload();

        // Act
        var f = Helper.GetFixture<FixtureWithOptions>();

        // Assert
        f.Prefix.Should().Be(value);
    }
}

[Fixture]
public sealed class FixtureWithOptions : IFixureRegistar
{
    public class Options
    {
        public string? Prefix { get; set; }
    }

    public static void RegisterFixture(IServiceCollection services)
    {
        services
            .AddOptions<Options>()
            .BindConfiguration(nameof(FixtureWithOptions))
            ;
    }

    public string? Prefix { get; }

    public FixtureWithOptions(IOptions<Options> opts)
    {
        Prefix = opts.Value.Prefix;
    }
}