using FEFF.TestFixtures.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class EnsureDbFixtureTests
{

    [Fact]
    public async Task EnsureDeleted__if_not_started__should_not_be_invoked()
    {
        var helper = TestContext.Current.GetFeffFixture<FixtureHelper>();
        var connectionStringSuffix = Guid.NewGuid().ToString();

        // use first scope for 'Assert'
        var assertContext = GetAssertContext(helper, connectionStringSuffix);

        await assertContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        try
        {
            await RunFixture(true, false, helper, connectionStringSuffix, assertContext);

            (await assertContext.DatabaseExistsAsync())
                .Should().BeTrue();
        }
        finally
        {
//TODO: to teardown??            
            // cleanup after test
            await assertContext.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true , false)]
    [InlineData(true , true)]
    public async Task Fixture__should_create_and_delete_db__when(bool useFixture, bool startApp)
    {
        var helper = TestContext.Current.GetFeffFixture<FixtureHelper>();
        var connectionStringSuffix = Guid.NewGuid().ToString();

        // use first scope for 'Assert'
        var assertContext = GetAssertContext(helper, connectionStringSuffix);

        // use first scope for 'Assert'
        // Assert: db not extists
        (await assertContext.DatabaseExistsAsync())
            .Should().BeFalse();

        try
        {
            await RunFixture(useFixture, startApp, helper, connectionStringSuffix, assertContext);

            // Assert: db not extists
            (await assertContext.DatabaseExistsAsync())
                .Should().BeFalse();
        }
        finally
        {
//TODO: to teardown??            
            // cleanup after test
            await assertContext.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        }
    }

    private static async Task RunFixture(bool useFixture, bool startApp, FixtureHelper helper, string connectionStringSuffix, ApplicationDbContext assertContext)
    {
        // use second scope for 'Act'
        // also scope allows to test Dispose()
        var scopeId = "1";
        var scope = helper.FixtureManager.GetScope(scopeId);
        var app = scope.GetFixture<TestApplicationFixture<Program>>();
        app.Configuration.UseDatabaseNamePostfix(connectionStringSuffix, Program.ConnectionStringName);

        var initialExist = await assertContext.DatabaseExistsAsync();

        if (useFixture)
            _ = scope.GetFixture<EnsureDbContextFixture<Program, ApplicationDbContext>>();

        if (startApp)
            // just start
            _ = app.LazyApplication.Services.GetRequiredService<IConfiguration>();

//TODO: return bool??
        // Assert: db extists if useFixture && startApp
        (await assertContext.DatabaseExistsAsync())
            .Should().Be((useFixture && startApp) || initialExist);

        await helper.FixtureManager.RemoveScopeAsync(scopeId);
    }

    private static ApplicationDbContext GetAssertContext(FixtureHelper fm, string connectioStringSuffix)
    {
        var app = fm.GetFixture<TestApplicationFixture<Program>>();
        app.Configuration.UseDatabaseNamePostfix(connectioStringSuffix, Program.ConnectionStringName);

        return fm
            .GetFixture<AppServicesFixture<Program>>()
            .LazyServiceProvider
            .GetRequiredService<ApplicationDbContext>();
    }
}

internal static class DbCheckExt
{
    public static async Task<bool> DatabaseExistsAsync(this DbContext src) =>
        //DbContextHealthCheck
        await src.Database.CanConnectAsync(TestContext.Current.CancellationToken);
        // src.Database.GetService<IRelationalDatabaseCreator>().Exists();
}