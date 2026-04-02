using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace FEFF.TestFixtures.Engine;

internal class AssemblyDiscoverer
{
    public static List<Assembly> GetAssemblies() => 
        new AssemblyDiscoverer().GetAssembliesInt();

    //---------------------------
    // encapsulate state
    //---------------------------

    private readonly HashSet<string> _visitedNames = [];
    // "FEFF.TestFixtures.Abstractions"
    private readonly string _mainFixtureAssemblyName;
    private readonly ImmutableArray<string> _defaultAssemblyFiles;

    private AssemblyDiscoverer()
    {
        var mainFixtureAssembly = typeof(FixtureAttribute).Assembly;
        _mainFixtureAssemblyName = ThrowHelper.EnsureNotNull(
            mainFixtureAssembly.GetName().Name
        );

        var localDir = ThrowHelper.EnsureNotNull(
            Path.GetDirectoryName(mainFixtureAssembly.Location)
        );
        var localAssemblies = Directory.GetFiles(localDir, "*.dll");

        var runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
        var runtimeAssemblies = Directory.GetFiles(runtimeDir, "*.dll");
        _defaultAssemblyFiles = [.. runtimeAssemblies, .. localAssemblies];
    }

    private static bool AssemblyNameFilter(AssemblyName an)
    {
        if(an.Name == null)
            return false;

        var name = an.Name;
        // Skip system and framework assemblies
        if (name.StartsWith("System.") ||
            // name.StartsWith("Microsoft.") ||
            name == "mscorlib" ||
            name == "netstandard" ||
            name == "testhost")
        {
            return false;
        }

        return true;
    }

    //HasDirectReferenceToMainFixtureAssembly
    private bool AssemblyFilter(Assembly a) => a
        .GetReferencedAssemblies()
        .Where(AssemblyNameFilter)
        .Select(x => x.Name)
        .Contains(_mainFixtureAssemblyName)
        ;

    private List<Assembly> GetAssembliesInt()
    {
        var allLoaded = AppDomain.CurrentDomain
            .GetAssemblies()
            ;

        var loadedResult = allLoaded
            .Where(AssemblyFilter)
            // .ToList()
            ;

        _visitedNames.UnionWith(allLoaded
            .Select(x => x.GetName())
            .Where(AssemblyNameFilter)
            .Select(x => x.Name)
            .WhereNotNull()
        );

        var referencedResult = DiscoverAndLoadReferencedAssemblies(allLoaded)
            // .ToList()
            ;

        var res = loadedResult
            .Concat(referencedResult)
            .ToList()
            ;

        return res;    
    }

    /*
        Loaded_Assembly (1) -> (n) Ref_AssemblyName

        Ref_AssemblyName -> Ref_Path (string)

        Ref_Path (n) 
            -> (n) Ref_Meta_Assembly
            -> [Filter]
            -> (n) Ref_Filtered_AssemblyName
    
        Ref_Filtered_AssemblyName -> Force_Loaded_Assembly
    */
    private IEnumerable<Assembly> DiscoverAndLoadReferencedAssemblies(Assembly[] allLoaded)
    {
        // 'Path'
        var referencedNonLoadedAssemblyPaths = allLoaded
            .SelectMany(GetRefLocationsAndAppendVisited)
            .ToList() // need materialize
            ;

        // 'AssemblyName'
        var refsToLoad = GetRefsToLoad(referencedNonLoadedAssemblyPaths);

        // 'Assembly'
        var referencedRealAssms = LoadRefs(refsToLoad);

        if (referencedRealAssms.Length <= 0)
            return referencedRealAssms;

        // discover recursively refs of refs
        var recursiveAssemblies =  DiscoverAndLoadReferencedAssemblies(referencedRealAssms);
        return referencedRealAssms.Concat(recursiveAssemblies);

//TODO: discover recursively all refs, not only Ref_Filtered_AssemblyName?
    }

    // FORCE
    // load referenced but not loaded yet assemblies
    // that has DIRECT reference to 'FEFF.TestFixtures.Abstractions'
    // ATTENTION: load by AssemblyName is the correct way
    // (loading by filePath causes to load multiple Assembly instances)
    private static Assembly[] LoadRefs(List<AssemblyName> refsToLoad)
    {
        var res = new Assembly[refsToLoad.Count];
        for(var i = 0; i < refsToLoad.Count; i++)
            res[i] = Assembly.Load(refsToLoad[i]);
        
        return res;
    }

    private IEnumerable<string> GetRefLocationsAndAppendVisited(Assembly a)
    {
        // resolve from Assembly to its deps
        var resolver = new AssemblyDependencyResolver(a.Location);
        var refs = a
            .GetReferencedAssemblies()
            .Where(AssemblyNameFilter)
            ;

        foreach(var r in refs)
        {
            if(r.Name == null)
                continue; //TODO:??
            if(_visitedNames.Contains(r.Name))
                continue;

            // AppendVisited
            _visitedNames.Add(r.Name);

            // get referenced Assembly Loacation based on "deps.json"
            // witch is located near or embedded into the source Assembly
            var loc = resolver.ResolveAssemblyToPath(r);
            if(loc == null)
                continue; //TODO: deps.json not found??

                // e.g.:
                // "Microsoft.AspNetCore"
                // "Microsoft.AspNetCore.Routing"
                // "Microsoft.Win32.Registry"
                // "Microsoft.AspNetCore.Hosting.Abstractions"
                // "Microsoft.Extensions.Logging"
                // "Microsoft.Extensions.Logging.Abstractions"

            yield return loc;
        }
    }

    // input Assembly Location
    // apply 'AssemblyFilter' to referencedNonLoadedAssemblyPaths reading its metadata
    // returns Assembly ref
    private List<AssemblyName> GetRefsToLoad(IEnumerable<string> referencedNonLoadedAssemblyPaths)
    {
//TODO: filter referencedMetaAssms by containing types (attribute & interface)

        var paths = _defaultAssemblyFiles
            .Concat(referencedNonLoadedAssemblyPaths)
            // .ToList()
            ;

        var resolver = new PathAssemblyResolver(paths);
        using var mlc = new MetadataLoadContext(resolver);

        // referencedMetaAssms will be disposed with MetadataLoadContext
        var referencedMetaAssms = referencedNonLoadedAssemblyPaths
            .Select(mlc.LoadFromAssemblyPath)
            .Where(AssemblyFilter)
            // .ToList()
            ;

        // Call .ToList() to materialize referencedRealAssms during mlc is alive
        return referencedMetaAssms
            .Select(x => x.GetName())
            .ToList()
            ;
    }
}