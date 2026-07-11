using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SPLA.Service.Auth;

/// <summary>The outcome of an account operation: success, or a human-readable reason it was refused.</summary>
public readonly record struct AccountResult(bool Ok, string? Error, LocalUser? User = null)
{
    public static AccountResult Success(LocalUser? user = null) => new(true, null, user);
    public static AccountResult Fail(string error) => new(false, error);
}

/// <summary>
/// The heart of local-credentials auth: registration, credential validation (with transparent
/// re-hash on algorithm upgrades), and the admin operations the panel drives (create, reset password,
/// enable/disable, set roles/groups, delete). Password hashing uses the first-party
/// <see cref="PasswordHasher{T}"/> (PBKDF2) — no third-party crypto. All persistence goes through
/// <see cref="IUserStore"/>; this class holds the policy (validation rules, the last-admin guard) and
/// seeds a first admin when the store is empty so a fresh deployment is reachable.
/// </summary>
public sealed class LocalAccountService
{
    private const int MinUsernameLength = 3;
    private const int MinPasswordLength = 6;

    private readonly IUserStore _store;
    private readonly PasswordHasher<LocalUser> _hasher = new();
    private readonly ILogger? _logger;

    /// <summary>Whether the public <c>/register</c> page is offered. When false, only an admin creates
    /// accounts (from the panel).</summary>
    public bool AllowSelfRegistration { get; }

    public LocalAccountService(IUserStore store, bool allowSelfRegistration, ILogger? logger = null)
    {
        _store = store;
        AllowSelfRegistration = allowSelfRegistration;
        _logger = logger;
        EnsureSeedAdmin();
    }

    public IReadOnlyList<LocalUser> ListUsers() => _store.All();

    public LocalUser? FindByKey(string userKey) => _store.FindByKey(userKey);

    /// <summary>Creates an account. <paramref name="roles"/>/<paramref name="groups"/> are honoured only
    /// on the admin path; self-registration passes null and gets the ordinary <c>user</c> role.</summary>
    public AccountResult Register(
        string username, string password, string? displayName,
        IEnumerable<string>? roles = null, IEnumerable<string>? groups = null)
    {
        username = (username ?? "").Trim();
        if (username.Length < MinUsernameLength)
            return AccountResult.Fail($"Username must be at least {MinUsernameLength} characters.");
        if (password is null || password.Length < MinPasswordLength)
            return AccountResult.Fail($"Password must be at least {MinPasswordLength} characters.");
        if (_store.FindByUsername(username) != null)
            return AccountResult.Fail("That username is already taken.");

        var user = new LocalUser
        {
            UserKey = $"local:{Guid.NewGuid():N}",
            Username = username,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? username : displayName!.Trim(),
            Roles = NormalizeRoles(roles),
            Groups = NormalizeGroups(groups),
            Enabled = true
        };
        user.PasswordHash = _hasher.HashPassword(user, password);
        _store.Upsert(user);
        _logger?.LogInformation("Local account created. User={Username} Admin={IsAdmin}", user.Username, user.IsAdmin);
        return AccountResult.Success(user);
    }

