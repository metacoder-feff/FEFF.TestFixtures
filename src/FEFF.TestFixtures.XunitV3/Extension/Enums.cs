namespace FEFF.TestFixtures.Xunit;

/// <summary>
/// Defines the lifetime scope for a fixture in xUnit v3 tests.
/// </summary>
public enum FixtureScopeType
{
    /// <summary>
    /// The fixture is scoped to an individual test case.
    /// </summary>
    TestCase,

    /// <summary>
    /// The fixture is scoped to a test class.
    /// </summary>
    Class,

    /// <summary>
    /// The fixture is scoped to a test collection.
    /// </summary>
    Collection,

    /// <summary>
    /// The fixture is scoped to the entire test assembly.
    /// </summary>
    Assembly
}
