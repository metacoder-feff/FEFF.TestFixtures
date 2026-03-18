using Microsoft.Extensions.Configuration;

namespace FEFF.TestFixtures.Tests;

public class TmpDirectoryFixtureOptionsTests : FixtureTestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Path__should_start_with_prefix(bool prefixExpected)
    {
        // Arrange
        var prefix = "prefix-";

        if(prefixExpected)
            UseSettingEnv("TmpDirectoryFixture__Prefix", prefix);
        else
            UseSettingEnv("TmpDirectoryFixture__Prefix", null);

        // Act
        var f = GetFixture<TmpDirectoryFixture>();

        // ssert
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
        UseSettingEnv("TmpDirectoryFixture__DisposeType", env);

        // Act
        var f = GetFixture<TmpDirectoryFixture>();
        Directory.Exists(f.Path).Should().BeTrue();
        f.Dispose();

        // Assert
        Directory.Exists(f.Path).Should().Be(expected);
    }
}