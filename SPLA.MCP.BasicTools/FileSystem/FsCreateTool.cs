using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsCreateTool : IMcpTool
{
    public string Name => "system.fs.create";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Creates a new file with the specified content. Returns an error if the file already exists.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Absolute or relative path to create the new file." },
                    content = new { type = "string", description = "Content to write to the file." }
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

            if (File.Exists(path))
            {
                return $"Error: File already exists at {path}. Use system.fs.patch or system.fs.write to modify existing files.";
            }

            // Create directories if they don't exist
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(path, content ?? string.Empty, cancellationToken);
            return $"Successfully created new file: {path}";
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error creating file: {ex.Message}";
        }
    }
}
