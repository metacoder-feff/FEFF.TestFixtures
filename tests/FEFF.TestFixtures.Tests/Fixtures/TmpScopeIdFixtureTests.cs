namespace FEFF.TestFixtures.Tests;

public class TmpScopeIdFixtureTests : FixtureTestBase
{
    [Fact]
    public void Value__should_be_unique()
    {
        var f1 = Helper.GetFixture<TmpScopeIdFixture>();

        var scope2 = Helper.FixtureManager.GetScope("testing-scope-2");
        var f2 = scope2.GetFixture<TmpScopeIdFixture>();

        f1.Value.Should().NotBe(f2.Value);
    }
}