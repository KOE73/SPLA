using System;
using System.IO;
using SPLA.Domain.Agent;
using SPLA.Domain.Host;

namespace SPLA.Tests;

/// <summary>
/// Wave 3.2 of docs/plans/PLAN_20260911_agent_roles-agents-md-compact.md — chain resolution over
/// the ADR's example tree:
/// <code>
/// root/AGENTS.md
/// root/src/AGENTS.md
/// root/src/backend/AGENTS.md
/// root/src/frontend/AGENTS.md
/// </code>
/// Uses a real temp directory through <see cref="LocalWorkspace"/> (in <see cref="BoundaryMode.Enforce"/>)
/// rather than the identity-mapping in-memory test double, because outside-root/other-drive
/// rejection and <c>..</c> collapsing are exactly what <see cref="PathBoundary"/> — the same
/// normalization every file tool already goes through — is being relied on for here.
/// </summary>
public sealed class AgentsScopeResolverTests : IDisposable
{
    private readonly string _root;
    private readonly AgentsScopeResolver _resolver;

    public AgentsScopeResolverTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "spla-agents-scope-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_root, "src", "backend"));
        Directory.CreateDirectory(Path.Combine(_root, "src", "frontend"));
        Directory.CreateDirectory(Path.Combine(_root, "src", "frontend", "components"));
        Directory.CreateDirectory(Path.Combine(_root, "mnt"));

        File.WriteAllText(Path.Combine(_root, "AGENTS.md"), "root rules");
        File.WriteAllText(Path.Combine(_root, "src", "AGENTS.md"), "src rules");
        File.WriteAllText(Path.Combine(_root, "src", "backend", "AGENTS.md"), "backend rules");
        File.WriteAllText(Path.Combine(_root, "src", "frontend", "AGENTS.md"), "frontend rules");

        var boundary = new PathBoundary(_root);
        var workspace = new LocalWorkspace(boundary, BoundaryMode.Enforce);
        _resolver = new AgentsScopeResolver(workspace);
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void DeepestScope_finds_the_nearest_ancestor_with_AGENTS_md()
    {
        Assert.Equal("src/backend", _resolver.DeepestScope("src/backend/x.cs"));
    }

    [Fact]
    public void DeepestScope_walks_up_when_the_immediate_folder_has_no_file()
    {
        // src/frontend/components has no AGENTS.md of its own — nearest is src/frontend.
        Assert.Equal("src/frontend", _resolver.DeepestScope("src/frontend/components/App.tsx"));
    }

    [Fact]
    public void DeepestScope_never_returns_the_root()
    {
        // Only the root itself has AGENTS.md on this path segment set — root is excluded.
        Directory.CreateDirectory(Path.Combine(_root, "no-agents"));
        Assert.Null(_resolver.DeepestScope("no-agents/file.txt"));
    }

    [Fact]
    public void Chain_orders_root_to_leaf_and_excludes_the_root_itself()
    {
        var chain = _resolver.Chain("src/backend");
        Assert.Equal(new[] { "src", "src/backend" }, chain);
    }

    [Fact]
    public void Sibling_scopes_do_not_mix()
    {
        var backend = _resolver.Chain("src/backend");
        var frontend = _resolver.Chain("src/frontend");

        Assert.DoesNotContain("src/frontend", backend);
        Assert.DoesNotContain("src/backend", frontend);
    }

    [Fact]
    public void Mnt_paths_never_resolve()
    {
        Assert.Null(_resolver.DeepestScope("mnt/shared/file.txt"));
        Assert.Empty(_resolver.Chain("mnt/shared"));
    }

    [Fact]
    public void Escaping_the_root_via_dotdot_never_resolves()
    {
        Assert.Null(_resolver.DeepestScope("../outside/file.txt"));
    }

    [Fact]
    public void Backslash_and_forward_slash_paths_agree()
    {
        Assert.Equal(
            _resolver.DeepestScope("src/backend/x.cs"),
            _resolver.DeepestScope(@"src\backend\..\backend\x.cs"));
    }
}
