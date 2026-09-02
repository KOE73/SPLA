using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Opens a correspondence with another role: finds this chat's already-open address for
/// (<c>role</c>, <c>topic</c>) or creates the correspondent's chat on demand, and delivers the first
/// (or next) message across it. See <c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.3 and
/// <c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c> — Wave 5's own note on why this
/// ordinary tool exists at all: the virtual <c>reply_&lt;role&gt;[_&lt;topic&gt;]</c> address cannot
/// open itself, since before this call there is nothing for it to be the address of.
/// <para>
/// An ordinary registered tool, unlike the virtual <c>reply_*</c> tools it creates: the shared
/// <c>McpHost</c> can disclose this one identically to every chat, because opening a correspondence
/// needs no chat-specific tool surface — only <em>answering</em> one does.
/// </para>
/// </summary>
public sealed class AgentCorrespondTool : IMcpTool
{
    public string Name => "agent_correspond";

    /// <summary>Everything about this tool that does not fit its one-line description.
    /// Disclosed together with the tool itself — see <c>ToolFunctionDefinition.Details</c>.</summary>
    private static readonly string DetailsText =
        """
        tool: agent_correspond

        summary: Opens a correspondence with another role and delivers a message across it. Finds the
                 correspondent's chat if this exact (role, topic) address is already open on this chat,
                 or creates it fresh on demand. After this call succeeds, a virtual reply tool named
                 reply_<role>[_<topic>] appears in THIS chat's own tool list for sending further
                 messages on the same address — it takes only a 'text' parameter; there is no chat id
                 to remember or pass anywhere.

        arguments:
          role:
            required: true
            description: The project role to correspond with (e.g. architect). Must be one of the
                         roles declared in the project manifest — roles do not self-assign, so an
                         undeclared role is refused with a list of the available ones.
          topic:
            required: true
            description: Why this correspondence is being opened. MANDATORY, even though it reads
                         like a convenience: it is the only thing that tells two correspondents
                         holding the same role apart, and it becomes part of the reply tool's name
                         when this chat already holds another correspondence with the same role.
          text:
            required: true
            description: The message to send — the whole first (or next) thing you want to say.

        returns:
          A delivery receipt on success, e.g. "delivered: correspondence with 'architect' (api-design)
          is open ... Use 'reply_architect_api_design' to send your next message." This is a RECEIPT,
          NOT the correspondent's answer — do not treat it as content, and do not wait here for a
          reply before continuing your own work. Their answer, when it comes, arrives as an ordinary
          message in this conversation on a later turn.
          "error: ..." when role/topic/text is missing, the role is not declared, or correspondence is
          not permitted for this chat.

        notes:
          - Calling this again with the same (role, topic) reuses the already-open correspondence and
            simply sends another message — it does not open a second one or reset anything.
          - If the correspondent turns out to be a spawned run that is still going, your message is
            queued and answered only once that run finishes.
          - A correspondence that goes quiet later (the other chat archived or deleted) is announced
            in this conversation, and its reply tool stops appearing.

        examples:
          - request:
              role: architect
              topic: api-design
              text: "Does this endpoint shape look right to you?"
        """;

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Details = DetailsText,
            Description = "Opens a correspondence with another role (or reuses an already-open one) " +
                          "and delivers a message. Returns a delivery receipt, not their answer — " +
                          "reply_<role>[_<topic>] appears afterwards for sending more.",
            Scope = ToolScope.Skill,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    role = new
                    {
                        type = "string",
                        description = "Project role to correspond with (e.g. architect). Must be a declared role."
                    },
                    topic = new
                    {
                        type = "string",
                        description = "Why this correspondence is open. Required — distinguishes two correspondents of the same role."
                    },
                    text = new
                    {
                        type = "string",
                        description = "The message to send."
                    }
                },
                required = new[] { "role", "topic", "text" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var role = ToolJson.GetStringTrimmed(root, "role");
            var topic = ToolJson.GetStringTrimmed(root, "topic");
            var text = ToolJson.GetStringTrimmed(root, "text");

            if (string.IsNullOrEmpty(role))
                return Task.FromResult(ToolResult.Fail("error: 'role' is required", "missing role"));
            if (string.IsNullOrEmpty(topic))
                return Task.FromResult(ToolResult.Fail(
                    "error: 'topic' is required — it is the only thing that tells two correspondents " +
                    "holding the same role apart", "missing topic"));
            if (string.IsNullOrEmpty(text))
                return Task.FromResult(ToolResult.Fail("error: 'text' is required", "missing text"));

            var host = AgentSessionScope.Current?.Correspondence;
            if (host is null)
                return Task.FromResult(ToolResult.Fail(
                    "error: correspondence is not available in this session", "no correspondence host"));

            var result = host.Correspond(role!, topic!, text!);
            return Task.FromResult(result.Outcome switch
            {
                CorrespondOutcome.Delivered => ToolResult.Text(result.Message),
                CorrespondOutcome.InvalidArgument => ToolResult.Fail(result.Message),
                CorrespondOutcome.UnknownRole => ToolResult.Fail(result.Message, "unknown role"),
                CorrespondOutcome.Denied => ToolResult.Refuse(result.Message, "correspondence denied"),
                CorrespondOutcome.CorrespondentGone => ToolResult.Refuse(result.Message, "correspondent gone"),
                _ => ToolResult.Fail(result.Message)
            });
        }
        catch (JsonException)
        {
            return Task.FromResult(ToolResult.Fail("error: invalid_json", "invalid json"));
        }
    }
}
