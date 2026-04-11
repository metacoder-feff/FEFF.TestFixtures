namespace FEFF.TestFixtures;

/// <summary>
/// Marks a class as a test fixture. When applied, the "FEFF.TestFixtures" framework will 
/// discover and register the fixture for use in tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class FixtureAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the type to register the fixture as. Use this when the fixture
    /// should be resolved as a supertype or interface rather than its concrete class.
    /// </summary>
    public Type? RegisterWithType { get; set; }
}