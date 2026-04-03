namespace FEFF.TestFixtures.AspNetCore;

public interface ITestApplicationExtension
{
    void Configure(ITestApplicationFixture app);
}

public static class TestApplicationFixtureExtensions
{
    // Chain result configuration of 'ITestApplicationFixture'
    public static ITestApplicationFixture AttachExtensions(this ITestApplicationFixture src, params ITestApplicationExtension[] extensions)
    {
        foreach(var e in extensions)
            e.Configure(src);
        
        return src;
    }
}