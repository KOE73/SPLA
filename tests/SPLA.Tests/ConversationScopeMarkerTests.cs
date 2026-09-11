using System.Linq;
using SPLA.Domain.Context;
using SPLA.Domain.Models;

namespace SPLA.Tests;

/// <summary>
/// Wave 3.1 of docs/plans/PLAN_20260911_agent_roles-agents-md-compact.md — the scope-marker record
/// itself (docs/adr/ADR_20260911-2_agent_agents-md-scopes.md §2.5). Markers must never reach the
/// model, must always persist regardless of saveToolCalls, must be removed by truncation and kept by
/// branching (both flow from ordinary List semantics, but are asserted explicitly here).
/// </summary>
public class ConversationScopeMarkerTests
{
    [Fact]
    public void AddScopeMarker_assigns_R_prefixed_msgid_and_sets_scope()
    {
        var convo = new Conversation();

        var marker = convo.AddScopeMarker("src/backend");

        Assert.Equal("src/backend", marker.ScopeMarker);
        Assert.StartsWith("R-", marker.MsgId);
        Assert.Same(marker, convo.Messages.Single());
    }

    [Fact]
    public void Scope_marker_sequence_is_independent_of_other_roles()
    {
        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "hi" });
        var m1 = convo.AddScopeMarker("src");
        convo.Add(new ChatMessage { Role = ChatRole.Assistant, Content = "hello" });
        var m2 = convo.AddScopeMarker("src/backend");

        Assert.Equal("R-1", m1.MsgId);
        Assert.Equal("R-2", m2.MsgId);
    }

    [Fact]
    public void Scope_marker_is_never_sent_to_the_model()
    {
        var convo = new[]
        {
            new ChatMessage { Role = ChatRole.User, Content = "hi" },
            new ChatMessage { Role = ChatRole.User, ScopeMarker = "src/backend", Content = "" },
            new ChatMessage { Role = ChatRole.Assistant, Content = "hello" }
        };

        var result = ContextAssembler.Assemble(convo);

        Assert.DoesNotContain(result, m => m.ScopeMarker != null);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Scope_marker_always_persists_even_with_saveToolCalls_and_saveAttempts_off()
    {
        var convo = new Conversation();
        var marker = convo.AddScopeMarker("src/backend");

        Assert.True(Conversation.ShouldPersist(marker, saveToolCalls: false, saveAttempts: false));
        Assert.Contains(marker, convo.PersistableWith(saveToolCalls: false, saveAttempts: false));
    }

    [Fact]
    public void Truncation_removes_markers_after_the_anchor()
    {
        var convo = new Conversation();
        var first = new ChatMessage { Role = ChatRole.User, Content = "hi" };
        convo.Add(first);
        convo.AddScopeMarker("src/backend");

        convo.TruncateTo(first.MsgId);

        Assert.DoesNotContain(convo.Messages, m => m.ScopeMarker != null);
    }

    [Fact]
    public void Branching_a_history_copies_its_markers()
    {
        // Branching is "copy the message list up to a point" elsewhere in the codebase (fork/rewind);
        // here we assert the primitive it relies on: a marker is an ordinary ChatMessage that survives
        // being copied into a new Conversation like any other entry.
        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "hi" });
        var marker = convo.AddScopeMarker("src/backend");

        var branch = new Conversation();
        foreach (var m in convo.Messages) branch.Add(new ChatMessage
        {
            MsgId = m.MsgId,
            Role = m.Role,
            Content = m.Content,
            ScopeMarker = m.ScopeMarker,
            CreatedAt = m.CreatedAt
        });

        Assert.Contains(branch.Messages, m => m.ScopeMarker == marker.ScopeMarker);
    }
}
