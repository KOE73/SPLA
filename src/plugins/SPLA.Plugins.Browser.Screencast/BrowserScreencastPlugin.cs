using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Browser.Screencast;

public sealed class BrowserScreencastPlugin : ISplaPlugin, ISplaPluginPanelProvider
{
    private readonly BrowserScreencastRuntime _runtime = new();

    public string PanelType => "browser.screencast";

    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings) =>
    [
        new BrowserScreencastNavigateTool(_runtime),
        new BrowserScreencastSnapshotTool(_runtime),
        new BrowserScreencastClickTool(_runtime),
        new BrowserScreencastTypeTool(_runtime),
        new BrowserScreencastGetTextTool(_runtime)
    ];

    public async Task<ISplaPluginPanelSession> OpenAsync(
        string panelId,
        IReadOnlyDictionary<string, string?> parameters,
        Func<SplaPluginPanelEvent, ValueTask> publish,
        CancellationToken cancellationToken)
    {
        var session = await _runtime.AttachAsync(publish, cancellationToken);
        if (parameters.GetValueOrDefault("url") is { Length: > 0 } url)
            await _runtime.NavigateAsync(url, cancellationToken);
        return session;
    }
}

internal sealed class BrowserScreencastRuntime
{
    private const string SnapshotScript = """
        (gen) => {
          function visible(el) {
            const style = window.getComputedStyle(el);
            if (style.display === 'none' || style.visibility === 'hidden' || style.opacity === '0') return false;
            const rect = el.getBoundingClientRect();
            return rect.width > 0 && rect.height > 0;
          }
          function name(el) {
            const aria = el.getAttribute('aria-label');
            if (aria) return aria.trim();
            const labelledBy = el.getAttribute('aria-labelledby');
            if (labelledBy) {
              const text = labelledBy.split(/\s+/).map(id => document.getElementById(id)?.innerText || '').join(' ').trim();
              if (text) return text;
            }
            if (el.matches('input, textarea')) {
              const placeholder = el.getAttribute('placeholder');
              if (placeholder) return placeholder.trim();
              if (el.id) {
                const label = document.querySelector(`label[for="${CSS.escape(el.id)}"]`);
                if (label) return label.innerText.trim();
              }
            }
            return (el.getAttribute('alt') || el.getAttribute('title') || el.innerText || el.value || '').trim().slice(0, 100);
          }
          function role(el) {
            const explicit = el.getAttribute('role');
            if (explicit) return explicit;
            const tag = el.tagName.toLowerCase();
            if (tag === 'a' && el.hasAttribute('href')) return 'link';
            if (tag === 'button') return 'button';
            if (tag === 'select') return 'combobox';
            if (tag === 'textarea') return 'textbox';
            if (/^h[1-6]$/.test(tag)) return 'heading';
            if (tag === 'input') {
              const type = (el.getAttribute('type') || 'text').toLowerCase();
              if (type === 'checkbox') return 'checkbox';
              if (type === 'radio') return 'radio';
              if (type === 'submit' || type === 'button') return 'button';
              return 'textbox';
            }
            if (tag === 'nav') return 'navigation';
            if (tag === 'main') return 'main';
            if (tag === 'form') return 'form';
            return tag;
          }
          function notable(el) {
            const tag = el.tagName.toLowerCase();
            return ['a','button','input','select','textarea','nav','main','form'].includes(tag)
              || /^h[1-6]$/.test(tag) || el.hasAttribute('role') || el.hasAttribute('onclick') || el.tabIndex >= 0;
          }
          function selector(el) {
            const tag = el.tagName.toLowerCase();
            for (const attr of ['data-testid', 'name', 'aria-label', 'placeholder']) {
              const value = el.getAttribute(attr);
              if (!value) continue;
              const candidate = `${tag}[${attr}=${JSON.stringify(value)}]`;
              if (document.querySelectorAll(candidate).length === 1) return `${candidate}:visible`;
            }
            if (el.id) return `#${CSS.escape(el.id)}`;
            const href = el.getAttribute('href');
            if (href) {
              const candidate = `${tag}[href=${JSON.stringify(href)}]`;
              if (document.querySelectorAll(candidate).length === 1) return `${candidate}:visible`;
            }

            const parts = [];
            let current = el;
            while (current && current !== document.body) {
              const currentTag = current.tagName.toLowerCase();
              const siblings = current.parentElement
                ? [...current.parentElement.children].filter(sibling => sibling.tagName === current.tagName)
                : [];
              const suffix = siblings.length > 1 ? `:nth-of-type(${siblings.indexOf(current) + 1})` : '';
              parts.unshift(currentTag + suffix);
              current = current.parentElement;
            }
            return `body > ${parts.join(' > ')}`;
          }

          let counter = 0;
          const lines = [];
          const targets = {};
          function walk(el, depth) {
            if (!el || el.nodeType !== 1) return;
            const style = window.getComputedStyle(el);
            if (style.display === 'none' || style.visibility === 'hidden') return;
            let nextDepth = depth;
            if (notable(el) && visible(el)) {
              const ref = `s${gen}.${++counter}`;
              el.setAttribute('data-spla-screencast-ref', ref);
              targets[ref] = selector(el);
              lines.push(`${'  '.repeat(depth)}- ${role(el)} "${name(el)}" [ref=${ref}]`);
              nextDepth++;
            }
            for (const child of el.children) walk(child, nextDepth);
          }
          walk(document.body, 0);
          return JSON.stringify({ tree: lines.join('\n') || '(no interactive elements found)', targets });
        }
        """;

