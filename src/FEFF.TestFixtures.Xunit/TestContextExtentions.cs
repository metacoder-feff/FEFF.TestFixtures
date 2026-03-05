using FEFF.TestFixtures;
using Xunit.Sdk;

namespace Xunit.v3;

/*
XUnit pipeline:
create XunitTestCase[]
---
foreach (var testCase in testCasesToRun)
---
 create test-class instance
  IBeforeAfterTestAttribute.Before
    RUN TEST
  IBeforeAfterTestAttribute.After
 test-class.DisposeAsync()
---
foreach (var testCase in testCasesToRun)
---
-> XunitTestCase.DisposeAsync()
-> DisposalTracker.DisposeAsync()
*/

internal class DisposalTrackerAdapter<T> : IAsyncDisposable
where T  : IAsyncDisposable
{
//TODO: func OnDispose?? 
    public string Key { get; }
    public T Disposable { get; }

    public DisposalTrackerAdapter(string key, T disposable)
    {
        Key = key;
        Disposable = disposable;
    }

    public ValueTask DisposeAsync()
    {
        TestContext.Current.KeyValueStorage.TryRemove(Key, out _);

        return Disposable.DisposeAsync();
    }
}

public static class TestContextExtentions
{
    public static IFixtureProvider GetTestCaseFixtureProvider(this ITestContext ctx)
    {
//TODO: Argument exceptions
        var testCase = ctx.TestCase as XunitTestCase;
        ThrowHelper.Assert(testCase != null);

//TODO: Test TestEngineStatus.Initializing
        ThrowHelper.Assert(ctx.TestStatus == TestEngineStatus.Initializing || ctx.TestStatus == TestEngineStatus.Running);
        //ThrowHelper.Assert(ctx.TestStatus == TestEngineStatus.Running);
        var test = ctx.Test;
        ThrowHelper.Assert(test != null);

        var k = GetKey(test);

//TODO: optimize (remove closure)        
        var obj = ctx.KeyValueStorage.GetOrAdd(k, (key) => 
        {
            var res = new FixtureContainer();
            var adapter = new DisposalTrackerAdapter<FixtureContainer>(key, res);
            testCase.DisposalTracker.Add(adapter);
            return res;
        });

//TODO: utils Cast()
        if(obj is not FixtureContainer res)
            throw new InvalidOperationException($"Stored object is not a {nameof(FixtureContainer)}");

        return res;
    }
    
//TODO: more unique
    private static string GetKey(ITestMetadata test)
    {
        return $"{nameof(DisposalTrackerAdapter<>)}<{nameof(FixtureContainer)}>-{test.UniqueID}";
    }
}