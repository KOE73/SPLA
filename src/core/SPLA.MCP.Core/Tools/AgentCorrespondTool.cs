using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Opens a correspondence and delivers the first (or next) message across it. See
/// <c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.3,
/// <c>docs/plans/PLAN_20260906_core_chat-directory-and-await.md</c> §2.1-2.3 and
/// <c>docs/adr/ADR_20260906_core_one-address.md</c> §2.3 — the address is a system-issued
/// instance number, never the caller-supplied <c>purpose</c> text; this ordinary tool exists at all
/// because the virtual <c>reply_&lt;role&gt;_&lt;n&gt;</c> address cannot open itself, since before
/// this call there is nothing for it to be the address of.
/// <para>
/// Its <c>role</c> argument does two jobs on purpose, and they are not the same job twice: a
/// <b>role</b> asks for a correspondent of a kind (creating his chat if there is none), a <b>public
/// name</b> asks for one particular chat that is already there. Until the second existed, addressing
/// somebody and creating him were a single inseparable act — which is why nobody could put two
/// existing chats in touch (ADR_20260906 §1).
/// </para>
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

        summary: Opens a correspondence and delivers a message across it. 'role' says WHO: either a
                 role, which gives you a new correspondent of that kind, or the public name of a chat
                 that already exists, which puts you in touch with that particular one. After this call
                 succeeds, a virtual reply tool named reply_<role>_<n> appears in THIS chat's own tool
                 list for sending further messages on the same address — it takes only a 'text'
                 parameter; there is no chat id to remember or pass anywhere. The number is
                 system-issued and belongs to the correspondent himself, so everyone talking to him
                 uses the same one — never something you compose or need to reproduce; read it off
                 the tool list each time.

        arguments:
          role:
            required: true
            description: >
                         Who to correspond with — a role name, or a chat's public name.
                         A ROLE (e.g. architect) must be one of the roles declared in the project
                         manifest; roles do not self-assign, so an undeclared one is refused. Naming a
                         role gives you a correspondent of that kind, creating his chat if you have
                         none yet.
                         A PUBLIC NAME (e.g. architect_2) reaches the chat that already answers to it,
                         and creates nothing. Public names are shown to you — do not assemble one
                         out of a role and a guessed number. A string that is neither is refused with
                         both lists.
          text:
            required: true
            description: The message to send — the whole first (or next) thing you want to say.
          purpose:
            required: false
            description: Why this correspondence is being opened, in your own words. Purely
                         explanatory — it never becomes part of the reply tool's name or address, so
                         there is nothing to spell exactly or remember later. 'topic' is accepted as an
                         older name for this field.
          another:
            required: false
            description: Set true to open an ADDITIONAL correspondence with this role even though one
                         is already open, instead of continuing the existing one. Rare — most roles
                         need only one correspondent at a time.

        returns:
          A delivery receipt on success, e.g. "delivered: correspondence with 'architect' is open ...
          Use 'reply_architect_2' to send your next message." This is a RECEIPT, NOT the correspondent's
          answer — do not treat it as content, and do not wait here for a reply before continuing your
          own work. Their answer, when it comes, arrives as an ordinary message in this conversation on
          a later turn.
          "error: ..." when role/text is missing, the role is not declared, or correspondence is not
          permitted for this chat.

        notes:
          - Calling this again for a role you already correspond with (without 'another') reuses the
            already-open correspondence and simply sends another message — it does not open a second
            one or reset anything. Naming a public name you already correspond with does the same,
            'another' or not: 'another' asks for another correspondent, and a public name is one chat.
          - If the correspondent turns out to be a spawned run that is still going, your message is
            queued and answered only once that run finishes.
          - A correspondence that goes quiet later (the other chat archived or deleted) is announced
            in this conversation, and its reply tool stops appearing.

        examples:
          - request:
              role: architect
              purpose: api design
              text: "Does this endpoint shape look right to you?"
          - request:
              role: architect_2
              purpose: continuing yesterday's review
              text: "You looked at the migration — does the new shape still hold?"
        """;

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Details = DetailsText,
            Description = "Opens a correspondence with a role (a new correspondent of that kind) or " +
                          "with an existing chat's public name like architect_2 (that very one), and " +
                          "delivers a message. Returns a delivery receipt, not their answer — " +
                          "reply_<role>_<n> appears afterwards for sending more.",
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
                        description = "Who to correspond with: a declared role (e.g. architect) for a new " +
                                      "correspondent of that kind, or an existing chat's public name " +
                                      "(e.g. architect_2) to reach that one. Read public names off what " +
                                      "is shown to you; never assemble one yourself."
                    },
                    text = new
                    {
                        type = "string",
                        description = "The message to send."
                    },
                    // Nullable-and-required rather than simply omitted from `required`: OpenAI strict
                    // mode wants every property listed there, so an optional field is spelled
                    // present-but-nullable — the same shape cwd/code_page use in RunCommandTool, and
                    // the one WithBackgroundParameter documents for the `background` flag.
                    purpose = new
                    {
                        type = new[] { "string", "null" },
                        description = "Why this correspondence is open, in your own words. Null = unstated; never part of the address."
                    },
                    another = new
                    {
                        type = new[] { "boolean", "null" },
                        description = "True opens an additional correspondence with this role instead of reusing the existing one. Null/omitted = reuse."
                    }
                },
                required = new[] { "role", "text", "purpose", "another" }
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
            var text = ToolJson.GetStringTrimmed(root, "text");
            // 'topic' is the pre-wave-0 name for the same free-text field — accepted as a synonym so
            // an older prompt/habit does not simply stop working, but never treated as an address.
            var purpose = ToolJson.GetStringTrimmed(root, "purpose") ?? ToolJson.GetStringTrimmed(root, "topic") ?? "";
            var another = ToolJson.GetBoolean(root, "another", false);

            if (string.IsNullOrEmpty(role))
                return Task.FromResult(ToolResult.Fail("error: 'role' is required", "missing role"));
            if (string.IsNullOrEmpty(text))
                return Task.FromResult(ToolResult.Fail("error: 'text' is required", "missing text"));

            var host = AgentSessionScope.Current?.Correspondence;
            if (host is null)
                return Task.FromResult(ToolResult.Fail(
                    "error: correspondence is not available in this session", "no correspondence host"));

            var result = host.Correspond(role!, purpose, text!, another);
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