    private readonly ConcurrentDictionary<Guid, Func<SplaPluginPanelEvent, ValueTask>> _viewers = new();
    private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
    private readonly ConcurrentDictionary<string, string> _selectors = new();
    private readonly object _stateLock = new();
    private TaskCompletionSource<IPage> _pageReady = NewPageReady();
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private CancellationTokenSource? _frameLifetime;
    private Task? _frames;
    private byte[]? _lastFrame;
    private int _refGeneration;

    public async Task<ISplaPluginPanelSession> AttachAsync(
        Func<SplaPluginPanelEvent, ValueTask> publish,
        CancellationToken cancellationToken)
    {
        var viewerId = Guid.NewGuid();
        _viewers[viewerId] = publish;
        try
        {
            await EnsureBrowserAsync(cancellationToken);
            if (_lastFrame is { } frame && _page is { } page)
                await publish(new("frame", new
                {
                    mimeType = "image/jpeg",
                    base64 = Convert.ToBase64String(frame),
                    url = page.Url
                }));
            return new BrowserScreencastViewerSession(this, viewerId);
        }
        catch
        {
            _viewers.TryRemove(viewerId, out _);
            throw;
        }
    }

    public async Task NavigateAsync(string url, CancellationToken cancellationToken)
    {
        var page = await RequirePageAsync(cancellationToken);
        await page.GotoAsync(NormalizeUrl(url), new PageGotoOptions
        {
            WaitUntil = WaitUntilState.Load,
            Timeout = 30_000
        });
        Interlocked.Exchange(ref _refGeneration, 0);
        _selectors.Clear();
    }

    public async Task<string> SnapshotAsync(int maxChars, CancellationToken cancellationToken)
    {
        var page = await RequirePageAsync(cancellationToken);
        var generation = Interlocked.Increment(ref _refGeneration);
        var snapshotJson = await page.EvaluateAsync<string>(SnapshotScript, generation);
        using var snapshot = JsonDocument.Parse(snapshotJson);
        var tree = snapshot.RootElement.GetProperty("tree").GetString() ?? "(no interactive elements found)";
        _selectors.Clear();
        foreach (var target in snapshot.RootElement.GetProperty("targets").EnumerateObject())
            _selectors[target.Name] = target.Value.GetString()!;
        if (tree.Length > maxChars) tree = $"{tree[..maxChars]}\n...(truncated at {maxChars} of {tree.Length} chars)";
        return $"Snapshot (gen {generation}) of \"{await page.TitleAsync()}\" - {page.Url}\n{tree}";
    }

    public async Task ClickAsync(string? reference, string? selector, CancellationToken cancellationToken)
    {
        var page = await RequirePageAsync(cancellationToken);
        var locator = ResolveTarget(page, reference, selector);
        await locator.ClickAsync(new LocatorClickOptions { Timeout = 10_000 });
    }

    public async Task TypeAsync(
        string? reference,
        string? selector,
        string value,
        int delayMilliseconds,
        bool pressEnter,
        CancellationToken cancellationToken)
    {
        var page = await RequirePageAsync(cancellationToken);
        var locator = ResolveTarget(page, reference, selector);
        await locator.ClickAsync(new LocatorClickOptions { Timeout = 10_000 });
        await locator.FillAsync(string.Empty, new LocatorFillOptions { Timeout = 10_000 });
        await locator.PressSequentiallyAsync(value, new LocatorPressSequentiallyOptions { Delay = delayMilliseconds });
        if (pressEnter) await locator.PressAsync("Enter");
    }

