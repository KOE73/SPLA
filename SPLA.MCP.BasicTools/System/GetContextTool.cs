using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.SystemTools;

public class GetContextTool : IMcpTool
{
    public string Name => "sys.context";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Gets local runtime context such as current working directory, OS, machine name, and username. This does not search the internet and should not be used for web lookup.",
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new { },
                required = System.Array.Empty<string>()
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var context = $"Current Working Directory: {Directory.GetCurrentDirectory()}\n" +
                      $"OS: {System.Environment.OSVersion}\n" +
                      $"Machine Name: {System.Environment.MachineName}\n" +
                      $"User: {System.Environment.UserName}";
                      
        return Task.FromResult(context);
    }
}
