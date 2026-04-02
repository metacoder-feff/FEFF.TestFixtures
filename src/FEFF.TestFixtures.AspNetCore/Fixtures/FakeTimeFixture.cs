using Microsoft.Extensions.Time.Testing;
using FEFF.Extentions.Testing.AspNetCore;

namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Replaces <see cref="TimeProvider"/> service with <see cref="FakeTimeProvider"/> singleton in a tested application.
/// </summary>
[Fixture]
public class FakeTimeFixture
{
    public readonly FakeTimeProvider Value = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    public FakeTimeFixture(TestApplicationExtention app)
    {
        app.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.TryReplaceSingleton<TimeProvider>(Value);
    }
}