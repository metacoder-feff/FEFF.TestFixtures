
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FEFF.TestFixtures.TUnit.Tests;

public class Infrastructure
{
    private static ConcurrentDictionary<string, int> _result = [];

    public static void Add(string s)
    {
        _result.AddOrUpdate(s, 1, (_, prev) => prev + 1);
    }

    // Dispose _manager here.
    [After(TestSession)]
    public async static Task AfterS(TestSessionContext ctx)
    {
        var s = JsonSerializer.Serialize(_result);

        var fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var d = fi.Directory!.FullName;
        File.WriteAllText($"{d}/test-subject-result.json", s, Encoding.UTF8);
    }
}