using System.IO;
using System.Reflection;

namespace SPLA.Domain.Identity;

/// <summary>Config for the pluggable identity provider DLL — an assembly + type name, wired explicitly
/// (e.g. from server.json). Absent/empty means "use the neutral built-in provider".</summary>
public sealed class IdentityProviderConfig
{
    /// <summary>Assembly to load — a file name resolved against the host's base dir, or an absolute path.</summary>
    public string? Assembly { get; set; }

    /// <summary>Full type name of the <see cref="IIdentityProvider"/> implementation to instantiate.</summary>
    public string? Type { get; set; }
}

/// <summary>
/// Loads the configured <see cref="IIdentityProvider"/> by assembly + type via reflection, or returns
/// the neutral <see cref="ClaimsIdentityProvider"/> when unconfigured. This is the whole point of the
/// pluggable design: the host has NO reference to any platform assembly — to target Windows you drop
/// in <c>SPLA.Identity.Windows.dll</c> and name it in config; to target Linux you name a different DLL.
/// The host binary and code never change, and never depend on a platform.
/// </summary>
public static class IdentityProviderLoader
{
    public static IIdentityProvider Load(IdentityProviderConfig? config, string baseDir)
    {
        if (string.IsNullOrWhiteSpace(config?.Assembly) || string.IsNullOrWhiteSpace(config.Type))
            return new ClaimsIdentityProvider();

        var path = Path.IsPathRooted(config.Assembly)
            ? config.Assembly
            : Path.Combine(baseDir, config.Assembly);

        var asm = Assembly.LoadFrom(path);
        var type = asm.GetType(config.Type, throwOnError: true)!;
        return (IIdentityProvider)Activator.CreateInstance(type)!;
    }
}
