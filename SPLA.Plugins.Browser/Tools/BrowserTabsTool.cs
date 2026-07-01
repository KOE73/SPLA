using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserTabsTool : IMcpTool
{
    public string Name => "browser_tabs";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Lists open tabs: id, URL, title, and which one is active.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new { type = "object", properties = new { } }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        var tabs = mgr.Pages.All();
        if (tabs.Count == 0) return "No open tabs.";

        var sb = new StringBuilder();
        foreach (var (id, page) in tabs)
        {
            string title;
            try { title = await page.TitleAsync(); } catch { title = "(unavailable)"; }
            var mark = id == mgr.Pages.ActiveTabId ? " [active]" : "";
            sb.AppendLine($"{id}{mark} — \"{title}\" — {page.Url}");
        }
        return sb.ToString().TrimEnd();
    }
}
