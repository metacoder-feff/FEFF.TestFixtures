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
    [InlineData("FEFF.TestFixtures")]
    [InlineData("FEFF.TestFixtures.Xunit")]
    //[InlineData("FEFF.TestFixtures.AspNetCore")]
    public Task API_should_not_change(string assemblyName)
    {

        var assembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .Single(x => x.GetName().Name == assemblyName)
            ;

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
            .AutoVerifyIfNotCI()
            ;

        return t;
    }
    
}

//TODO: utils
internal static class VerifyExtentions
{
    public static bool IsCI() => BuildServerDetector.IsGitLab || BuildServerDetector.IsGithubAction;

    // WORKAROUND:
    // AutoVerify(includeBuildServer: false)
    // uses BuildServerDetector.Detected
    // witch uses BuildServerDetector.IsDocker - not compatible with devcontainers
    internal static SettingsTask AutoVerifyIfNotCI(this SettingsTask src)
    {
        if(IsCI() == true)
            return src;

        return src.AutoVerify();
    }
}