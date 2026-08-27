using System.Text.Json.Nodes;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Security;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Mcp.Client;

/// <summary>
/// One foreign tool, made to look like ours. The whole of "step 3" lives here: everything a
/// <see cref="McpServerSession"/> hands back in the protocol's own words gets turned into exactly
/// what <c>McpHost</c>'s pipeline expects, and nothing about the pipeline has to know the tool came
/// from somewhere else.
///
/// <para><b>The verdict is naive by construction, not by omission.</b> Every instance declares
/// <see cref="ToolScope.Foreign"/>, <see cref="ToolEffect.Write"/> and at least
/// <see cref="ToolRisk.High"/> — there is no attempt to read the server's description and infer
/// something narrower, because the description was written by whoever runs that server and trusting
/// it would make the safety boundary negotiable by them. See
/// <c>ADR_20260826_service_mcp-client</c> §2.</para>
/// </summary>
public sealed class McpProxyTool : IMcpTool
{
    private readonly McpServerSession _session;
    private readonly McpToolInfo _info;
    private readonly string _serverId;
    private readonly bool _serverIsNamedOrigin;

    public McpProxyTool(McpServerSession session, McpToolInfo info, string serverId, bool serverIsNamedOrigin)
    {
        _session = session;
        _info = info;
        _serverId = serverId;
        _serverIsNamedOrigin = serverIsNamedOrigin;

        Name = McpToolNaming.Prefixed(serverId, info.Name, out var refusal)
            ?? throw new ArgumentException(
                $"tool '{info.Name}' from server '{serverId}' cannot be registered: {refusal}");
    }

    public string Name { get; }

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            // The source is named in the text the model reads, not only in a field it never sees —
            // a person reading the transcript later should not have to already know this tool is
            // foreign to understand why it behaved like one.
            Description = string.IsNullOrWhiteSpace(_info.Description)
                ? $"Provided by MCP server '{_serverId}'."
                : $"{_info.Description}\n\nProvided by MCP server '{_serverId}'.",
            Parameters = _info.InputSchema ?? new JsonObject { ["type"] = "object" },
            Scope = ToolScope.Foreign,
            Effect = ToolEffect.Write,
            // destructiveHint may only ever push the risk up, never down — a server cannot talk its
            // way to a lighter verdict by simply not setting a flag. readOnlyHint is read nowhere in
            // this file, on purpose: see the type remarks.
            Risk = _info.DestructiveHint == true ? ToolRisk.Danger : ToolRisk.High,
            // A stranger's schema is not written to satisfy OpenAI's strict-mode contract (every
            // property listed in `required`, no free-form objects) — asking a provider to enforce
            // that contract on it would fail calls a lenient schema would have accepted fine.
            StrictSchema = false,
            ConversationBound = false,
            SupportsBackground = false
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        if (_session.State != McpSessionState.Ready)
            return ToolResult.Fail(
                $"MCP server '{_serverId}' is not connected" +
                (_session.LastError is null ? "." : $": {_session.LastError}"));

        JsonNode result;
        try
        {
            result = await _session.CallToolAsync(
                _info.Name, argumentsJson, ReportProgress, cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return ToolResult.Fail($"MCP server '{_serverId}' call failed: {ex.Message}");
        }

        // The pipe worked and the server itself has something to say. Observed here, not only on the
        // happy path below: a refused or failing call still tells the model what an unnamed source
        // said, which is exactly the thing the doubt flag exists to track.
        if (!_serverIsNamedOrigin)
            AgentSessionScope.Current?.Doubt.Observe(
                new DataOrigin($"mcp:{_serverId}", OperatorNamed: false), Name);

        var isError = result["isError"]?.GetValue<bool>() ?? false;
        var content = Project(result["content"] as JsonArray);

        return isError
            ? new ToolResult { Outcome = ToolOutcome.Failed, Content = content }
            : new ToolResult { Outcome = ToolOutcome.Ok, Content = content };
    }

    /// <summary>The mirror of <c>McpStdioServer.Project</c> on the serving side, read backwards: the
    /// wire's field names decoded into our content types instead of ours encoded into the wire's.
    /// <c>resource_link</c> is folded into the same <see cref="ToolResource"/> as <c>resource</c> —
    /// both are "here is a pointer, not the body" to a model reading the result.</summary>
    private static List<ToolContent> Project(JsonArray? blocks)
    {
        var content = new List<ToolContent>();
        if (blocks is null) return content;

        foreach (var block in blocks)
        {
            if (block is null) continue;

            switch (block["type"]?.GetValue<string>())
            {
                case "text":
                    content.Add(new ToolText(block["text"]?.GetValue<string>() ?? string.Empty));
                    break;

                case "image":
                    var data = block["data"]?.GetValue<string>();
                    var mimeType = block["mimeType"]?.GetValue<string>();
                    if (data is not null && mimeType is not null)
                        content.Add(new ToolImage(data, mimeType));
                    break;

                case "resource":
                    var resource = block["resource"];
                    if (resource?["uri"]?.GetValue<string>() is { } uri)
                        content.Add(new ToolResource(
                            uri,
                            resource["mimeType"]?.GetValue<string>(),
                            resource["text"]?.GetValue<string>()));
                    break;

                case "resource_link":
                    if (block["uri"]?.GetValue<string>() is { } linkUri)
                        content.Add(new ToolResource(
                            linkUri,
                            block["mimeType"]?.GetValue<string>(),
                            block["description"]?.GetValue<string>() ?? block["name"]?.GetValue<string>()));
                    break;

                // "audio" and anything a future protocol revision adds: silently skipped rather than
                // refused. A server that also sends something we cannot show yet should not lose the
                // text alongside it.
            }
        }

        if (content.Count == 0) content.Add(new ToolText(string.Empty));
        return content;
    }

    private static void ReportProgress(McpProgress progress) =>
        ProgressScope.Report(new ToolProgress
        {
            Current = (long)progress.Progress,
            Total = progress.Total.HasValue ? (long)progress.Total.Value : null,
            Message = progress.Message
        });
}
