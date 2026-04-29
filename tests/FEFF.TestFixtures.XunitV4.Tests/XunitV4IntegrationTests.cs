using System.Diagnostics;
using System.Text;
using AwesomeAssertions;
using AwesomeAssertions.Json;
using Xunit;

namespace FEFF.TestFixtures.XunitV4.Tests;

public class XunitV4IntegrationTests
{
    // Fixture creating is tested inside TestSubject
    [Fact]
    public async Task Fixtures__should_be_disposed()
    {
        // Arrange
        var resultFile = TestSubjects.Infrastructure.ResultName;
        var testSubject = TestSubjects.Infrastructure.AssemblyName;

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
            await p.WaitForExitAsync(TestContext.Current.CancellationToken);
            p.HasExited.Should().BeTrue();
            p.ExitCode.Should().Be(0);
        }

        // Assert
        File.Exists(resultFile).Should().BeTrue();
        var res = File.ReadAllLines(resultFile, Encoding.UTF8);

        // Assert that all fixures are disposed (and created)
        // order may varry because tests run in parallel
        res.Should().BeEquivalentTo(
        [
            "TestFix:{'test':'TestMethod_1','collection':'','class':'TestSubject'}", 
            "TestFix:{'test':'TestMethod_1','collection':'collecion-a','class':'SecondTestSubject'}", 
            "TestFix:{'test':'TestMethod_2','collection':'','class':'TestSubject'}", 
            "TestFix:{'test':'TestMethod_2','collection':'collecion-a','class':'SecondTestSubject'}", 
            "ClassFix:{'collection':'','class':'TestSubject'}", 
            "ClassFix:{'collection':'collecion-a','class':'SecondTestSubject'}", 
            "CollectionFix:{'collection':''}", // default collection for 'TestSubject'
            "TestFix:{'test':'TestMethod_2','collection':'collecion-a','class':'ThirdTestSubject'}", 
            "TestFix:{'test':'TestMethod_1','collection':'collecion-a','class':'ThirdTestSubject'}", 
            "ClassFix:{'collection':'collecion-a','class':'ThirdTestSubject'}", 
            "CollectionFix:{'collection':'collecion-a'}", 
            "AssemblyFix:{}", 
            "SingletonTester:{}",
        ]);

        // Assert that fixture disposal order depends on FixtureScope
        // strict order checking
        res.Should().ContainInOrder(
        [
            "TestFix:{'test':'TestMethod_1','collection':'collecion-a','class':'ThirdTestSubject'}", 
            "ClassFix:{'collection':'collecion-a','class':'ThirdTestSubject'}", 
            "CollectionFix:{'collection':'collecion-a'}", 
            "AssemblyFix:{}", 
            "SingletonTester:{}",
        ]);


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
