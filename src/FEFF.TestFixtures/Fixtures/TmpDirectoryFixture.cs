namespace FEFF.TestFixtures;

/// <summary>
/// Returns a unique directory for each scope where the fixture is requested.<br/>
/// The directory is deleted after every scope finishes.
/// </summary>
/// <remarks>
/// Every call to the fixture within the same scope returns the same fixture instance.
/// </remarks>
[Fixture]
public sealed class TmpDirectoryFixture : IDisposable
{
//TODO: configure prefix
//TODO: lazy + _isDisposed

    public string Path { get; } = Directory.CreateTempSubdirectory().FullName;

    public void Dispose()
    {
        // double dispose guard
        try
        {
            Directory.Delete(Path, true);
        }
        catch (DirectoryNotFoundException)
        {
        }
    }
}