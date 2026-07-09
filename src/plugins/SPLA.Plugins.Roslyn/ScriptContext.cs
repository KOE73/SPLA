using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Roslyn;

/// <summary>
/// The single object handed to a <c>roslyn_script_run</c> script as globals. The script knows nothing
/// about the plugin system — it invokes any tool by name through <see cref="Run"/>, reports progress
/// through <see cref="Progress(string)"/>, and emits text through <see cref="Log"/>. Calls made via
/// <see cref="Run"/> go through the same host pipeline as the model's own tool calls, so permissions
/// and the progress tree apply automatically (each call nests under the script's node).
/// </summary>
public sealed class ScriptContext
{
    private static readonly JsonSerializerOptions ArgOptions = new()
    {
        PropertyNamingPolicy = null // keep property names verbatim; tool params are snake_case literals
    };

    private readonly IToolHost _host;
    private readonly AgentMode _mode;
    private readonly StringBuilder _log;
    private readonly object _logGate = new();
    private readonly CancellationToken _ct;

    internal ScriptContext(IToolHost host, AgentMode mode, StringBuilder log, CancellationToken ct)
    {
        _host = host;
        _mode = mode;
        _log = log;
        _ct = ct;
    }

    /// <summary>
    /// Self-reference so scripts can use the documented <c>ctx.Run(...)</c> style. The globals members
    /// are also in scope unqualified (e.g. <c>Run(...)</c>), but <c>ctx.</c> reads clearer and is what
    /// the tool help advertises.
    /// </summary>
    public ScriptContext ctx => this;

    /// <summary>The script's cancellation token (tied to the run timeout); pass it to your own awaits.</summary>
    public CancellationToken Cancellation => _ct;

    /// <summary>
    /// Invokes a tool by name and returns its raw text result. <paramref name="args"/> may be an
    /// anonymous object (serialized to JSON), a raw JSON string, or null (no arguments). Safe to call
    /// in parallel via <c>Task.WhenAll</c> — each call becomes a child node in the progress tree.
    /// </summary>
    public Task<string> Run(string tool, object? args = null)
    {
        _ct.ThrowIfCancellationRequested();
        var json = args switch
        {
            null => "{}",
            string s => s,                                  // already JSON
            _ => JsonSerializer.Serialize(args, ArgOptions) // anonymous/typed object
        };
        return _host.ExecuteToolAsync(_mode, tool, json, _ct);
    }

    /// <summary>Reports a human-readable progress message on the script's node.</summary>
    public void Progress(string message) => ProgressScope.Report(new ToolProgress { Message = message });

    /// <summary>Reports "current of total" progress on the script's node.</summary>
    public void Progress(long current, long total, string? message = null)
        => ProgressScope.Report(current, total, message);

    /// <summary>Appends a line to the script's captured output (returned to the model when it finishes).</summary>
    public void Log(object? value)
    {
        lock (_logGate) _log.AppendLine(value?.ToString() ?? string.Empty);
    }
}
