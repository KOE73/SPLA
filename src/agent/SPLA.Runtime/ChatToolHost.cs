using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;

namespace SPLA.Runtime;

/// <summary>
/// Per-chat wrapper around the runtime's shared <see cref="IToolHost"/> (in practice
/// <c>McpHost</c>). Sits between <see cref="ChatRuntime"/> and the
/// <c>ConversationOrchestrator</c> it builds, so the orchestrator's view of "what tools exist and
/// how to run them" is a seam this chat owns rather than the runtime-wide host itself.
///
/// <para><b>This wave adds nothing.</b> Every member here delegates verbatim to the inner host —
/// proven empty on purpose, the same discipline
/// <c>docs/adr/ADR_20260827-2_core_roles.md</c> asks of every other seam cut ahead of what fills
/// it. Wave 5 of
/// <c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c> is what actually uses this
/// class: it will mix this chat's virtual <c>reply_&lt;role&gt;[_&lt;topic&gt;]</c> tools into
/// <see cref="GetToolDefinitions"/> and route calls to them from <see cref="ExecuteToolAsync"/>,
/// without ever registering them in the shared <c>McpHost</c> — the whole point being that a
/// chat's tool surface must be able to differ from every other chat's, which a host shared by the
/// whole runtime cannot express.</para>
/// </summary>
public sealed class ChatToolHost(IToolHost inner) : IToolHost
{
    public IEnumerable<ToolDefinition> GetToolDefinitions() => inner.GetToolDefinitions();

    public Task<ToolResult> ExecuteToolAsync(
        AgentMode mode,
        string name,
        string argumentsJson,
        CancellationToken cancellationToken = default,
        ToolCallContext? context = null)
        => inner.ExecuteToolAsync(mode, name, argumentsJson, cancellationToken, context);
}
