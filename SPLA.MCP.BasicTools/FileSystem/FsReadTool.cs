using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsReadTool : IMcpTool
{
    public string Name => "system.fs.read";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Reads the content of a file, returning lines with line numbers. Supports reading specific line ranges.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Absolute or relative path to the file." },
                    startLine = new { type = "integer", description = "1-indexed starting line to read (default: 1)." },
                    lineCount = new { type = "integer", description = "Number of lines to read. If omitted, reads to the end of the file." }
                },
                required = new[] { "path" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("path", out var pathElement))
            {
                return "Error: Missing 'path' parameter.";
            }

            var path = pathElement.GetString();
            if (string.IsNullOrEmpty(path)) return "Error: path is empty.";

            if (!File.Exists(path))
            {
                return $"Error: File not found at {path}";
            }

            var startLine = doc.RootElement.TryGetProperty("startLine", out var startEl) && startEl.TryGetInt32(out var s)
                ? Math.Max(1, s)
                : 1;

            var lineCount = doc.RootElement.TryGetProperty("lineCount", out var countEl) && countEl.TryGetInt32(out var c)
                ? (int?)Math.Max(1, c)
                : null;

            var lines = await File.ReadAllLinesAsync(path, cancellationToken);
            if (lines.Length == 0)
            {
                return "[File is empty]";
            }

            var sb = new StringBuilder();
            var endLine = lineCount.HasValue ? Math.Min(lines.Length, startLine + lineCount.Value - 1) : lines.Length;

            if (startLine > lines.Length)
            {
                return $"Error: startLine ({startLine}) is beyond file length ({lines.Length}).";
            }

            for (var i = startLine - 1; i < endLine; i++)
            {
                sb.AppendLine($"{i + 1}: {lines[i]}");
            }

            return sb.ToString();
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
