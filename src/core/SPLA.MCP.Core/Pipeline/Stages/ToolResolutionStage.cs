using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Turns the requested name into a tool, or ends the call. Outermost, because every other link needs
/// the tool and none of them can say anything useful about a name that does not exist.
/// </summary>
public sealed class ToolResolutionStage : IToolMiddleware
{
    private readonly Func<string, IMcpTool?> _resolve;
    private readonly ILogger? _logger;

    public ToolResolutionStage(Func<string, IMcpTool?> resolve, ILogger? logger)
    {
        _resolve = resolve;
        _logger = logger;
    }

    public ToolPipelineStage Stage => ToolPipelineStage.Resolution;

    public Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        var tool = _resolve(call.Name);
        if (tool is null)
        {
            _logger?.LogWarning("Tool not found. Tool={ToolName} Mode={Mode}", call.Name, call.Mode);
            return Task.FromResult(ToolResult.Fail($"Error: Tool '{call.Name}' not found.", "tool not found"));
        }

        call.Tool = tool;
        return next(call, ct);
    }
}
