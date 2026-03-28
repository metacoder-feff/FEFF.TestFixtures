using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace FEFF.TestFixtures.Engine;

internal class AssemblyDiscoverer
{
    public static List<Assembly> GetAssemblies() => 
        new AssemblyDiscoverer().GetAssembliesInt();

    // ecapsulate
    private AssemblyDiscoverer()
    {
    }

    // "FEFF.TestFixtures.Abstractions"
    private readonly string _mainFixtureAssemblyName = ThrowHelper.EnsureNotNull(
        typeof(FixtureAttribute).Assembly.GetName().Name
    );
    
    private readonly HashSet<string> _visitedNames = [];

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
        // PATHS
        var referencedNonLoadedAssemblyPaths = allLoaded
            .SelectMany(GetRefLocationsAndAppendVisited)
            .ToList() // need materialize
            ;

        // AssemblyName
        var refsToLoad = GetRefsToLoad(referencedNonLoadedAssemblyPaths);

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
            var loc = resolver.ResolveAssemblyToPath(r);
            if(loc == null)
                continue; //TODO:??

            yield return loc;
        }
    }

    // input Assembly Location
    // apply 'AssemblyFilter' to referencedNonLoadedAssemblyPaths reading its metadata
    // returns Assembly ref
    private List<AssemblyName> GetRefsToLoad(IEnumerable<string> referencedNonLoadedAssemblyPaths)
    {
//TODO: filter referencedMetaAssms by containing types (attribute & interface)
//TODO: DRY
        var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
        var paths = runtimeAssemblies
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