using System.Reflection;
using Newtonsoft.Json.Linq;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace FEFF.TestFixtures.Tests;

public class XunitIntegrationTests
{   
    [Fact]
    public async ValueTask Fixtures__after_their_scopes_end__should_be_disposed()
    {
        var results = await RunAsync(typeof(TestSubject));

        var filtered = results.Select(x => x switch
        {
            IDiagnosticMessage dm => $"{x.GetType().Name}: {dm.Message}",
            _ => x.GetType().Name
        });

        // Assert that the result message stream should contain some messages in strict order
        JToken.FromObject(filtered)
            .Should().ContainSubtree(
        """
        [
            "AfterTestStarting",
            
            // !!
            "DiagnosticMessage: disposed TestFix",
            "AfterTestFinished",

            "TestPassed",
            "TestFinished",
            "TestCaseFinished",
            "TestMethodFinished",

            // !!
            "DiagnosticMessage: disposed ClassFix",
            "TestClassFinished",

            // !!
            "DiagnosticMessage: disposed CollectionFix",
            "TestCollectionFinished",

            // !!
            "DiagnosticMessage: disposed AssemblyFix",
            "TestAssemblyFinished"
        ]
        """);
    }

    public static ValueTask<List<IMessageSinkMessage>> RunAsync(
        Type type,
        bool preEnumerateTheories = true,
        ExplicitOption? explicitOption = null,
        IMessageSink? diagnosticMessageSink = null) =>
            RunAsync([type], preEnumerateTheories, explicitOption, diagnosticMessageSink);



    public static ValueTask<List<IMessageSinkMessage>> RunAsync(
        Type[] types,
        bool preEnumerateTheories = true,
        ExplicitOption? explicitOption = null,
        IMessageSink? diagnosticMessageSink = null)
    {
        var runSink = new SpyMessageSink();
        diagnosticMessageSink ??= runSink;

        var tcs = new TaskCompletionSource<List<IMessageSinkMessage>>();

        ThreadPool.QueueUserWorkItem(async _ =>
        {
            TestContext.SetForInitialization(diagnosticMessageSink, diagnosticMessages: diagnosticMessageSink is not null, internalDiagnosticMessages: diagnosticMessageSink is not null);

            try
            {

                await using var testFramework = new XunitTestFramework();

                var testAssembly = Assembly.GetEntryAssembly()!;
                var discoverer = testFramework.GetDiscoverer(testAssembly);
                var testCases = new List<ITestCase>();
                await discoverer.Find(testCase => { testCases.Add(testCase); return new(true); }, TestFrameworkDiscoveryOptions(preEnumerateTheories: preEnumerateTheories), types);

                var executor = testFramework.GetExecutor(testAssembly);
                await executor.RunTestCases(testCases, runSink, TestFrameworkExecutionOptions(explicitOption: explicitOption));

                foreach (var testCase in testCases)
                    if (testCase is IAsyncDisposable asyncDisposable)
                        await asyncDisposable.DisposeAsync();
                    else if (testCase is IDisposable disposable)
                        disposable.Dispose();

                tcs.TrySetResult(runSink.Messages.ToList());
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
//TODO: revert old CTX            
            // finally
            // {
            //     try
            //     {
            //         TestContextInternal.Current.Dispose();
            //     }
            //     catch { }
            // }
        });

        return new(tcs.Task);
    }

	public static ITestFrameworkDiscoveryOptions TestFrameworkDiscoveryOptions(
		string? culture = null,
		bool? diagnosticMessages = null,
		bool? includeSourceInformation = null,
		bool? internalDiagnosticMessages = null,
		TestMethodDisplay? methodDisplay = null,
		TestMethodDisplayOptions? methodDisplayOptions = null,
		bool? preEnumerateTheories = null,
		int? printMaxEnumerableLength = null,
		int? printMaxObjectDepth = null,
		int? printMaxObjectMemberCount = null,
		int? printMaxStringLength = null,
		bool? synchronousMessageReporting = null)
	{
		ITestFrameworkDiscoveryOptions result = TestFrameworkOptions.Empty();

		result.SetCulture(culture);
		result.SetDiagnosticMessages(diagnosticMessages);
		result.SetIncludeSourceInformation(includeSourceInformation);
		result.SetInternalDiagnosticMessages(internalDiagnosticMessages);
		result.SetMethodDisplay(methodDisplay);
		result.SetMethodDisplayOptions(methodDisplayOptions);
		result.SetPreEnumerateTheories(preEnumerateTheories);
		result.SetPrintMaxEnumerableLength(printMaxEnumerableLength);
		result.SetPrintMaxObjectDepth(printMaxObjectDepth);
		result.SetPrintMaxObjectMemberCount(printMaxObjectMemberCount);
		result.SetPrintMaxStringLength(printMaxStringLength);
		result.SetSynchronousMessageReporting(synchronousMessageReporting);

		return result;
	}

	public static ITestFrameworkExecutionOptions TestFrameworkExecutionOptions(
		int? assertEquivalentMaxDepth = null,
		string? culture = null,
		bool? diagnosticMessages = null,
		bool? disableParallelization = null,
		ExplicitOption? explicitOption = null,
		bool? failSkips = null,
		bool? failTestsWithWarnings = null,
		bool? internalDiagnosticMessages = null,
		int? maxParallelThreads = null,
		ParallelAlgorithm? parallelAlgorithm = null,
		int? printMaxEnumerableLength = null,
		int? printMaxObjectDepth = null,
		int? printMaxObjectMemberCount = null,
		int? printMaxStringLength = null,
		int? seed = null,
		bool? showLiveOutput = null,
		bool? stopOnFail = null,
		bool? synchronousMessageReporting = null)
	{
		ITestFrameworkExecutionOptions result = TestFrameworkOptions.Empty();

		result.SetAssertEquivalentMaxDepth(assertEquivalentMaxDepth);
		result.SetCulture(culture);
		result.SetDiagnosticMessages(diagnosticMessages);
		result.SetDisableParallelization(disableParallelization);
		result.SetExplicitOption(explicitOption);
		result.SetFailSkips(failSkips);
		result.SetFailTestsWithWarnings(failTestsWithWarnings);
		result.SetInternalDiagnosticMessages(internalDiagnosticMessages);
		result.SetMaxParallelThreads(maxParallelThreads);
		result.SetParallelAlgorithm(parallelAlgorithm);
		result.SetPrintMaxEnumerableLength(printMaxEnumerableLength);
		result.SetPrintMaxObjectDepth(printMaxObjectDepth);
		result.SetPrintMaxObjectMemberCount(printMaxObjectMemberCount);
		result.SetPrintMaxStringLength(printMaxStringLength);
		result.SetSeed(seed);
		result.SetShowLiveOutput(showLiveOutput);
		result.SetStopOnTestFail(stopOnFail);
		result.SetSynchronousMessageReporting(synchronousMessageReporting);

		return result;
	}
}

public class SpyMessageSink : IMessageSink
{
    public List<IMessageSinkMessage> Messages { get; } = [];

    public virtual bool OnMessage(IMessageSinkMessage message)
    {
        Messages.Add(message);
        return  true;
    }
}