using FEFF.Extensions.Testing;

namespace FEFF.TestFixtures.AspNetCore;

public interface IFakeRandomFixture
{
    FakeRandom Value { get; }
}

/// <summary>
/// Replaces <see cref="Random"/> service with <see cref="FakeRandom"/> singleton in a tested application.
/// </summary>
[Fixture]
public class FakeRandomFixture<TEntryPoint> : IFakeRandomFixture
where TEntryPoint: class
{
    public FakeRandom Value { get; } = new();
    public FakeRandomFixture(TestApplicationFixture<TEntryPoint> app)
    {
        app.Configuration.UseRandom(Value);
    }
}