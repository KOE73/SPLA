using System.Reflection;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.Instances;
using SPLA.Mcp;
using SPLA.MCP.Core;
using SPLA.Runtime;

namespace SPLA.CLI;

/// <summary>
/// <c>spla mcp</c> — serve this project's tools to a foreign head over MCP on stdin/stdout.
/// <para>
/// <b>Join-or-start, by default.</b> A project has exactly one writer (<see cref="SPLA.Domain.Project.InstanceLock"/>),
/// and building a fresh <see cref="AgentRuntime"/> here — the only thing this command used to do —
/// takes that lease for as long as the MCP connection lives. That is the exact problem this command
/// exists to avoid causing: an app window, a REPL, or a second <c>spla mcp</c> on the same project all
/// get refused for as long as this one stdio pipe is open, even though none of them actually conflict
/// with a tool call in progress — they conflict with a writer-lease that only exists because this
/// command insisted on building its own body. <c>POST /mcp</c> on <c>spla serve</c> exists for exactly
/// this caller (see the comment on that route in <c>SplaServiceHost.cs</c>): it dispatches against a
/// runtime the host already has open, so any number of MCP clients can share it the way any number of
/// browser windows already share <c>/ws</c>. So the default here is: find a live instance and proxy
/// stdio ⟷ its <c>/mcp</c> route (<see cref="McpHttpProxy"/>); if nobody is serving the project yet,
/// start one — a loopback child, on an idle timeout, that quietly outlives this connection until
/// nothing needs it. Either way this process never becomes the writer.
/// </para>
/// <para>
/// <b><c>--standalone</c> opts back into the old behaviour</b> — building an <see cref="AgentRuntime"/>
/// in this very process and serving it directly, taking the writer lease for the life of the
/// connection. That is still the right call when this really is going to be the only thing touching the
/// project (a CI job, a throwaway sandbox), where the extra process join-or-start would spawn is pure
/// overhead, and where there being no other writer to conflict with makes the whole join-or-start
/// question moot.
/// </para>
/// <para>
/// <b>The session is held open by a socket, not by the calls.</b> <c>HandleMcpAsync</c> holds the
/// instance lease only for the duration of each individual HTTP request, because <c>/mcp</c> is
/// otherwise stateless per request (see <see cref="McpHttpProxy"/>'s remarks for the related gap that
/// falls out of the same design, around <c>notifications/cancelled</c>). That alone would let an
/// instance idle itself out from under a session that is merely quiet between calls rather than
/// finished — the failure this command exists to be robust against, since an MCP client that went to
/// lunch has no way to start a service back up. So this process also opens one ordinary WebSocket to
/// the instance and simply holds it, doing nothing, for as long as the stdio pipe lives: the lease
/// counts connected clients (<c>ConnectionHub</c>), so a socket that says nothing still says "somebody
/// is here". That restores the model the whole design rests on — the client owns the lifetime. This
/// pipe closes, the hold drops, and the instance reclaims itself on its own schedule.
/// </para>
/// <para>
/// Headless by definition, in both modes: there is no window in which to ask a person anything. Tools
/// whose permission verdict is "ask" therefore refuse, and the refusal has to say so in terms the
/// calling model can act on — which is why the speaking refusal was a prerequisite for this command and
/// not a nicety.
/// </para>
/// </summary>
public static class McpCommand
{
    /// <summary>How long a <c>serve</c> child this command starts for itself is allowed to sit idle
    /// before it reclaims itself. Matches <see cref="SPLA.Instances.CliInstanceSpawner"/>'s own default
    /// (fifteen minutes) rather than the desktop shell's five: nobody is holding a window open on this
    /// one, and — unlike the desktop shell — there is no user sitting there to simply reopen a window if
    /// this reclaims itself a little early.
    /// <para>Note that this timeout only starts to matter once this process is gone: while the stdio
    /// pipe is open the hold socket keeps the child alive regardless of how quiet the session is (see
    /// the class remarks). The number therefore governs the gap between "the MCP client exited" and
    /// "the child noticed", not anything a live session can trip over.</para></summary>
    private static readonly TimeSpan ChildIdleTimeout = TimeSpan.FromMinutes(15);

    public static bool IsMcpCommand(string[] args) =>
        args.Length > 0 && args[0].Equals("mcp", StringComparison.OrdinalIgnoreCase);

