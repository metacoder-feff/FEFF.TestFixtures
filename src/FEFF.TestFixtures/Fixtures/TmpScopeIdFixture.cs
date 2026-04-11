namespace FEFF.TestFixtures;

/// <summary>
/// Returns a unique string for each scope where the fixture is requested.
/// </summary>
/// <remarks>
/// Every call to the fixture within the same scope returns the same fixture instance.
/// </remarks>
[Fixture]
public class TmpScopeIdFixture
{
    /// <summary>
    /// Gets the unique identifier string for this fixture instance.
    /// </summary>
    public string Value { get; } = Guid.NewGuid().ToString();
}