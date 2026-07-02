using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsDeleteTool : IMcpTool
{
    public string Name => "system_delete_file";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Deletes a file at the specified path.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Absolute or relative path to the file to delete." }
                },
                required = new[] { "path" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var path = ToolJson.GetStringTrimmed(doc.RootElement, "path");
            if (path is null) return Task.FromResult("Error: Missing 'path' parameter.");

            var ws = HostServices.Sandbox.Workspace;
            if (!ws.FileExists(path))
            {
                return Task.FromResult($"Error: File not found at {path}");
            }

            ws.DeleteFile(path);
            return Task.FromResult($"Successfully deleted file: {path}");
        }
        catch (JsonException)
        {
            return Task.FromResult("Error: Invalid JSON arguments.");
        }
        catch (Exception ex)
        {
            return Task.FromResult($"Error deleting file: {ex.Message}");
        }
    }
}
