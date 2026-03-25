using DiffEngine;
using PublicApiGenerator;
using VerifyTests.DiffPlex;

namespace FEFF.TestFixtures.Tests;

public class ApiVerificationTests
{
    static ApiVerificationTests() => VerifyDiffPlex.Initialize(OutputType.Minimal);
    
    [Fact]
    public Task VerifyXunit_checks_should_be_positive() => VerifyChecks.Run();

    [Theory]
    // this assemblies are referenced by project to set build order
    [InlineData("FEFF.TestFixtures")]
    [InlineData("FEFF.TestFixtures.XunitV3")]
    [InlineData("FEFF.TestFixtures.TUnit")]
    [InlineData("FEFF.TestFixtures.AspNetCore")]
    public Task API_should_not_change(string assemblyName)
    {
        // var assembly = AppDomain.CurrentDomain
        //     .GetAssemblies()
        //     .Single(x => x.GetName().Name == assemblyName)
        //     ;

        var loc = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var dirAsm = new FileInfo(loc).Directory?.FullName;
        var assembly = System.Reflection.Assembly.LoadFile($"{dirAsm}/{assemblyName}.dll");

        var publicApi = assembly.GeneratePublicApi(options: null);

        //.UseDirectory(Path.Combine("../../../Files/ApprovedApi", libName))
        var dir = Path.GetFullPath($"../../../../Files/API");
        if(Directory.Exists(dir) == false)
            Directory.CreateDirectory(dir);

        var filePrefix = assemblyName;

        var t = Verifier
            .Verify(publicApi)
            .ScrubLinesContaining("InternalsVisibleTo(\"FEFF.TestFixtures.Tests\")")
            .UseDirectory(dir)
            .UseFileName(filePrefix)
            //.DisableDiff()
//TODO: split tests ??
            .AutoVerifyWhenLocalDevelopment()
            ;

        return t;
    }
    
}

//TODO: utils
internal static class VerifyExtentions
{
    public static bool IsCI() => BuildServerDetector.IsGitLab || BuildServerDetector.IsGithubAction;
    
    public static bool IsLocalDev()
    {
//TODO: other env
        var e = Environment.GetEnvironmentVariable("REMOTE_CONTAINERS");
        if(e == null)
            return false;
        return e.Equals("true", StringComparison.InvariantCultureIgnoreCase);
    }

    // WORKAROUND:
    // AutoVerify(includeBuildServer: false)
    // uses BuildServerDetector.Detected
    // witch uses BuildServerDetector.IsDocker - not compatible with devcontainers
    // internal static SettingsTask AutoVerifyIfNotCI(this SettingsTask src)
    // {
    //     if(IsCI() == true)
    //         return src;
    //     return src.AutoVerify();
    // }

    internal static SettingsTask AutoVerifyWhenLocalDevelopment(this SettingsTask src)
    {
        if(IsLocalDev() == false)
            return src;

        return src.AutoVerify();
    }
}