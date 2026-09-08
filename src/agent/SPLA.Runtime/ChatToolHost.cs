using System.Linq;
using System.Text.Json;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.ToolSets;

namespace SPLA.Runtime;

/// <summary>
/// Per-chat wrapper around the runtime's shared <see cref="IToolHost"/> (in practice
/// <c>McpHost</c>). Sits between <see cref="ChatRuntime"/> and the
/// <c>ConversationOrchestrator</c> it builds, so the orchestrator's view of "what tools exist and
/// how to run them" is a seam this chat owns rather than the runtime-wide host itself.
///
/// <para>
/// Wave 5 of <c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c> is what fills this
/// seam: this chat's own virtual <c>reply_&lt;role&gt;[_&lt;topic&gt;]</c> tools — one per entry in
/// <paramref name="owner"/>'s <see cref="IReplyToolSource.Correspondences"/> — are mixed into
/// <see cref="GetToolDefinitions"/> and answered directly by <see cref="ExecuteToolAsync"/>, WITHOUT
/// ever reaching <paramref name="inner"/> (the shared <c>McpHost</c>, registered once for the whole
/// runtime). That is the entire reason this seam exists (ADR_20260827-2 §2.3): a chat's tool surface
/// must be able to differ from every other chat's, and a host shared by the whole runtime cannot
/// express that.
/// </para>
/// <para>
/// With <paramref name="owner"/> null (or no open correspondences), every member here still
/// delegates verbatim to <paramref name="inner"/> — the shape wave 0 proved empty and this wave
/// preserves for that case.
/// </para>
/// <para>
/// Wave 5б adds the other half of the same seam: a chat opened under an <c>as:</c> role narrows its
/// own tool surface here, the same way <c>SpawnedAgentRunner</c> narrows a spawned run's — a filter
/// built fresh from the shared, read-only <paramref name="toolSets"/> registry and this chat's own
/// resolved <paramref name="roleToolSets"/>, never a mutation of the registry itself (that would leak
/// the narrowing into every other chat sharing it). <paramref name="roleToolSets"/> is null for a
/// chat with no role — the deliberate no-op case: <see cref="GetToolDefinitions"/> then skips the
/// filter entirely rather than running it against an empty selection, which is what "a chat without
/// <c>as:</c> narrows nothing" actually requires (an omitted filter, not one fed an empty set).
/// </para>
/// </summary>
public sealed class ChatToolHost(
    IToolHost inner,
    IReplyToolSource? owner = null,
    ToolSetRegistry? toolSets = null,
    IReadOnlyDictionary<string, string>? roleToolSets = null) : IToolHost
{
    public IEnumerable<ToolDefinition> GetToolDefinitions()
    {
        var definitions = inner.GetToolDefinitions();

        // Narrows only the inner (real) tools — never the virtual reply_* ones added below, which no
        // ToolSetRegistry set claims and so IsDisclosedForRole would keep anyway; skipped outright
        // with a null roleToolSets rather than filtering against one, so a role-less chat's surface is
        // byte-for-byte what it always was.
        if (toolSets != null && roleToolSets != null)
            definitions = definitions.Where(t => ToolSetRegistry.IsDisclosedForRole(t.Function.Name, toolSets, roleToolSets));

        if (owner is null) return definitions;

        var correspondences = owner.Correspondences;
        return correspondences.Count == 0
            ? definitions
            : definitions.Concat(correspondences.Select(BuildReplyDefinition));
    }

    public Task<ToolResult> ExecuteToolAsync(
        AgentMode mode,
        string name,
        string argumentsJson,
        CancellationToken cancellationToken = default,
        ToolCallContext? context = null)
    {
        if (owner is not null)
        {
            var correspondence = owner.Correspondences.FirstOrDefault(c => c.ToolName == name);
            if (correspondence is not null)
                return Task.FromResult(ExecuteReply(correspondence, argumentsJson));
        }

        return inner.ExecuteToolAsync(mode, name, argumentsJson, cancellationToken, context);
    }

    /// <summary>Runs one virtual reply call: parses the sole <c>text</c> parameter and routes it
    /// through <see cref="ChatRuntime.SendReply"/> — the same edge, gate check and depth counter
    /// <c>agent_correspond</c>'s first message on this address already went through.</summary>
    private ToolResult ExecuteReply(Correspondence correspondence, string argumentsJson)
    {
        string? text;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            text = doc.RootElement.TryGetProperty("text", out var v) && v.ValueKind == JsonValueKind.String
                ? v.GetString()
                : null;
        }
        catch (JsonException)
        {
            return ToolResult.Fail("error: invalid_json", "invalid json");
        }

        if (string.IsNullOrWhiteSpace(text))
            return ToolResult.Fail("error: 'text' is required", "missing text");

        var result = owner!.SendReply(correspondence.Role, correspondence.InstanceNo, text);
        return result.Outcome switch
        {
            ChatRuntime.ReplyOutcome.Delivered => ToolResult.Text(
                // A receipt, never the correspondent's answer (ADR §2.3, plan trap 10) — deliberately
                // free of any content that could be mistaken for a reply. Their actual words, when
                // they come, arrive as an ordinary user-role message on this chat's own next turn.
                "delivered: reply queued. This is a delivery receipt, not their answer — their reply " +
                "will arrive on its own turn; it has not happened yet."),
            ChatRuntime.ReplyOutcome.Denied => ToolResult.Refuse($"error: {result.Reason}", result.Reason),
            _ => ToolResult.Refuse($"error: {result.Reason}", result.Reason)
        };
    }

    /// <summary>The definition of one correspondence's virtual reply tool. <see cref="Correspondence.ToolName"/>
    /// is already the whole address (ADR §2.3: "имя инструмента есть адрес") — nothing here ever
    /// carries <see cref="Correspondence.ChatId"/> into the schema or the description.</summary>
    private static ToolDefinition BuildReplyDefinition(Correspondence c) => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = c.ToolName,
            Description = $"Sends your next message to {c.Role} on this correspondence" +
                           (c.Purpose.Length > 0 ? $" ({c.Purpose})." : "."),
            Details =
                $"""
                tool: {c.ToolName}

                summary: Sends one message across an already-open correspondence with '{c.Role}'
                         ({(c.Purpose.Length > 0 ? c.Purpose : "no stated purpose")}). This is a REPLY,
                         addressed by this tool's own name — there is no chat id parameter, and there
                         never will be one for this address: the name IS the address, and it stays this
                         name for as long as the correspondence is open.

                arguments:
                  text:
                    required: true
                    description: What to say. The whole message — there is no other field.

                returns:
                  A delivery receipt ("delivered: reply queued..."), NEVER the correspondent's answer.
                  Waiting for a reply here would block this turn while the correspondent is entitled to
                  think for several turns of their own — do not treat the receipt as content, and do
                  not stop and wait for a response before continuing your own work. Their reply, when
                  it comes, arrives as an ordinary message in this conversation on a later turn — you
                  will not miss it.

                notes:
                  - If this correspondence points at a spawned run that is still going, your reply is
                    queued and answered only after that run finishes — it does not interrupt it.
                  - A correspondence that goes quiet (the other chat archived or was deleted) is
                    announced in this conversation; this tool then stops appearing.
                """,
            Scope = ToolScope.Skill,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    text = new { type = "string", description = "The message to send." }
                },
                required = new[] { "text" }
            }
        }
    };
}
