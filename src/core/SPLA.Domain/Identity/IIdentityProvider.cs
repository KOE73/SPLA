using System.Security.Claims;

namespace SPLA.Domain.Identity;

/// <summary>
/// How a host learns identity. Two entry points: the host's own principal (for diagnostics), and the
/// mapping of an authenticated request principal to <see cref="IIdentity"/>. This is the seam that
/// keeps the host free of any platform reference — the concrete provider (Windows, Linux/Kerberos,
/// OIDC) is a DLL named in config and loaded by <see cref="IdentityProviderLoader"/>, never a
/// compile-time dependency of the host.
/// </summary>
public interface IIdentityProvider
{
    /// <summary>The host process's own principal — used for startup diagnostics/logging.</summary>
    IIdentity Current();

    /// <summary>Map an authenticated request principal (from the auth pipeline) to an identity.</summary>
    IIdentity FromPrincipal(ClaimsPrincipal principal);
}

/// <summary>
/// The neutral, always-available provider: request principals are mapped through standard claims
/// (<see cref="ClaimsIdentityResolver"/>), and the host's own principal degrades to
/// <see cref="LocalIdentity.Single"/> since there is no platform call here. This is what a host uses
/// when no platform provider is configured — no Windows, no reflection, works everywhere.
/// </summary>
public sealed class ClaimsIdentityProvider : IIdentityProvider
{
    public IIdentity Current() => LocalIdentity.Single;
    public IIdentity FromPrincipal(ClaimsPrincipal principal) => ClaimsIdentityResolver.Resolve(principal);
}
