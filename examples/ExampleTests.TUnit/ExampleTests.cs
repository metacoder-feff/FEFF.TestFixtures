using System.Text;
using AwesomeAssertions;
using FEFF.TestFixtures;
using FEFF.TestFixtures.TUnit;

namespace ExampleTests.XunitV3;

public class SystemUnderTest
{
    public static void Write(string filePath) =>
        File.WriteAllText(filePath, "some-data", Encoding.UTF8);
}

public class ExampleTests
{
    protected TmpDirectoryFixture TmpDir { get; } = TestContext.Current!.GetFeffFixture<TmpDirectoryFixture>();

    [Test]
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