using System.Reflection;
using System.Runtime.Loader;

namespace SPLA.MCP.Core.Plugins;

/// <summary>
/// Isolated load context for a DLL plugin. Contract assemblies shared with the host
/// (<see cref="SharedAssemblies"/>) resolve to the host's already-loaded copy (return null →
/// fall back to the default context); everything else loads from the plugin's own folder via the
/// <see cref="AssemblyDependencyResolver"/>. This keeps a plugin's private dependencies from
/// colliding with the host's, while a plugin's <c>IMcpTool</c>/<c>ISplaPlugin</c> types still unify
/// with the host's because they come from the shared assemblies.
/// </summary>
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private static readonly HashSet<string> SharedAssemblies = new(StringComparer.OrdinalIgnoreCase)
    {
        "SPLA.Domain",
        "SPLA.MCP.Core",
        "SPLA.Observability"
    };

    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: false)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name != null && SharedAssemblies.Contains(assemblyName.Name))
        {
            return null;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
}
