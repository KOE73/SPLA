using System.Runtime.Versioning;
using System.Security.Claims;
using SPLA.Domain.Identity;
using IIdentity = SPLA.Domain.Identity.IIdentity;

namespace SPLA.Identity.Windows;

/// <summary>
/// The Windows platform provider, loaded by config (never referenced by the host at compile time —
/// see <see cref="IdentityProviderLoader"/>). The host's own principal comes from the Windows token;
/// request principals arrive from Negotiate already carrying SID claims, so they map through the same
/// neutral <see cref="ClaimsIdentityResolver"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsIdentityProvider : IIdentityProvider
{
    public IIdentity Current() => WindowsIdentityResolver.Current();
    public IIdentity FromPrincipal(ClaimsPrincipal principal) => ClaimsIdentityResolver.Resolve(principal);
}
