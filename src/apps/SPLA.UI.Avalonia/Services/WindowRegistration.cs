using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Project;
using SPLA.Instances;

namespace SPLA.UI.Avalonia.Services;

/// <summary>
/// Announces this window to the machine hub, so the hub can address it.
///
/// <para><b>Why a window registers at all.</b> The registry used to hold only agents, and every gap
/// that produced was the same gap: nobody could raise a window that already existed, because nobody
/// knew it existed, and closing a project stopped its agent while leaving its windows pointed at a
/// service that would never answer again. A window that says "I am here, looking at this project" is
/// the whole fix — it needs no new transport, no heartbeat and no new liveness rule, because the
/// socket already answers "is it still there" exactly as it does for an agent.</para>
///
/// <para><b>It holds nothing.</b> A window is not a writer: it takes no lock, and its
/// <see cref="InstanceInfo.Endpoint"/> is the service it is *looking at*, not one it serves. Two
/// windows on one project are therefore perfectly legal, and after this they are also addressable —
/// which is what makes them safe to allow rather than something to prevent.</para>
///
/// <para><b>Never a dependency.</b> Same rule the agent side has held from the start: an unreachable
/// hub must not stop anything from working. With no hub this class is never constructed, and if the
/// hub dies later the registrar reconnects on its own. All that is lost meanwhile is addressability.</para>
/// </summary>
public sealed class WindowRegistration : IAsyncDisposable
{
    private readonly InstanceRegistrar _registrar;

    private WindowRegistration(InstanceRegistrar registrar) => _registrar = registrar;

    /// <summary>
    /// Registers this window with the hub, or returns null when there is no hub to register with —
    /// which callers must treat as ordinary, not as a failure.
    /// </summary>
    /// <param name="hubUrl">The machine hub, or null when none was reached.</param>
    /// <param name="projectId">The manifest path this window is looking at, or null when it opened
    /// project-less. A project-less window still registers: "a window exists here" is worth knowing
    /// even when it is not attached to anything yet.</param>
    /// <param name="serviceUrl">The service this window talks to. Recorded so an observer can tell
    /// which agent a window belongs to without guessing from the project id alone.</param>
    public static WindowRegistration? StartIfHubAvailable(
        string? hubUrl, string? projectId, string? projectName, string? serviceUrl, ILogger log)
    {
        if (string.IsNullOrWhiteSpace(hubUrl)) return null;

        var registration = new RegisterFrame
        {
            ProjectId = projectId ?? "(no-project)",
            ProjectName = projectName,
            Role = ParticipantRoles.Window,
            Info = new InstanceInfo
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                Endpoint = serviceUrl,
                Mode = "ui",
                Machine = Environment.MachineName,
                Pid = Environment.ProcessId,
                StartedAt = DateTimeOffset.UtcNow
            }
        };

        var registrar = new InstanceRegistrar(
            hubUrl,
            token: null,
            registration,
            readStatus: ReadStatus,
            onStopRequested: CloseAsync,
            log,
            onFocusRequested: RaiseAsync);

        registrar.Start();
        return new WindowRegistration(registrar);
    }

    /// <summary>A window has no turn to be busy with, so it is always idle and always has exactly the
    /// one client it is. Reported anyway rather than omitted: an observer counting participants should
    /// not have to special-case which of them bothered to answer.</summary>
    private static StatusFrame ReadStatus()
        => new() { State = InstanceStates.Name(InstanceState.Idle), Clients = 1 };

    /// <summary>
    /// Closing on request is unconditional, and that is the difference between a window and an agent:
    /// an agent refuses mid-turn because it holds work nobody else has, while a window holds nothing.
    /// The work lives in the service and survives its viewer.
    /// </summary>
    private static Task CloseAsync(bool force)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (global::Avalonia.Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        });
        return Task.CompletedTask;
    }

    /// <summary>
    /// Comes to the front on request — what "Open" does when a window for the project already exists,
    /// instead of starting a second one.
    ///
    /// <para><b>Known limit.</b> Windows only lets the foreground process hand the foreground on, so
    /// <see cref="Window.Activate"/> called from a background process is free to be ignored and often
    /// is (the taskbar button flashes instead). Making it reliable needs the party the person actually
    /// clicked — the tray shell — to call <c>AllowSetForegroundWindow</c> for this pid before the hub
    /// relays the request. Until that exists this is best-effort, and restoring a minimised window
    /// (the part that does work from anywhere) is already worth having.</para>
    /// </summary>
    private static Task RaiseAsync()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (global::Avalonia.Application.Current?.ApplicationLifetime
                is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }) return;

            if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
            window.Show();
            window.Activate();
        });
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => _registrar.DisposeAsync();
}
