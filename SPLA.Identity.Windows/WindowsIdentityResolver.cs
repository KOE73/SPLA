using System.Runtime.Versioning;
using System.Security.Principal;
using SPLA.Domain.Identity;
using IIdentity = SPLA.Domain.Identity.IIdentity;

namespace SPLA.Identity.Windows;

/// <summary>
/// Fills the platform-neutral <see cref="IIdentity"/> from a Windows principal. On the server the
/// principal comes from Negotiate (NTLM/Kerberos) auth on the connection handshake — no passwords in
/// config; here <see cref="Current"/> resolves the running process's token, which is what the tests
/// use to prove real domain identity end to end.
///
/// UserKey = account SID (stable across renames, filesystem-safe). Groups = group SIDs, so sharing
/// compares SID-to-SID and is immune to group renames.
/// </summary>
[SupportedOSPlatform("windows")]
public static class WindowsIdentityResolver
{
    public static IIdentity Resolve(WindowsIdentity windows)
    {
        var userKey = windows.User?.Value
            ?? throw new InvalidOperationException("Windows identity has no user SID.");

        var groups = windows.Groups is { } g
            ? g.Select(r => r.Value).ToArray()
            : Array.Empty<string>();

        return new ClaimIdentity(userKey, windows.Name, groups);
    }

    /// <summary>The current process/thread's Windows identity — used by tests and single-user hosts.</summary>
    public static IIdentity Current() => Resolve(WindowsIdentity.GetCurrent());
}
