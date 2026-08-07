using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Plugins;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Refuses a call into a plugin the project has switched off. Live gating: the assemblies stay
/// loaded, only exposure is gated, so the answer can differ between two calls a second apart.
/// </summary>
public sealed class PluginAvailabilityStage : IToolMiddleware
{
    private readonly PluginManager? _plugins;
    private readonly ILogger? _logger;

    public PluginAvailabilityStage(PluginManager? plugins, ILogger? logger)
    {
        _plugins = plugins;
        _logger = logger;
    }

    public ToolPipelineStage Stage => ToolPipelineStage.Availability;

    public Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        if (_plugins != null && !_plugins.IsToolAvailable(call.Tool!))
        {
            _logger?.LogWarning("Tool refused: owning plugin is disabled. Tool={ToolName}", call.Name);
            return Task.FromResult(ToolResult.Refuse(
                $"Error: tool '{call.Name}' belongs to a plugin that is currently disabled.",
                "plugin disabled"));
        }

        return next(call, ct);
    }
}
