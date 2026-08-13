using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsReadTool : IMcpTool
{
    public string Name => "system_read_file";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Reads the content of a file, returning lines with line numbers. Supports reading specific line ranges. " +
                          "Set output='blob' to capture the full content as a blob:<handle> (bypasses context) — useful for large files or binary-adjacent formats (docx, zip) that another tool will consume.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path        = new { type = "string",                       description = "Absolute or relative path to the file." },
                    start_line  = new { type = new[] { "integer", "null" },    description = "1-indexed starting line to read (default: 1)." },
                    line_count  = new { type = new[] { "integer", "null" },    description = "Number of lines to read. Null = read to end of file." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "path", "start_line", "line_count", "output", "output_name" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var path = ToolJson.GetStringTrimmed(doc.RootElement, "path");
            if (path is null) return ToolResult.Fail("Error: Missing 'path' parameter.", "missing path");

            var ws = HostServices.Sandbox.Workspace;
            if (!ws.FileExists(path))
            {
                return ToolResult.Fail($"Error: File not found at {path}", "file not found");
            }

            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
            if (target != OutputTarget.Context)
            {
                var rawContent = await ws.ReadAllBytesAsync(path, cancellationToken);
                var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
                return ToolResult.Text(DataChannel.Route(
                    target,
                    BlobPayload.OfBytes(rawContent),
                    $"system_read_file: raw content from {path}",
                    blobName));
            }

            var startLine = Math.Max(1, ToolJson.GetInt32(doc.RootElement, "start_line", 1));
            var lineCount = ToolJson.GetInt32(doc.RootElement, "line_count") is { } c ? (int?)Math.Max(1, c) : null;

            var lines = await ws.ReadAllLinesAsync(path, cancellationToken);
            if (lines.Length == 0)
            {
                return ToolResult.Text("[File is empty]");
            }

            var sb = new StringBuilder();
            var endLine = lineCount.HasValue ? Math.Min(lines.Length, startLine + lineCount.Value - 1) : lines.Length;

            if (startLine > lines.Length)
            {
                return ToolResult.Fail($"Error: start_line ({startLine}) is beyond file length ({lines.Length}).", "start_line out of range");
            }

            for (var i = startLine - 1; i < endLine; i++)
            {
                sb.AppendLine($"{i + 1}: {lines[i]}");
            }

            return ToolResult.Text(sb.ToString());
        }
        catch (JsonException)
        {
            return ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json");
        }
        // A boundary refusal is a DECISION and must not be flattened into "error reading file" by
        // the catch below: told it was a fault, the model retries or starts repairing something it
        // was never allowed to touch. The pipeline turns this into a refusal with its reason.
        catch (PathBoundaryException) { throw; }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Error reading file: {ex.Message}", "read failed");
        }
    }
}
