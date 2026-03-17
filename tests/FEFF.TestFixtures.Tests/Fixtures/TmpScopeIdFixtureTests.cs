namespace FEFF.TestFixtures.Tests;

public class TmpScopeIdFixtureTests : XunitIntegratedFixtureTestBase
{
    [Fact]
    public void Value__should_be_unique()
    {
        var f1 = GetFixture<TmpScopeIdFixture>();
        var f2 = GetFixture<TmpScopeIdFixture>(Xunit.FixtureScopeType.Assembly);

        f1.Value.Should().NotBe(f2.Value);
    }
}