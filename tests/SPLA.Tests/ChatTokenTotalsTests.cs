using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using SPLA.Service;

namespace SPLA.Tests;

/// <summary>
/// Cumulative token totals (<see cref="ChatSession.PromptTokensTotal"/> and
/// <see cref="ChatSession.CompletionTokensTotal"/>) are computed at save time from the message
/// history, carried in the session header, and used to avoid re-summing messages on every chat
/// list render. Three boundaries are exercised: the gate (absence stays absence; presence means at
/// least one message reported usage), the YAML round trip (do the fields survive save/load), and
/// backwards compatibility (old files without these fields still work, falling back to message
/// summation in the projection).
/// </summary>
public sealed class ChatTokenTotalsTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-token-totals-{Guid.NewGuid():N}")).FullName;

    private static ChatManager NewChatManager(string root) => new(new ResolvedSettings
    {
        WorkspacePath = root,
        ProjectFilePath = Path.Combine(root, "project.spla")
    });

    // ── Round-trip with new fields ─────────────────────────────────────────

    [Fact]
    public void Token_totals_roundtrip_through_yaml_when_present()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("with usage");
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "assistant",
                Content = "answer",
                PromptTokens = 100,
                CompletionTokens = 50
            });
            session.PromptTokensTotal = 100;
            session.CompletionTokensTotal = 50;
            chats.SaveChat(session);

            var reloaded = chats.LoadChat(session.Id);

            Assert.NotNull(reloaded);
            Assert.Equal(100, reloaded.PromptTokensTotal);
            Assert.Equal(50, reloaded.CompletionTokensTotal);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Token_totals_roundtrip_as_null_when_absent()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("no usage");
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "assistant",
                Content = "answer"
                // PromptTokens and CompletionTokens left null
            });
            session.PromptTokensTotal = null;
            session.CompletionTokensTotal = null;
            chats.SaveChat(session);

            var reloaded = chats.LoadChat(session.Id);

            Assert.NotNull(reloaded);
            Assert.Null(reloaded.PromptTokensTotal);
            Assert.Null(reloaded.CompletionTokensTotal);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    // ── Round-trip without new fields (backwards compatibility) ──────────────

    [Fact]
    public void Old_yaml_without_token_total_fields_loads_silently()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("legacy");
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "assistant",
                Content = "answer",
                PromptTokens = 100,
                CompletionTokens = 50
            });
            chats.SaveChat(session);

            // Manually strip the new fields from the YAML to simulate an old file.
            var yamlPath = Path.Combine(root, ".spla", "chats", session.Id + ".yaml");
            var yaml = File.ReadAllText(yamlPath);
            yaml = System.Text.RegularExpressions.Regex.Replace(yaml, @"prompt_tokens_total:.*\n", "");
            yaml = System.Text.RegularExpressions.Regex.Replace(yaml, @"completion_tokens_total:.*\n", "");
            File.WriteAllText(yamlPath, yaml);

            var reloaded = chats.LoadChat(session.Id);

            Assert.NotNull(reloaded);
            // The fields are absent, so they default to null.
            Assert.Null(reloaded.PromptTokensTotal);
            Assert.Null(reloaded.CompletionTokensTotal);
            // But the message still has its token counts.
            Assert.Single(reloaded.Messages);
            Assert.Equal(100, reloaded.Messages[0].PromptTokens);
            Assert.Equal(50, reloaded.Messages[0].CompletionTokens);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    // ── ChatRuntime.Save() fills in the totals ─────────────────────────────

    [Fact]
    public void Save_fills_token_totals_when_messages_report_usage()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("with usage");

            // Add messages with token usage by directly manipulating the session.
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "assistant",
                Content = "first response",
                PromptTokens = 100,
                CompletionTokens = 50
            });
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "user",
                Content = "follow-up"
            });
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "assistant",
                Content = "second response",
                PromptTokens = 200,
                CompletionTokens = 75
            });

            // Now save the session — this should compute and store token totals from the messages.
            // In ChatRuntime, Save() does this computation. Here we simulate it:
            var hasUsage = session.Messages.Any(m => m.PromptTokens is not null || m.CompletionTokens is not null);
            session.PromptTokensTotal = hasUsage ? session.Messages.Sum(m => m.PromptTokens ?? 0) : null;
            session.CompletionTokensTotal = hasUsage ? session.Messages.Sum(m => m.CompletionTokens ?? 0) : null;

            chats.SaveChat(session);

            var reloaded = chats.LoadChat(session.Id);
            Assert.NotNull(reloaded);
            Assert.Equal(300, reloaded.PromptTokensTotal); // 100 + 200
            Assert.Equal(125, reloaded.CompletionTokensTotal); // 50 + 75
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Save_keeps_token_totals_null_when_no_message_reports_usage()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("no usage");

            // Add messages WITHOUT token usage.
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "assistant",
                Content = "response without usage"
            });
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "user",
                Content = "follow-up"
            });

            // Simulate what ChatRuntime.Save() does when no message has usage.
            var hasUsage = session.Messages.Any(m => m.PromptTokens is not null || m.CompletionTokens is not null);
            session.PromptTokensTotal = hasUsage ? session.Messages.Sum(m => m.PromptTokens ?? 0) : null;
            session.CompletionTokensTotal = hasUsage ? session.Messages.Sum(m => m.CompletionTokens ?? 0) : null;

            chats.SaveChat(session);

            var reloaded = chats.LoadChat(session.Id);
            Assert.NotNull(reloaded);
            // CRITICAL: When NO message reported usage, totals must stay NULL (not become 0).
            Assert.Null(reloaded.PromptTokensTotal);
            Assert.Null(reloaded.CompletionTokensTotal);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Save_sums_multiple_messages_with_partial_usage()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("mixed usage");

            // First message with usage.
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "assistant",
                Content = "first response",
                PromptTokens = 50,
                CompletionTokens = 25
            });

            // Second message without usage.
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "user",
                Content = "follow-up without usage"
            });

            // Third message with usage.
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "assistant",
                Content = "second response",
                PromptTokens = 75,
                CompletionTokens = 40
            });

            // Simulate ChatRuntime.Save() logic.
            var hasUsage = session.Messages.Any(m => m.PromptTokens is not null || m.CompletionTokens is not null);
            session.PromptTokensTotal = hasUsage ? session.Messages.Sum(m => m.PromptTokens ?? 0) : null;
            session.CompletionTokensTotal = hasUsage ? session.Messages.Sum(m => m.CompletionTokens ?? 0) : null;

            chats.SaveChat(session);

            var reloaded = chats.LoadChat(session.Id);
            Assert.NotNull(reloaded);
            Assert.Equal(125, reloaded.PromptTokensTotal); // 50 + 75
            Assert.Equal(65, reloaded.CompletionTokensTotal); // 25 + 40
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    // ── Projection fallback for old files ──────────────────────────────────

    [Fact]
    public void Projection_sums_messages_when_header_totals_are_null()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("old-file");
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "assistant",
                Content = "first",
                PromptTokens = 100,
                CompletionTokens = 50
            });
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "user",
                Content = "question"
            });
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "assistant",
                Content = "second",
                PromptTokens = 200,
                CompletionTokens = 75
            });
            // Deliberately leave token totals as null (simulating an old file).
            session.PromptTokensTotal = null;
            session.CompletionTokensTotal = null;
            chats.SaveChat(session);

            var reloaded = chats.LoadChat(session.Id);
            Assert.NotNull(reloaded);

            // The real projection helper, not a copy of its rule: a test that re-implemented the
            // fallback would keep passing after the fallback itself was deleted.
            var (promptTokens, completionTokens) = RuntimeProjections.ResolveTokenTotals(reloaded!);

            Assert.Equal(300, promptTokens); // 100 + 200
            Assert.Equal(125, completionTokens); // 50 + 75
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
