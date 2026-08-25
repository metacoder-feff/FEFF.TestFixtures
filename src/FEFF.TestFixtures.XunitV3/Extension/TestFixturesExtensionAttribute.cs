using FEFF.TestFixtures.Xunit.Internal;
using Xunit;

namespace FEFF.TestFixtures.Xunit;

/// <summary>
/// Manages <see cref="Engine.FixtureManager"/> for xUnit tests. <br/>
/// Enables the use of <see cref="global::Xunit.v3.TestContextExtensions.GetFeffFixture{T}(ITestContext, FixtureScopeType)"/>.
/// </summary>
/// <remarks>
/// Apply this attribute at the assembly level in an <c>AssemblyInfo.cs</c> or any source file:
/// <code>[assembly: TestFixturesExtension]</code>
/// </remarks>
// [AttributeUsage(AttributeTargets.Assembly)]
public class TestFixturesExtensionAttribute : AssemblyFixtureAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestFixturesExtensionAttribute"/> class.
    /// </summary>
    public TestFixturesExtensionAttribute() : base(typeof(FixtureAdapter))
    {
    }
}
