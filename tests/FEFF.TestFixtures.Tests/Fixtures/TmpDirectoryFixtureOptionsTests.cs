using Microsoft.Extensions.Configuration;

namespace FEFF.TestFixtures.Tests;

[Collection("env-tests-run-sequentially")]
// FixtureScopeTestBase - use new ServiceProvider for each test of 'TmpDirectory'
public class TmpDirectoryFixtureOptionsTests : FixtureScopeTestBase
{
    // Auto restore ENV after test
    // Use regular fuxture integration for EnvironmentFixture used here as a helper
    protected EnvironmentFixture Env {get;} = TestContext.Current.GetFeffFixture<EnvironmentFixture>();

    // [Fact]
    // public void prefix()
    // {
    //     Env.SetEnvironmentVariable("TmpDirectoryFixture__Prefix", "prefix");
    //     // Force IConfiguration reread Env
    //     var c = GetFixture<IConfiguration>() as IConfigurationRoot;
    //     c!.Reload();

    //     var f = GetFixture<TmpDirectoryFixture>();

    //     f.Path.Should().Be("zzz");
    // }

    [Theory]
    [InlineData("Skip"  , true , Label = "Skip")]
    [InlineData("Delete", false, Label = "Delete")]
    public void Directory__after_dispose__should_exist__when_option(string env, bool expected)
    {
        // Arrange
        Env.SetEnvironmentVariable("TmpDirectoryFixture__DisposeType", env);

        // Act
        var f = GetFixture<TmpDirectoryFixture>();
        Directory.Exists(f.Path).Should().BeTrue();
        f.Dispose();

        // Assert
        Directory.Exists(f.Path).Should().Be(expected);
    }
}