using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures;

// should not be used on open generics
//TODO: analyzer
public interface IFixtureRegistrar
{
    static abstract void RegisterFixture(IServiceCollection services);
}