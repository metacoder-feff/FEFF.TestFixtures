using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Tests;
using Core;

/// <remarks>
/// Do not use TestFixtures to test <see cref="FixtureScope"/> here 
/// because error in <see cref="FixtureScope"/> or integration would fail everything
/// <remarks/>
public sealed class FixtureScopeTests : IAsyncDisposable
{
    private readonly FixtureServiceProvider Factory;
    private readonly FixtureScope Scope;

    public FixtureScopeTests()
    {
        var services = new ServiceCollection();
        // services.AddFixtures();
        services.AddScoped<CustomFixture>();

        Factory = new FixtureServiceProvider(services);
        Scope = Factory.CreateScope();
    }

    public async ValueTask DisposeAsync()
    {
        await Scope.DisposeAsync().ConfigureAwait(false);;
        await Factory.DisposeAsync().ConfigureAwait(false);;
    }

    [Fact]
    public void Fixture__should_be_registered_and_returned()
    {
        var f1 = Scope.GetFixture<CustomFixture>();

        f1.Value.Should().Be("hello");
    }

    [Fact]
    public void Fixtures__from_same_scope__should_be_equal()
    {
        var f1 = Scope.GetFixture<CustomFixture>();
        var f2 = Scope.GetFixture<CustomFixture>();

        f2.Should().Be(f1);
    }

    [Fact]
    public async Task Fixtures__from_diff_scopes__should_NOT_be_equal()
    {
        await using var sc2 = Factory.CreateScope();

        var f1 = Scope.GetFixture<CustomFixture>();
        var f2 = sc2.GetFixture<CustomFixture>();

        f2.Should().NotBe(f1);
    }
}

[Fixture]
internal class CustomFixture
{
    public string Value { get; } = "hello";
}