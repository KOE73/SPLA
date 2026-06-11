using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Test;

public class PingTool : IMcpTool
{
    public string Name => "test.sys.ping";

    public ToolDefinition GetDefinition()
    {
        return new ToolDefinition
        {
            Function = new ToolFunctionDefinition
            {
                Name = Name,
                Description = "A simple ping tool to test the plugin system.",
                Scope = ToolScope.Project,
                Effect = ToolEffect.Read,
                Risk = ToolRisk.Low,
                Parameters = new
                {
                    type = "object",
                    properties = new { }
                }
            }
        };
    }

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("pong! the test plugin is alive.");
    }
}
