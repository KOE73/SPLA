using SPLA.Domain.Identity;

namespace SPLA.Domain.Secrets;

/// <summary>
/// Decides whether an identity may use or manage an entry. Two implementations, chosen by the host:
/// permissive for a single-user local install, ACL-backed for a server. Same call sites either way —
/// the local case is not a special case, it is a policy that says yes.
/// <para>
/// <b>Enforced at resolve time, not only when listing.</b> A policy that merely hides entries from a
/// picker is decoration: anything that knows the key would still get the material.
/// </para>
/// </summary>
public interface ISecretAccessPolicy
{
    bool CanUse(IIdentity identity, SecretScope scope, string key, SecretAcl? acl);

    bool CanManage(IIdentity identity, SecretScope scope, string key, SecretAcl? acl);
}

/// <summary>Local/worker policy: one person, one machine, nothing to arbitrate.</summary>
public sealed class PermissiveSecretAccessPolicy : ISecretAccessPolicy
{
    public static readonly PermissiveSecretAccessPolicy Instance = new();

    public bool CanUse(IIdentity identity, SecretScope scope, string key, SecretAcl? acl) => true;

    public bool CanManage(IIdentity identity, SecretScope scope, string key, SecretAcl? acl) => true;
}

/// <summary>
/// Server policy.
/// <list type="bullet">
/// <item><see cref="SecretScope.User"/> — always allowed: the store is the caller's own area, so
/// reaching it at all already means it is theirs.</item>
/// <item><see cref="SecretScope.Project"/> — allowed to anyone who has the project open; the project
/// grant is the credential grant.</item>
/// <item><see cref="SecretScope.Shared"/> — the ACL decides. An entry with no recorded ACL is
/// <b>closed</b> to everyone but administrators: an unowned shared credential is a mistake, and
/// failing open would silently publish it.</item>
/// </list>
/// </summary>
public sealed class AclSecretAccessPolicy : ISecretAccessPolicy
{
    private readonly Func<IIdentity, bool> _isAdmin;

    public AclSecretAccessPolicy(Func<IIdentity, bool> isAdmin) => _isAdmin = isAdmin;

    public bool CanUse(IIdentity identity, SecretScope scope, string key, SecretAcl? acl)
        => Check(identity, scope, acl, SecretRight.Use);

    public bool CanManage(IIdentity identity, SecretScope scope, string key, SecretAcl? acl)
        => Check(identity, scope, acl, SecretRight.Manage);

    private bool Check(IIdentity identity, SecretScope scope, SecretAcl? acl, SecretRight right)
    {
        if (scope != SecretScope.Shared) return true;
        if (_isAdmin(identity)) return true;
        return acl is not null && acl.Allows(identity, right);
    }
}