    /// <summary>Prints the embedded MCP usage doc (Assets/MCP_USAGE.md) to stdout. Reads it from the
    /// assembly, not from disk, so it works run from any directory and survives the single-file publish.</summary>
    public static void PrintHelpMcp()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SPLA.CLI.MCP_USAGE.md");
        if (stream is null)
        {
            Console.Error.WriteLine("(MCP_USAGE.md was not embedded in this build)");
            return;
        }
        using var reader = new StreamReader(stream);
        Console.WriteLine(reader.ReadToEnd());
    }

    /// <param name="args">The full CLI argument vector (starting with <c>"mcp"</c>), so this command can
    /// read its own flags — today, just <c>--standalone</c> — without <c>Program.cs</c> having to know
    /// about them ahead of the raw pre-Spectre dispatch that routes here. See the class remarks for why
    /// <c>mcp</c> is dispatched before Spectre ever sees the argument vector.</param>
    public static async Task RunAsync(ResolvedSettings settings, ILoggerFactory loggerFactory, string[] args)
    {
        var standalone = args.Any(a => a.Equals("--standalone", StringComparison.OrdinalIgnoreCase));

        using var stopping = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; stopping.Cancel(); };

        if (!standalone)
        {
            try
            {
                await RunProxiedAsync(settings, stopping.Token);
                return;
            }
            catch (OperationCanceledException) when (stopping.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // Nothing to join, nowhere to spawn a child (no SPLA.CLI.exe/.dll reachable from
                // AppContext.BaseDirectory — see SelfInvocationLauncher), a child that never came up:
                // all land here. Falling back to the old in-process behaviour means a broken join-or-
                // start degrades to "works, but takes the writer lease" instead of "does not work at
                // all" — the same trade EmbeddedServiceLauncher makes by starting its own child when
                // joining fails, just one step further out.
                Console.Error.WriteLine(
                    $"[spla-mcp] could not join or start a shared instance ({ex.Message}); " +
                    "falling back to a standalone runtime in this process.");
            }
        }

        await RunStandaloneAsync(settings, loggerFactory, stopping.Token);
    }

    /// <summary>
    /// The default path: find or start a live instance for this project and proxy stdio to its
    /// <c>/mcp</c> route instead of building a runtime here.
    /// </summary>
    private static async Task RunProxiedAsync(ResolvedSettings settings, CancellationToken ct)
    {
        using var connector = new LocalServiceConnector();

        // Join first — see the class remarks for why this beats spawning a second (refused) writer.
        // The joined instance is only accepted if it actually maps /mcp: an instance somebody else
        // started (a desktop window, an older config) may well have mcp.enabled left at its project
        // default of false, and POSTing our first real request to find that out the hard way would
        // waste the round trip that matters — the client's actual `initialize` call.
        var joined = await LocalServiceConnector.TryJoinAsync(settings.WorkspacePath, ct);
        string url;
        bool started;

        if (joined is not null && await McpHttpProxy.SupportsMcpAsync(joined, ct))
        {
            url = joined;
            started = false;
        }
        else
        {
            // Either nobody is serving this project, or somebody is but without /mcp mapped. Either way
            // the fix is the same: start our own — with --mcp forced on, since the only reason this
            // command is starting a child at all is to answer that exact route (see
            // LocalServiceConnector.StartAsync's enableMcp remarks).
            url = await connector.StartAsync(
                settings.WorkspacePath, ChildIdleTimeout, hubUrl: null, enableMcp: true, ct: ct);
            started = true;
        }

        Console.Error.WriteLine(
            $"[spla-mcp] project: {settings.WorkspacePath}");
        Console.Error.WriteLine(
            $"[spla-mcp] {(started ? "started" : "joined")} instance at {url} — proxying stdio to POST {url}/mcp");

        // The hold: one idle socket for as long as this pipe lives. See the class remarks for why the
        // per-request lease HandleMcpAsync takes is not enough on its own.
        await using var hold = await TryHoldAsync(url, ct);

        var proxy = new McpHttpProxy(url);
        await proxy.RunAsync(Console.In, Console.Out, ct);
    }

    /// <summary>
    /// Opens the WebSocket whose only job is to exist, so the instance counts somebody as present for
    /// the whole MCP session rather than only during each request.
    ///
    /// <para><b>Never fatal.</b> A hold that cannot be established — an instance somebody else started
    /// behind a token this process was not given, a protocol the other end declines — costs the session
    /// its idle-timeout protection and nothing else: proxying works exactly as well without it. Failing
    /// the whole command over the loss of a safety net would be trading a working MCP server for a dead
    /// one, so this reports and carries on. The symptom that remains is the original one (an instance
    /// that may reclaim itself while the client is quiet), which is why it is said out loud.</para>
    /// </summary>
    /// <returns>The live hold, or a no-op when one could not be taken.</returns>
    private static async Task<IAsyncDisposable> TryHoldAsync(string url, CancellationToken ct)
    {
        try
        {
            return await Wire.CliWireClient.ConnectAsync(url, null, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.Error.WriteLine(
                $"[spla-mcp] could not hold the instance open ({ex.Message}); it may reclaim itself " +
                "if this session goes quiet for long enough.");
            return NoHold.Instance;
        }
    }

    /// <summary>Stands in for a hold that could not be taken, so the call site keeps one shape.</summary>
    private sealed class NoHold : IAsyncDisposable
    {
        public static readonly NoHold Instance = new();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    /// <summary>The original behaviour, kept verbatim for <c>--standalone</c> and as the fallback when
    /// join-or-start itself cannot get off the ground: build a runtime in this process, take the writer
    /// lease, serve it directly over stdio for the life of the connection.</summary>
    private static async Task RunStandaloneAsync(
        ResolvedSettings settings, ILoggerFactory loggerFactory, CancellationToken ct)
    {
        // Nothing may reach stdout but the protocol. The runtime's own chatter goes to the file log
        // and to stderr.
        using var runtime = new AgentRuntime(settings, loggerFactory, instanceMode: "mcp");

        var exposure = ToolExposure.Default;
        var offered = runtime.McpHost.GetToolDefinitionsFor(exposure).ToList();
        Console.Error.WriteLine($"[spla-mcp] project: {settings.WorkspacePath}");
        Console.Error.WriteLine($"[spla-mcp] offering {offered.Count} tools (standalone)");

        var server = new McpStdioServer(
            runtime.McpHost,
            () => runtime.McpHost.GetToolDefinitionsFor(exposure),
            log: Console.Error);

        await server.RunAsync(Console.In, Console.Out, ct);
    }
}
