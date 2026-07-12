using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;

namespace SPLA.Secrets.Dpapi;

/// <summary>
/// Registration entry point. App entry points call <see cref="Register"/> once at startup, before
/// the first <see cref="ConfigLoader.LoadAndResolve"/>, so the "dpapi" backend becomes selectable.
/// Keeping this here (rather than in each app) means the platform guard lives in exactly one place.
/// </summary>
public static class DpapiSecrets
{
    /// <summary>Wires the DPAPI backend into <see cref="ConfigLoader.SecretStoreFactory"/>. On non-Windows,
    /// or for any backend other than <c>dpapi</c>, returns null so the config loader falls back to the
    /// plaintext file store.</summary>
    public static void Register(Action<string>? warn = null)
    {
        ConfigLoader.SecretStoreFactory = (backend, workspace, machineDir) =>
        {
            if (!OperatingSystem.IsWindows()) return null;
            if (!string.Equals(backend, "dpapi", StringComparison.OrdinalIgnoreCase)) return null;
            return new DpapiFileSecretStore(workspace, machineDir, warn);
        };
    }
}
