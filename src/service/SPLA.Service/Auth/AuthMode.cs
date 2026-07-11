using System.Security.Claims;

namespace SPLA.Service.Auth;

/// <summary>How the service host authenticates connections.</summary>
public enum AuthMode
{
    /// <summary>No authentication — loopback/embedded single-user use. The OS gates access.</summary>
    None,

    /// <summary>Domain auth: Negotiate (NTLM/Kerberos) → cookie. The identity is the OS principal.</summary>
    Negotiate,

    /// <summary>Local credentials: username/password validated against a server-side user store,
    /// with self-registration and an admin panel. Suited to home and small-workgroup deployments
    /// where there is no domain. OIDC later slots in as a third mode that fills the same cookie.</summary>
    Local
}

/// <summary>The claim types carried in the <c>spla.auth</c> cookie, shared by every auth mode so the
/// WebSocket upgrade reads identity the same way regardless of how the user signed in.</summary>
public static class AuthClaims
{
    /// <summary>Stable, filesystem-safe key that owns the user's area (SID for Negotiate,
    /// <c>local:&lt;guid&gt;</c> for local accounts).</summary>
    public const string UserKey = "spla.userkey";

    /// <summary>Group membership carried into <see cref="Domain.Identity.IIdentity.Groups"/> — the
    /// basis for future share-ACL checks. Populated from a local user's groups; empty for Negotiate
    /// today (group SIDs are re-resolved from the key when sharing lands).</summary>
    public const string Group = "spla.group";

    /// <summary>Role claim (uses the standard <see cref="ClaimTypes.Role"/> so <c>RequireRole</c>
    /// works). <c>admin</c> unlocks the admin panel.</summary>
    public const string Role = ClaimTypes.Role;
}
