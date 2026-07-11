using System.Text.Json.Serialization;

namespace SPLA.Service.Auth;

/// <summary>One local account in the server-side user store. Persisted as JSON; the password is only
/// ever stored as a PBKDF2 hash (never plaintext). Roles gate the admin panel; groups flow into the
/// connection identity for workgroup sharing.</summary>
public sealed class LocalUser
{
    /// <summary>Stable owner key for this user's storage area — <c>local:&lt;guid&gt;</c>, minted once
    /// and never changed even if the username is edited (mirrors the SID's role in domain mode).</summary>
    public string UserKey { get; set; } = "";

    /// <summary>Login name — unique (case-insensitive), the credential the user types.</summary>
    public string Username { get; set; } = "";

    /// <summary>Human-facing name shown in the UI.</summary>
    public string DisplayName { get; set; } = "";

    /// <summary>PBKDF2 hash produced by <see cref="Microsoft.AspNetCore.Identity.PasswordHasher{T}"/>.</summary>
    public string PasswordHash { get; set; } = "";

    /// <summary>Assigned roles. <c>admin</c> unlocks user management; <c>user</c> is the ordinary role.</summary>
    public List<string> Roles { get; set; } = [];

    /// <summary>Workgroup membership — carried into the connection identity's groups for sharing.</summary>
    public List<string> Groups { get; set; } = [];

    /// <summary>When false the account cannot sign in (admin can disable without deleting).</summary>
    public bool Enabled { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginUtc { get; set; }

    [JsonIgnore]
    public bool IsAdmin => Roles.Any(r => string.Equals(r, "admin", StringComparison.OrdinalIgnoreCase));
}
