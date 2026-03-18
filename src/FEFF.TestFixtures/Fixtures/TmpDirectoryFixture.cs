using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FEFF.TestFixtures;
//TODO: lazy-CreateTempSubdirectory + _isDisposed + threadsafe ?
//TODO: DisposeType.Deffered

/// <summary>
/// Returns a unique directory for each scope where the fixture is requested.<br/>
/// The directory is deleted after every scope finishes.
/// </summary>
/// <remarks>
/// Every call to the fixture within the same scope returns the same fixture instance.
/// </remarks>
[Fixture]
public sealed class TmpDirectoryFixture : IDisposable, IFixureRegistrator
{
    #region Advanced Registration
    public enum DisposeType { Delete, Skip };
    public class Options
    {
        public DisposeType DisposeType { get; set; } = DisposeType.Delete;
        public string? Prefix { get; set; }
    }

    public static void RegisterFixture(IServiceCollection services)
    {
        services
            .AddOptions<Options>()
            .BindConfiguration(nameof(TmpDirectoryFixture))
            ;
    }
    # endregion

    private readonly Options _opts;
    public string Path { get; }

    public TmpDirectoryFixture(IOptions<Options> opts)
    {
        _opts = opts.Value;
        Path = Directory.CreateTempSubdirectory(_opts.Prefix).FullName;
    }

    public void Dispose()
    {
        if(_opts.DisposeType != DisposeType.Delete)
            return;

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