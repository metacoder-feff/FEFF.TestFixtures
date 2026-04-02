namespace FEFF.TestFixtures.Engine.Tests.Subjects;

[Fixture]
public class RefFixture(RefRefFixture refRef)
{
    public string RefRefValue => refRef.Value;
}
