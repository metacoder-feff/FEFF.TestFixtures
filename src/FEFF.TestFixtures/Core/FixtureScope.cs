using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Core;

internal sealed class FixtureScope : IAsyncDisposable, IFixtureScope
{
    private readonly AsyncServiceScope _serviceScope;

    public FixtureScope(ServiceProvider sp)
    {
        _serviceScope = sp.CreateAsyncScope();
    }

    public ValueTask DisposeAsync()
    {
        return _serviceScope.DisposeAsync();
    }

    public T GetFixture<T>()
    where T : notnull
    {
        return _serviceScope.ServiceProvider.GetRequiredService<T>();
    }
}
