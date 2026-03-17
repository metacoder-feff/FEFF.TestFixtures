namespace FEFF.TestFixtures.Tests;

[Collection(EnvironmentCollection)]
public class EnvironmentFixtureTest : XunitIntegratedFixtureTestBase
{
    private static string? GetEnv(string key)
    {
        return Environment.GetEnvironmentVariable(key);
    }

    [Theory]
    [InlineData(null     , "123", Label = "when_added")]
    [InlineData("default", "123", Label = "when_modified")]
    [InlineData("default", null , Label = "when_removed")]
    public void Env__after_dispose__should_be_restored(string? initial, string? modified)
    {
        // Arrange
        var k = $"new_key_{Guid.NewGuid()}";

        Environment.SetEnvironmentVariable(k, initial);
        GetEnv(k).Should().Be(initial);

        // Act
        var f = GetFixture<EnvironmentFixture>();

        Environment.SetEnvironmentVariable(k, modified);
        GetEnv(k).Should().Be(modified);

        f.Dispose();

        var initialStored = f.InitialSnapshot.TryGetOrNull(k);
        initialStored.Should().Be(initial);

        // Assert
        GetEnv(k).Should().Be(initial);
    }

    [Fact]
    public void Double_dispose__should_not_throw()
    {
        var f = GetFixture<EnvironmentFixture>();

        f.Dispose();
        f.Dispose();
    }
}