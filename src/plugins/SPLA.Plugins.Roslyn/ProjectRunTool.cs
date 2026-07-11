using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using System;
using System.Text.Json;

namespace SPLA.Plugins.Roslyn;

/// <summary>
/// <c>roslyn_project_run</c> — runs a REAL program from disk via <c>dotnet run</c>: either a project
/// (<c>--project</c>) or a single <c>.cs</c> file (a .NET 10 file-based app). This is what actually
/// executes scaffolded code end-to-end — the piece missing between "the project builds" and "the
/// program produces the right output". Distinct from <see cref="ScriptRunTool"/>, which runs an
/// in-memory Roslyn <em>script</em> to orchestrate tool calls, not a file on disk.
/// </summary>
internal sealed class ProjectRunTool : DotnetProjectTool
{
    public ProjectRunTool(string workspaceRoot) : base(workspaceRoot) { }

    public override string Name => "roslyn_project_run";
    protected override int DefaultTimeoutSeconds => 120;
    protected override string Verb => "run";

    public override ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Runs a REAL program from the workspace via 'dotnet run' and returns its console output and exit code. "
                + "'path' may be a .csproj / project folder, OR a single .cs file (run as a .NET file-based app). "
                + "Use this to actually EXECUTE scaffolded code and see its output — not roslyn_script_run, which runs an in-memory orchestration script rather than a file on disk. "
                + "Pass program arguments in 'args'.",
            Scope = ToolScope.Shell,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.High,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path            = new { type = "string", description = "Path (relative to the workspace) to a .csproj, a project folder, or a single .cs file to run. Omit to run the project in the workspace root." },
                    args            = new { type = "string", description = "Optional arguments passed to the program (appended after '--')." },
                    configuration   = new { type = "string", description = "Build configuration: 'Debug' (default) or 'Release'." },
                    timeout_seconds = new { type = "integer", description = "Hard timeout in seconds (default 120, max 600). The process tree is killed on timeout — use it to bound programs that might hang or wait for input." },
                    output          = SchemaParts.Output,
                    output_name     = SchemaParts.OutputName
                },
                required = Array.Empty<string>()
            }
        }
    };

    public override string? GetHelpText() => """
        tool: roslyn_project_run

        summary: Runs a real program (project or single .cs file) via 'dotnet run' and returns its output.

        arguments:
          path: relative path to a .csproj, a project folder, or a single .cs file. Omit for the workspace root.
          args: optional program arguments (appended after '--').
          configuration: Debug (default) or Release.
          timeout_seconds: hard timeout (default 120, max 600). Bounds hanging / input-waiting programs.

        difference from roslyn_script_run:
          - script_run compiles and runs an in-memory C# SCRIPT to orchestrate tool calls (ctx.Run/Log).
          - project_run executes an actual program on disk (a .csproj or a .cs file-based app) via the SDK.

        output:
          - "ok: true/false" + "exit_code: N", then the program's stdout, then stderr if any.

        notes:
          - A single .cs file is run as a .NET 10 file-based app (dotnet run file.cs).
          - The program runs with the host's permissions — the shell permission gate applies.

        examples:
          - request: { "path": "src/Calculator" }
          - request: { "path": "Program.cs" }
          - request: { "path": "src/Tool/Tool.csproj", "args": "--input data.txt" }
        """;

    protected override string BuildArguments(string resolvedPath, JsonElement root)
    {
        var config = ToolJson.GetStringTrimmed(root, "configuration");
        var configArg = string.IsNullOrWhiteSpace(config) ? "Debug" : config;
        var programArgs = ToolJson.GetStringTrimmed(root, "args");

        // A single .cs file is a .NET file-based app: `dotnet run file.cs`. Anything else (a .csproj or a
        // folder) goes through `--project`.
        var target = resolvedPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
            ? $"\"{resolvedPath}\""
            : $"--project \"{resolvedPath}\"";

        var line = $"run {target} -c {configArg} --nologo";
        if (!string.IsNullOrWhiteSpace(programArgs))
            line += $" -- {programArgs}";
        return line;
    }
}
