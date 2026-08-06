using SPLA.Domain.Models;

namespace SPLA.MCP.Core.Interfaces;

public interface IMcpTool
{
    string Name { get; }
    ToolDefinition GetDefinition();

    /// <summary>
    /// Runs the call and reports both what happened and what came of it. Returning
    /// <see cref="ToolResult"/> rather than a string is what lets a caller tell success from
    /// refusal from failure without reading the prose, and what lets a tool hand back a picture or
    /// a pointer to bulk data instead of describing it in a sentence.
    /// <para>Use <see cref="ToolResult.Fail"/> for anything the tool attempted and could not do —
    /// bad arguments included. A message that merely starts with the word "error" inside an
    /// <see cref="ToolOutcome.Ok"/> result is invisible to metrics, audit and middleware.</para>
    /// </summary>
    Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default);
}
