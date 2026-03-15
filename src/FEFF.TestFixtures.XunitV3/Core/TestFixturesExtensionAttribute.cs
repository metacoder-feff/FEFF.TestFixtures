using System.Reflection;
using Xunit.v3;

namespace FEFF.TestFixtures.Xunit;

/// <summary>
/// Manages <see cref="FixtureScopeManager"/> for XUnit tests. <br/>
/// Allows to use <see cref="TestContextExtentions.GetFeffFixture"/>.
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