    public async Task<string> GetTextAsync(
        string? reference,
        string? selector,
        int maxChars,
        CancellationToken cancellationToken)
    {
        var page = await RequirePageAsync(cancellationToken);
        var locator = !string.IsNullOrWhiteSpace(reference) || !string.IsNullOrWhiteSpace(selector)
            ? ResolveTarget(page, reference, selector)
            : page.Locator("body");
        var text = await locator.InnerTextAsync(new LocatorInnerTextOptions { Timeout = 10_000 });
        return text.Length <= maxChars ? text : $"{text[..maxChars]}\n...(truncated at {maxChars} of {text.Length} chars)";
    }

    public async Task HandleInputAsync(string inputType, JsonElement payload, CancellationToken cancellationToken)
    {
        var page = await RequirePageAsync(cancellationToken);
        switch (inputType)
        {
            case "navigate":
                await NavigateAsync(payload.GetProperty("url").GetString() ?? "about:blank", cancellationToken);
                break;
            case "click":
                await page.Mouse.ClickAsync(payload.GetProperty("x").GetSingle(), payload.GetProperty("y").GetSingle());
                break;
            case "pointerDown":
                await page.Mouse.MoveAsync(payload.GetProperty("x").GetSingle(), payload.GetProperty("y").GetSingle());
                await page.Mouse.DownAsync(new MouseDownOptions { Button = ResolveMouseButton(payload) });
                break;
            case "pointerMove":
                await page.Mouse.MoveAsync(payload.GetProperty("x").GetSingle(), payload.GetProperty("y").GetSingle());
                break;
            case "pointerUp":
                await page.Mouse.MoveAsync(payload.GetProperty("x").GetSingle(), payload.GetProperty("y").GetSingle());
                await page.Mouse.UpAsync(new MouseUpOptions { Button = ResolveMouseButton(payload) });
                break;
            case "wheel":
                await page.Mouse.WheelAsync(payload.GetProperty("deltaX").GetSingle(), payload.GetProperty("deltaY").GetSingle());
                break;
            case "text":
                await page.Keyboard.InsertTextAsync(payload.GetProperty("text").GetString() ?? string.Empty);
                break;
            case "key":
                await page.Keyboard.PressAsync(payload.GetProperty("key").GetString() ?? string.Empty);
                break;
            case "resize":
                await page.SetViewportSizeAsync(payload.GetProperty("width").GetInt32(), payload.GetProperty("height").GetInt32());
                break;
        }
    }

