using FEFF.Extensions.Testing;

namespace FEFF.TestFixtures.AspNetCore;

[Fixture]
public class FakeRandomFixture : ITestApplicationExtension
{
    public FakeRandom Value { get; } = new();

    public void Configure(ITestApplicationFixture app)
    {
        app.Configuration.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.TryReplaceSingleton<Random>(Value);
    }
}

/// <summary>
/// Replaces <see cref="Random"/> service with <see cref="FakeRandom"/> singleton in a tested application.
/// </summary>
[Fixture]
public class FakeRandomFixture<TEntryPoint> : FakeRandomFixture
where TEntryPoint: class
{
    public FakeRandomFixture(TestApplicationFixture<TEntryPoint> app)
    {
        Configure(app);
    }
}