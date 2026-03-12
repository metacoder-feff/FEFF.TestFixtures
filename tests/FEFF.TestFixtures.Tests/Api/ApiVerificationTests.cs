using PublicApiGenerator;
using VerifyTests.DiffPlex;

namespace FEFF.TestFixtures.Tests;

//ApiApprovalTests
public class ApiVerificationTests
{
    static ApiVerificationTests() => VerifyDiffPlex.Initialize(OutputType.Minimal);

    [Theory]
    [InlineData("FEFF.TestFixtures")]
    [InlineData("FEFF.TestFixtures.Xunit")]
    //[InlineData("FEFF.TestFixtures.AspNetCore")]
    public Task API_should_not_change(string assemblyName)
    {
//TODO: split tests ??
//TODO: .AutoVerify(includeBuildServer: false)

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
            ;

        if(IsCIEnv() == false)
            t = t.AutoVerify();

        return t;
    }
    
//TODO: utils
    public const string CiEnvVarName = "IS_CI_TEST";

    public static bool IsCIEnv() =>
        Environment.GetEnvironmentVariable(CiEnvVarName)?
        .ToLowerInvariant()
        == "true";
}