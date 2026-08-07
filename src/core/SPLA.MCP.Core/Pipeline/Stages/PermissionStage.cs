using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Applies the permission verdict, and — when the verdict is <see cref="PermissionResult.Ask"/> —
/// puts the question to whoever is attached to this chat.
/// <para>
/// The verdict itself is computed elsewhere and is a pure function of mode, definition and
/// arguments; this link only carries it out. That split is what lets a head compute the same verdict
/// ahead of time, to learn whether a human will have to be asked, without running anything.
/// </para>
/// </summary>
public sealed class PermissionStage : IToolMiddleware
{
    private readonly IPermissionManager _permissions;
    private readonly ILogger? _logger;

    public PermissionStage(IPermissionManager permissions, ILogger? logger)
    {
        _permissions = permissions;
        _logger = logger;
    }

    public ToolPipelineStage Stage => ToolPipelineStage.Policy;

    public async Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        var def = call.Tool!.GetDefinition();
        var verdict = _permissions.CheckPermission(call.Mode, def.Function, call.ArgumentsJson);

        if (verdict.Result == PermissionResult.Deny)
        {
            _logger?.LogWarning(
                "Tool execution denied by policy. Tool={ToolName} Mode={Mode} Reason={Reason}",
                call.Name, call.Mode, verdict.Reason);
            return ToolResult.Refuse(
                $"Error: Execution of tool '{call.Name}' was denied by permission policy in {call.Mode} mode ({verdict.Reason}).",
                verdict.Reason);
        }

        if (verdict.Result == PermissionResult.Ask)
        {
            _logger?.LogInformation(
                "Tool requires user confirmation — awaiting permission. Tool={ToolName} Mode={Mode} Scope={Scope} Effect={Effect} Risk={Risk}",
                call.Name, call.Mode, def.Function.Scope, def.Function.Effect, def.Function.Risk);
            var pending = PermissionScope.RequestAsync(def.Function, call.ArgumentsJson);
            if (pending != null)
            {
                var decision = await pending;
                _logger?.LogInformation(
                    "Permission decision received. Tool={ToolName} Decision={Decision}", call.Name, decision);
                if (decision == PermissionDecision.Deny)
                {
                    _logger?.LogWarning(
                        "Tool execution denied by user. Tool={ToolName} Mode={Mode}", call.Name, call.Mode);
                    return ToolResult.Refuse(
                        $"Error: Execution of tool '{call.Name}' was denied by the user. " +
                        "This was a decision, not a fault — calling it again unchanged will be denied again. " +
                        "Take a different approach, or ask what to do instead.",
                        "denied by the user");
                }
            }
            else
            {
                // Nobody is attached to ask. Until now this said "no permission handler is attached",
                // which describes the host's wiring — something the model can neither act on nor fix,
                // so a headless run simply died here. What it can act on is the situation: approval is
                // needed, there is nobody to give it, and repeating the call cannot change that.
                _logger?.LogWarning(
                    "Tool requires confirmation but NO permission handler is attached — execution blocked. Tool={ToolName} Mode={Mode}",
                    call.Name, call.Mode);
                return ToolResult.Refuse(
                    $"Error: '{call.Name}' needs a person to approve it ({verdict.Reason}), and this run has nobody to ask. " +
                    "Repeating the call will not help. Either reach the goal a way that needs no approval, " +
                    "or stop and report that this step is waiting on a human decision.",
                    "no permission handler attached");
            }
        }

        return await next(call, ct);
    }
}
