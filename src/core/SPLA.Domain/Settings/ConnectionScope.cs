namespace SPLA.Domain.Settings;

/// <summary>
/// Where a connection is declared. Deliberately the same three words as
/// <see cref="Secrets.SecretScope"/>, and for the same reason: a connection is a credential plus an
/// endpoint, so "whose is it, and who else can see it" is decided by the same question — the
/// location it lives in.
///
/// <para>The layers merge by id, least authoritative first:
/// <see cref="Shared"/> → <see cref="User"/> → <see cref="Project"/>. A project entry replaces a
/// user entry of the same id wholesale, the same rule <c>connections:</c> already used between
/// <c>defaults.yaml</c> and <c>.spla</c>. Merging is by id only; nothing is guessed from the
/// endpoint or the provider name.</para>
///
/// <para>The point of <see cref="User"/> is that a person configures their keys once and every
/// project they open sees them, instead of every repository carrying its own copy.</para>
/// </summary>
public enum ConnectionScope
{
    /// <summary>Administered, shared between people —
    /// <c>&lt;sharedDir&gt;/connections.shared.yaml</c>. Locally this is the same home as
    /// <see cref="User"/>; on a server it is the deployment's own file and one person editing their
    /// own connections cannot touch it.</summary>
    Shared,

    /// <summary>The person's own — <c>&lt;personalDir&gt;/connections.yaml</c>, i.e. <c>~/.spla</c>
    /// locally and the caller's private area on a multi-user server. Never committed, available in
    /// every project that person opens.</summary>
    User,

    /// <summary>Travels with the project, in the manifest's own <c>connections:</c> block. What a
    /// repository declares for anyone who checks it out.</summary>
    Project
}

/// <summary>The scope vocabulary as it appears in config, on the wire and in error messages. Parsing
/// lives here so the names exist in exactly one place, the way <see cref="Secrets.SecretRef"/> owns
/// the secret-reference grammar.</summary>
public static class ConnectionScopes
{
    public static string Name(ConnectionScope scope) => scope switch
    {
        ConnectionScope.Shared => "shared",
        ConnectionScope.User => "user",
        ConnectionScope.Project => "project",
        _ => throw new ArgumentOutOfRangeException(nameof(scope))
    };

    /// <summary>Every scope name, in merge order (least authoritative first).</summary>
    public static IReadOnlyList<string> AllNames { get; } = new[] { "shared", "user", "project" };

    /// <summary>Parses a scope name. No default and no guessing — an unknown name fails, so a
    /// typo in a manifest is a message with the name in it rather than a silent relocation.</summary>
    public static bool TryParse(string? name, out ConnectionScope scope)
    {
        switch (name?.Trim().ToLowerInvariant())
        {
            case "shared": scope = ConnectionScope.Shared; return true;
            case "user": scope = ConnectionScope.User; return true;
            case "project": scope = ConnectionScope.Project; return true;
            default: scope = default; return false;
        }
    }
}
