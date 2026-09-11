using System;
using System.IO;
using System.Linq;
using SPLA.Agent.Composition;
using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Composition;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 3.4 of docs/plans/PLAN_20260911_agent_roles-agents-md-compact.md — the prompt tail that turns
/// this session's scope markers into <c>&lt;agents&gt;</c> blocks.
/// </summary>
public sealed class ScopedAgentsContributorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "spla-scoped-agents-" + Guid.NewGuid().ToString("N"));

    public ScopedAgentsContributorTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src", "backend"));
        Directory.CreateDirectory(Path.Combine(_root, "src", "frontend"));
        File.WriteAllText(Path.Combine(_root, "AGENTS.md"), "root rules");
        File.WriteAllText(Path.Combine(_root, "src", "AGENTS.md"), "src rules");
        File.WriteAllText(Path.Combine(_root, "src", "backend", "AGENTS.md"), "backend rules");
        File.WriteAllText(Path.Combine(_root, "src", "frontend", "AGENTS.md"), "frontend rules");
    }

    public void Dispose() { try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ } }

    private AgentContributionContext Context() =>
        new(new ResolvedSettings { AgentsMd = AgentsMdMode.Inject, WorkspacePath = _root }, _root);

    private IDisposable Scope(Conversation conversation)
    {
        var workspace = new LocalWorkspace(new PathBoundary(_root), BoundaryMode.Shadow);
        var session = new AgentSession(
            new KeyValueStore("session"), new MarkManager(), new SkillSession(),
            sandbox: new PassthroughSandbox(workspace, shell: null))
        { Conversation = conversation };
        return AgentSessionScope.Begin(session);
    }

    [Fact]
    public void No_session_contributes_nothing()
    {
        var contributor = new ScopedAgentsContributor();
        var result = contributor.Contribute(Context());
        Assert.Empty(result.Context);
    }

    [Fact]
    public void One_marker_yields_its_whole_chain_root_to_leaf()
    {
        var conversation = new Conversation();
        conversation.AddScopeMarker("src/backend");
        using var _ = Scope(conversation);

        var result = new ScopedAgentsContributor().Contribute(Context());

        Assert.Equal(new[] { "src/AGENTS.md", "src/backend/AGENTS.md" }, result.Context.Select(i => i.Source));
        Assert.Contains("backend rules", result.Context.Last().Body);
        Assert.Contains("scope=\"src/backend\"", result.Context.Last().Text);
    }

    [Fact]
    public void Two_markers_share_an_ancestor_only_once()
    {
        var conversation = new Conversation();
        conversation.AddScopeMarker("src/backend");
        conversation.AddScopeMarker("src/frontend");
        using var _ = Scope(conversation);

        var result = new ScopedAgentsContributor().Contribute(Context());

        // src/ once, then backend, then frontend — first-appearance order.
        Assert.Equal(
            new[] { "src/AGENTS.md", "src/backend/AGENTS.md", "src/frontend/AGENTS.md" },
            result.Context.Select(i => i.Source));
    }

    [Fact]
    public void Same_history_assembles_byte_identical_twice()
    {
        var conversation = new Conversation();
        conversation.AddScopeMarker("src/backend");
        using var _ = Scope(conversation);
        var contributor = new ScopedAgentsContributor();

        var first = string.Concat(contributor.Contribute(Context()).Context.Select(i => i.Text));
        var second = string.Concat(contributor.Contribute(Context()).Context.Select(i => i.Text));

        Assert.Equal(first, second);
    }

    [Fact]
    public void Marker_removed_by_truncation_drops_its_block()
    {
        var conversation = new Conversation();
        var anchor = new ChatMessage { Role = ChatRole.User, Content = "hi" };
        conversation.Add(anchor);
        conversation.AddScopeMarker("src/backend");
        using var _ = Scope(conversation);

        conversation.TruncateTo(anchor.MsgId);

        var result = new ScopedAgentsContributor().Contribute(Context());
        Assert.Empty(result.Context);
    }

    [Fact]
    public void Editing_the_file_changes_the_text_with_no_new_marker_needed()
    {
        var conversation = new Conversation();
        conversation.AddScopeMarker("src/backend");
        using var _ = Scope(conversation);
        var contributor = new ScopedAgentsContributor();

        var before = contributor.Contribute(Context()).Context.Last().Body;
        Assert.Equal("backend rules", before);

        var path = Path.Combine(_root, "src", "backend", "AGENTS.md");
        File.WriteAllText(path, "backend rules v2");
        // Force the mtime forward explicitly: some filesystems' write-time resolution is coarser
        // than this test can otherwise guarantee between two writes microseconds apart.
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddSeconds(2));

        var after = contributor.Contribute(Context()).Context.Last().Body;
        Assert.Equal("backend rules v2", after);
        Assert.Single(conversation.Messages.Where(m => m.ScopeMarker != null));
    }

    [Fact]
    public void Ignore_mode_contributes_nothing_even_with_markers()
    {
        var conversation = new Conversation();
        conversation.AddScopeMarker("src/backend");
        using var _ = Scope(conversation);

        var result = new ScopedAgentsContributor().Contribute(
            new AgentContributionContext(new ResolvedSettings { AgentsMd = AgentsMdMode.Ignore, WorkspacePath = _root }, _root));

        Assert.Empty(result.Context);
    }
}
