namespace SPLA.Domain.Identity;

/// <summary>
/// Who a call is being made on behalf of. Platform-neutral on purpose: the core never mentions
/// Windows, NTLM or Kerberos — a Windows resolver fills this from a <c>WindowsPrincipal</c> today,
/// a Linux/Kerberos or token resolver can fill the same shape later without touching the core.
/// </summary>
public interface IIdentity
{
    /// <summary>Stable, filesystem-safe key that owns this user's area. On Windows = the account
    /// SID (survives renames); locally = a fixed single-user key. Never a display string.</summary>
    string UserKey { get; }

    /// <summary>Human-facing name (<c>DOMAIN\user</c> on Windows). For display only, never for keys.</summary>
    string DisplayName { get; }

    /// <summary>Group keys this user belongs to (group SIDs on Windows) — what sharing checks against.</summary>
    IReadOnlyCollection<string> Groups { get; }
}

/// <summary>Plain carrier for an identity, produced by whatever resolver the host uses.</summary>
public sealed record ClaimIdentity(
    string UserKey,
    string DisplayName,
    IReadOnlyCollection<string> Groups) : IIdentity;

/// <summary>
/// The implicit single user for local / embedded runs, where there is no server auth. Keeps the
/// core identity-aware everywhere so the server path is not a special case — it just swaps the
/// resolver.
/// </summary>
public static class LocalIdentity
{
    public static readonly IIdentity Single =
        new ClaimIdentity("local", System.Environment.UserName, System.Array.Empty<string>());
}
