using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Roslyn;

/// <summary>
/// <c>roslyn_script_run</c> — compiles and runs a C# script that the model authored, executing its own
/// plan deterministically. The script invokes tools by name via <c>ctx.Run(...)</c> (same host, same
/// permissions, same progress tree), loops and parallelizes with ordinary C#, and reports through
/// <c>ctx.Progress</c>/<c>ctx.Log</c>. Purpose: collapse a long, repetitive tool plan into one
/// deterministic run — saving intermediate context and enabling parallelism — rather than the model
/// hand-driving every step.
/// </summary>
public sealed class ScriptRunTool : IMcpTool, IToolHelpProvider
{
    private const int DefaultTimeoutSeconds = 60;
    private const int MaxTimeoutSeconds = 600;

    public string Name => "roslyn_script_run";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Compiles and runs a C# script (top-level statements) that drives your plan in ONE deterministic call — "
                + "use instead of issuing many tool calls yourself, e.g. to loop over inputs or parallelize with Task.WhenAll. "
                + "The script has a 'ctx' global with: await ctx.Run(\"tool_name\", new { param = value }) — call any tool by name and get its text result; "
                + "ctx.Log(value) — emit a line to the output; ctx.Progress(message) or ctx.Progress(current, total, message) — report progress; "
                + "ctx.Cancellation — the timeout CancellationToken to pass to your awaits. Tool calls inside obey the same permissions. "
                + "Example: ctx.Log(await ctx.Run(\"system_read_file\", new { path = \"a.txt\" }));",
            Scope = ToolScope.Shell,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.High,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    code = new { type = "string", description = "C# script body (top-level statements). The 'ctx' globals expose: Task<string> Run(string tool, object? args), void Progress(...), void Log(object), CancellationToken Cancellation. Use await ctx.Run(\"tool_name\", new { param = value })." },
                    timeout_seconds = new { type = "integer", description = $"Hard timeout in seconds (default {DefaultTimeoutSeconds}, max {MaxTimeoutSeconds}). Cancels awaits that honor ctx.Cancellation; a pure CPU loop cannot be interrupted." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "code" }
            }
        }
    };

    public string? GetHelpText() => $$"""
        tool: roslyn_script_run

        summary: Compile and run a C# script that executes your plan, calling tools via ctx.Run.

        globals (members of 'ctx', available directly):
          Task<string> Run(string tool, object? args)  - invoke a tool by name; args is an anonymous
                                                          object, a raw JSON string, or null.
          void Progress(string message)                - report a progress message.
          void Progress(long current, long total, string? message) - report N-of-total progress.
          void Log(object value)                       - append a line to the returned output.
          CancellationToken Cancellation               - the run's timeout token; pass to your awaits.

        the script body is top-level statements. usings already imported: System, System.Linq,
        System.Collections.Generic, System.Threading.Tasks, System.Text. Console.WriteLine is captured.

        arguments:
          code: the C# script body (required).
          timeout_seconds: hard timeout (default {{DefaultTimeoutSeconds}}, max {{MaxTimeoutSeconds}}).

        notes:
          - ctx.Run goes through the normal tool pipeline: mode permissions apply and may prompt/deny.
          - Parallelize with Task.WhenAll; each ctx.Run becomes a child node in the progress tree.
          - Timeout cancels awaits that honor ctx.Cancellation; a tight CPU loop cannot be interrupted.

        output:
          - "ok: true/false", optional "return: <last expression value>", then the captured output.
          - On a compile error: "ok: false" followed by the compiler diagnostics.

        example:
          code: |
            var hosts = new[] { "8.8.8.8", "1.1.1.1" };
            var results = await Task.WhenAll(hosts.Select(h => ctx.Run("network_ping_host", new { host = h })));
            for (int i = 0; i < hosts.Length; i++) ctx.Log($"{hosts[i]}: {results[i]}");
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string code;
        int timeoutSeconds;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;
            var parsed = ToolJson.GetString(root, "code");
            if (string.IsNullOrWhiteSpace(parsed))
                return "Error: Missing 'code' parameter.";
            code = parsed;
            timeoutSeconds = ToolJson.GetInt32Clamped(root, "timeout_seconds", DefaultTimeoutSeconds, 1, MaxTimeoutSeconds);
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }

        var ambient = ToolHostScope.Current;
        if (ambient is null)
            return "Error: No tool host is available to the script (roslyn_script_run must run inside the agent host).";

        var output = new StringBuilder();
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var ctx = new ScriptContext(ambient.Host, ambient.Mode, output, timeoutCts.Token);

        var options = ScriptOptions.Default
            .WithReferences(ReferenceAssemblies.Bcl)
            .WithImports("System", "System.Linq", "System.Collections.Generic", "System.Threading.Tasks", "System.Text");

        // Capture Console output for the duration. Top-level tool calls run sequentially, so the
        // process-wide Console redirection here does not overlap with another script run.
        var originalOut = Console.Out;
        var capture = TextWriter.Synchronized(new StringWriter(output));
        Console.SetOut(capture);
        try
        {
            // The script's globals type (ScriptContext) lives in THIS plugin assembly, which the host
            // loaded into an isolated PluginLoadContext. Roslyn's scripting engine loads the compiled
            // script assembly through its own InteractiveAssemblyLoader; left to its defaults it would
            // resolve the plugin assembly a SECOND time from disk, giving a different ScriptContext type
            // identity and an InvalidCastException on the globals — even for a trivial `ctx.Log(...)`.
            // RegisterDependency pins the reference to the already-loaded assembly so the identities unify.
            using var assemblyLoader = new InteractiveAssemblyLoader();
            assemblyLoader.RegisterDependency(typeof(ScriptContext).Assembly);
            var script = CSharpScript.Create<object>(code, options, typeof(ScriptContext), assemblyLoader);
            var state = await script.RunAsync(ctx, timeoutCts.Token);

            var sb = new StringBuilder();
            sb.AppendLine("ok: true");
            if (state.ReturnValue is { } rv)
                sb.AppendLine($"return: {rv}");
            AppendOutput(sb, output);
            var scriptResult = sb.ToString().TrimEnd();

            OutputTarget outputTarget;
            string? blobName;
            try
            {
                using var argDoc = JsonDocument.Parse(argumentsJson);
                outputTarget = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(argDoc.RootElement, "output"));
                blobName = ToolJson.GetStringTrimmed(argDoc.RootElement, "output_name");
            }
            catch { outputTarget = OutputTarget.Context; blobName = null; }

            if (outputTarget == OutputTarget.Context) return scriptResult;
            return DataChannel.Route(outputTarget, BlobPayload.OfText(scriptResult), "roslyn_script_run: ok", blobName);
        }
        catch (CompilationErrorException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ok: false");
            sb.AppendLine($"diagnostics: {ex.Diagnostics.Length}");
            foreach (var d in ex.Diagnostics)
            {
                var pos = d.Location.GetLineSpan().StartLinePosition;
                sb.AppendLine($"  {d.Severity.ToString().ToLowerInvariant()} {d.Id} ({pos.Line + 1},{pos.Character + 1}): {d.GetMessage()}");
            }
            return sb.ToString().TrimEnd();
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ok: false");
            sb.AppendLine($"error: script timed out after {timeoutSeconds}s");
            AppendOutput(sb, output);
            return sb.ToString().TrimEnd();
        }
        catch (OperationCanceledException)
        {
            throw; // caller-initiated cancellation
        }
        catch (Exception ex)
        {
            // Runtime exception thrown by the script itself — report it, don't crash the host.
            var sb = new StringBuilder();
            sb.AppendLine("ok: false");
            sb.AppendLine($"error: {ex.GetType().Name}: {ex.Message}");
            AppendOutput(sb, output);
            return sb.ToString().TrimEnd();
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    private static void AppendOutput(StringBuilder sb, StringBuilder output)
    {
        if (output.Length == 0) return;
        sb.AppendLine("output:");
        sb.Append(output);
    }
}
