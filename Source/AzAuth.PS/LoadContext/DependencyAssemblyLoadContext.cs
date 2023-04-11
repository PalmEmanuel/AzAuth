using System.Reflection;
using System.Runtime.Loader;

namespace AzAuth.PS.LoadContext;

public class DependencyAssemblyLoadContext : AssemblyLoadContext
{
    private readonly string dependenciesDirectory;

    // Save the full path to the dependencies directory when creating the context
    public DependencyAssemblyLoadContext(string path) => dependenciesDirectory = path;
    
    protected override Assembly Load(AssemblyName assemblyName)
    {
        // Create a path to the assembly in the dependencies directory
        string assemblyPath = Path.Combine(
            dependenciesDirectory,
            $"{assemblyName.Name}.dll");

        // Make sure the assembly exists in the directory before attempting to load it
        // Otherwise we will try to load things like the netstandard assembly directly
        if (File.Exists(assemblyPath))
        {
            // Use an inherited method for loading
            // A static Load method from the Assembly class would load the assembly into the shared context
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null!;
    }
}