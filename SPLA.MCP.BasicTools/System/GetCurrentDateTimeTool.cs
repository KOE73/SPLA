using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Globalization;

namespace SPLA.MCP.BasicTools.SystemTools;

public class GetCurrentDateTimeTool : IMcpTool
{
    public string Name => "agent.now";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Gets the current local date and time, UTC date and time, timezone, and ISO timestamps. Use this whenever the user asks about today, now, current date, current time, or relative dates.",
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new { },
                required = Array.Empty<string>()
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.Now;
        var utcNow = DateTimeOffset.UtcNow;
        var timezone = TimeZoneInfo.Local;

        var result =
            $"Local date: {now:yyyy-MM-dd}\n" +
            $"Local time: {now:HH:mm:ss}\n" +
            $"Local date/time: {now:yyyy-MM-dd HH:mm:ss zzz}\n" +
            $"Local day of week: {now.ToString("dddd", CultureInfo.InvariantCulture)}\n" +
            $"UTC date/time: {utcNow:yyyy-MM-dd HH:mm:ss 'UTC'}\n" +
            $"Time zone: {timezone.DisplayName}\n" +
            $"Time zone id: {timezone.Id}\n" +
            $"ISO local: {now:O}\n" +
            $"ISO UTC: {utcNow:O}";

        return Task.FromResult(result);
    }
}
