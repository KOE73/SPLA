using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// The single gate that decides whether a connecting client is allowed in, and with what
/// capabilities. Deliberately a stub at this stage: on loopback it is allow-all; when a connect
/// token is configured (for a network-bound server) it must match. Identity and per-actor
/// capability grants are reserved — the protocol already carries the fields, so this can grow from
/// "allow-all" → "shared secret" → "per-actor groups" without touching the wire format.
/// </summary>
public sealed class AuthGate
{
    private readonly string? _requiredToken;

    /// <param name="requiredToken">
    /// When non-null, a client must present a matching <see cref="AuthInfo.Token"/>. Null means no
    /// token is required — appropriate for a loopback-only bind where the OS already gates access.
    /// </param>
    public AuthGate(string? requiredToken = null) => _requiredToken = requiredToken;

    public bool RequiresToken => _requiredToken != null;

    /// <summary>Decides admission. Returns the granted actor id and capability set on success.</summary>
    public AuthResult Authorize(AuthInfo? auth)
    {
        if (_requiredToken != null && !string.Equals(auth?.Token, _requiredToken, StringComparison.Ordinal))
            return new AuthResult(false, "", Array.Empty<string>());

        // Stage 0/1: everyone admitted gets the full capability set. Groups arrive later.
        return new AuthResult(true, auth?.ActorId ?? "local", Capabilities.Full);
    }
}

public readonly record struct AuthResult(bool Ok, string ActorId, string[] Capabilities);
