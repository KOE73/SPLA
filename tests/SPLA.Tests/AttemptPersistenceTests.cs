using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// Abandoned generations (<see cref="GenerationAttempt"/>) become part of a chat's persisted record
/// only when <c>agent: save_attempts</c> asks for it — see <see cref="Conversation.ShouldPersist"/>
/// and <see cref="ChatSessionAttempt"/>. Two boundaries are exercised here: the pure gating decision
/// (does a message that is otherwise empty except for its attempts survive?), and the actual YAML
/// round trip through <see cref="ChatManager"/> (does the text really come back?). Both are needed —
/// the gate could be right while the YAML shape silently dropped a field, or vice versa.
/// </summary>
public sealed class AttemptPersistenceTests
{
    private static ChatMessage DegenerateRecord() => new()
    {
        Role = ChatRole.Assistant,
        Content = "", // never the abandoned text — see ConversationOrchestrator's degenerate handling
        Attempts = new List<GenerationAttempt>
        {
            new()
            {
                Index = 1,
                Outcome = AttemptOutcome.Repetition,
                Content = "the answer it kept repeating",
                Reasoning = "the reasoning it kept repeating",
                Note = "repetition in content: period 12 chars, x40",
                Chars = 480,
                Duration = TimeSpan.FromSeconds(2.5)
            }
        }
    };

    // ── The gate: Conversation.ShouldPersist / PersistableWith ────────────────

    [Fact]
    public void An_attempts_only_message_survives_only_when_save_attempts_is_on()
    {
        var msg = DegenerateRecord();

        Assert.False(Conversation.ShouldPersist(msg, saveToolCalls: false, saveAttempts: false));
        Assert.True(Conversation.ShouldPersist(msg, saveToolCalls: false, saveAttempts: true));

        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "hi" });
        convo.Add(msg);

        Assert.DoesNotContain(convo.PersistableWith(saveToolCalls: false, saveAttempts: false), m => m == msg);
        Assert.Contains(convo.PersistableWith(saveToolCalls: false, saveAttempts: true), m => m == msg);
    }

    [Fact]
    public void A_normal_message_with_attempts_riding_along_is_unaffected_by_the_setting()
    {
        // Attempts are never what KEEPS an ordinary answer — only what keeps an otherwise-empty one.
        var msg = new ChatMessage { Role = ChatRole.Assistant, Content = "the real answer",
            Attempts = DegenerateRecord().Attempts };

        Assert.True(Conversation.ShouldPersist(msg, saveAttempts: false));
        Assert.True(Conversation.ShouldPersist(msg, saveAttempts: true));
    }

    // ── The wire: ChatManager save + load through real YAML ───────────────────

    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-attempt-persist-{Guid.NewGuid():N}")).FullName;

    private static ChatManager NewChatManager(string root) => new(new ResolvedSettings
    {
        WorkspacePath = root,
        ProjectFilePath = Path.Combine(root, "project.spla")
    });

    private static ChatSessionMessage ToSessionMessage(ChatMessage m, bool saveAttempts) => new()
    {
        Role = m.Role.ToString().ToLower(),
        Content = m.Content ?? "",
        Attempts = saveAttempts && m.Attempts?.Count > 0
            ? m.Attempts.Select(a => new ChatSessionAttempt
            {
                Index = a.Index,
                Outcome = a.Outcome.ToString(),
                Content = a.Content,
                Reasoning = a.Reasoning,
                Note = a.Note,
                Chars = a.Chars,
                DurationMs = (long)a.Duration.TotalMilliseconds
            }).ToList()
            : null
    };

    [Fact]
    public void Attempt_text_survives_a_real_save_and_load_when_the_setting_is_on()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("attempts on");
            session.Messages.Add(ToSessionMessage(DegenerateRecord(), saveAttempts: true));
            chats.SaveChat(session);

            var reloaded = chats.LoadChat(session.Id);

            var attempts = Assert.Single(Assert.Single(reloaded!.Messages).Attempts!);
            Assert.Equal(1, attempts.Index);
            Assert.Equal("Repetition", attempts.Outcome);
            Assert.Equal("the answer it kept repeating", attempts.Content);
            Assert.Equal("the reasoning it kept repeating", attempts.Reasoning);
            Assert.Equal("repetition in content: period 12 chars, x40", attempts.Note);
            Assert.Equal(480, attempts.Chars);
            Assert.Equal(2500, attempts.DurationMs);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Nothing_about_attempts_is_written_when_the_setting_is_off()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("attempts off");
            // A normal, non-empty assistant answer that also happens to carry attempts (a retry
            // succeeded after one loop) — it persists regardless of the setting because its Content
            // does, exactly like ChatRuntime.Save() would build it with save_attempts off: Attempts
            // left null even though the message itself survives.
            var succeeded = new ChatMessage { Role = ChatRole.Assistant, Content = "the real answer",
                Attempts = DegenerateRecord().Attempts };
            session.Messages.Add(ToSessionMessage(succeeded, saveAttempts: false));
            chats.SaveChat(session);

            var raw = File.ReadAllText(Path.Combine(root, ".spla", "chats", session.Id + ".yaml"));
            Assert.DoesNotContain("attempts:", raw);
            Assert.DoesNotContain("kept repeating", raw);

            var reloaded = chats.LoadChat(session.Id);
            Assert.All(reloaded!.Messages, m => Assert.Null(m.Attempts));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
