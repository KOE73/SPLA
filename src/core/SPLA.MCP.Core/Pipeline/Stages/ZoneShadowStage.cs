using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using SPLA.Domain.Security;
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
    private readonly Func<Zone, DataOrigin?>? _originOfZone;

    /// <param name="originOfZone">How far a zone's content is believed, for the zones whose answer is
    /// not a constant — mounts, whose standing is written per folder in the manifest. Null for hosts
    /// that have no such zones.</param>
    public ZoneShadowStage(
        EdgeClassifier classifier,
        EdgeLedger ledger,
        ILogger? logger,
        Func<Zone, DataOrigin?>? originOfZone = null)
    {
        _classifier = classifier;
        _ledger = ledger;
        _logger = logger;
        _originOfZone = originOfZone;
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
            NoteDoubt(edge, call.Name);

            // Information, not Debug: the file log starts at Information, so at Debug this line
            // existed only in a debugger — which for a step whose entire product is a record is the
            // same as not writing it. The ledger is the summary; this is the trail behind it.
            _logger?.LogInformation("Zone edge (shadow). Tool={ToolName} Edge={Edge} Effect={Effect}",
                call.Name, edge.ToString(), edge.Effect);
        }

        return await next(call, ct);
    }

    /// <summary>
    /// Content leaving a source nobody vouched for raises the chat's flag. The only such source with
    /// a per-instance answer is a mount declared <c>trust: untrusted</c> — a folder other people put
    /// files into — and it travels the same wire a dirty blob or a dirty KV entry does.
    ///
    /// <para>Raised here rather than inside the workspace on purpose: the flag is a property of the
    /// call, and this is the one place that already knows what the call moves and where from. Doing
    /// it at the file seam would mean teaching a mechanism about sessions.</para>
    ///
    /// <para>Refusing nothing, like the rest of this stage. The flag only costs a re-asked question
    /// when something later goes outward.</para>
    /// </summary>
    private void NoteDoubt(ZoneEdge edge, string toolName)
    {
        if (_originOfZone is null) return;
        if (_originOfZone(edge.Source) is not { RaisesDoubt: true } origin) return;

        SPLA.Domain.Agent.AgentSessionScope.Current?.Doubt.Observe(origin, toolName);
    }
}
