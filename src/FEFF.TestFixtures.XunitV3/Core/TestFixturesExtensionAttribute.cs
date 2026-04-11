using System.Reflection;
using Xunit.v3;

namespace FEFF.TestFixtures.Xunit;

// Remarks: Dispose() is not called for IBeforeAfterTestAttribute

/// <summary>
/// Manages <see cref="Engine.FixtureManager"/> for Xunit tests. <br/>
/// Enables the use of <see cref="TestContextExtensions.GetFeffFixture"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class TestFixturesExtensionAttribute : Attribute, IBeforeAfterTestAttribute, IAssemblyFixtureAttribute
{
    public Type AssemblyFixtureType => typeof(AssemblyTestTracker);

    // Before is called after TestClass ctor
    public void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
    }

    public void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        ScopeRemoverHelper.RemoveTestCaseScope(test);
    }
}