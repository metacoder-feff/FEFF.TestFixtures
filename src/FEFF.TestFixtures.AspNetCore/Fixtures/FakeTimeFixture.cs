using Microsoft.Extensions.Time.Testing;
using FEFF.Extensions.Testing.AspNetCore;

namespace FEFF.TestFixtures.AspNetCore;

[Fixture]
public class FakeTimeFixture : ITestApplicationExtension
{
    public FakeTimeProvider Value { get; } = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    public void Configure(ITestApplicationFixture app)
    {
        app.Configuration.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.TryReplaceSingleton<TimeProvider>(Value);
    }
}

/// <summary>
/// Replaces <see cref="TimeProvider"/> service with <see cref="FakeTimeProvider"/> singleton in a tested application.
/// </summary>
[Fixture]
public class FakeTimeFixture<TEntryPoint> : FakeTimeFixture
where TEntryPoint: class
{
    public FakeTimeFixture(TestApplicationFixture<TEntryPoint> app)
    {
        Configure(app);
    }
}