    /// <summary>Verifies a credential and, on success, stamps the last-login time. Returns null for an
    /// unknown user, a disabled account, or a wrong password.</summary>
    public LocalUser? Validate(string username, string password)
    {
        var user = _store.FindByUsername((username ?? "").Trim());
        if (user is null || !user.Enabled) return null;

        var outcome = _hasher.VerifyHashedPassword(user, user.PasswordHash, password ?? "");
        if (outcome == PasswordVerificationResult.Failed) return null;

        if (outcome == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = _hasher.HashPassword(user, password!);

        user.LastLoginUtc = DateTime.UtcNow;
        _store.Upsert(user);
        return user;
    }

    /// <summary>Changes a user's password (self-service or admin). Enforces the length rule.</summary>
    public AccountResult SetPassword(string userKey, string newPassword)
    {
        var user = _store.FindByKey(userKey);
        if (user is null) return AccountResult.Fail("User not found.");
        if (newPassword is null || newPassword.Length < MinPasswordLength)
            return AccountResult.Fail($"Password must be at least {MinPasswordLength} characters.");

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        _store.Upsert(user);
        return AccountResult.Success(user);
    }

    /// <summary>Verifies a user's current password by key — used by self-service password change.</summary>
    public bool VerifyPassword(string userKey, string password)
    {
        var user = _store.FindByKey(userKey);
        if (user is null || !user.Enabled) return false;
        return _hasher.VerifyHashedPassword(user, user.PasswordHash, password ?? "") != PasswordVerificationResult.Failed;
    }

    public AccountResult SetEnabled(string userKey, bool enabled)
    {
        var user = _store.FindByKey(userKey);
        if (user is null) return AccountResult.Fail("User not found.");
        if (!enabled && user.IsAdmin && IsLastAdmin(user))
            return AccountResult.Fail("Cannot disable the last remaining admin.");

        user.Enabled = enabled;
        _store.Upsert(user);
        return AccountResult.Success(user);
    }

    public AccountResult SetRoles(string userKey, IEnumerable<string> roles)
    {
        var user = _store.FindByKey(userKey);
        if (user is null) return AccountResult.Fail("User not found.");

        var next = NormalizeRoles(roles);
        var losesAdmin = user.IsAdmin && !next.Any(r => string.Equals(r, "admin", StringComparison.OrdinalIgnoreCase));
        if (losesAdmin && IsLastAdmin(user))
            return AccountResult.Fail("Cannot demote the last remaining admin.");

        user.Roles = next;
        _store.Upsert(user);
        return AccountResult.Success(user);
    }

    public AccountResult SetGroups(string userKey, IEnumerable<string> groups)
    {
        var user = _store.FindByKey(userKey);
        if (user is null) return AccountResult.Fail("User not found.");
        user.Groups = NormalizeGroups(groups);
        _store.Upsert(user);
        return AccountResult.Success(user);
    }

    public AccountResult SetProfile(string userKey, string? displayName)
    {
        var user = _store.FindByKey(userKey);
        if (user is null) return AccountResult.Fail("User not found.");
        if (!string.IsNullOrWhiteSpace(displayName)) user.DisplayName = displayName.Trim();
        _store.Upsert(user);
        return AccountResult.Success(user);
    }

    public AccountResult Delete(string userKey)
    {
        var user = _store.FindByKey(userKey);
        if (user is null) return AccountResult.Fail("User not found.");
        if (user.IsAdmin && IsLastAdmin(user))
            return AccountResult.Fail("Cannot delete the last remaining admin.");

        _store.Delete(userKey);
        _logger?.LogInformation("Local account deleted. User={Username}", user.Username);
        return AccountResult.Success(user);
    }

    private bool IsLastAdmin(LocalUser candidate)
        => _store.All().Count(u => u.IsAdmin && u.Enabled && u.UserKey != candidate.UserKey) == 0;

    private static List<string> NormalizeRoles(IEnumerable<string>? roles)
    {
        var set = (roles ?? ["user"])
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();
        if (set.Count == 0) set.Add("user");
        return set;
    }

    private static List<string> NormalizeGroups(IEnumerable<string>? groups)
        => (groups ?? [])
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(g => g.Trim())
            .Distinct()
            .ToList();

    /// <summary>When the store is empty, mint an <c>admin</c> account with a random password printed
    /// once to the console and the log, so a fresh deployment is immediately reachable without a
    /// hardcoded default credential.</summary>
    private void EnsureSeedAdmin()
    {
        if (_store.All().Count > 0) return;

        var password = GeneratePassword();
        var result = Register("admin", password, "Administrator", roles: ["admin", "user"]);
        if (!result.Ok) return;

        // ASCII only — this prints to the server console, which is not guaranteed to be UTF-8, and the
        // operator must be able to read the seeded password back reliably.
        var rule = new string('=', 64);
        var banner =
            $"\n{rule}\n" +
            "  SPLA local auth: no users found - seeded an admin account.\n" +
            "    username: admin\n" +
            $"    password: {password}\n" +
            "  Sign in at /admin and change this password immediately.\n" +
            $"{rule}\n";
        Console.WriteLine(banner);
        _logger?.LogWarning("Seeded default admin account 'admin' with a random password. Change it immediately.");
    }

    private static string GeneratePassword()
    {
        // Ambiguity-free alphabet so the console-printed seed can be typed back reliably.
        const string alphabet = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        return string.Concat(bytes.Select(b => alphabet[b % alphabet.Length]));
    }
}
