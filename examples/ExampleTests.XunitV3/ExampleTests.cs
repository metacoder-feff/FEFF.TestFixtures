using System.Text;
using AwesomeAssertions;
using FEFF.TestFixtures;
using Xunit.v3;

[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]

namespace ExampleTests.XunitV3;

public class SystemUnderTest
{
    public static void Write(string filePath) =>
        File.WriteAllText(filePath, "some-data", Encoding.UTF8);
}

public class ExampleTests
{
    protected TmpDirectoryFixture TmpDir { get; } = TestContext.Current.GetFeffFixture<TmpDirectoryFixture>();

    [Fact]
    public void File__should_be_created()
    {
        // Arrange
        var filePath = TmpDir.Path + "/file.tmp";
        File.Exists(filePath).Should().BeFalse();

        // Act
        SystemUnderTest.Write(filePath);

        // Assert
        File.Exists(filePath)
            .Should().BeTrue();

        File.ReadAllText(filePath, Encoding.UTF8)
            .Should().Be("some-data");
    }
}