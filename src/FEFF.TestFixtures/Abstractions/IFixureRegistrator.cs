using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures;

public interface IFixureRegistrator
{
    static abstract void RegisterFixture(IServiceCollection services);
}