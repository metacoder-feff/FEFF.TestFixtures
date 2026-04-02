using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures;

// should not be used on open generics
//TODO: analizer
public interface IFixureRegistar
{
    static abstract void RegisterFixture(IServiceCollection services);
}