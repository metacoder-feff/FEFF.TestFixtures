namespace FEFF.TestFixtures.Tests;

public class FixtureTestBase
{
    internal FixtureHelper Helper = TestContext.Current.GetFeffFixture<FixtureHelper>();
}