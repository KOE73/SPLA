using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsListTool : IMcpTool
{
    public string Name => "system_list_files";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Lists files and directories in a specified directory path.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new
                    {
                        type = "string",
                        description = "The path to list."
                    }
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
            if (ws.DirectoryExists(path))
            {
                var sb = new StringBuilder();
                var dirs = ws.GetDirectories(path);
                var files = ws.GetFiles(path);

                sb.AppendLine($"Directory contents of {path}:");
                foreach (var d in dirs) sb.AppendLine($"[DIR] {Path.GetFileName(d)}");
                foreach (var f in files) sb.AppendLine($"[FILE] {Path.GetFileName(f)}");

                return Task.FromResult(sb.ToString());
            }
            return Task.FromResult($"Error: Directory not found at {path}");
        }
        catch (JsonException)
        {
            return Task.FromResult("Error: Invalid JSON arguments.");
        }
    }
}
