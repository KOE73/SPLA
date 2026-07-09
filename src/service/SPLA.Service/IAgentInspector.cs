using SPLA.Domain.Context;
using SPLA.Domain.Models;
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
                if (chat is { } lc)
                {
                    // LastContext is captured live from the most recent LLM turn and is process-memory
                    // only — empty right after a fresh service start even though chat history is on
                    // disk. Fall back to recomputing what WOULD be sent right now so the panel is
                    // useful immediately, not just after the next message.
                    var isLive = lc.LastContext.Count > 0;
                    var sentContext = isLive ? lc.LastContext : ContextAssembler.Assemble(lc.Messages);
                    snap.ContextIsLive = isLive;

                    var ctxSet = new HashSet<string>(sentContext.Select(m => m.MsgId).Where(id => id != null)!);
                    var allMessages = lc.Messages;
                    var idx = 0;
                    var lines = new List<ContextLineDto>();

                    // All messages (full history) — dimmed if not in context
                    foreach (var m in allMessages)
                    {
                        lines.Add(new ContextLineDto
                        {
                            Index = ++idx,
                            MsgId = string.IsNullOrEmpty(m.Mark) ? m.MsgId : $"{m.MsgId} [{m.Mark}]",
                            Source = DescribeSource(m),
                            Full = BuildFull(m),
                            Preview = Flatten(BuildFull(m)),
                            ApproxTokens = (BuildFull(m).Length + 3) / 4,
                            InContext = !string.IsNullOrEmpty(m.MsgId) && ctxSet.Contains(m.MsgId)
                        });
                    }
                    // Working-memory and other injected messages absent from history
                    foreach (var m in sentContext.Where(m => string.IsNullOrEmpty(m.MsgId) || !allMessages.Any(h => h.MsgId == m.MsgId)))
                    {
                        lines.Add(new ContextLineDto
                        {
                            Index = ++idx,
                            MsgId = "(injected)",
                            Source = DescribeSource(m),
                            Full = BuildFull(m),
                            Preview = Flatten(BuildFull(m)),
                            ApproxTokens = (BuildFull(m).Length + 3) / 4,
                            InContext = true
                        });
                    }
                    var ctxLines = lines.Where(l => l.InContext).ToList();
                    snap.ContextLines = lines;
                    snap.ContextCount = ctxLines.Count;
                    snap.TotalCount = lines.Count;
                    snap.ApproxTokens = ctxLines.Sum(l => l.ApproxTokens);
                }
                else
                {
                    snap.Text = "(no request captured yet — send a message first)";
                }
                break;

            default:
                snap.Text = $"(snapshot kind '{kind}' not implemented yet)";
                break;
        }
        return snap;
    }

    private static string DescribeSource(ChatMessage m)
    {
        if (m.IsLabel) return string.IsNullOrEmpty(m.Mark) ? "label" : $"label [{m.Mark}]";
        return m.Role switch
        {
            ChatRole.System when (m.Content ?? "").StartsWith("--- Working memory", StringComparison.Ordinal) => "working-mem",
            ChatRole.System    => "system",
            ChatRole.User      => "user",
            ChatRole.Assistant when m.ToolCalls is { Count: > 0 } => "asst→tool",
            ChatRole.Assistant => "assistant",
            ChatRole.Tool      => "tool-result",
            _                  => m.Role.ToString().ToLowerInvariant()
        };
    }

    private static string BuildFull(ChatMessage m)
    {
        if (m.ToolCalls is { Count: > 0 })
        {
            var calls = string.Join(", ", m.ToolCalls.Select(t => $"{t.Function.Name}({t.Function.Arguments})"));
            var head = string.IsNullOrEmpty(m.Content) ? "" : m.Content + " ";
            return $"{head}[tools: {calls}]";
        }
        return m.Content ?? string.Empty;
    }

    private static string Flatten(string text)
    {
        if (string.IsNullOrEmpty(text)) return "(empty)";
        var s = text.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Trim();
        while (s.Contains("  ")) s = s.Replace("  ", " ");
        return s;
    }

    private static string Truncate(string? s, int max)
        => s == null ? "" : s.Length <= max ? s : s[..max] + $"… (+{s.Length - max} chars)";
}
