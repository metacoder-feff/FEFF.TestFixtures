using System.Diagnostics;
using System.Text;
using AwesomeAssertions;
using AwesomeAssertions.Json;
using Newtonsoft.Json.Linq;

namespace FEFF.TestFixtures.XunitV4.Tests;

//TODO: test event order: https://tunit.dev/docs/writing-tests/event-subscribing/ 

public class TUnitIntegrationTests
{
    // Fixture creating is tested inside TestSubject
    [Test]
    public async Task Fixtures__should_be_disposed()
    {
        // Arrange
        var fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var d = fi.Directory!.FullName;
        // testSubject is referenced by this project, therefore it is built and copied here
        var testSubject = $"{d}/XunitV4.TestSubject.dll";
        // testSubject creates a file in its dir
        var resultFile = $"{d}/test-subject-result.json";

        TryDelete(resultFile);
        File.Exists(resultFile).Should().BeFalse();

        // Act
        var pi = new ProcessStartInfo("dotnet")
        {
            Arguments = testSubject
        };
        var p = Process.Start(pi);
        p.Should().NotBeNull();

        using (p)
        {
            await p.WaitForExitAsync();
            p.HasExited.Should().BeTrue();
            p.ExitCode.Should().Be(0);
        }

        // Assert
        File.Exists(resultFile).Should().BeTrue();

        // resultFile contains dispose calls per fixture counters
        var res = File.ReadAllText(resultFile, Encoding.UTF8);
        JToken.Parse(res)
            .Should().BeEquivalentTo(
        """
        {
            "TestFix":4,
            "ClassFix":2,
            "AssemblyFix":1,
            "SessionFix":1,
            "SingletonFix":1,
        }
        """);
    }

    private static void TryDelete(string f)
    {
        try
        {
            new FileInfo(f).Delete();
        }
        catch (FileNotFoundException)
        {
        }
    }
}