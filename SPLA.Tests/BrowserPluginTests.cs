using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.Browser;
using SPLA.Plugins.Browser.Tools;
using System.Text.Json;

namespace SPLA.Tests;

/// <summary>
/// Smoke tests that don't need a real browser process: every tool's metadata is well-formed, and
/// every tool behaves gracefully (returns a clear error string, never throws) when called outside
/// an active chat session or before browser_start. Actually driving a browser is exercised manually
/// per docs/browser-playwright-plugin-plan.md — launching Chromium/Edge in CI is out of scope here.
/// </summary>
public sealed class BrowserPluginTests
{
    private static IDisposable Scope() =>
        AgentSessionScope.Begin(new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession()));

    [Fact]
    public void Initialize_returns_unique_well_formed_tools()
    {
        var tools = new BrowserPlugin().Initialize(new ResolvedSettings()).ToList();

        Assert.NotEmpty(tools);
        Assert.Equal(tools.Count, tools.Select(t => t.Name).Distinct().Count());

        foreach (var tool in tools)
        {
            var def = tool.GetDefinition();
            Assert.Equal(tool.Name, def.Function.Name);
            Assert.False(string.IsNullOrWhiteSpace(def.Function.Description));
            // browser_list_profiles is a local filesystem read (no browser/network involved); every
            // other tool drives the browser.
            Assert.Contains(def.Function.Scope, new[] { ToolScope.Internet, ToolScope.Local });

            // Parameters must be a valid JSON schema object (round-trips through serialization).
            var json = JsonSerializer.Serialize(def.Function.Parameters);
            using var doc = JsonDocument.Parse(json);
            Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        }
    }

    [Theory]
    [InlineData("browser_stop")]
    [InlineData("browser_status")]
    [InlineData("browser_tabs")]
    [InlineData("browser_new_tab")]
    [InlineData("browser_switch_tab")]
    [InlineData("browser_close_tab")]
    [InlineData("browser_navigate")]
    [InlineData("browser_wait_navigation")]
    [InlineData("browser_snapshot")]
    [InlineData("browser_screenshot")]
    [InlineData("browser_inspect")]
    [InlineData("browser_get_text")]
    [InlineData("browser_click")]
    [InlineData("browser_scroll")]
    [InlineData("browser_fill")]
    [InlineData("browser_press")]
    [InlineData("browser_select")]
    [InlineData("browser_wait_element")]
    [InlineData("browser_upload")]
    [InlineData("browser_console")]
    [InlineData("browser_page_errors")]
    public async Task Tools_other_than_start_report_not_running_without_throwing(string toolName)
    {
        var tools = new BrowserPlugin().Initialize(new ResolvedSettings()).ToDictionary(t => t.Name);
        var tool = tools[toolName];

        using var _ = Scope();
        // Minimal valid args per tool so the "not running" check is what's actually exercised,
        // not an upstream "missing required field" short-circuit.
        var args = toolName switch
        {
            "browser_switch_tab" => """{"tab_id":"t1"}""",
            "browser_navigate" => """{"url":"https://example.com"}""",
            "browser_click" or "browser_wait_element" => """{"ref":"e1.1"}""",
            "browser_fill" => """{"ref":"e1.1","value":"x"}""",
            "browser_press" => """{"key":"Enter"}""",
            "browser_select" => """{"ref":"e1.1","values":["a"]}""",
            "browser_upload" => """{"ref":"e1.1","files":["C:/nope.txt"]}""",
            "browser_inspect" => """{"ref":"e1.1"}""",
            _ => "{}"
        };

        var result = await tool.ExecuteAsync(args);
        Assert.False(string.IsNullOrWhiteSpace(result));
        // Either "not running" or a normal validation error — never an unhandled exception (which
        // would have already failed this test via a thrown exception instead of a returned string).
    }

    [Fact]
    public async Task Browser_start_without_active_session_reports_clear_error_not_an_exception()
    {
        var tool = new BrowserStartTool(
            SPLA.Domain.Project.LocalProject.For(new SPLA.Domain.Settings.ResolvedSettings()),
            new BrowserSettings());
        var result = await tool.ExecuteAsync("{}");
        Assert.Contains("No active chat session", result);
    }

    [Fact]
    public async Task Browser_list_profiles_runs_without_a_chat_session_and_lists_the_project_option()
    {
        // Read-only — no AgentSessionScope needed, never touches Playwright.
        var tool = new BrowserListProfilesTool(SPLA.Domain.Project.LocalProject.For(
            new SPLA.Domain.Settings.ResolvedSettings
            {
                ProjectFilePath = Path.Combine(Path.GetTempPath(), "browser-test.spla"),
                WorkspacePath = Path.GetTempPath()
            }));
        var result = await tool.ExecuteAsync("{}");

        Assert.Contains("\"project\"", result);
        Assert.Contains("\"new\"", result);
    }

    [Fact]
    public async Task Image_view_without_active_session_reports_clear_error()
    {
        var tool = new ImageViewTool();
        var result = await tool.ExecuteAsync("""{"handle":"blob:abc"}""");
        Assert.Contains("no active chat session", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Image_view_missing_blob_reports_clear_error()
    {
        using var _ = Scope();
        var tool = new ImageViewTool();
        var result = await tool.ExecuteAsync("""{"handle":"blob:does-not-exist"}""");
        Assert.Contains("no blob found", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Image_view_pushes_data_url_into_pending_sink()
    {
        var session = new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());
        using var _ = AgentSessionScope.Begin(session);

        var bytes = new byte[] { 1, 2, 3, 4 };
        var handle = session.Blobs.Put(BlobPayload.OfBytes(bytes, "image/png"));

        var tool = new ImageViewTool();
        var result = await tool.ExecuteAsync($$"""{"handle":"{{handle}}"}""");

        Assert.Contains("ok:", result);
        var pending = session.Images.DrainAll();
        Assert.Single(pending);
        Assert.StartsWith("data:image/png;base64,", pending[0]);
    }
}
