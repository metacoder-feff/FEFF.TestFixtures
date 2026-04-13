namespace FEFF.TestFixtures.AspNetCore.Randomness.Tests;

public class MiscFakeRandomTests
{
    [Fact]
    public void FixedNextStrategy__should_be_created_and_updated()
    {
        var strategy = FixedNextStrategy.From(10);
        strategy.Next()
            .Should().Be(10);

        strategy.Value = 15;
        strategy.Next()
            .Should().Be(15);
    }

    [Fact]
    public void Set_NormalizationStrategy__when_null__should_throw()
    {
        var rand = new FakeRandom();
        rand.Invoking(r => r.NormalizationStrategy = null!)
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("value");
    }
}
