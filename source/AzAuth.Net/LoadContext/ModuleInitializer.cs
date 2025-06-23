using System.Management.Automation;
using System.Reflection;
using System.Runtime.Loader;

namespace PipeHow.AzAuth.LoadContext;

public class ModuleAssemblyContextHandler : IModuleAssemblyInitializer
{
    
    // This will run when the module is imported
    public void OnImport() =>
        // Hook up our own assembly resolving method
        // It will run when the default load context fails to resolve an assembly
        AssemblyLoadContext.Default.Resolving += ResolveAssembly;

    private static Assembly? ResolveAssembly(AssemblyLoadContext defaultAlc, AssemblyName assemblyToResolve) =>
        assemblyToResolve.Name == "AzAuth.Net" ?
        dependencyLoadContext.LoadFromAssemblyName(assemblyToResolve) :
        null;
}