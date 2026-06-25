using SPLA.Domain.Agent;
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

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var path = ToolJson.GetStringTrimmed(doc.RootElement, "path");
            if (path is null) return "Error: Missing 'path' parameter.";

            if (!File.Exists(path))
            {
                return $"Error: File not found at {path}";
            }

            var startLine = Math.Max(1, ToolJson.GetInt32(doc.RootElement, "start_line", 1));
            var lineCount = ToolJson.GetInt32(doc.RootElement, "line_count") is { } c ? (int?)Math.Max(1, c) : null;

            var lines = await File.ReadAllLinesAsync(path, cancellationToken);
            if (lines.Length == 0)
            {
                return "[File is empty]";
            }

            var sb = new StringBuilder();
            var endLine = lineCount.HasValue ? Math.Min(lines.Length, startLine + lineCount.Value - 1) : lines.Length;

            if (startLine > lines.Length)
            {
                return $"Error: start_line ({startLine}) is beyond file length ({lines.Length}).";
            }

            for (var i = startLine - 1; i < endLine; i++)
            {
                sb.AppendLine($"{i + 1}: {lines[i]}");
            }

            var content = sb.ToString();
            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
            if (target == OutputTarget.Context) return content;
            var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
            return DataChannel.Route(target, BlobPayload.OfText(content), $"system_read_file: {lines.Length} lines from {path}", blobName);
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }
}
