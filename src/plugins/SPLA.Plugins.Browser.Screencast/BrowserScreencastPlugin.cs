using System.Text.Json;
using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Browser.Screencast;

public sealed class BrowserScreencastPlugin : ISplaPlugin, ISplaPluginPanelProvider
{
    public string PanelType => "browser.screencast";

    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings) => [new BrowserScreencastInfoTool()];

    public async Task<ISplaPluginPanelSession> OpenAsync(
        string panelId,
        IReadOnlyDictionary<string, string?> parameters,
        Func<SplaPluginPanelEvent, ValueTask> publish,
        CancellationToken cancellationToken)
    {
        var session = await BrowserScreencastSession.CreateAsync(publish, cancellationToken);
        if (parameters.GetValueOrDefault("url") is { Length: > 0 } url) await session.NavigateAsync(url);
        return session;
    }
}

internal sealed class BrowserScreencastInfoTool : IMcpTool
{
    public string Name => "browser_screencast_info";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Reports the purpose and experimental status of the interactive browser screencast panel.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new { type = "object", properties = new { } }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default) =>
        Task.FromResult("Experimental panel type: browser.screencast. It is a separate plugin and does not replace the browser automation plugin.");
}

internal sealed class BrowserScreencastSession : ISplaPluginPanelSession
{
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private readonly IPage _page;
    private readonly Func<SplaPluginPanelEvent, ValueTask> _publish;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly Task _frames;
    private byte[]? _lastFrame;

    private BrowserScreencastSession(IPlaywright playwright, IBrowser browser, IPage page, Func<SplaPluginPanelEvent, ValueTask> publish)
    {
        _playwright = playwright;
        _browser = browser;
        _page = page;
        _publish = publish;
        _frames = StreamFramesAsync(_lifetime.Token);
    }

    public static async Task<BrowserScreencastSession> CreateAsync(Func<SplaPluginPanelEvent, ValueTask> publish, CancellationToken ct)
    {
        var playwright = await Playwright.CreateAsync();
        IBrowser browser;
        try
        {
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true, Channel = "msedge" });
        }
        catch (PlaywrightException)
        {
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }
        var page = await browser.NewPageAsync(new BrowserNewPageOptions { ViewportSize = new ViewportSize { Width = 1280, Height = 720 } });
        return new(playwright, browser, page, publish);
    }

    public Task NavigateAsync(string url) => _page.GotoAsync(NormalizeUrl(url));

    public async Task HandleInputAsync(string inputType, JsonElement payload, CancellationToken cancellationToken)
    {
        switch (inputType)
        {
            case "navigate":
                await NavigateAsync(payload.GetProperty("url").GetString() ?? "about:blank");
                break;
            case "click":
                await _page.Mouse.ClickAsync(payload.GetProperty("x").GetSingle(), payload.GetProperty("y").GetSingle());
                break;
            case "pointerDown":
                await _page.Mouse.MoveAsync(payload.GetProperty("x").GetSingle(), payload.GetProperty("y").GetSingle());
                await _page.Mouse.DownAsync(new MouseDownOptions { Button = ResolveMouseButton(payload) });
                break;
            case "pointerMove":
                await _page.Mouse.MoveAsync(payload.GetProperty("x").GetSingle(), payload.GetProperty("y").GetSingle());
                break;
            case "pointerUp":
                await _page.Mouse.MoveAsync(payload.GetProperty("x").GetSingle(), payload.GetProperty("y").GetSingle());
                await _page.Mouse.UpAsync(new MouseUpOptions { Button = ResolveMouseButton(payload) });
                break;
            case "wheel":
                await _page.Mouse.WheelAsync(payload.GetProperty("deltaX").GetSingle(), payload.GetProperty("deltaY").GetSingle());
                break;
            case "text":
                await _page.Keyboard.InsertTextAsync(payload.GetProperty("text").GetString() ?? string.Empty);
                break;
            case "key":
                await _page.Keyboard.PressAsync(payload.GetProperty("key").GetString() ?? string.Empty);
                break;
            case "resize":
                await _page.SetViewportSizeAsync(payload.GetProperty("width").GetInt32(), payload.GetProperty("height").GetInt32());
                break;
        }
    }

    private async Task StreamFramesAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var bytes = await _page.ScreenshotAsync(new PageScreenshotOptions { Type = ScreenshotType.Jpeg, Quality = 70 });
                if (_lastFrame is null || !_lastFrame.AsSpan().SequenceEqual(bytes))
                {
                    _lastFrame = bytes;
                    await _publish(new("frame", new { mimeType = "image/jpeg", base64 = Convert.ToBase64String(bytes), url = _page.Url }));
                }
                await Task.Delay(200, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (PlaywrightException ex)
            {
                await _publish(new("error", new { message = ex.Message }));
                await Task.Delay(1000, ct);
            }
        }
    }

    private static string NormalizeUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out _) ? url : $"https://{url}";

    private static Microsoft.Playwright.MouseButton ResolveMouseButton(JsonElement payload) => payload.GetProperty("button").GetInt32() switch
    {
        1 => Microsoft.Playwright.MouseButton.Middle,
        2 => Microsoft.Playwright.MouseButton.Right,
        _ => Microsoft.Playwright.MouseButton.Left
    };

    public async ValueTask DisposeAsync()
    {
        await _lifetime.CancelAsync();
        try { await _frames; } catch (OperationCanceledException) { }
        await _browser.DisposeAsync();
        _playwright.Dispose();
        _lifetime.Dispose();
    }
}
