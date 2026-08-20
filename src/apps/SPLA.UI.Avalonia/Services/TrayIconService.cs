using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SPLA.Domain.Project;
using SPLA.Instances;
using SPLA.Platform;

namespace SPLA.UI.Avalonia.Services;

/// <summary>
/// The machine-wide tray icon: one badge for every SPLA instance running on this box, not just the
/// one behind this window.
///
/// <para><b>Why code-only, not AXAML.</b> <see cref="TrayIcon"/> and <see cref="NativeMenu"/> are OS
/// shell objects, not a laid-out visual tree — there is no XAML equivalent of "one menu item per
/// live instance, rebuilt every time the hub pushes a change". The menu is data-driven from
/// <see cref="RegistryWatcher.Current"/>, so building it declaratively in markup would just be a
/// second copy of this loop. See <c>agents/avalonia.md</c> §6.</para>
///
/// <para>Exists only when <see cref="App.HubUrl"/> is non-null: with no hub there is nothing to show
/// beyond this one project, which the window already does.</para>
/// </summary>
public sealed class TrayIconService : IAsyncDisposable
{
    // Blink cadence for the "somebody is waiting" badge. Slow enough not to be annoying, fast enough
    // to actually catch a glance at the tray.
    private static readonly TimeSpan BlinkInterval = TimeSpan.FromMilliseconds(700);

    private readonly string _hubUrl;
    private readonly RegistryWatcher _watcher;
    private readonly TrayIcon _tray;
    private readonly WindowIcon? _normalIcon;
    private readonly DispatcherTimer _blinkTimer;
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(5) };

    private bool _blinkOn;
    private bool _isBlinking;

    private TrayIconService(string hubUrl)
    {
        _hubUrl = hubUrl.TrimEnd('/');
        _normalIcon = LoadIcon();

        _tray = new TrayIcon { Icon = _normalIcon, IsVisible = true };
        _blinkTimer = new DispatcherTimer { Interval = BlinkInterval };
        _blinkTimer.Tick += (_, _) => Blink();

        _watcher = new RegistryWatcher(_hubUrl);
        _watcher.Changed += list => Dispatcher.UIThread.Post(() => Render(list));
        _watcher.Connected += up => Dispatcher.UIThread.Post(() => { if (!up) Render([]); });
    }

    /// <summary>Starts watching the hub and shows the tray icon. Returns null (and shows nothing)
    /// when there is no hub — callers must not throw on a machine running solo.</summary>
    public static TrayIconService? StartIfHubAvailable(string? hubUrl)
    {
        if (string.IsNullOrWhiteSpace(hubUrl)) return null;

        var service = new TrayIconService(hubUrl);
        service._watcher.Start();
        return service;
    }

    private static WindowIcon? LoadIcon()
    {
        try
        {
            var uri = new Uri("avares://SPLA.UI.Avalonia/Assets/spla.ico");
            using var stream = AssetLoader.Open(uri);
            return new WindowIcon(stream);
        }
        catch { return null; }
    }

    private void Render(System.Collections.Generic.IReadOnlyList<InstanceRecord> instances)
    {
        var menu = new NativeMenu();

        if (instances.Count == 0)
        {
            menu.Items.Add(new NativeMenuItem("No instances running") { IsEnabled = false });
        }
        else
        {
            foreach (var record in instances.OrderBy(r => r.ProjectName ?? r.ProjectId))
                menu.Items.Add(BuildInstanceItem(record));
        }

        _tray.Menu = menu;
        _tray.ToolTipText = Summarize(instances);

        var waiting = instances.Any(r => r.State == InstanceState.Waiting);
        SetBlinking(waiting);
    }

    private NativeMenuItem BuildInstanceItem(InstanceRecord record)
    {
        var name = record.ProjectName ?? Path.GetFileNameWithoutExtension(record.ProjectId);
        var state = record.IsServing ? InstanceStates.Name(record.State) : "not serving";
        var clients = record.Clients?.ToString() ?? "-";

        var item = new NativeMenuItem($"{name} — {state} ({clients} clients)");
        var submenu = new NativeMenu();

        var open = new NativeMenuItem("Open");
        open.Click += (_, _) => LaunchProject(record.ProjectId);
        submenu.Items.Add(open);

        // Unload only makes sense for something actually serving — a REPL/mcp holder has no wire to
        // ask, and the hub's stop route needs an instance id to relay to.
        if (record.IsServing)
        {
            var unload = new NativeMenuItem("Unload");
            unload.Click += (_, _) => _ = UnloadAsync(record.Info.InstanceId);
            submenu.Items.Add(unload);
        }

        item.Menu = submenu;
        return item;
    }

    private static string Summarize(System.Collections.Generic.IReadOnlyList<InstanceRecord> instances)
    {
        if (instances.Count == 0) return "SPLA — no instances running";
        var waiting = instances.Count(r => r.State == InstanceState.Waiting);
        return waiting > 0
            ? $"SPLA — {instances.Count} running, {waiting} waiting"
            : $"SPLA — {instances.Count} running";
    }

    /// <summary>Opens a project the same way the project menu does: a fresh window process pointed at
    /// its manifest. Uses the swallowing overload because the tray has nowhere to put an error message
    /// without stealing focus — which is a gap, not a design: "clicked Open and nothing happened" is
    /// exactly what it looks like from outside. Reporting it belongs with the same work that gives the
    /// tray somewhere to speak.</summary>
    private static void LaunchProject(string manifestPath)
        => SelfInvocationLauncher.TryLaunch("SPLA.UI.Avalonia.exe", manifestPath);

    /// <summary>Asks the hub to relay a stop. A refusal is normal — the instance may be mid-turn —
    /// and is not surfaced as an error, only silently dropped: the tray has no place to put a message
    /// box without stealing focus, and the person can just try again once it settles.</summary>
    private async Task UnloadAsync(string instanceId)
    {
        try
        {
            var url = $"{_hubUrl}{RegistryRoutes.Stop}?instance={Uri.EscapeDataString(instanceId)}";
            await _http.PostAsync(url, content: null);
        }
        catch { /* hub down or instance already gone — nothing actionable */ }
    }

    private void SetBlinking(bool shouldBlink)
    {
        if (shouldBlink == _isBlinking) return;
        _isBlinking = shouldBlink;

        if (shouldBlink)
        {
            _blinkOn = true;
            _blinkTimer.Start();
        }
        else
        {
            _blinkTimer.Stop();
            _blinkOn = false;
            _tray.Icon = _normalIcon;
        }
    }

    private void Blink()
    {
        _blinkOn = !_blinkOn;
        // Blinking = toggling between the icon and nothing; a null tray icon is a legal, briefly
        // blank frame on every platform Avalonia targets here.
        _tray.Icon = _blinkOn ? _normalIcon : null;
    }

    public async ValueTask DisposeAsync()
    {
        _blinkTimer.Stop();
        _tray.IsVisible = false;
        _tray.Dispose();
        _http.Dispose();
        await _watcher.DisposeAsync();
    }
}
