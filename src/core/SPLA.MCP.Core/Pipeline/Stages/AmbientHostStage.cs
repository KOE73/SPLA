using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Publishes the host and the mode to the running tool, so that a tool can invoke other tools by
/// name — a script's <c>ctx.Run</c> — under the same mode, with permissions and progress intact.
/// <para>This is the link that makes the pipeline re-entrant in practice: what it publishes is the
/// entry point back into the pipeline itself.</para>
/// </summary>
public sealed class AmbientHostStage : IToolMiddleware
{
    private readonly IToolHost _host;

    public AmbientHostStage(IToolHost host) => _host = host;

    public ToolPipelineStage Stage => ToolPipelineStage.Ambient;

    public async Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        using var hostScope = ToolHostScope.Begin(_host, call.Mode);
        return await next(call, ct);
    }
}
