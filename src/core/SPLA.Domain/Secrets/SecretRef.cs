namespace SPLA.Domain.Secrets;

/// <summary>Where a secret lives. Always stated explicitly — never inferred, never searched for.</summary>
public enum SecretScope
{
    /// <summary>The person's own store. Locally <c>~/.spla/secrets.yaml</c>; on a server the user's
    /// private area. "Mine, and nobody else's" — the location itself is the isolation.</summary>
    User,

    /// <summary>Travels with the project (<c>&lt;project&gt;/.spla/secrets.yaml</c>).</summary>
    Project,

    /// <summary>Administered, shared between people. This is the only scope where an ACL decides
    /// who may see and use an entry — the other two are already isolated by where they live.</summary>
    Shared
}

/// <summary>
/// <b>The single place the reference syntax exists.</b> Nothing else in the codebase may hard-code
/// <c>"secret:"</c>, the scope separator or the field separator — parse and format through here, so
/// changing the syntax is one edit rather than a hunt.
///
/// <para>Grammar — the scope is mandatory:</para>
/// <code>
///   secret:&lt;scope&gt;:&lt;key&gt;
///   secret:&lt;scope&gt;:&lt;key&gt;#&lt;field&gt;
///   env:&lt;VAR&gt;
/// </code>
/// <para>
/// The scope is part of the reference because making writes explicit while leaving reads to a search
/// would move the ambiguity rather than remove it. There is no "look in project, then fall back to
/// machine": <c>user</c> means <c>user</c>. A reference that does not resolve is an error with a
/// name in it, never a silent substitution of somebody else's credential.
/// </para>
/// <para>
/// Splitting is on the FIRST separator after the prefix, so keys may contain <c>:</c> and <c>/</c>
/// freely (<c>sql:mydb</c>, <c>ssh/homelab/oleg</c>) without escaping.
/// </para>
/// </summary>
public static class SecretRef
{
    /// <summary>Marks a reference into the secret store.</summary>
    public const string SecretPrefix = "secret:";

    /// <summary>Marks a reference into the process environment.</summary>
    public const string EnvPrefix = "env:";

    /// <summary>Separates scope from key. Applied once, at the first occurrence.</summary>
    public const char ScopeSeparator = ':';

    /// <summary>Separates key from field. May not appear in a key.</summary>
    public const char FieldSeparator = '#';

    /// <summary>Canonical lowercase scope names as they appear in references, config and the wire.</summary>
    public static string Name(SecretScope scope) => scope switch
    {
        SecretScope.User => "user",
        SecretScope.Project => "project",
        SecretScope.Shared => "shared",
        _ => throw new ArgumentOutOfRangeException(nameof(scope))
    };

    /// <summary>Every scope name, for building pickers and error messages.</summary>
    public static IReadOnlyList<string> AllNames { get; } = new[] { "user", "project", "shared" };

    /// <summary>Parses a scope name. No default and no guessing: an unknown or empty name fails.</summary>
    public static bool TryParseScope(string? name, out SecretScope scope)
    {
        switch (name?.Trim().ToLowerInvariant())
        {
            case "user": scope = SecretScope.User; return true;
            case "project": scope = SecretScope.Project; return true;
            case "shared": scope = SecretScope.Shared; return true;
            default: scope = default; return false;
        }
    }

    /// <summary>Builds a reference. The only supported way to produce one.</summary>
    public static string Format(SecretScope scope, string key, string? field = null)
        => field is null
            ? $"{SecretPrefix}{Name(scope)}{ScopeSeparator}{key}"
            : $"{SecretPrefix}{Name(scope)}{ScopeSeparator}{key}{FieldSeparator}{field}";

    /// <summary>True when the value points at the store or the environment rather than being a literal.</summary>
    public static bool IsReference(string? value)
        => IsSecretReference(value) || IsEnvReference(value);

    public static bool IsSecretReference(string? value)
        => value is not null && value.StartsWith(SecretPrefix, StringComparison.OrdinalIgnoreCase);

    public static bool IsEnvReference(string? value)
        => value is not null && value.StartsWith(EnvPrefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>Environment variable name of an <c>env:</c> reference.</summary>
    public static string EnvVariable(string reference) => reference[EnvPrefix.Length..].Trim();

    /// <summary>
    /// Parses <c>secret:&lt;scope&gt;:&lt;key&gt;[#field]</c>. Returns false with a human-readable
    /// <paramref name="error"/> — a malformed reference must say what was expected, because the
    /// alternative (guessing a scope) is exactly what this design removes.
    /// </summary>
    public static bool TryParse(
        string? reference,
        out SecretScope scope,
        out string key,
        out string? field,
        out string? error)
    {
        scope = default;
        key = string.Empty;
        field = null;
        error = null;

        if (!IsSecretReference(reference))
        {
            error = $"not a secret reference (expected '{SecretPrefix}<scope>{ScopeSeparator}<key>')";
            return false;
        }

        var body = reference![SecretPrefix.Length..].Trim();

        var sep = body.IndexOf(ScopeSeparator);
        if (sep <= 0)
        {
            error = $"missing scope — expected '{SecretPrefix}<scope>{ScopeSeparator}<key>' " +
                    $"where <scope> is one of: {string.Join(", ", AllNames)}";
            return false;
        }

        var scopeName = body[..sep];
        if (!TryParseScope(scopeName, out scope))
        {
            error = $"unknown scope '{scopeName}' — expected one of: {string.Join(", ", AllNames)}";
            return false;
        }

        var rest = body[(sep + 1)..];
        var hash = rest.IndexOf(FieldSeparator);
        if (hash < 0)
        {
            key = rest.Trim();
        }
        else
        {
            key = rest[..hash].Trim();
            field = rest[(hash + 1)..].Trim();
            if (field.Length == 0) field = null;
        }

        if (key.Length == 0)
        {
            error = "empty key";
            return false;
        }

        return true;
    }
}
