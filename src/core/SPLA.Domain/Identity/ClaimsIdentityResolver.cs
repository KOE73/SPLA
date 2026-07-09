using System.Linq;
using System.Security.Claims;

namespace SPLA.Domain.Identity;

/// <summary>
/// Maps an authenticated <see cref="ClaimsPrincipal"/> (as produced by Negotiate/NTLM/Kerberos on the
/// server) to the platform-neutral <see cref="IIdentity"/> — purely through standard claims, so it
/// lives in the core with no Windows dependency and works for any auth scheme that emits SID claims.
/// <see cref="WindowsIdentityResolver"/> (the Windows project) is its process-side sibling; this is
/// the request/connection side used by the ASP.NET pipeline.
/// </summary>
public static class ClaimsIdentityResolver
{
    public static IIdentity Resolve(ClaimsPrincipal principal)
    {
        var name = principal.Identity?.Name ?? "unknown";
        // PrimarySid is the stable, path-safe owner key; fall back to the name if a non-Windows
        // scheme did not emit it.
        var userKey = principal.FindFirst(ClaimTypes.PrimarySid)?.Value ?? name;
        var groups = principal.FindAll(ClaimTypes.GroupSid).Select(c => c.Value).ToArray();
        return new ClaimIdentity(userKey, name, groups);
    }
}
