using System.Text;

namespace FEFF.TestFixtures.Xunit.V4.TestSubjects;

public static class Infrastructure
{
    public static string ResultName { get; } = GetFileName();
    public static string AssemblyName => System.Reflection.Assembly.GetExecutingAssembly().Location;

    private static readonly Lock _lock = new();

    private static string GetFileName()
    {
        var fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var d = fi.Directory!.FullName;
        return Path.Combine(d, "test-subject-result.txt");
    }

    public static void Add(string s)
    {
        lock(_lock)
        {
            File.AppendAllLines(ResultName, [s], Encoding.UTF8);
        }
    }
}
