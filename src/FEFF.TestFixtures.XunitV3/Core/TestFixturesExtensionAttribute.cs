using System.Reflection;
using Xunit.v3;

namespace FEFF.TestFixtures.Xunit;

// Remarks: Dispose() is not called for IBeforeAfterTestAttribute

/// <summary>
/// Manages <see cref="Engine.FixtureManager"/> for xUnit tests. <br/>
/// Enables the use of <see cref="TestContextExtensions.GetFeffFixture{T}"/>.
/// </summary>
/// <remarks>
/// Apply this attribute at the assembly level in an <c>AssemblyInfo.cs</c> or any source file:
/// <c>[assembly: TestFixturesExtension]</c>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public class TestFixturesExtensionAttribute : Attribute, IBeforeAfterTestAttribute, IAssemblyFixtureAttribute
{
    /// <inheritdoc/>
    public Type AssemblyFixtureType => typeof(AssemblyTestTracker);

    /// <inheritdoc/>
    /// <remarks>
    /// 'Before' is called after TestClass ctor
    /// </remarks>
    public void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
    }

    /// <inheritdoc/>
    public void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        ScopeRemoverHelper.RemoveTestCaseScope(test);
    }
}