using System.Linq;
using SPLA.Domain.Context;
using SPLA.Domain.Models;

namespace SPLA.Tests;

public class ContextAssemblerTests
{
    private static ChatMessage User(string text) => new() { Role = ChatRole.User, Content = text };

    [Fact]
    public void Ephemeral_messages_are_never_sent()
    {
        var convo = new[]
        {
            User("hello"),
            new ChatMessage { Role = ChatRole.System, Content = "Stopped.", IsEphemeral = true },
            new ChatMessage { Role = ChatRole.Assistant, Content = "hi" }
        };

        var result = ContextAssembler.Assemble(convo);

        Assert.DoesNotContain(result, m => m.Content == "Stopped.");
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void System_prompt_is_sent_even_though_it_is_a_system_message()
    {
        // The old UI bug: a system-role prompt was filtered out by render type.
        // The domain rule sends it because it is not ephemeral.
        var convo = new[]
        {
            new ChatMessage { Role = ChatRole.System, Content = "You are SPLA." },
            User("hi")
        };

        var result = ContextAssembler.Assemble(convo);

        Assert.Contains(result, m => m.Role == ChatRole.System && m.Content == "You are SPLA.");
    }

    [Fact]
    public void Never_retention_is_dropped()
    {
        var convo = new[]
        {
            User("keep"),
            new ChatMessage { Role = ChatRole.Assistant, Content = "drop me", RetentionPolicy = ContextRetention.Never }
        };

        var result = ContextAssembler.Assemble(convo);

        Assert.Single(result);
        Assert.Equal("keep", result[0].Content);
    }

    [Fact]
    public void UntilSuperseded_keeps_only_the_latest_per_key()
    {
        var convo = new[]
        {
            new ChatMessage { Role = ChatRole.User, Content = "old status", RetentionPolicy = ContextRetention.UntilSuperseded, ReplacementKey = "status" },
            new ChatMessage { Role = ChatRole.User, Content = "new status", RetentionPolicy = ContextRetention.UntilSuperseded, ReplacementKey = "status" },
            User("after")
        };

        var result = ContextAssembler.Assemble(convo);

        Assert.DoesNotContain(result, m => m.Content == "old status");
        Assert.Contains(result, m => m.Content == "new status");
    }

    [Fact]
    public void Assistant_toolcall_message_with_empty_content_is_kept()
    {
        var convo = new[]
        {
            new ChatMessage
            {
                Role = ChatRole.Assistant,
                Content = "",
                ToolCalls = new() { new ToolCall { Id = "1", Function = new FunctionCall { Name = "system_read_file", Arguments = "{}" } } }
            }
        };

        var result = ContextAssembler.Assemble(convo);

        Assert.Single(result);
    }

    [Fact]
    public void Order_is_preserved_oldest_first()
    {
        var convo = new[] { User("1"), User("2"), User("3") };

        var result = ContextAssembler.Assemble(convo);

        Assert.Equal(new[] { "1", "2", "3" }, result.Select(m => m.Content));
    }
}
