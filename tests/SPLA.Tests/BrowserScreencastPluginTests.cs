using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Plugins;
using SPLA.Plugins.Browser.Screencast;

namespace SPLA.Tests;

public sealed class BrowserScreencastPluginTests
{
    [Fact]
    public void Initialize_returns_the_narrow_embedded_browser_tool_set()
    {
        var tools = new BrowserScreencastPlugin().Initialize(new ResolvedSettings()).ToList();

        Assert.Equal(
            [
                "browser_screencast_navigate",
                "browser_screencast_snapshot",
                "browser_screencast_click",
                "browser_screencast_type",
                "browser_screencast_get_text"
            ],
            tools.Select(tool => tool.Name));

        foreach (var tool in tools)
        {
            var definition = tool.GetDefinition();
            Assert.Equal(tool.Name, definition.Function.Name);
            Assert.Equal(ToolScope.Internet, definition.Function.Scope);
            using var _ = JsonDocument.Parse(JsonSerializer.Serialize(definition.Function.Parameters));
        }
    }

    [Fact]
    public void Explicit_tool_flags_are_resolved_by_full_lower_snake_case_name()
    {
        var settings = new ResolvedSettings
        {
            Plugins = new Dictionary<string, SplaPluginSection>
            {
                ["browser_screencast"] = new()
                {
                    Tools = new Dictionary<string, bool>
                    {
                        ["browser_screencast_snapshot"] = false
                    }
                }
            }
        };

        var manager = new PluginManager(settings);

        Assert.False(manager.IsToolEnabled("browser_screencast_snapshot"));
        Assert.True(manager.IsToolEnabled("browser_screencast_navigate"));
    }
}
