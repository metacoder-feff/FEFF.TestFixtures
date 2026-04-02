/* TODO:

using System.Data.Common;
using FEFF.Extensions.Testing.AspNetCore;

namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Mutates the connection string before the tested application is started.<br/>
/// Prefixes DB name with test-unique (random) string.
/// </summary>
//TODO: remove db after test (without dbcontext)
public class DbNameFixtureBase
{
    private readonly string _prefix;
    private readonly string _connectionStringName;
    private string? _oldCs;
    private string? _newCs;

    public DbNameFixtureBase(ITestApplicationFixture app, TmpScopeIdFixture testId, string connectionStringName)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionStringName);
//TODO: DRY
        _prefix = $"test-{testId.Value}-";
        _connectionStringName = connectionStringName;

        app.Configuration.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(WebHostBuilderContext ctx, IServiceCollection _)
    {
        var config = (ConfigurationManager)ctx.Configuration;
        var key = "ConnectionStrings:" + _connectionStringName;
        ChangeDbName(config, key);
    }
    
    private void ChangeDbName(ConfigurationManager config, string key)
    {
        var cs = config[key];
        var csb = new DbConnectionStringBuilder
        {
            ConnectionString = cs
        };
        csb["Database"] = _prefix + csb["Database"];
        var newCs = csb.ConnectionString;
        config[key] = newCs;

        _oldCs = cs;
        _newCs = newCs;
    }

    // public async ValueTask DisposeAsync()
    // {
        // need to delete at all ?
        // or just leave ?
        // or intelligent background batch delete after a number of tests finish?
        //await TryDeleteDatabaseAsync().ConfigureAwait(false);
    // }
    
//TODO: delete without DbCtx
//     private async Task TryDeleteDatabaseAsync()
//     {
// //TODO: (optimization) check if app is started
//         // e.g. App cannot be started in a negative test
//         try
//         {
//             await DbCtx.Database.EnsureDeletedAsync().ConfigureAwait(false);
//         }
//         catch { }
//     }
}

//*/