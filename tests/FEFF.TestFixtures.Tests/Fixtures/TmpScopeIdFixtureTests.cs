namespace FEFF.TestFixtures.Tests;

public class TmpScopeIdFixtureTests //: FixtureScopeTestBase
{
    [Fact]
    public void Value__should_be_unique()
    {
        var f1 = TestContext.Current.GetFeffFixture<TmpScopeIdFixture>();
        var f2 = TestContext.Current.GetFeffFixture<TmpScopeIdFixture>(Xunit.FixtureScopeType.Assembly);

        f1.Value.Should().NotBe(f2.Value);
    }
}