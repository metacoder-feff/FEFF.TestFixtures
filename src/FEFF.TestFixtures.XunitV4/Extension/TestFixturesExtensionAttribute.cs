using FEFF.TestFixtures.Xunit.V4.Internal;
using Xunit;
using Xunit.v3;

namespace FEFF.TestFixtures.Xunit.V4;

//TODO: revert xml comment
//TestContextExtensions.GetFeffFixture{T}

/// <summary>
/// Manages <see cref="Engine.FixtureManager"/> for xUnit tests. <br/>
/// Enables the use of <see cref="TestContextExtensions"/>.
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
