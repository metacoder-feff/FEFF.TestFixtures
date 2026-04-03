using Microsoft.Extensions.DependencyInjection;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class AppServicesFixtureTests
{
    protected IAppServicesFixture ServicesFx = TestContext.Current.GetFeffFixture<AppServicesFixture<Program>>();

    protected FakeLoggerFixture FakeLoggerFx = TestContext.Current.GetFeffFixture<FakeLoggerFixture<Program>>();

    [Fact]
    public async Task AppServices_should_be_resolved()
    {
        var svc = ServicesFx.LazyServiceProvider.GetRequiredService<SomeService>();

        svc.Data.Should().Be("123");
    }
}