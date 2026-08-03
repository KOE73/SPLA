using SPLA.Domain.Secrets;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Manages the global secret store from the Settings UI — the browser half of the CLI's
/// <c>spla secret</c>. Entries are named field records (user+password, a lone token…).
/// Field values only ever flow browser→server (the user typing a secret in); the server NEVER sends
/// a value back — every answer is <see cref="SecretListResultPayload"/>: keys, scopes and field
/// names only. This keeps to the rule that secrets go in out-of-band and never through the chat/LLM.
/// <para>
/// The scope is required on every write and travels back on every entry. Nothing here defaults it:
/// picking a store on the user's behalf is how the same key ends up in two places with one silently
/// shadowing the other.
/// </para>
/// </summary>
internal sealed class SecretHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
        [MessageTypes.SecretList, MessageTypes.SecretSet, MessageTypes.SecretDelete];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.SecretList   => List(ctx),
        MessageTypes.SecretSet    => Set(ctx),
        MessageTypes.SecretDelete => Delete(ctx),
        _ => Task.CompletedTask
    };

    private static async Task List(RequestContext ctx) => await Reply(ctx);

    private static async Task Set(RequestContext ctx)
    {
        var p = ctx.Payload<SecretSetPayload>();
        var fields = p?.Fields?.Where(f => !string.IsNullOrWhiteSpace(f.Key) && !string.IsNullOrEmpty(f.Value))
            .ToDictionary(f => f.Key.Trim(), f => f.Value, StringComparer.OrdinalIgnoreCase);
        if (p == null || string.IsNullOrWhiteSpace(p.Key) || fields is not { Count: > 0 })
        {
            await Reply(ctx, "Key and at least one field are required.");
            return;
        }

        if (!SecretRef.TryParseScope(p.Scope, out var scope))
        {
            await Reply(ctx, $"Scope is required and must be one of: {string.Join(", ", SecretRef.AllNames)}.");
            return;
        }

        var (store, policy, identity, projectOpen) = Resolve(ctx);
        if (scope == SecretScope.Project && !projectOpen)
        {
            await Reply(ctx, "No project is open — cannot store a project-scoped secret.");
            return;
        }

        var key = p.Key.Trim();
        var acl = await store.Acl.GetAsync(scope, key);
        var existing = await store.GetEntryAsync(key, scope);

        // Overwriting somebody else's shared credential requires the right to manage it. A new entry
        // needs no permission — it has no owner yet; creating it is what makes the caller the owner.
        if (existing is not null && !policy.CanManage(identity, scope, key, acl))
        {
            await Reply(ctx, $"You are not allowed to change '{SecretRef.Format(scope, key)}'.");
            return;
        }

        // Merge over the existing entry so adding a field never wipes its siblings.
        var merged = existing?.Fields
            .ToDictionary(f => f.Key, f => f.Value, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, value) in fields) merged[name] = value;

        await store.SetEntryAsync(key, merged, scope);

        // First write records ownership, so a shared entry is never left unowned — an unowned shared
        // credential is closed to everyone but admins, which is a support ticket waiting to happen.
        if (acl is null)
            await store.Acl.SetAsync(scope, key, SecretAcl.OwnedBy(identity.UserKey));

        await Reply(ctx);
    }

    private static async Task Delete(RequestContext ctx)
    {
        var p = ctx.Payload<SecretDeletePayload>();
        if (p == null || string.IsNullOrWhiteSpace(p.Key))
        {
            await Reply(ctx, "Key is required.");
            return;
        }

        if (!SecretRef.TryParseScope(p.Scope, out var scope))
        {
            await Reply(ctx, $"Scope is required and must be one of: {string.Join(", ", SecretRef.AllNames)}.");
            return;
        }

        var (store, policy, identity, _) = Resolve(ctx);
        var key = p.Key.Trim();

        var acl = await store.Acl.GetAsync(scope, key);
        if (!policy.CanManage(identity, scope, key, acl))
        {
            await Reply(ctx, $"You are not allowed to change '{SecretRef.Format(scope, key)}'.");
            return;
        }

        if (string.IsNullOrWhiteSpace(p.Field))
        {
            await store.DeleteAsync(key, scope);
            await store.Acl.RemoveAsync(scope, key);
        }
        else if (await store.GetEntryAsync(key, scope) is { } entry)
        {
            var remaining = entry.Fields.ToDictionary(f => f.Key, f => f.Value, StringComparer.OrdinalIgnoreCase);
            if (remaining.Remove(p.Field.Trim()))
            {
                await store.SetEntryAsync(key, remaining, scope); // empty set deletes the entry
                if (remaining.Count == 0) await store.Acl.RemoveAsync(scope, key);
            }
        }
        await Reply(ctx);
    }

    /// <summary>Store, access policy, caller and whether a project is open — everything the three
    /// operations need, resolved once.</summary>
    private static (ISecretStore Store, ISecretAccessPolicy Policy, Domain.Identity.IIdentity Identity, bool ProjectOpen)
        Resolve(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var settings = entry.Runtime.Settings;
        return (settings.Secrets, settings.SecretAccessPolicy, ctx.Session.Identity, settings.ProjectFilePath != null);
    }

    /// <summary>Sends the current entry lists (keys, scopes and field names, never values) back to
    /// the requester. Shared entries the caller may not even see are filtered out here — server-side,
    /// because a filter applied in the browser is not a permission.</summary>
    private static async Task Reply(RequestContext ctx, string? error = null)
    {
        var (store, policy, identity, projectOpen) = Resolve(ctx);

        async Task<List<SecretEntryDto>> ToDto(SecretScope scope)
        {
            var result = new List<SecretEntryDto>();
            foreach (var e in await store.ListEntriesAsync(scope))
            {
                var acl = await store.Acl.GetAsync(scope, e.Key);
                if (!policy.CanUse(identity, scope, e.Key, acl)) continue;

                result.Add(new SecretEntryDto
                {
                    Key       = e.Key,
                    Fields    = e.Fields.ToList(),
                    Scope     = SecretRef.Name(scope),
                    Reference = SecretRef.Format(scope, e.Key),
                    CanManage = policy.CanManage(identity, scope, e.Key, acl)
                });
            }
            return result;
        }

        await ctx.Reply(MessageTypes.SecretResult, new SecretListResultPayload
        {
            User        = await ToDto(SecretScope.User),
            Project     = projectOpen ? await ToDto(SecretScope.Project) : [],
            Shared      = await ToDto(SecretScope.Shared),
            ProjectOpen = projectOpen,
            Error       = error
        });
    }
}
