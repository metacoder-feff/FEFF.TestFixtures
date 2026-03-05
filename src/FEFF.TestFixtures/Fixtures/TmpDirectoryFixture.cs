namespace FEFF.TestFixtures;

/// <summary>
/// Returns a unique directory for each scope where the fixture is requested.<br/>
/// The directory is deleted after every test in the scope finishes.
/// </summary>
/// <remarks>
/// Every call to the fixture within the same scope returns the same fixture instance.
/// </remarks>
[Fixture]
public class TmpDirectoryFixture
{
    private readonly string _path;

    public TmpDirectoryFixture(TmpScopeIdFixture scopeId )
    {
        _path = scopeId.Value;
    }
}