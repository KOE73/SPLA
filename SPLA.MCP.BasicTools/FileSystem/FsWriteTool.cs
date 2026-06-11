using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsWriteTool : IMcpTool
{
    public string Name => "system.fs.write";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Overwrites or creates a file with the specified content. Use system.fs.patch for partial edits of existing large files.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Absolute or relative path to the file." },
                    content = new { type = "string", description = "The entire content to write into the file." }
                },
                required = new[] { "path", "content" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("path", out var pathElement) ||
                !doc.RootElement.TryGetProperty("content", out var contentElement))
            {
                return "Error: Missing 'path' or 'content' parameter.";
            }

            var path = pathElement.GetString();
            var content = contentElement.GetString();

            if (string.IsNullOrEmpty(path)) return "Error: path is empty.";

            // Create directories if they don't exist
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(path, content ?? string.Empty, cancellationToken);
            return $"Successfully wrote content to: {path}";
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error writing file: {ex.Message}";
        }
    }
}
