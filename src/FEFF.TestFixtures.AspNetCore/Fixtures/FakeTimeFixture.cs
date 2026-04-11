using Microsoft.Extensions.Time.Testing;

namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Exposes a <see cref="FakeTimeProvider"/> for controlling time in tests.
/// </summary>
public interface IFakeTimeFixture
{
    /// <summary>
    /// Gets the <see cref="FakeTimeProvider"/> instance.
    /// </summary>
    FakeTimeProvider Value { get; }
}

/// <summary>
/// Replaces the <see cref="TimeProvider"/> service with a <see cref="FakeTimeProvider"/> singleton
/// in the application under test.
/// </summary>
/// <typeparam name="TEntryPoint">The application entry point type.</typeparam>
[Fixture]
public class FakeTimeFixture<TEntryPoint> : IFakeTimeFixture
where TEntryPoint: class
{
    /// <inheritdoc/>
    public FakeTimeProvider Value { get; } = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    /// <summary>
    /// Creates a new <see cref="FakeTimeFixture{TEntryPoint}"/> and registers the fake time provider
    /// with the application under test.
    /// </summary>
    /// <param name="app">The application manager fixture.</param>
    public FakeTimeFixture(AppManagerFixture<TEntryPoint> app)
    {
        app.ConfigurationBuilder.UseTimeProvider(Value);
    }
}