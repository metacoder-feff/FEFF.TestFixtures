using Microsoft.Extensions.Configuration;

namespace FEFF.TestFixtures.Tests;

[Collection("env-tests-run-sequentially")]
// FixtureScopeTestBase - use new ServiceProvider for each test of 'TmpDirectory'
public class TmpDirectoryFixtureOptionsTests : FixtureScopeTestBase
{
    // Auto restore ENV after test
    // Use regular fuxture integration for EnvironmentFixture used here as a helper
    protected EnvironmentFixture Env {get;} = TestContext.Current.GetFeffFixture<EnvironmentFixture>();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Path__should_start_with_prefix(bool prefixExpected)
    {
        var prefix = "prefix-";

        if(prefixExpected)
            Env.SetEnvironmentVariable("TmpDirectoryFixture__Prefix", prefix);
        else
            Env.SetEnvironmentVariable("TmpDirectoryFixture__Prefix", null);

        // Force IConfiguration reread Env
        var c = GetFixture<IConfiguration>() as IConfigurationRoot;
        c!.Reload();

        var f = GetFixture<TmpDirectoryFixture>();

        //f.Path.Should().Be("zzz");
        var di = new DirectoryInfo(f.Path);

        if(prefixExpected)
            di.Name.Should().StartWith(prefix);
        else
            di.Name.Should().NotStartWith(prefix);
    }

    [Theory]
    [InlineData("Skip"  , true , Label = "Skip")]
    [InlineData("Delete", false, Label = "Delete")]
    public void Directory__after_dispose__should_exist__when_option(string env, bool expected)
    {
        // Arrange
        Env.SetEnvironmentVariable("TmpDirectoryFixture__DisposeType", env);
        // Force IConfiguration reread Env
        var c = GetFixture<IConfiguration>() as IConfigurationRoot;
        c!.Reload();

        // Act
        var f = GetFixture<TmpDirectoryFixture>();
        Directory.Exists(f.Path).Should().BeTrue();
        f.Dispose();

        // Assert
        Directory.Exists(f.Path).Should().Be(expected);
    }
}