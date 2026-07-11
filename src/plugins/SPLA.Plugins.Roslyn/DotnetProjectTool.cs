using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Roslyn;

/// <summary>
/// Shared plumbing for the <c>roslyn_project_*</c> tools that shell out to the <c>dotnet</c> SDK against
/// real files in the workspace (build / run / test). Each subclass only supplies its name, schema/help
/// and the concrete <c>dotnet</c> argument line; parsing the <c>path</c>, workspace containment, timeout,
/// process execution and result routing all live here so the three stay consistent.
/// </summary>
internal abstract class DotnetProjectTool : IMcpTool, IToolHelpProvider
{
    protected const int MaxTimeoutSeconds = 600;

    private readonly string _workspaceRoot;

    protected DotnetProjectTool(string workspaceRoot) => _workspaceRoot = workspaceRoot;

    public abstract string Name { get; }
    public abstract ToolDefinition GetDefinition();
    public abstract string? GetHelpText();

    /// <summary>Default hard timeout when the caller does not specify one. Builds/tests get longer.</summary>
    protected abstract int DefaultTimeoutSeconds { get; }

    /// <summary>The verb shown in messages ("build", "run", "test").</summary>
    protected abstract string Verb { get; }

    /// <summary>
    /// Builds the full <c>dotnet</c> argument line from the workspace-resolved <paramref name="path"/>
    /// (already quoted-safe as an absolute path) and the raw request <paramref name="root"/> for any
    /// tool-specific options.
    /// </summary>
    protected abstract string BuildArguments(string resolvedPath, JsonElement root);

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? path;
        int timeoutSeconds;
        OutputTarget outputTarget;
        string? blobName;
        string dotnetArgs;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;
            path = ToolJson.GetStringTrimmed(root, "path");
            timeoutSeconds = ToolJson.GetInt32Clamped(root, "timeout_seconds", DefaultTimeoutSeconds, 1, MaxTimeoutSeconds);
            outputTarget = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
            blobName = ToolJson.GetStringTrimmed(root, "output_name");

            if (!DotnetCli.ResolveInWorkspace(_workspaceRoot, path, out var resolved, out var pathError))
                return $"ok: false\nerror: {pathError}";

            dotnetArgs = BuildArguments(resolved, root);
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }

        DotnetCli.Result result;
        try
        {
            result = await DotnetCli.RunAsync(dotnetArgs, _workspaceRoot, timeoutSeconds, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw; // caller-initiated cancellation
        }
        catch (Exception ex)
        {
            return $"ok: false\nerror: could not run 'dotnet {dotnetArgs}': {ex.Message}";
        }

        var formatted = Format(result, timeoutSeconds);
        if (outputTarget == OutputTarget.Context) return formatted;

        var ok = !result.TimedOut && result.ExitCode == 0;
        return DataChannel.Route(outputTarget, BlobPayload.OfText(formatted),
            $"{Name}: {(ok ? "ok" : "failed")} (exit {result.ExitCode})", blobName);
    }

    private string Format(DotnetCli.Result result, int timeoutSeconds)
    {
        var sb = new StringBuilder();
        if (result.TimedOut)
        {
            sb.AppendLine("ok: false");
            sb.AppendLine($"error: dotnet {Verb} timed out after {timeoutSeconds}s (process tree killed)");
        }
        else
        {
            sb.AppendLine(result.ExitCode == 0 ? "ok: true" : "ok: false");
            sb.AppendLine($"exit_code: {result.ExitCode}");
        }

        var stdout = result.Stdout.TrimEnd();
        var stderr = result.Stderr.TrimEnd();
        if (stdout.Length > 0)
        {
            sb.AppendLine("output:");
            sb.AppendLine(stdout);
        }
        if (stderr.Length > 0)
        {
            sb.AppendLine("stderr:");
            sb.AppendLine(stderr);
        }
        if (stdout.Length == 0 && stderr.Length == 0)
            sb.AppendLine("(no output)");

        return sb.ToString().TrimEnd();
    }
}
