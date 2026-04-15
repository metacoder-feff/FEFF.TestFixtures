using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FEFF.TestFixtures;
//TODO: lazy-CreateTempSubdirectory + _isDisposed + thread-safe ?
//TODO: DisposeType.Deferred

/// <summary>
/// Returns a unique directory for each scope where the fixture is requested.<br/>
/// The directory is deleted after every scope finishes.
/// </summary>
/// <remarks>
/// Every call to the fixture within the same scope returns the same fixture instance.
/// </remarks>
[Fixture]
public sealed class TmpDirectoryFixture : IDisposable, IFixtureRegistrar
{
    #region Advanced Registration
    /// <summary>
    /// Specifies the behavior when the fixture is disposed.
    /// </summary>
    public enum DisposeType
    {
        /// <summary>
        /// Deletes the temporary directory and its contents on disposal.
        /// </summary>
        Delete,

        /// <summary>
        /// Skips deletion of the temporary directory on disposal.
        /// </summary>
        /// <remarks>
        /// Can be used for optimization in CI environments.
        /// </remarks>
        Skip
    }

    /// <summary>
    /// Configuration options for <see cref="TmpDirectoryFixture"/>.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets whether the temporary directory should be deleted on disposal.
        /// Defaults to <see cref="DisposeType.Delete"/>.
        /// </summary>
        public DisposeType DisposeType { get; set; } = DisposeType.Delete;

        /// <summary>
        /// Gets or sets the prefix for the temporary directory name.
        /// </summary>
        public string? Prefix { get; set; }
    }

    /// <summary>
    /// Registers <see cref="TmpDirectoryFixture"/> configuration options with the service collection,
    /// binding from the <c>TmpDirectoryFixture</c> configuration section.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
    public static void RegisterFixture(IServiceCollection services)
    {
        services
            .AddOptions<Options>()
            .BindConfiguration(nameof(TmpDirectoryFixture))
            ;
    }
    # endregion

    private readonly Options _opts;
    /// <summary>
    /// Gets the full path of the temporary directory created for this scope.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Creates a unique temporary subdirectory for the current test scope.
    /// </summary>
    /// <param name="opts">The configured options for directory creation and disposal.</param>
    public TmpDirectoryFixture(IOptions<Options> opts)
    {
        _opts = opts.Value;
        Path = Directory.CreateTempSubdirectory(_opts.Prefix).FullName;
    }

    /// <summary>
    /// Deletes the temporary directory if <see cref="Options.DisposeType"/> is set to <see cref="DisposeType.Delete"/>.
    /// Safe to call multiple times; ignores missing directories.
    /// </summary>
    public void Dispose()
    {
        if (_opts.DisposeType != DisposeType.Delete)
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
