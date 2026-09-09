using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// Copying a chat — what <c>chat.fork</c> and "duplicate" both come down to.
///
/// <para>Two things are pinned here, and they are the two halves of one real failure: a chat open on
/// screen could not be forked because a tool result had put a gzip blob into its history, YamlDotNet
/// wrote the control bytes into the file verbatim, and the copy — made by reading that file back —
/// died with "did not find expected key". The chat itself went on working, which is exactly why the
/// damage stayed invisible until someone pressed fork.</para>
///
/// <para>So: the copy is taken from the live session (no round trip to fail), and the file the chat
/// writes stays readable even when its history carries bytes YAML cannot represent.</para>
/// </summary>
public sealed class ChatDuplicationTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-chat-copy-{Guid.NewGuid():N}")).FullName;

    private static ChatManager NewChatManager(string root) => new(new ResolvedSettings
    {
        WorkspacePath = root,
        ProjectFilePath = Path.Combine(root, "project.spla")
    });

    /// <summary>A gzip header is the real thing that broke this — 0x1f 0x8b, then arbitrary bytes.</summary>
    private const string BinaryToolResult =
        "9.3.1-10\n\n    \u001f\u008b\u0008\u0000\u0002\u0003\u000e\u0014\u001b tail";

    [Fact]
    public void A_chat_whose_history_carries_control_bytes_still_loads_back()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("binary in a tool result");
            session.Messages.Add(new ChatSessionMessage { Role = "tool", Content = BinaryToolResult });
            chats.SaveChat(session);

            // Before the fix this threw a YamlException: the raw control bytes went into the literal
            // block and no parser would take them back.
            var reloaded = chats.LoadChat(session.Id);

            Assert.NotNull(reloaded);
            var content = Assert.Single(reloaded!.Messages).Content;
            Assert.StartsWith("9.3.1-10", content);
            Assert.DoesNotContain(content, c => c < ' ' && c != '\n' && c != '\t' && c != '\r');
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_copy_is_made_from_the_live_session_not_from_its_file()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("live");
            session.Messages.Add(new ChatSessionMessage { Role = "user", Content = "hi" });
            // Deliberately never saved: if duplication went through the file, this message would be
            // missing from the copy — which is the whole point of taking the source object in.
            var copy = chats.DuplicateChat(session);

            Assert.NotEqual(session.Id, copy.Id);
            Assert.Equal("hi", Assert.Single(copy.Messages).Content);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_copy_shares_nothing_with_the_chat_it_came_from()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("original");
            session.Messages.Add(new ChatSessionMessage { Role = "user", Content = "first" });
            session.Kv["task:current"] = "one";
            session.Agent = new SplaAgentSection { Instructions = new List<string> { "a.md" } };
            session.Context = new ChatSessionContext { Commands = { "ls" } };

            var copy = chats.DuplicateChat(session);

            // Every edit below would be visible in the copy if the two shared their lists.
            session.Messages.Add(new ChatSessionMessage { Role = "user", Content = "second" });
            session.Kv["task:current"] = "two";
            session.Agent!.Instructions!.Add("b.md");
            session.Context!.Commands.Add("pwd");

            Assert.Single(copy.Messages);
            Assert.Equal("one", copy.Kv["task:current"]);
            Assert.Equal(new[] { "a.md" }, copy.Agent!.Instructions!);
            Assert.Equal(new[] { "ls" }, copy.Context!.Commands);
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
