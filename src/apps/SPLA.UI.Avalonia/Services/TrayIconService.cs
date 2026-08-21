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

        // One row per PROJECT, not per participant. Windows register too now, so a project you have
        // open would otherwise appear twice — the thing being managed is the project, and its agent
        // and windows are how it is currently manifested.
        var projects = instances
            .GroupBy(r => r.ProjectId, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.First().ProjectName ?? g.Key)
            .ToList();

        if (projects.Count == 0)
        {
            menu.Items.Add(new NativeMenuItem("No projects running") { IsEnabled = false });
        }
        else
        {
            foreach (var project in projects) menu.Items.Add(BuildProjectItem(project.ToList()));
        }

        // The manager, in a frame and in a browser. Both because they are genuinely different tools:
        // the window is at hand, and the browser survives this shell being restarted — and a hub bound
        // beyond loopback can be opened from another machine entirely, where no tray exists at all.
        menu.Items.Add(new NativeMenuItemSeparator());

        var manage = new NativeMenuItem("Manage projects…");
        manage.Click += (_, _) => new SurfaceWindow("hub", "Projects", baseUrl: _hubUrl).Show();
        menu.Items.Add(manage);

        var inBrowser = new NativeMenuItem("Manage in browser");
        inBrowser.Click += (_, _) => BrowserLauncher.Open($"{_hubUrl}/?surface=hub");
        menu.Items.Add(inBrowser);

        _tray.Menu = menu;
        _tray.ToolTipText = Summarize(instances);

        var waiting = instances.Any(r => r.State == InstanceState.Waiting);
        SetBlinking(waiting);
    }

    private NativeMenuItem BuildProjectItem(IReadOnlyList<InstanceRecord> participants)
    {
        var agent = participants.FirstOrDefault(r => r.Role == ParticipantRoles.Agent);
        var windows = participants.Count(r => r.Role == ParticipantRoles.Window);
        var first = participants[0];

        var name = first.ProjectName ?? Path.GetFileNameWithoutExtension(first.ProjectId);
        var state = agent is null ? "no agent"
            : agent.IsServing ? InstanceStates.Name(agent.State)
            : "not serving";
        var windowNote = windows switch { 0 => "no window", 1 => "1 window", _ => $"{windows} windows" };

        var item = new NativeMenuItem($"{name} — {state}, {windowNote}");
        var submenu = new NativeMenu();

        var open = new NativeMenuItem("Open");
        open.Click += (_, _) => _ = OpenAsync(first.ProjectId);
        submenu.Items.Add(open);

        submenu.Items.Add(new NativeMenuItemSeparator());

        // Close asks; the agent refuses mid-turn and windows go regardless. Kill is the same request
        // with the refusal waived — a separate item rather than a modifier, because losing a turn in
        // progress should take a deliberate second choice and never a mis-click on the first.
        var close = new NativeMenuItem("Close");
        close.Click += (_, _) => _ = CloseProjectAsync(first.ProjectId, force: false);
        submenu.Items.Add(close);

        var kill = new NativeMenuItem("Kill (discards running work)");
        kill.Click += (_, _) => _ = CloseProjectAsync(first.ProjectId, force: true);
        submenu.Items.Add(kill);

        item.Menu = submenu;
        return item;
    }

    private static string Summarize(System.Collections.Generic.IReadOnlyList<InstanceRecord> instances)
    {
        // Counted by project for the same reason the menu is grouped by it: "3 running" must not mean
        // "one project with an agent and two windows".
        var projects = instances
            .Select(r => r.ProjectId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (projects == 0) return "SPLA — nothing running";

        var waiting = instances.Count(r => r.State == InstanceState.Waiting);
        return waiting > 0
            ? $"SPLA — {projects} project(s), {waiting} waiting"
            : $"SPLA — {projects} project(s)";
    }

    /// <summary>
    /// Opens a project: raises the window that already has it, or starts one when none does.
    ///
    /// <para>Asking the hub first is the entire point of registering windows. Before that, Open could
    /// only ever start another process, so the second click on a project you already had open gave you
    /// a duplicate — the registry knew about agents and had no idea a window existed.</para>
    ///
    /// <para>Falling through to a launch when the focus request fails is deliberate: the window may
    /// have closed between the listing and the click, and ending up with a window is what was asked
    /// for either way.</para>
    /// </summary>
    private async Task OpenAsync(string manifestPath)
    {
        if (_watcher.Current.FirstOrDefault(r =>
                r.Role == ParticipantRoles.Window &&
                string.Equals(r.ProjectId, manifestPath, StringComparison.OrdinalIgnoreCase))
            is { } window && await FocusAsync(window.Info.InstanceId))
        {
            return;
        }

        SelfInvocationLauncher.TryLaunch("SPLA.UI.Avalonia.exe", manifestPath);
    }

    /// <summary>Asks the hub to raise one window. False when it is gone or the hub could not be
    /// reached, which the caller answers by opening a new one.</summary>
    private async Task<bool> FocusAsync(string instanceId)
    {
        try
        {
            var url = $"{_hubUrl}{RegistryRoutes.Focus}?instance={Uri.EscapeDataString(instanceId)}";
            var response = await _http.PostAsync(url, content: null);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    /// <summary>
    /// Closes a whole project — its agent and every window on it, asked together.
    ///
    /// <para>This replaces the old "Unload", which never said what it was unloading and, worse, only
    /// ever addressed the agent: the windows stayed, pointed at a service that would never answer
    /// again. Addressing the project instead of one instance is what registering windows bought.</para>
    ///
    /// <para>A refusal is still normal — the agent may be mid-turn — and still goes unreported here.
    /// The tray has nowhere to say so without stealing focus; the person sees the project simply stay
    /// in the list, and Kill is right underneath. Saying it properly needs the hub window.</para>
    /// </summary>
    private async Task CloseProjectAsync(string projectId, bool force)
    {
        try
        {
            var url = $"{_hubUrl}{RegistryRoutes.StopProject}" +
                      $"?project={Uri.EscapeDataString(projectId)}&force={(force ? "true" : "false")}";
            await _http.PostAsync(url, content: null);
        }
        catch { /* hub down or project already gone — nothing actionable */ }
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
