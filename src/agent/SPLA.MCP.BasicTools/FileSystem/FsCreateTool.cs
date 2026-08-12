using SPLA.Domain.Host;
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

public class FsCreateTool : IMcpTool
{
    public string Name => "system_create_file";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Creates a new file with the specified content. Returns an error if the file already exists. " +
                          "'content' may be a literal string or a blob:<handle> from a producing tool.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Absolute or relative path to create the new file." },
                    content = new { type = "string", description = "Content to write, OR a blob:<handle> to write stored data directly." }
                },
                required = new[] { "path", "content" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var path    = ToolJson.GetStringTrimmed(doc.RootElement, "path");
            var content = ToolJson.GetString(doc.RootElement, "content");

            if (path is null || content is null)
                return ToolResult.Fail("Error: Missing 'path' or 'content' parameter.", "missing arguments");

            if (!DataChannel.ResolveText(content, out content, out var resolveError))
                return ToolResult.Fail($"Error: {resolveError}", "blob resolve failed");

            var ws = HostServices.Sandbox.Workspace;
            if (ws.FileExists(path))
            {
                return ToolResult.Refuse($"Error: File already exists at {path}. Use system_patch_file or system_write_file to modify existing files.", "file exists");
            }

            // Create directories if they don't exist
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !ws.DirectoryExists(dir))
            {
                ws.CreateDirectory(dir);
            }

            await ws.WriteAllTextAsync(path, content ?? string.Empty, cancellationToken);
            return ToolResult.Text($"Successfully created new file: {path}");
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
            return ToolResult.Fail($"Error creating file: {ex.Message}", "create failed");
        }
    }
}
