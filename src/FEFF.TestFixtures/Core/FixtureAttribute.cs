using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures;

[AttributeUsage(AttributeTargets.Class)]
public class FixtureAttribute : Attribute
{
    public Type? RegisterWithType { get; set; }
}

public interface IFixureRegistrator
{
    static abstract void RegisterFixture(IServiceCollection services);
}