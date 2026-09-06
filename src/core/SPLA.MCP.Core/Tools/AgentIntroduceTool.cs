using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Introduces two other chats to each other — see
/// <c>docs/adr/ADR_20260906_core_one-address.md</c> §2.4 and wave 4 of
/// <c>docs/plans/PLAN_20260906_core_chat-directory-and-await.md</c>.
/// <para>
/// The tool that could not exist before public names did: addressing somebody and creating him were a
/// single act, so a third party had nothing to say out loud that meant "that one, the architect you are
/// both already talking about" (ADR §1). Now it does — and the identifier still never appears in any
/// message, because the two names go through the runtime and each party simply finds a new
/// <c>reply_&lt;role&gt;_&lt;n&gt;</c> in its own tool list.
/// </para>
/// <para>
/// Kept apart from <c>agent_correspond</c> rather than folded into it as a third argument: the two
/// differ in exactly one thing that matters, who ends up on the edge, and a flag that decides whether
/// the caller is a participant or a bystander is the kind of flag a model sets by accident.
/// </para>
/// </summary>
public sealed class AgentIntroduceTool : IMcpTool
{
    public string Name => "agent_introduce";

    /// <summary>Everything about this tool that does not fit its one-line description.
    /// Disclosed together with the tool itself — see <c>ToolFunctionDefinition.Details</c>.</summary>
    private static readonly string DetailsText =
        """
        tool: agent_introduce

        summary: Puts two OTHER chats in touch with each other. Both get an address to the other and can
                 talk from then on; you do not. Name each of them by its public name (e.g. architect_2)
                 — an introduction connects two chats that already exist, so a role name is refused.
                 Your 'text' is delivered to the FIRST one you name: it is the one that starts the
                 conversation, and without a message nothing would start at all. The second one needs no
                 message from you — the next thing it hears is the first one's own reply.

        arguments:
          first:
            required: true
            description: Public name of the chat that will speak first and receive your text
                         (e.g. architect_2). Read public names off what is shown to you; never assemble
                         one out of a role and a guessed number.
          second:
            required: true
            description: Public name of the other chat (e.g. reviewer_3).
          text:
            required: true
            description: What to tell the first one so the conversation can start — normally why these
                         two are being introduced and what you would like them to settle between
                         themselves. They will be told it came from you.
          purpose:
            required: false
            description: Why these two are being introduced, in your own words. Purely explanatory: it
                         is carried into both parties' description of the new correspondence and never
                         becomes part of any address. Null = unstated.

        returns:
          A receipt on success, e.g. "introduced: 'architect_2' and 'reviewer_3' now correspond ...".
          This is a RECEIPT, NOT a result and NOT a conversation. You are not on that edge: you will not
          see what they say to each other, neither of them is answering you, and there is nothing here
          to wait for. Say what you needed to say and carry on with your own work.
          "error: ..." when a name is not a chat, the two names are the same chat, you named YOURSELF as
          one of the two (that is ordinary correspondence — use agent_correspond), the chat is archived,
          or those two already correspond.

        notes:
          - The two already correspond → refused, and nothing is opened twice. Your words would
            otherwise land inside a conversation you were never part of, attributed to one of them.
          - Naming yourself is refused rather than quietly redirected: agent_correspond is the tool for
            talking to somebody yourself, and the difference is who ends up on the edge.
          - Archived chats and spawned runs have no public name and cannot be introduced.

        examples:
          - request:
              first: architect_2
              second: reviewer_3
              purpose: the migration review
              text: "Reviewer_3 has read the migration and disagrees about the index. Settle it between
                     you and tell me what you decided."
        """;

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Details = DetailsText,
            Description = "Introduces two other chats to each other by their public names (e.g. " +
                          "architect_2 and reviewer_3), and delivers your message to the first one so " +
                          "the conversation starts. Returns a receipt, not a conversation — you are " +
                          "not part of what they say to each other.",
            Scope = ToolScope.Skill,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    first = new
                    {
                        type = "string",
                        description = "Public name of the chat that speaks first and receives your text (e.g. architect_2)."
                    },
                    second = new
                    {
                        type = "string",
                        description = "Public name of the other chat (e.g. reviewer_3)."
                    },
                    text = new
                    {
                        type = "string",
                        description = "What to tell the first one so the conversation can start."
                    },
                    // Nullable-and-required rather than simply omitted from `required`: OpenAI strict
                    // mode wants every property listed there, so an optional field is spelled
                    // present-but-nullable — the same shape AgentCorrespondTool's own `purpose` uses.
                    purpose = new
                    {
                        type = new[] { "string", "null" },
                        description = "Why these two are being introduced. Null = unstated; never part of any address."
                    }
                },
                required = new[] { "first", "second", "text", "purpose" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var first = ToolJson.GetStringTrimmed(root, "first");
            var second = ToolJson.GetStringTrimmed(root, "second");
            var text = ToolJson.GetStringTrimmed(root, "text");
            var purpose = ToolJson.GetStringTrimmed(root, "purpose") ?? "";

            if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second))
                return Task.FromResult(ToolResult.Fail(
                    "error: both 'first' and 'second' are required", "missing party"));
            if (string.IsNullOrEmpty(text))
                return Task.FromResult(ToolResult.Fail("error: 'text' is required", "missing text"));

            var host = AgentSessionScope.Current?.Correspondence;
            if (host is null)
                return Task.FromResult(ToolResult.Fail(
                    "error: correspondence is not available in this session", "no correspondence host"));

            var result = host.Introduce(first!, second!, purpose, text!);
            return Task.FromResult(result.Outcome switch
            {
                IntroduceOutcome.Introduced => ToolResult.Text(result.Message),
                IntroduceOutcome.InvalidArgument => ToolResult.Fail(result.Message),
                IntroduceOutcome.UnknownAddressee => ToolResult.Fail(result.Message, "unknown addressee"),
                IntroduceOutcome.SameChat => ToolResult.Fail(result.Message, "same chat"),
                IntroduceOutcome.IntroducerIsParty => ToolResult.Fail(result.Message, "introducer is a party"),
                IntroduceOutcome.AlreadyLinked => ToolResult.Fail(result.Message, "already linked"),
                IntroduceOutcome.Denied => ToolResult.Refuse(result.Message, "correspondence denied"),
                IntroduceOutcome.AddresseeGone => ToolResult.Refuse(result.Message, "addressee gone"),
                _ => ToolResult.Fail(result.Message)
            });
        }
        catch (JsonException)
        {
            return Task.FromResult(ToolResult.Fail("error: invalid_json", "invalid json"));
        }
    }
}
