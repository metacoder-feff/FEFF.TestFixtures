using FEFF.Extensions.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Testing.Platform.Services;
using WebApiTestSubject;

namespace FEFF.TestFixtures.AspNetCore.Tests;

public class ApplicationFixtureExtensionsTests
{
    protected ITestApplicationFixture App = TestContext.Current.GetFeffFixture<TestApplicationFixture<Program>>();
    protected IAppServicesFixture AppServices = TestContext.Current.GetFeffFixture<AppServicesFixture<Program>>();

    [Theory]
    [InlineData(null)]
    [InlineData("fixture-configure-app")]
    [InlineData("app-attach-extension")]
    [InlineData("generic-auto-configure")]
    public void Fixture_extensions__should_be_attached_with(string? configMethod)
    {
        // Arrange: Register App Extensions with different methods

        // option 1: call non-generic-fixture.Configure(ITestApplicationFixture app)
        if(configMethod == "fixture-configure-app")
        {
            var fakeTimeFx = TestContext.Current.GetFeffFixture<FakeTimeFixture>();
            var fakeRandomFx = TestContext.Current.GetFeffFixture<FakeRandomFixture>();
            var fakeLoggerFx = TestContext.Current.GetFeffFixture<FakeLoggerFixture>();

            fakeTimeFx.Configure(App);
            fakeRandomFx.Configure(App);
            fakeLoggerFx.Configure(App);
        }

        // option 2: call ITestApplicationFixture.AttachExtensions(params ITestApplicationExtension[] extensions)
        // with non-generic-fixture-list as arguments
        if(configMethod == "app-attach-extension")
        {
            var fakeTimeFx = TestContext.Current.GetFeffFixture<FakeTimeFixture>();
            var fakeRandomFx = TestContext.Current.GetFeffFixture<FakeRandomFixture>();
            var fakeLoggerFx = TestContext.Current.GetFeffFixture<FakeLoggerFixture>();

            App.AttachExtensions(fakeTimeFx, fakeRandomFx, fakeLoggerFx);
        }

        // option 3: use generic Fixture<T>
        // It automatically finds TestApplicationFixture<T> and attaches to it
        if(configMethod == "generic-auto-configure")
        {
            _ = TestContext.Current.GetFeffFixture<FakeTimeFixture<Program>>();
            _ = TestContext.Current.GetFeffFixture<FakeRandomFixture<Program>>();
            _ = TestContext.Current.GetFeffFixture<FakeLoggerFixture<Program>>();
            // in the example we do not need to use the variables
            // we need only to create the Fixtures
            // the registration is made at their constructors
        }

        // NOTE:
        // Whichever option is used to register the extensions, it should be applied before the app is built or requested.

        // Assert:
        // Assert services are NOT substituted
        if(configMethod == null)
        {
            AppServices.LazyServiceProvider.GetRequiredService<TimeProvider>()
                .Should().NotBeOfType<FakeTimeProvider>()   // SystemTimeProvider
                ;
            AppServices.LazyServiceProvider.GetRequiredService<Random>()
                .Should().NotBeOfType<FakeRandom>()         // ThreadSafeRandom
                ;
            AppServices.LazyServiceProvider.GetRequiredService<IEnumerable<ILoggerProvider>>()
                .Select(x => x.GetType())
                .Should().NotContain(typeof(FakeLoggerProvider))
                ;
        }
        else
        // Assert services ARE substituted inside the TestApp
        {
            AppServices.LazyServiceProvider.GetRequiredService<TimeProvider>()
                .Should().BeOfType<FakeTimeProvider>()
                ;
            AppServices.LazyServiceProvider.GetRequiredService<Random>()
                .Should().BeOfType<FakeRandom>()
                ;
            AppServices.LazyServiceProvider.GetRequiredService<IEnumerable<ILoggerProvider>>()
                .Select(x => x.GetType())
                .Should().Contain(typeof(FakeLoggerProvider))
                ;
        }
    }
}