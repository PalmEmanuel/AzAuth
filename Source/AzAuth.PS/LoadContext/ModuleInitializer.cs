using System.Management.Automation;
using System.Reflection;
using System.Runtime.Loader;

namespace PipeHow.AzAuth.LoadContext;

public class ModuleAssemblyContextHandler : IModuleAssemblyInitializer
{
    // Get the path of the dependencies directory relative to the module file
    private static readonly string dependencyDirPath = Path.GetFullPath(
        Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "dependencies"));

    // Create the custom load context to use, with the path to the dependencies directory
    private static readonly DependencyAssemblyLoadContext dependencyLoadContext = new (dependencyDirPath);

    // This will run when the module is imported
    public void OnImport() =>
        // Hook up our own assembly resolving method
        // It will run when the default load context fails to resolve an assembly
        AssemblyLoadContext.Default.Resolving += ResolveAssembly;

    private static Assembly? ResolveAssembly(AssemblyLoadContext defaultAlc, AssemblyName assemblyToResolve) =>
        assemblyToResolve.Name == "AzAuth.Core" ?
        dependencyLoadContext.LoadFromAssemblyName(assemblyToResolve) :
        null;
}