using SPLA.Domain.Identity;

namespace SPLA.Domain.Secrets;

/// <summary>What a principal is allowed to do with an entry.</summary>
public enum SecretRight
{
    /// <summary>Resolve it at point of use — an SSH password an operator's agent may connect with,
    /// without ever seeing the value.</summary>
    Use,

    /// <summary>See it in listings, overwrite it, delete it, change its ACL.</summary>
    Manage
}

/// <summary>
/// Who may do what with one entry. Deliberately about <b>principals</b> (user keys and group keys),
/// not about roles: "the SSH password to production is for admins, not for managers" is a statement
/// about a specific credential, not about a job title.
/// <para>
/// The owner is whoever created the entry and always holds both rights, including the right to grant
/// them to others. Grants are additive; there is no deny list, because a deny that can be
/// out-voted by another grant is a bug generator.
/// </para>
/// <para>
/// ACLs live beside the store in plaintext, never inside it: an ACL is not a secret, and it must be
/// readable to filter a listing <i>without</i> decrypting anything. That is what keeps the promise
/// that listing never touches secret material.
/// </para>
/// </summary>
public sealed record SecretAcl(
    string Owner,
    IReadOnlyCollection<string> Use,
    IReadOnlyCollection<string> Manage)
{
    public static SecretAcl OwnedBy(string ownerUserKey)
        => new(ownerUserKey, Array.Empty<string>(), Array.Empty<string>());

    /// <summary>True when the identity holds <paramref name="right"/> — as owner, by user key, or
    /// through any of its groups.</summary>
    public bool Allows(IIdentity identity, SecretRight right)
    {
        if (string.Equals(Owner, identity.UserKey, StringComparison.OrdinalIgnoreCase))
            return true;

        var granted = right == SecretRight.Use ? Use : Manage;

        // Manage implies Use: someone who may overwrite a credential can trivially use it.
        if (right == SecretRight.Use && Matches(Manage, identity)) return true;

        return Matches(granted, identity);
    }

    private static bool Matches(IReadOnlyCollection<string> principals, IIdentity identity)
    {
        foreach (var p in principals)
        {
            if (string.Equals(p, identity.UserKey, StringComparison.OrdinalIgnoreCase)) return true;
            foreach (var g in identity.Groups)
                if (string.Equals(p, g, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }
}

/// <summary>
/// Stores the ACL of each entry, separately from the entry itself. Only the <see cref="SecretScope.Shared"/>
/// scope has meaningful ACLs — <see cref="SecretScope.User"/> is isolated by living in the user's own
/// area and <see cref="SecretScope.Project"/> by being part of the project — but the interface is
/// scope-agnostic so the rule lives in one policy rather than being re-derived by every caller.
/// </summary>
public interface ISecretAclStore
{
    /// <summary>The entry's ACL, or null when none was ever recorded.</summary>
    ValueTask<SecretAcl?> GetAsync(SecretScope scope, string key, CancellationToken ct = default);

    ValueTask SetAsync(SecretScope scope, string key, SecretAcl acl, CancellationToken ct = default);

    ValueTask RemoveAsync(SecretScope scope, string key, CancellationToken ct = default);
}
