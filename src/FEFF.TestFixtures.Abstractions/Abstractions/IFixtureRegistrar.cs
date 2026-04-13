using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures;

/// <summary>
/// Defines a contract for performing complex <see cref="IServiceCollection"/> manipulation
/// during fixture registration. Implement this interface for any public type when a fixture 
/// requires advanced control over service registration beyond simple type mapping.
/// </summary>
/// <remarks>
/// This interface defines a contract for a static method, therefore it should not be used 
/// on open generic types.
/// </remarks>
//TODO: analyzer
public interface IFixtureRegistrar
{
    /// <summary>
    /// Performs complex <see cref="IServiceCollection"/> manipulation to register
    /// the fixture and any related services required for testing.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    static abstract void RegisterFixture(IServiceCollection services);
}
