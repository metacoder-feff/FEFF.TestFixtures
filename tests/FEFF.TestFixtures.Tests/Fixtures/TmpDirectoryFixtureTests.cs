using System.Text;

namespace FEFF.TestFixtures.Tests;

public class TmpDirectoryFixtureTests : FixtureTestBase
{
    private readonly TmpDirectoryFixture _f;

    public TmpDirectoryFixtureTests()
    {
        _f = GetFixture<TmpDirectoryFixture>();
    }

    [Fact]
    public void Path__should_be_unique()
    {
        var scope2 = FixtureManager.GetScope("testing-scope-2");
        var f2 = scope2.GetFixture<TmpDirectoryFixture>();

        _f.Path.Should().NotBe(f2.Path);
    }

    [Fact]
    public void Directory__should_exist()
    {
        Directory.Exists(_f.Path).Should().BeTrue();
    }

    [Fact]
    public void Directory__after_dispose__should_NOT_exist()
    {
        //Directory__should_exist();

        _f.Dispose();
        Directory.Exists(_f.Path).Should().BeFalse();
    }

    [Fact]
    public void File__should_be_created()
    {
        var filePath = _f.Path + "/file.tmp";
        var fileData = "123";

        File.Exists(filePath).Should().BeFalse();

        File.WriteAllText(filePath, fileData, Encoding.UTF8);

        File.Exists(filePath).Should().BeTrue();

        var written = File.ReadAllText(filePath, Encoding.UTF8);

        written.Should().Be(fileData);
    }

    [Fact]
    public void Directory__with_content_after_dispose__should_NOT_exist()
    {
        var filePath = _f.Path + "/file.tmp";
        File.WriteAllText(filePath, "123", Encoding.UTF8);

        Directory__after_dispose__should_NOT_exist();
    }
}