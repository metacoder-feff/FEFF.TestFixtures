using FEFF.TestFixtures.AspNetCore.Randomness;

namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Defines the contract for exposing a <see cref="FakeRandom"/> instance for controlling randomness in tests.
/// </summary>
public interface IFakeRandomFixture
{
    /// <summary>
    /// Gets the <see cref="FakeRandom"/> instance.
    /// </summary>
    FakeRandom Value { get; }
}

/// <summary>
/// Replaces the <see cref="Random"/> service with a <see cref="FakeRandom"/> singleton
/// in the application under test.
/// </summary>
/// <typeparam name="TEntryPoint">The application entry point type.</typeparam>
[Fixture]
public class FakeRandomFixture<TEntryPoint> : IFakeRandomFixture
where TEntryPoint : class
{
    /// <inheritdoc/>
    public FakeRandom Value { get; } = new();

    /// <summary>
    /// Initializes a new instance of <see cref="FakeRandomFixture{TEntryPoint}"/> and registers the fake random
    /// service with the application under test.
    /// </summary>
    /// <param name="app">The application manager fixture.</param>
    public FakeRandomFixture(AppManagerFixture<TEntryPoint> app)
    {
        app.ConfigurationBuilder.UseRandom(Value);
    }
}
