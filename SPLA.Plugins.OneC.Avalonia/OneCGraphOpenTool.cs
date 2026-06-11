using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.OneC.Graph;
using SPLA.Plugins.OneC.Storage;
using SPLA.Plugins.OneC.Avalonia.Views.OneC;
using System.Text.Json;

namespace SPLA.Plugins.OneC.Avalonia;

public sealed class OneCGraphOpenTool(string workspacePath) : IMcpTool, IToolHelpProvider
{
    private readonly string _databasePath = Path.Combine(workspacePath, ".spla", "onec.sqlite");

    public string Name => "onec.graph.open";

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "[H] Build and open a large interactive 1C graph window. Use this when the user asks to show, draw, or open a graph of dependencies, references, or data flow. If the exact fullName is unknown, first call onec.object.find.",
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    fullName = new { type = "string", description = "Exact 1C object full name, for example Report.ГрафикПродажПоКлиентам." },
                    mode = new { type = "string", description = "Graph mode: dependencies, references, or dataflow." },
                    depth = new { type = "integer", description = "Traversal depth. Recommended 1..5.", minimum = 1 },
                    limit = new { type = "integer", description = "Max edge count. Recommended 100..1200.", minimum = 1 }
                },
                required = new[] { "fullName" }
            }
        }
    };

    public string? GetHelpText() =>
        """
        tool: onec.graph.open

        summary: Builds a 1C graph and opens it in a dedicated large window.

        use_when:
          - the user asks to show or draw dependencies
          - the user asks to open a graph in a separate window
          - the user wants references graph
          - the user wants data flow graph

        arguments:
          fullName: exact object full name.
          mode: dependencies | references | dataflow. Default: dependencies.
          depth: traversal depth. Default: 3.
          limit: edge limit. Default: 400.

        examples:
          - fullName: Report.ГрафикПродажПоКлиентам
            mode: dependencies
            depth: 3
            limit: 400
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement.TryGetProperty("fullName", out var rootElement)
            ? rootElement.GetString()
            : null;
        if (string.IsNullOrWhiteSpace(root))
        {
            return "opened: false\nreason: missing fullName";
        }

        var mode = ParseMode(doc.RootElement);
        var depth = doc.RootElement.TryGetProperty("depth", out var depthElement) && depthElement.TryGetInt32(out var parsedDepth)
            ? parsedDepth
            : 3;
        var limit = doc.RootElement.TryGetProperty("limit", out var limitElement) && limitElement.TryGetInt32(out var parsedLimit)
            ? parsedLimit
            : 400;

        using var db = new OneCIndexDatabase(_databasePath);
        db.EnsureCreated();
        var builder = new OneCGraphBuilder(db);
        var preview = builder.Build(new(root, mode, depth, limit));

        var opened = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var window = new OneCGraphWindow(_databasePath, root, mode, depth, limit);
            if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is { } owner)
            {
                window.Show(owner);
            }
            else
            {
                window.Show();
            }

            return true;
        }).GetTask();

        return $"""
            opened: {opened.ToString().ToLowerInvariant()}
            root: {root}
            mode: {mode}
            depth: {depth}
            limit: {limit}
            nodeCount: {preview.Summary.NodeCount}
            edgeCount: {preview.Summary.EdgeCount}
            truncated: {preview.Summary.Truncated.ToString().ToLowerInvariant()}
            """;
    }

    private static OneCGraphMode ParseMode(JsonElement rootElement)
    {
        var value = rootElement.TryGetProperty("mode", out var modeElement)
            ? modeElement.GetString()
            : null;

        return value?.Trim().ToLowerInvariant() switch
        {
            "references" => OneCGraphMode.References,
            "dataflow" => OneCGraphMode.DataFlow,
            "data_flow" => OneCGraphMode.DataFlow,
            _ => OneCGraphMode.Dependencies
        };
    }
}
