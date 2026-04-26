using FEFF.TestFixtures.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.EF.Tests;

public class DatabaseLifecycleFixtureTests
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
        // Assert: db does not exist
        (await assertContext.DatabaseExistsAsync())
            .Should().BeFalse();

        try
        {
            await RunFixture(useFixture, startApp, helper, connectionStringSuffix, assertContext);

            // Assert: db does not exist
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

    [Fact]
    public async Task Fixture_init_should_not_hang()
    {
        var app = TestContext.Current.GetFeffFixture<AppManagerFixture<Program>>();
        app.ConfigurationBuilder.UseDatabaseNamePostfix("123", [Program.ConnectionStringName]);

        var f1 = TestContext.Current.GetFeffFixture<DatabaseLifecycleFixture<Program, ApplicationDbContext>>();
        var f2 = TestContext.Current.GetFeffFixture<AppServicesFixture<Program>>();

        var act = () => Task.Run(
            () => f2.LazyServiceProvider
        );
        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(1));
    }

    private static async Task RunFixture(bool useFixture, bool startApp, FixtureHelper helper, string connectionStringSuffix, ApplicationDbContext assertContext)
    {
        // use second scope for 'Act'
        // also scope allows to test Dispose()
        var scopeId = "1";
        var scope = helper.FixtureManager.GetScope(scopeId);
        var app = scope.GetFixture<AppManagerFixture<Program>>();
        app.ConfigurationBuilder.UseDatabaseNamePostfix(connectionStringSuffix, [Program.ConnectionStringName]);

        var initialExist = await assertContext.DatabaseExistsAsync();

        if (useFixture)
        {
            var f = scope.GetFixture<DatabaseLifecycleFixture<Program, ApplicationDbContext>>();

            if (startApp)
            {
                await f.EnsureCreatedAsync(TestContext.Current.CancellationToken);
                // request to LazyDbContext starts application if it is not started
                f.LazyDbContext.Should().NotBeNull();
            }
        }

//TODO: return bool??
        // Assert: db exists if useFixture && startApp
        (await assertContext.DatabaseExistsAsync())
            .Should().Be((useFixture && startApp) || initialExist);

        await helper.FixtureManager.RemoveScopeAsync(scopeId);
    }

    private static ApplicationDbContext GetAssertContext(FixtureHelper fm, string connectionStringSuffix)
    {
        var app = fm.GetFixture<AppManagerFixture<Program>>();
        app.ConfigurationBuilder.UseDatabaseNamePostfix(connectionStringSuffix, [Program.ConnectionStringName]);

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
