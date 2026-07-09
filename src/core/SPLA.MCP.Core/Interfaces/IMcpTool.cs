using SPLA.Domain.Models;

namespace SPLA.MCP.Core.Interfaces;

public interface IMcpTool
{
    string Name { get; }
    ToolDefinition GetDefinition();
    Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default);
}
