using System;
using System.Collections.Generic;
using System.Text;
using SPLA.Domain.Host;

namespace SPLA.Domain.Agent;

/// <summary>
/// Finds the per-folder <c>AGENTS.md</c> chain for a path — Wave 3.2 of
/// <c>docs/plans/PLAN_20260911_agent_roles-agents-md-compact.md</c>, implementing
/// <c>docs/adr/ADR_20260911-2_agent_agents-md-scopes.md</c> §2.3/§2.5. Used by
/// <c>AgentsScopeStage</c> (which folder does a tool call fall in) and
/// <c>ScopedAgentsContributor</c> (which files does a marker's chain read).
/// <para>
/// Path handling is deliberately NOT reimplemented here: <see cref="IWorkspace.MapPathToHost"/> /
/// <see cref="IWorkspace.MapPathToProject"/> are the same round trip every file tool already goes
/// through (<c>PathBoundary</c>), so <c>src\backend\..\backend\x.cs</c> and <c>src/backend/x.cs</c>
/// land on the same canonical, forward-slash, case-normalized (Windows host) form, and a path
/// outside the project root or on another drive comes back <c>null</c> for free.
/// </para>
/// <para>
/// <b>Excluded on purpose</b> (ADR §2.3): a path whose first segment is the mount prefix
/// (<c>mnt/...</c>) resolves to <c>null</c> even though <see cref="IWorkspace"/> would happily
/// resolve it into a declared mount — a mount has no common root with the project, and an
/// <c>AGENTS.md</c> dropped into an untrusted mount would be a ready-made injection channel. Shell,
/// SSH, and network tools never call this at all (<c>AgentsScopeStage</c> only looks at
/// filesystem-tool <c>path</c> arguments), which is a caller-side exclusion, not something this
/// class can see.
/// </para>
/// </summary>
public sealed class AgentsScopeResolver
{
    private const string AgentsFileName = "AGENTS.md";
    private const string MountPrefix = "mnt";

    private readonly IWorkspace _workspace;

    public AgentsScopeResolver(IWorkspace workspace) => _workspace = workspace;

    /// <summary>
    /// The deepest folder on <paramref name="relativePath"/> (including the path itself, if it names
    /// a directory) that contains an <c>AGENTS.md</c> — the project root is never returned, even when
    /// it is the only one that has the file: the root's file is <c>RootAgents</c>'s job
    /// (<c>ProjectAgentsContributor</c>), not a scope. Returns <c>null</c> when no folder on the path
    /// (other than the root) has one, or when the path cannot be canonicalized at all — outside the
    /// project root, on another drive, or under <c>mnt/</c>.
    /// </summary>
    public string? DeepestScope(string relativePath)
    {
        var folders = FoldersOn(relativePath);
        if (folders is null) return null;

        for (var i = folders.Count - 1; i >= 0; i--)
            if (HasAgentsMd(folders[i]))
                return folders[i];

        return null;
    }

    /// <summary>
    /// The folders from the project root (excluded) down to <paramref name="scope"/> (included) that
    /// contain an <c>AGENTS.md</c>, root-to-leaf — the order <see cref="Composition.ScopedAgentsContributor"/>
    /// (Wave 3.4) needs to emit blocks narrowest-overrides-broadest while reading root-first. Empty
    /// when <paramref name="scope"/> does not canonicalize (see <see cref="DeepestScope"/>) or is the
    /// root itself.
    /// </summary>
    public IReadOnlyList<string> Chain(string scope)
    {
        var canonical = Canonicalize(scope);
        if (string.IsNullOrEmpty(canonical)) return Array.Empty<string>();

        var chain = new List<string>();
        var sb = new StringBuilder();
        foreach (var segment in canonical.Split('/'))
        {
            if (sb.Length > 0) sb.Append('/');
            sb.Append(segment);
            var folder = sb.ToString();
            if (HasAgentsMd(folder)) chain.Add(folder);
        }

        return chain;
    }

    /// <summary>
    /// The folders on <paramref name="relativePath"/> from the project root (excluded) to the path's
    /// own containing folder (included; the path itself, if it is a directory). <c>null</c> when the
    /// path does not canonicalize.
    /// </summary>
    private List<string>? FoldersOn(string relativePath)
    {
        var canonical = Canonicalize(relativePath);
        if (canonical is null) return null;
        if (canonical.Length == 0) return new List<string>();

        var isDirectory = _workspace.DirectoryExists(canonical);
        var segments = canonical.Split('/');
        var folderSegmentCount = isDirectory ? segments.Length : segments.Length - 1;

        var folders = new List<string>(folderSegmentCount);
        var sb = new StringBuilder();
        for (var i = 0; i < folderSegmentCount; i++)
        {
            if (sb.Length > 0) sb.Append('/');
            sb.Append(segments[i]);
            folders.Add(sb.ToString());
        }

        return folders;
    }

    /// <summary>
    /// The canonical, project-relative, forward-slash form of <paramref name="relativePath"/>
    /// (empty string for the root itself), or <c>null</c> if it is outside the root, on another
    /// drive, or addresses a mount.
    /// </summary>
    private string? Canonicalize(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return string.Empty;

        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        var firstSegment = normalized.Split('/', 2)[0];
        if (firstSegment.Equals(MountPrefix, StringComparison.OrdinalIgnoreCase)) return null;

        var host = _workspace.MapPathToHost(relativePath);
        if (host is null) return null;

        var canonical = _workspace.MapPathToProject(host);
        if (canonical is null) return null;

        canonical = canonical.Replace('\\', '/').Trim('/');
        if (canonical == ".") canonical = string.Empty;

        // Belt and suspenders: a mount is a legal destination for IWorkspace but never for scopes.
        if (canonical.Equals(MountPrefix, StringComparison.OrdinalIgnoreCase) ||
            canonical.StartsWith(MountPrefix + "/", StringComparison.OrdinalIgnoreCase))
            return null;

        return canonical;
    }

    private bool HasAgentsMd(string folder) =>
        _workspace.FileExists(folder.Length == 0 ? AgentsFileName : $"{folder}/{AgentsFileName}");
}
