using SPLA.Domain.Models;
using SPLA.Service;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Tests;

/// <summary>
/// The debug "context" table must show the request in the order the model received it. It used to be
/// built in two passes — history first, then whatever the request contained that history did not —
/// which parked the composed system prompt at the bottom of the list and made it look as though the
/// prompt were sent last.
/// </summary>
public sealed class ContextInspectorTests
{
    private static ChatMessage Msg(ChatRole role, string content, string msgId = "")
        => new() { Role = role, Content = content, MsgId = msgId };

    [Fact]
    public void Composed_system_prompt_is_listed_first_not_last()
    {
        // What the orchestrator actually sends: a freshly composed system message (no MsgId, because
        // it is rebuilt per iteration), then the stored history.
        var user = Msg(ChatRole.User, "hello", "U-1");
        var assistant = Msg(ChatRole.Assistant, "hi", "A-1");

        var sent = new List<ChatMessage> { Msg(ChatRole.System, "SYSTEM PROMPT"), user, assistant };
        var history = new List<ChatMessage> { user, assistant };

        var lines = LiveAgentInspector.BuildContextLines(sent, history);

        Assert.Equal(new[] { "(injected)", "U-1", "A-1" }, lines.Select(l => l.MsgId));
        Assert.Equal(new[] { 1, 2, 3 }, lines.Select(l => l.Index));
        Assert.All(lines, l => Assert.True(l.InContext));
    }

    [Fact]
    public void Injections_keep_their_real_position_between_history_messages()
    {
        var user = Msg(ChatRole.User, "hello", "U-1");
        var sent = new List<ChatMessage>
        {
            Msg(ChatRole.System, "SYSTEM PROMPT"),
            Msg(ChatRole.System, "--- Working memory ---"),
            user
        };

        var lines = LiveAgentInspector.BuildContextLines(sent, new List<ChatMessage> { user });

        Assert.Equal(new[] { "(injected)", "(injected)", "U-1" }, lines.Select(l => l.MsgId));
        Assert.Equal("working-mem", lines[1].Source);
    }

    [Fact]
    public void History_dropped_from_the_request_is_shown_dimmed_in_place()
    {
        var older = Msg(ChatRole.User, "trimmed away", "U-1");
        var kept = Msg(ChatRole.User, "still here", "U-2");
        var tail = Msg(ChatRole.Assistant, "not sent yet", "A-9");

        var sent = new List<ChatMessage> { Msg(ChatRole.System, "SYSTEM PROMPT"), kept };
        var history = new List<ChatMessage> { older, kept, tail };

        var lines = LiveAgentInspector.BuildContextLines(sent, history);

        Assert.Equal(new[] { "(injected)", "U-1", "U-2", "A-9" }, lines.Select(l => l.MsgId));
        Assert.Equal(new[] { true, false, true, false }, lines.Select(l => l.InContext));
    }
}
