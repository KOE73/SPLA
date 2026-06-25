using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsWriteTool : IMcpTool
{
    public string Name => "system_write_file";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Overwrites or creates a file with the specified content. " +
                          "'content' may be a literal string or a blob:<handle> from a producing tool " +
                          "(e.g. sql_query output='blob'); a handle is written straight to the file without " +
                          "passing through context. Use system_patch_file for partial edits of existing large files.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Absolute or relative path to the file." },
                    content = new { type = "string", description = "The entire content to write, OR a blob:<handle> to write stored data directly." }
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
            var path    = ToolJson.GetStringTrimmed(doc.RootElement, "path");
            var content = ToolJson.GetString(doc.RootElement, "content");

            if (path is null || content is null)
                return "Error: Missing 'path' or 'content' parameter.";

            // 'content' may be a blob:<handle> produced by another tool — resolve it (data flows
            // store→file, bypassing context). A literal string resolves to itself.
            if (!DataChannel.ResolveText(content, out content, out var resolveError))
                return $"Error: {resolveError}";

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