    public async Task DetachAsync(Guid viewerId)
    {
        _viewers.TryRemove(viewerId, out _);
        if (!_viewers.IsEmpty) return;

        await _lifecycleGate.WaitAsync();
        try
        {
            if (!_viewers.IsEmpty) return;
            if (_frameLifetime is not null) await _frameLifetime.CancelAsync();
            if (_frames is not null)
            {
                try { await _frames; }
                catch (OperationCanceledException) { }
            }
            if (_browser is not null) await _browser.DisposeAsync();
            _playwright?.Dispose();
            _frameLifetime?.Dispose();

            _playwright = null;
            _browser = null;
            _page = null;
            _frameLifetime = null;
            _frames = null;
            _lastFrame = null;
            _refGeneration = 0;
            _selectors.Clear();
            lock (_stateLock) _pageReady = NewPageReady();
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    private async Task EnsureBrowserAsync(CancellationToken cancellationToken)
    {
        await _lifecycleGate.WaitAsync(cancellationToken);
        try
        {
            if (_page is not null) return;
            _playwright = await Playwright.CreateAsync();
            try
            {
                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true, Channel = "msedge" });
            }
            catch (PlaywrightException)
            {
                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            }

            _page = await _browser.NewPageAsync(new BrowserNewPageOptions
            {
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
            });
            lock (_stateLock) _pageReady.TrySetResult(_page);
            _frameLifetime = new CancellationTokenSource();
            _frames = StreamFramesAsync(_frameLifetime.Token);
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    private async Task<IPage> RequirePageAsync(CancellationToken cancellationToken)
    {
        Task<IPage> ready;
        lock (_stateLock) ready = _pageReady.Task;
        var completed = await Task.WhenAny(ready, Task.Delay(TimeSpan.FromSeconds(15), cancellationToken));
        if (completed != ready)
            throw new InvalidOperationException("The Browser Lab panel is not open. Open it in the Web UI and retry.");
        return await ready;
    }

    private ILocator ResolveTarget(IPage page, string? reference, string? selector)
    {
        if (!string.IsNullOrWhiteSpace(reference))
        {
            var dot = reference.IndexOf('.');
            if (dot <= 1 || reference[0] != 's' || !int.TryParse(reference.AsSpan(1, dot - 1), out var generation))
                throw new ArgumentException($"Invalid ref '{reference}'. Use a ref from browser_screencast_snapshot.");
            if (generation != Volatile.Read(ref _refGeneration))
                throw new ArgumentException($"Ref '{reference}' is stale. Take a new browser_screencast_snapshot.");
            if (!_selectors.TryGetValue(reference, out var resolvedSelector))
                throw new ArgumentException($"Ref '{reference}' is unknown. Take a new browser_screencast_snapshot.");
            return page.Locator(resolvedSelector);
        }

        if (!string.IsNullOrWhiteSpace(selector)) return page.Locator(selector);
        throw new ArgumentException("Either 'ref' or 'selector' is required.");
    }

    private async Task StreamFramesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var page = _page;
                if (page is null) break;
                var bytes = await page.ScreenshotAsync(new PageScreenshotOptions { Type = ScreenshotType.Jpeg, Quality = 70 });
                if (_lastFrame is null || !_lastFrame.AsSpan().SequenceEqual(bytes))
                {
                    _lastFrame = bytes;
                    await PublishAsync(new("frame", new
                    {
                        mimeType = "image/jpeg",
                        base64 = Convert.ToBase64String(bytes),
                        url = page.Url
                    }));
                }
                await Task.Delay(200, cancellationToken);
            }
            catch (OperationCanceledException) { break; }
            catch (PlaywrightException exception)
            {
                await PublishAsync(new("error", new { message = exception.Message }));
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private async Task PublishAsync(SplaPluginPanelEvent panelEvent)
    {
        foreach (var publish in _viewers.Values)
        {
            try { await publish(panelEvent); }
            catch { /* The connection-owned panel manager removes disconnected viewers. */ }
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

    private static TaskCompletionSource<IPage> NewPageReady() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

internal sealed class BrowserScreencastViewerSession(BrowserScreencastRuntime runtime, Guid viewerId) : ISplaPluginPanelSession
{
    public Task HandleInputAsync(string inputType, JsonElement payload, CancellationToken cancellationToken) =>
        runtime.HandleInputAsync(inputType, payload, cancellationToken);

    public ValueTask DisposeAsync() => new(runtime.DetachAsync(viewerId));
}

internal abstract class BrowserScreencastTool(BrowserScreencastRuntime runtime) : IMcpTool
{
    protected BrowserScreencastRuntime Runtime { get; } = runtime;
    public abstract string Name { get; }
    public abstract ToolDefinition GetDefinition();
    public abstract Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default);

    protected static string? String(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()?.Trim()
            : null;

    protected static string? RawString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    protected static async Task<string> RunAsync(Func<Task<string>> action)
    {
        try { return await action(); }
        catch (JsonException) { return "Error: invalid JSON arguments."; }
        catch (OperationCanceledException) { return "Error: browser operation was cancelled."; }
        catch (Exception exception) when (exception is PlaywrightException or TimeoutException or InvalidOperationException or ArgumentException)
        {
            return $"Error: {exception.Message}";
        }
    }
}

internal sealed class BrowserScreencastNavigateTool(BrowserScreencastRuntime runtime) : BrowserScreencastTool(runtime)
{
    public override string Name => "browser_screencast_navigate";

    public override ToolDefinition GetDefinition() => Definition(Name, "Opens a URL in the embedded Browser Lab page.", ToolEffect.Write, new
    {
        type = "object",
        properties = new { url = new { type = "string", description = "URL to open." } },
        required = new[] { "url" }
    });

    public override Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default) => RunAsync(async () =>
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
        var url = String(document.RootElement, "url") ?? throw new ArgumentException("'url' is required.");
        await Runtime.NavigateAsync(url, cancellationToken);
        return $"Navigated the embedded browser to {url}.";
    });

    internal static ToolDefinition Definition(string name, string description, ToolEffect effect, object parameters) => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = name,
            Description = description,
            Scope = ToolScope.Internet,
            Effect = effect,
            Risk = effect == ToolEffect.Read ? ToolRisk.Low : ToolRisk.Medium,
            Parameters = parameters
        }
    };
}

internal sealed class BrowserScreencastSnapshotTool(BrowserScreencastRuntime runtime) : BrowserScreencastTool(runtime)
{
    public override string Name => "browser_screencast_snapshot";

