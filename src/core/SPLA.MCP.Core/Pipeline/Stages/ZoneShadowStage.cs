using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Security;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Works out which movement each call is and counts it. Refuses nothing.
///
/// <para>This is the whole of the shadow step, and its emptiness is the point: the list of edges a
/// real project needs cannot be guessed, so it is collected instead. Turning enforcement on before
/// there is a week of this behind it would mean choosing the exceptions by imagination — which is
/// how policy systems end up being run permissive by everyone who has one.</para>
///
/// <para>Sits at <see cref="ToolPipelineStage.Policy"/>, beside the permission check rather than
/// inside it: they answer the same question from different ends, and when the verdict eventually
/// moves onto the edge it moves into a neighbour rather than into a stranger.</para>
/// </summary>
public sealed class ZoneShadowStage : IToolMiddleware
{
    private readonly EdgeClassifier _classifier;
    private readonly EdgeLedger _ledger;
    private readonly ILogger? _logger;

    public ZoneShadowStage(EdgeClassifier classifier, EdgeLedger ledger, ILogger? logger)
    {
        _classifier = classifier;
        _ledger = ledger;
        _logger = logger;
    }

    public ToolPipelineStage Stage => ToolPipelineStage.Policy;

    public async Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        // Only what the tool declares plus what the call carries; nothing is fetched and nothing is
        // touched, so a classification can never itself be the thing that goes wrong.
        if (call.Tool?.GetDefinition().Function is { } definition)
        {
            var edge = _classifier.Classify(definition, call.ArgumentsJson);
            _ledger.Record(edge, call.Name);

            // Information, not Debug: the file log starts at Information, so at Debug this line
            // existed only in a debugger — which for a step whose entire product is a record is the
            // same as not writing it. The ledger is the summary; this is the trail behind it.
            _logger?.LogInformation("Zone edge (shadow). Tool={ToolName} Edge={Edge} Effect={Effect}",
                call.Name, edge.ToString(), edge.Effect);
        }

        return await next(call, ct);
    }
}
