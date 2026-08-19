using System.Reflection;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.Mcp;
using SPLA.MCP.Core;
using SPLA.Runtime;

namespace SPLA.CLI;

/// <summary>
/// <c>spla mcp</c> — serve this project's tools to a foreign head over MCP on stdin/stdout.
/// <para>
/// This is the "own body" deployment: the command builds a runtime for itself, serves it, and it
/// dies with the connection. Nothing listens on a port, so the process boundary is the whole of the
/// authentication story. The other deployment — attaching to a runtime that is already serving a UI
/// — reuses <see cref="McpStdioServer"/> unchanged, because the server is handed its host rather
/// than building one.
/// </para>
/// <para>
/// Headless by definition: there is no window in which to ask a person anything. Tools whose
/// permission verdict is "ask" therefore refuse, and the refusal has to say so in terms the calling
/// model can act on — which is why the speaking refusal was a prerequisite for this command and not
/// a nicety.
/// </para>
/// </summary>
public static class McpCommand
{
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

    public static async Task RunAsync(ResolvedSettings settings, ILoggerFactory loggerFactory)
    {
        // Nothing may reach stdout but the protocol. The runtime's own chatter goes to the file log
        // and to stderr; this is also why Program.cs must route here before printing its banner.
        using var runtime = new AgentRuntime(settings, loggerFactory);

        var exposure = ToolExposure.Default;
        var offered = runtime.McpHost.GetToolDefinitionsFor(exposure).ToList();
        Console.Error.WriteLine($"[spla-mcp] project: {settings.WorkspacePath}");
        Console.Error.WriteLine($"[spla-mcp] offering {offered.Count} tools");

        var server = new McpStdioServer(
            runtime.McpHost,
            () => runtime.McpHost.GetToolDefinitionsFor(exposure),
            log: Console.Error);

        using var stopping = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; stopping.Cancel(); };

        await server.RunAsync(Console.In, Console.Out, stopping.Token);
    }
}
