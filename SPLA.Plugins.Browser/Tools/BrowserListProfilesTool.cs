using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

/// <summary>Read-only preview of what browser_start would offer when asking about a profile: the
/// project-local persistent profile and any real Edge/Chrome profiles found on this machine.</summary>
public sealed class BrowserListProfilesTool : IMcpTool
{
    private readonly string _workspacePath;

    public BrowserListProfilesTool(string workspacePath) => _workspacePath = workspacePath;

    public string Name => "browser_list_profiles";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Lists the browser profile options browser_start can use: the project-local persistent " +
                          "profile, and any real Edge/Chrome profiles found on this machine (by display name/account " +
                          "— pass one of those as browser_start's 'profile' to use it, or just call browser_start " +
                          "without 'profile' to be asked).",
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Medium, // reveals locally-saved account display names/emails
            Parameters = new { type = "object", properties = new { } }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();

        var hasState = BrowserProfilePaths.ProjectProfileHasState(_workspacePath);
        sb.AppendLine($"\"project\" — {BrowserProfilePaths.ProjectProfileDir(_workspacePath)}" +
                      (hasState ? " (has saved state from earlier runs)" : " (not created yet)"));

        var detected = BrowserProfileDiscovery.Discover();
        if (detected.Count == 0)
        {
            sb.AppendLine("No real Edge/Chrome profiles detected on this machine.");
        }
        else
        {
            foreach (var p in detected)
            {
                var label = p.Account != null ? $"{p.DisplayName} ({p.Account})" : p.DisplayName;
                sb.AppendLine($"\"{label}\" — {p.Channel} profile \"{p.ProfileDirectory}\". " +
                              "Using it gives the agent access to its saved logins/cookies/history.");
            }
        }

        sb.AppendLine("\"new\" — fresh, temporary profile, no saved state.");
        return Task.FromResult(sb.ToString().TrimEnd());
    }
}
