using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Agent;
using SPLA.Domain.Context;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 4 of <c>docs/plans/PLAN_20260914_plugins_geometry-workspace.md</c> — the <c>agent.tool_images</c>
/// switch. The orchestrator is the producer of a retention policy here; <see cref="ContextAssembler"/>
/// (the consumer) is unchanged and covered by its own tests.
///
/// <para>What these tests are really guarding is that this is <b>eviction from the assembled context,
/// not deletion</b>: every picture stays in the <see cref="Conversation"/>, and the tool-call pair it
/// sits next to is never disturbed.</para>
/// </summary>
public class ToolImageRetentionTests
{
    private sealed class FakeLlm : ILlmGateway
    {
        private readonly Queue<ChatMessage> _responses;
        public List<List<ChatMessage>> SeenContexts { get; } = new();

        public FakeLlm(IEnumerable<ChatMessage> responses) => _responses = new(responses);

        public Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default)
        {
            SeenContexts.Add(ctx.Messages.ToList());
            return Task.FromResult(new LlmTurnResult { Message = _responses.Dequeue(), Status = LlmTurnStatus.Ok });
        }
    }

    /// <summary>Every call answers with text plus one picture — the shape of a screenshot or a
    /// geometry frame.</summary>
    private sealed class ImageToolHost : IToolHost
    {
        public IEnumerable<ToolDefinition> GetToolDefinitions() => Array.Empty<ToolDefinition>();

        public Task<ToolResult> ExecuteToolAsync(
            AgentMode mode, string name, string argumentsJson,
            CancellationToken cancellationToken = default, ToolCallContext? context = null)
            => Task.FromResult(ToolResult.From(
                new ToolText($"result of {name}"),
                new ToolImage("QUJD", "image/png")));
    }

    private static ToolCall Call(string id, string name) =>
        new() { Id = id, Function = new FunctionCall { Name = name, Arguments = "{}" } };

    /// <summary>Two tool calls that each return a picture, then a plain answer.</summary>
    private static async Task<Conversation> RunTwoImageCalls(ToolImagesMode mode)
    {
        var llm = new FakeLlm(new[]
        {
            new ChatMessage { Role = ChatRole.Assistant, Content = "", ToolCalls = new() { Call("1", "geom_box") } },
            new ChatMessage { Role = ChatRole.Assistant, Content = "", ToolCalls = new() { Call("2", "geom_box") } },
            new ChatMessage { Role = ChatRole.Assistant, Content = "done" }
        });
        var orch = new ConversationOrchestrator(llm, new ImageToolHost())
        {
            ToolFilter = (t, _) => t,
            ToolImages = mode
        };
        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "place a box" });
        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, new AgentCallbacks());
        return convo;
    }

    private static List<ChatMessage> ImageMessages(IEnumerable<ChatMessage> messages) =>
        messages.Where(m => m.Images is { Count: > 0 }).ToList();

    [Fact]
    public async Task Last_keeps_both_pictures_in_the_chat_but_sends_only_the_newest()
    {
        var convo = await RunTwoImageCalls(ToolImagesMode.Last);

        // Nothing is deleted: the chat still holds both, which is what makes the setting reversible.
        Assert.Equal(2, ImageMessages(convo.Messages).Count);

        var assembled = ContextAssembler.Assemble(convo.Messages);
        var sent = ImageMessages(assembled);
        Assert.Single(sent);
        Assert.Same(ImageMessages(convo.Messages).Last(), sent[0]);
        Assert.All(ImageMessages(convo.Messages), m =>
        {
            Assert.Equal(ContextRetention.UntilSuperseded, m.RetentionPolicy);
            Assert.Equal(ConversationOrchestrator.ToolImageReplacementKey, m.ReplacementKey);
        });
    }

    [Fact]
    public async Task All_is_a_no_op_and_is_what_an_unset_setting_resolves_to()
    {
        var convo = await RunTwoImageCalls(ToolImagesMode.All);

        var pictures = ImageMessages(convo.Messages);
        Assert.Equal(2, pictures.Count);
        Assert.Equal(2, ImageMessages(ContextAssembler.Assemble(convo.Messages)).Count);
        Assert.All(pictures, m =>
        {
            Assert.Equal(ContextRetention.Persistent, m.RetentionPolicy);
            Assert.Null(m.ReplacementKey);
        });

        // The default is what an orchestrator nobody configured already does, and what a manifest
        // with no tool_images key resolves to — so an existing project sees no change at all.
        Assert.Equal(ToolImagesMode.All, new ConversationOrchestrator(new FakeLlm([]), new ImageToolHost()).ToolImages);
        Assert.Equal(ToolImagesMode.All, new ResolvedSettings().ToolImages);
    }

    /// <summary>
    /// The trap from <c>PLAN_20260827_core_context-retention.md</c> §2: retention acts on a message,
    /// but a tool call is a <b>pair</b> — an assistant message carrying <c>tool_calls</c> and a tool
    /// message carrying the matching <c>ToolCallId</c> — and dropping half of one leaves a dangling
    /// call every provider rejects. It does not apply here because the picture is a <b>separate
    /// synthetic user message</b>: the pair it follows is never the thing being superseded.
    /// </summary>
    [Fact]
    public async Task Superseding_a_picture_never_breaks_the_call_result_pair()
    {
        var assembled = ContextAssembler.Assemble((await RunTwoImageCalls(ToolImagesMode.Last)).Messages);

        var calls = assembled.Where(m => m.ToolCalls is { Count: > 0 }).SelectMany(m => m.ToolCalls!).ToList();
        var results = assembled.Where(m => m.Role == ChatRole.Tool).ToList();

        // Both pairs survive, whole, even though the first call's picture was evicted.
        Assert.Equal(2, calls.Count);
        Assert.Equal(2, results.Count);
        Assert.Equal(
            calls.Select(c => c.Id).OrderBy(x => x),
            results.Select(r => r.ToolCallId).OrderBy(x => x));
        // No orphan on either side.
        Assert.All(results, r => Assert.Contains(calls, c => c.Id == r.ToolCallId));

        // And the evicted message is not a tool result or a tool call — it is the extra user message.
        var evicted = ImageMessages(assembled);
        Assert.Single(evicted);
        Assert.Equal(ChatRole.User, evicted[0].Role);
    }

    // ── The setting's own round trip ─────────────────────────────────────────

    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-toolimg-{Guid.NewGuid():N}")).FullName;

    private static string WriteManifest(string root, string? toolImages)
    {
        var manifest = Path.Combine(root, "test.spla");
        var line = toolImages is null ? "" : $"\n  tool_images: {toolImages}";
        File.WriteAllText(manifest, $"""
            version: 1
            name: ToolImagesTest
            agent:
              mode: Edit{line}
            connections:
              - id: fake
                name: Fake
                provider: fake
                endpoint: http://127.0.0.1:1/v1
                api_key: fake-key
                models:
                  - id: fake-model
                    model: fake-model
                    default: true
            """);
        return manifest;
    }

    [Theory]
    [InlineData(null, ToolImagesMode.All)]
    [InlineData("all", ToolImagesMode.All)]
    [InlineData("last", ToolImagesMode.Last)]
    public void Manifest_value_resolves_through_the_cascade(string? written, ToolImagesMode expected)
    {
        var root = TempRoot();
        try
        {
            Assert.Equal(expected, ConfigLoader.LoadAndResolve(WriteManifest(root, written)).ToolImages);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void An_unknown_value_is_refused_rather_than_silently_meaning_all()
    {
        var root = TempRoot();
        try
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => ConfigLoader.LoadAndResolve(WriteManifest(root, "newest")));
            Assert.Contains("tool_images", ex.Message);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    /// <summary>
    /// Both halves of the policy have to survive a restart. <c>RetentionPolicy</c> already did;
    /// <c>ReplacementKey</c> did not, and an <c>UntilSuperseded</c> message whose key was lost reads
    /// to <see cref="ContextAssembler"/> as "keep" — the eviction would have quietly stopped working
    /// at the first reopen.
    /// </summary>
    [Fact]
    public void Retention_and_replacement_key_survive_a_chat_save_and_reload()
    {
        var root = TempRoot();
        AgentRuntime? runtime = null;
        try
        {
            var settings = ConfigLoader.LoadAndResolve(WriteManifest(root, "last"));
            runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);
            var chats = new ChatRegistry(runtime);

            var chat = chats.CreateNew("t");
            chat.InjectMessage(ChatRole.User, "[Image from geom_box]");
            var picture = chat.Messages.Last(m => m.Role == ChatRole.User);
            picture.RetentionPolicy = ContextRetention.UntilSuperseded;
            picture.ReplacementKey = ConversationOrchestrator.ToolImageReplacementKey;
            chat.Save();

            var reloaded = runtime.ChatManager.LoadChat(chat.ChatId);
            Assert.NotNull(reloaded);
            var reopened = new ChatRuntime(runtime, reloaded!);

            var restored = reopened.Messages.Single(m => m.Content == "[Image from geom_box]");
            Assert.Equal(ContextRetention.UntilSuperseded, restored.RetentionPolicy);
            Assert.Equal(ConversationOrchestrator.ToolImageReplacementKey, restored.ReplacementKey);
        }
        finally
        {
            runtime?.Dispose();
            Directory.Delete(root, recursive: true);
        }
    }
}
