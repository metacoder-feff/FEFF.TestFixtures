namespace FEFF.TestFixtures;

[AttributeUsage(AttributeTargets.Class)]
public class FixtureAttribute : Attribute
{
    public Type? RegisterWithType { get; set; }
}
