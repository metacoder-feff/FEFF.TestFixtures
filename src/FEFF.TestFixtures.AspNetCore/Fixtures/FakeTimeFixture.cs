using Microsoft.Extensions.Time.Testing;

namespace FEFF.TestFixtures.AspNetCore;

public interface IFakeTimeFixture
{
    FakeTimeProvider Value { get; }
}

/// <summary>
/// Replaces <see cref="TimeProvider"/> service with <see cref="FakeTimeProvider"/> singleton in a tested application.
/// </summary>
[Fixture]
public class FakeTimeFixture<TEntryPoint> : IFakeTimeFixture
where TEntryPoint: class
{
    public FakeTimeProvider Value { get; } = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));
    public FakeTimeFixture(TestApplicationFixture<TEntryPoint> app)
    {
        app.Configuration.UseTimeProvider(Value);
    }
}