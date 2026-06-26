using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Read-only window into a running agent's internal state for debugging — the data the native
/// KV/context/prompt debug windows show. Modelled as an interface with the deliberate intent
/// discussed in design: the embedded host returns live objects directly, while the service host
/// serializes the same snapshots over the protocol. Either way the debug windows (and any other
/// client, e.g. a browser) consume one shape, so they are written once and work in both modes.
/// </summary>
public interface IAgentInspector
{
    /// <summary>
    /// Produces a snapshot for one <see cref="DebugKinds"/> view. <paramref name="chat"/> is the chat
    /// in focus (needed for session-scoped kinds); may be null for project/global kinds.
    /// </summary>
    DebugSnapshotPayload Snapshot(string kind, ChatRuntime? chat);
}

/// <summary>In-process inspector that reads the live agent objects directly.</summary>
public sealed class LiveAgentInspector : IAgentInspector
{
    private readonly AgentRuntime _runtime;

    public LiveAgentInspector(AgentRuntime runtime) => _runtime = runtime;

    public DebugSnapshotPayload Snapshot(string kind, ChatRuntime? chat)
    {
        var snap = new DebugSnapshotPayload { Kind = kind };
        switch (kind)
        {
            case DebugKinds.KvSession:
                snap.Entries = chat?.SessionKvEntries
                    .Select(e => new DebugKvEntryDto { Key = e.Key, Value = e.Value })
                    .ToList() ?? new();
                break;

            case DebugKinds.KvProject:
                snap.Entries = _runtime.ProjectKv.Store.List()
                    .Select(e => new DebugKvEntryDto { Key = e.Key, Value = e.Value })
                    .ToList();
                break;

            case DebugKinds.Prompt:
                snap.Text = _runtime.SystemPrompt;
                snap.Segments = new List<DebugSegmentDto>
                {
                    new() { Title = "System Prompt", Body = _runtime.SystemPrompt }
                };
                break;

            case DebugKinds.Blobs:
                snap.Entries = chat?.BlobEntries
                    .Select(b => new DebugKvEntryDto
                    {
                        Key = b.Handle,
                        Value = $"{b.Kind} · {b.Size} b{(string.IsNullOrEmpty(b.Name) ? "" : " · " + b.Name)}"
                    })
                    .ToList() ?? new();
                break;

            case DebugKinds.LastContext:
                snap.Text = chat is { } c && c.LastContext.Count > 0
                    ? string.Join("\n\n", c.LastContext.Select((m, i) =>
                        $"[{i}] {m.Role}:\n{Truncate(m.Content, 2000)}"))
                    : "(no request captured yet — send a message first)";
                break;

            default:
                snap.Text = $"(snapshot kind '{kind}' not implemented yet)";
                break;
        }
        return snap;
    }

    private static string Truncate(string? s, int max)
        => s == null ? "" : s.Length <= max ? s : s[..max] + $"… (+{s.Length - max} chars)";
}
