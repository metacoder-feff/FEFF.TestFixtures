using FEFF.Extentions.Testing;
using FEFF.Extentions.Testing.AspNetCore;

namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Replaces <see cref="Random"/> service with <see cref="FakeRandom"/> singleton in a tested application.
/// </summary>
[Fixture]
public class FakeRandomFixture
{
    public readonly FakeRandom Value = new();

    public FakeRandomFixture(ITestApplicationFixture app)
    {
        app.ApplicationBuilder.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.TryReplaceSingleton<Random>(Value);
    }
}