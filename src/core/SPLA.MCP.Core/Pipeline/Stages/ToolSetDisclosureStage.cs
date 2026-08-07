using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Refuses a call to a tool whose set is not raised in this chat.
/// <para>
/// Ahead of the permission link on purpose: whether a caller may run a tool is a question worth
/// asking only about a tool the caller was shown. Asking it the other way round would also mean
/// evaluating policy — and, at <c>Ask</c>, bothering a human — over a tool that is not on the table.
/// </para>
/// <para>The refusal wording is the registry's, not this link's: it is the registry that knows the
/// difference between a set the user switched off (which must not be admitted to exist) and one that
/// is merely not raised (which is the useful thing to say).</para>
/// </summary>
public sealed class ToolSetDisclosureStage : IToolMiddleware
{
    private readonly Func<string, string?> _refusalFor;
    private readonly ILogger? _logger;

    public ToolSetDisclosureStage(Func<string, string?> refusalFor, ILogger? logger)
    {
        _refusalFor = refusalFor;
        _logger = logger;
    }

    public ToolPipelineStage Stage => ToolPipelineStage.Disclosure;

    public Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        if (_refusalFor(call.Name) is { } refusal)
        {
            _logger?.LogInformation("Tool refused: its set is not raised. Tool={ToolName}", call.Name);
            return Task.FromResult(ToolResult.Refuse(refusal, "tool set not raised"));
        }

        return next(call, ct);
    }
}
