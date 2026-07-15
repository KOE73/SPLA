using SPLA.Domain.Secrets;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Manages the global secret store (machine + project scope) from the Settings UI — the browser half
/// of the CLI's <c>spla secret</c>. Entries are named field records (user+password, a lone token…).
/// Field values only ever flow browser→server (the user typing a secret in); the server NEVER sends
/// a value back — every answer is <see cref="SecretListResultPayload"/>: keys and field names only.
/// This keeps to the rule that secrets go in out-of-band and never through the chat/LLM.
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

        var (store, scope, projectOpen) = Resolve(ctx, p.Scope);
        if (scope == SecretScope.Project && !projectOpen)
        {
            await Reply(ctx, "No project is open — cannot store a project-scoped secret.");
            return;
        }

        // Merge over the existing entry so adding a field never wipes its siblings.
        var key = p.Key.Trim();
        var merged = (await store.GetEntryAsync(key, scope))?.Fields
            .ToDictionary(f => f.Key, f => f.Value, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, value) in fields) merged[name] = value;

        await store.SetEntryAsync(key, merged, scope);
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

        var (store, scope, _) = Resolve(ctx, p.Scope);
        var key = p.Key.Trim();

        if (string.IsNullOrWhiteSpace(p.Field))
        {
            await store.DeleteAsync(key, scope);
        }
        else if (await store.GetEntryAsync(key, scope) is { } entry)
        {
            var fields = entry.Fields.ToDictionary(f => f.Key, f => f.Value, StringComparer.OrdinalIgnoreCase);
            if (fields.Remove(p.Field.Trim()))
                await store.SetEntryAsync(key, fields, scope); // empty set deletes the entry
        }
        await Reply(ctx);
    }

    /// <summary>The store for this connection's resolved project, plus whether a project is actually
    /// open (project-scope edits are refused otherwise).</summary>
    private static (ISecretStore Store, SecretScope Scope, bool ProjectOpen) Resolve(RequestContext ctx, string scopeName)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var settings = entry.Runtime.Settings;
        var scope = scopeName.Equals("project", StringComparison.OrdinalIgnoreCase)
            ? SecretScope.Project : SecretScope.Machine;
        return (settings.Secrets, scope, settings.ProjectFilePath != null);
    }

    /// <summary>Sends the current entry lists (keys + field names, never values) back to the requester.</summary>
    private static async Task Reply(RequestContext ctx, string? error = null)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var store = entry.Runtime.Settings.Secrets;
        var projectOpen = entry.Runtime.Settings.ProjectFilePath != null;

        static List<SecretEntryDto> ToDto(IReadOnlyList<SecretEntryInfo> entries) =>
            entries.Select(e => new SecretEntryDto { Key = e.Key, Fields = e.Fields.ToList() }).ToList();

        var machine = await store.ListEntriesAsync(SecretScope.Machine);
        var project = projectOpen ? await store.ListEntriesAsync(SecretScope.Project) : [];

        await ctx.Reply(MessageTypes.SecretResult, new SecretListResultPayload
        {
            Machine = ToDto(machine),
            Project = ToDto(project),
            ProjectOpen = projectOpen,
            Error = error
        });
    }
}
