using Microsoft.Extensions.Logging;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Composition;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace SPLA.Agent.Composition;

/// <summary>
/// Wave 3.4 of <c>docs/plans/PLAN_20260911_agent_roles-agents-md-compact.md</c> — the per-folder
/// half of <c>docs/adr/ADR_20260911-2_agent_agents-md-scopes.md</c> §2.4/§2.5: reads the current
/// session's scope markers (<see cref="ChatMessage.ScopeMarker"/>, planted by
/// <see cref="SPLA.MCP.Core.Pipeline.Stages.AgentsScopeStage"/>) and emits one <c>&lt;agents&gt;</c>
/// block per folder in their combined <c>AgentsScopeResolver.Chain</c>, root-to-leaf, deduplicated in
/// order of first appearance.
///
/// <para>
/// <b>MUST stay last</b> in <see cref="AgentContributors.Default"/> — after
/// <see cref="WorkingMemoryContributor"/> and after anything added to the list later. This is the
/// literal content of ADR §2.4 note 2: working memory and active skills already change the prompt
/// mid-session, and a scope entering view must not shift what sits behind them. If a future
/// contributor is appended to <c>Default</c>, it goes before this one, never after — record that rule
/// again wherever a new contributor is added, not only here.
/// </para>
/// <para>
/// Root scope never appears here — the root's own <c>AGENTS.md</c> is <see cref="ProjectAgentsContributor"/>'s
/// job (<c>RootAgents</c> in the ADR), and <c>AgentsScopeResolver.Chain</c> already excludes the root.
/// </para>
/// </summary>
public sealed class ScopedAgentsContributor : IAgentContributor
{
    /// <summary>Above this, a file still reaches the prompt whole (§2.4: "the text is not cut — cut
    /// rules are worse than no rules") but is logged as a warning, since it is the kind of thing a
    /// project owner would want to know is eating the window.</summary>
    private const long WarnBytes = 32 * 1024;

    private readonly ILogger<ScopedAgentsContributor>? _logger;

    /// <summary>Read cache, keyed by absolute file path, holding the mtime it was read at — shared
    /// across every chat's composition (contributors are process-wide), since the file on disk is the
    /// same file whichever chat's scope chain names it. A stale entry is caught the moment the file's
    /// mtime moves, without needing anything to invalidate it explicitly.</summary>
    private readonly ConcurrentDictionary<string, (DateTime Mtime, string Body)> _cache = new();

    public ScopedAgentsContributor(ILogger<ScopedAgentsContributor>? logger = null) => _logger = logger;

    public string Id => "scoped-agents";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        if (context.Settings.AgentsMd != AgentsMdMode.Inject) return AgentContribution.None;

        var session = AgentSessionScope.Current;
        var conversation = session?.Conversation;
        var workspace = session?.Sandbox.Workspace;
        if (conversation is null || workspace is null) return AgentContribution.None;

        var resolver = new AgentsScopeResolver(workspace);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var items = new List<ContextItem>();

        // Order of first appearance across the whole history, root-to-leaf within each marker's own
        // chain — the same history always assembles into the same block order (Wave 3.4 test:
        // "deterministic order").
        foreach (var message in conversation.Messages)
        {
            if (message.ScopeMarker is not { } scope) continue;

            foreach (var folder in resolver.Chain(scope))
            {
                if (!seen.Add(folder)) continue; // shared ancestor already emitted by an earlier marker

                var fullPath = Path.Combine(
                    context.Settings.WorkspacePath, folder.Replace('/', Path.DirectorySeparatorChar), "AGENTS.md");

                var body = Read(fullPath);
                if (body is null) continue; // the file went away since the marker was planted

                items.Add(new ContextItem
                {
                    Source = $"{folder}/AGENTS.md",
                    Title = $"AGENTS.md ({folder})",
                    Prefix = $"\n\n<agents scope=\"{folder}\" source=\"{folder}/AGENTS.md\">\n",
                    Body = body,
                    Suffix = "\n</agents>"
                });
            }
        }

        return AgentContribution.FromContext(items);
    }

    /// <summary>The file's text, cached by (path, mtime) — a changed file (Wave 3.4 test: "file
    /// edited → new text, no new marker needed") is re-read the next time this is called, since the
    /// marker itself never expires and this method is what actually notices the edit. Null if the
    /// file cannot be read (gone, or an I/O error — either way there is nothing to show).</summary>
    private string? Read(string fullPath)
    {
        DateTime mtime;
        try { mtime = File.GetLastWriteTimeUtc(fullPath); }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }

        if (_cache.TryGetValue(fullPath, out var cached) && cached.Mtime == mtime)
            return cached.Body;

        string body;
        try { body = File.ReadAllText(fullPath); }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }

        if (body.Length > WarnBytes)
            _logger?.LogWarning(
                "AGENTS.md at '{Path}' is {Bytes} bytes, over the {Threshold}-byte attention threshold " +
                "— it still reaches the prompt whole (never truncated), this is only a heads-up.",
                fullPath, body.Length, WarnBytes);

        _cache[fullPath] = (mtime, body);
        return body;
    }
}