    public override ToolDefinition GetDefinition() => BrowserScreencastNavigateTool.Definition(
        Name,
        "Returns a compact tree of visible page controls with refs for the embedded Browser Lab page.",
        ToolEffect.Read,
        new { type = "object", properties = new { max_chars = new { type = "integer", description = "Maximum output length. Default: 8000." } } });

    public override Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default) => RunAsync(async () =>
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
        var maxChars = document.RootElement.TryGetProperty("max_chars", out var value) && value.TryGetInt32(out var parsed)
            ? Math.Clamp(parsed, 500, 30_000)
            : 8000;
        return await Runtime.SnapshotAsync(maxChars, cancellationToken);
    });
}

internal sealed class BrowserScreencastClickTool(BrowserScreencastRuntime runtime) : BrowserScreencastTool(runtime)
{
    public override string Name => "browser_screencast_click";

    public override ToolDefinition GetDefinition() => BrowserScreencastNavigateTool.Definition(
        Name,
        "Clicks one element in the embedded Browser Lab page by snapshot ref or CSS selector.",
        ToolEffect.Write,
        TargetParameters());

    public override Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default) => RunAsync(async () =>
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
        var reference = String(document.RootElement, "ref");
        var selector = String(document.RootElement, "selector");
        await Runtime.ClickAsync(reference, selector, cancellationToken);
        return $"Clicked {reference ?? selector}.";
    });

    internal static object TargetParameters() => new
    {
        type = "object",
        properties = new
        {
            @ref = new { type = "string", description = "Element ref from browser_screencast_snapshot." },
            selector = new { type = "string", description = "CSS selector when no ref is available." }
        }
    };
}

internal sealed class BrowserScreencastTypeTool(BrowserScreencastRuntime runtime) : BrowserScreencastTool(runtime)
{
    public override string Name => "browser_screencast_type";

    public override ToolDefinition GetDefinition() => BrowserScreencastNavigateTool.Definition(
        Name,
        "Clears a field and types text into it character by character in the embedded Browser Lab page.",
        ToolEffect.Write,
        new
        {
            type = "object",
            properties = new
            {
                @ref = new { type = "string", description = "Element ref from browser_screencast_snapshot." },
                selector = new { type = "string", description = "CSS selector when no ref is available." },
                value = new { type = "string", description = "Text to type." },
                delay_ms = new { type = "integer", description = "Delay per character. Default: 120 ms." },
                press_enter = new { type = "boolean", description = "Press Enter after typing. Default: false." }
            },
            required = new[] { "value" }
        });

    public override Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default) => RunAsync(async () =>
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
        var root = document.RootElement;
        var value = RawString(root, "value") ?? throw new ArgumentException("'value' is required.");
        var delay = root.TryGetProperty("delay_ms", out var delayValue) && delayValue.TryGetInt32(out var parsed)
            ? Math.Clamp(parsed, 20, 1000)
            : 120;
        var pressEnter = root.TryGetProperty("press_enter", out var enterValue) && enterValue.ValueKind == JsonValueKind.True;
        await Runtime.TypeAsync(String(root, "ref"), String(root, "selector"), value, delay, pressEnter, cancellationToken);
        return $"Typed {value.Length} characters{(pressEnter ? " and pressed Enter" : string.Empty)}.";
    });
}

internal sealed class BrowserScreencastGetTextTool(BrowserScreencastRuntime runtime) : BrowserScreencastTool(runtime)
{
    public override string Name => "browser_screencast_get_text";

    public override ToolDefinition GetDefinition() => BrowserScreencastNavigateTool.Definition(
        Name,
        "Returns visible text from the embedded Browser Lab page, a snapshot ref, or a CSS selector.",
        ToolEffect.Read,
        new
        {
            type = "object",
            properties = new
            {
                @ref = new { type = "string", description = "Element ref from browser_screencast_snapshot." },
                selector = new { type = "string", description = "CSS selector. Omit with ref to read the whole page." },
                max_chars = new { type = "integer", description = "Maximum output length. Default: 5000." }
            }
        });

    public override Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default) => RunAsync(async () =>
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
        var root = document.RootElement;
        var maxChars = root.TryGetProperty("max_chars", out var value) && value.TryGetInt32(out var parsed)
            ? Math.Clamp(parsed, 200, 30_000)
            : 5000;
        return await Runtime.GetTextAsync(String(root, "ref"), String(root, "selector"), maxChars, cancellationToken);
    });
}
