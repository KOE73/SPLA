using SPLA.Runtime;
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

    /// <summary>One row, with its label in its own column. Mixing the origin into the value would
    /// put it where nobody scans for it — the point of showing it at all is that a glance answers
    /// "which of these came from outside".</summary>
    private static DebugKvEntryDto Kv(SPLA.Domain.Agent.KvEntry e) => new()
    {
        Key = e.Key,
        Value = e.Value,
        Origin = e.Origin?.Zone,
        Doubtful = e.Origin?.RaisesDoubt ?? false
    };

    public DebugSnapshotPayload Snapshot(string kind, ChatRuntime? chat)
    {
        var snap = new DebugSnapshotPayload { Kind = kind };
        switch (kind)
        {
            case DebugKinds.KvSession:
                snap.Entries = chat?.SessionKvOrigins.Select(Kv).ToList() ?? new();
                break;

            case DebugKinds.KvProject:
                snap.Entries = _runtime.ProjectKv.Store.Entries().Select(Kv).ToList();
                break;

            case DebugKinds.Prompt:
            {
                // Composed in the chat's scope when there is one: the active skill and this chat's
                // working memory are per-chat state, and a runtime-wide composition would show a
                // surface that disagrees with what the chat actually sends.
                var composed = chat is not null ? chat.ComposeContext() : _runtime.ComposeContext();
                snap.Text = composed.SystemPrompt;
                snap.ApproxTokens = composed.Manifest.ApproxTokens;

                snap.Segments = composed.Items
                    .Select(item => new DebugSegmentDto
                    {
                        Title = item.Title,
                        Body = item.Body,
                        Contributor = item.Contributor,
                        Source = item.Source,
                        Placement = item.Placement == SPLA.MCP.Core.Composition.ContextPlacement.SystemPrompt ? "prompt" : "turn",
                        ApproxTokens = item.ApproxTokens
                    })
                    // A contributor that threw has no item — only a manifest line saying why. Showing
                    // it is the whole point: missing text with a reason beats missing text.
                    .Concat(composed.Manifest.Entries
                        .Where(e => e.Problem is not null)
                        .Select(e => new DebugSegmentDto
                        {
                            Title = e.Title,
                            Body = e.Problem!,
                            Contributor = e.Contributor,
                            Source = e.Source,
                            Placement = "failed",
                            Problem = e.Problem
                        }))
                    .ToList();
                break;
            }

            case DebugKinds.Blobs:
                snap.Entries = chat?.BlobEntries
                    .Select(b => new DebugKvEntryDto
                    {
                        Key = b.Handle,
                        Value = $"{b.Kind} · {b.Size} b{(string.IsNullOrEmpty(b.Name) ? "" : " · " + b.Name)}",
                        Origin = b.Origin?.Zone,
                        Doubtful = b.Origin?.RaisesDoubt ?? false
                    })
                    .ToList() ?? new();
                break;

            case DebugKinds.Edges:
                snap.Edges = _runtime.McpHost.Edges.List()
                    .Select(t => new DebugEdgeDto
                    {
                        Source = t.Edge.Source.ToString(),
                        Sink = t.Edge.Sink.ToString(),
                        Effect = t.Edge.Effect.ToString().ToLowerInvariant(),
                        Calls = t.Calls,
                        LastTool = t.LastTool,
                        // What leaves the project, in either direction of trust: content going out,
                        // and content coming in from somewhere nobody vouched for.
                        Outward = t.Edge.Sink != SPLA.Domain.Security.Zone.Project
                                  && t.Edge.Sink != SPLA.Domain.Security.Zone.Context
                                  || t.Edge.Source == SPLA.Domain.Security.Zone.Web
                    })
                    .ToList();
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

                    var lines = BuildContextLines(sentContext, lc.Messages);
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

    /// <summary>
    /// Lays out the context table in the order the model actually saw: walk the sent request, and slot
    /// each history message that did NOT make it in just before the next one that did.
    /// <para>
    /// The earlier two-pass build — all history, then injected messages appended — put the composed
    /// system prompt at the bottom of the table. It is rebuilt fresh on every loop iteration and so
    /// carries no MsgId to match history on, which is the only reason it landed in the second pass.
    /// The table read as "the system prompt is sent last". It is sent first.
    /// </para>
    /// Public and static because it is a pure function of (request, history) and is worth testing as
    /// one, without standing up a runtime.
    /// </summary>
    public static List<ContextLineDto> BuildContextLines(
        IReadOnlyList<ChatMessage> sentContext, IReadOnlyList<ChatMessage> history)
    {
        var idx = 0;
        var lines = new List<ContextLineDto>();
        var next = 0;   // position in history not yet emitted

        ContextLineDto Line(ChatMessage m, bool inContext, bool fromHistory)
        {
            var full = BuildFull(m);
            return new ContextLineDto
            {
                Index = ++idx,
                MsgId = !fromHistory ? "(injected)"
                      : string.IsNullOrEmpty(m.Mark) ? m.MsgId
                      : $"{m.MsgId} [{m.Mark}]",
                Source = DescribeSource(m),
                Full = full,
                Preview = Flatten(full),
                ApproxTokens = (full.Length + 3) / 4,
                InContext = inContext
            };
        }

        foreach (var m in sentContext)
        {
            var at = string.IsNullOrEmpty(m.MsgId)
                ? -1
                : IndexOfFrom(history, next, m.MsgId!);

            if (at < 0)
            {
                // Composed system prompt, working-memory snapshot, ephemeral injections — real
                // positions in the request, no counterpart in stored history.
                lines.Add(Line(m, inContext: true, fromHistory: false));
                continue;
            }

            for (; next < at; next++)                       // history dropped before this one
                lines.Add(Line(history[next], inContext: false, fromHistory: true));
            next = at + 1;
            lines.Add(Line(m, inContext: true, fromHistory: true));
        }

        // History after the last message that survived into the request.
        for (; next < history.Count; next++)
            lines.Add(Line(history[next], inContext: false, fromHistory: true));

        return lines;
    }

    private static int IndexOfFrom(IReadOnlyList<ChatMessage> history, int start, string msgId)
    {
        for (var i = start; i < history.Count; i++)
            if (history[i].MsgId == msgId) return i;
        return -1;
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
