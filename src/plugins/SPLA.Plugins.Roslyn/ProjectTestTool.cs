using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using System;
using System.Text.Json;

namespace SPLA.Plugins.Roslyn;

/// <summary>
/// <c>roslyn_project_test</c> — runs <c>dotnet test</c> on a REAL test project/solution in the workspace
/// and returns the pass/fail summary and failure details. Complements build/run: it proves scaffolded
/// tests actually pass, not just that the code compiles.
/// </summary>
internal sealed class ProjectTestTool : DotnetProjectTool
{
    public ProjectTestTool(string workspaceRoot) : base(workspaceRoot) { }

    public override string Name => "roslyn_project_test";
    protected override int DefaultTimeoutSeconds => 240;
    protected override string Verb => "test";

    public override ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Runs 'dotnet test' on a REAL test project/solution in the workspace and returns the pass/fail summary with failure details. "
                + "Use it to verify scaffolded tests pass. 'path' is relative to the workspace; omit to test the project/solution in the workspace root. "
                + "Narrow to specific tests with 'filter' (dotnet --filter expression).",
            Scope = ToolScope.Shell,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.High,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path            = new { type = "string", description = "Path (relative to the workspace) to a test .csproj/.sln/.slnx or a folder. Omit to test the workspace root." },
                    filter          = new { type = "string", description = "Optional 'dotnet test --filter' expression to run a subset (e.g. \"FullyQualifiedName~Calculator\")." },
                    configuration   = new { type = "string", description = "Build configuration: 'Debug' (default) or 'Release'." },
                    timeout_seconds = new { type = "integer", description = "Hard timeout in seconds (default 240, max 600). The process tree is killed on timeout." },
                    output          = SchemaParts.Output,
                    output_name     = SchemaParts.OutputName
                },
                required = Array.Empty<string>()
            }
        }
    };

    public override string? GetHelpText() => """
        tool: roslyn_project_test

        summary: Runs 'dotnet test' on a real test project/solution and reports pass/fail.

        arguments:
          path: relative path to a test .csproj/.sln/.slnx or folder. Omit for the workspace root.
          filter: optional --filter expression to run a subset of tests.
          configuration: Debug (default) or Release.
          timeout_seconds: hard timeout (default 240, max 600).

        output:
          - "ok: true/false" + "exit_code: N" (0 = all passed), then the test runner output.

        examples:
          - request: { "path": "tests/Calculator.Tests" }
          - request: { "path": "tests/Calculator.Tests", "filter": "FullyQualifiedName~Add" }
        """;

    protected override string BuildArguments(string resolvedPath, JsonElement root)
    {
        var config = ToolJson.GetStringTrimmed(root, "configuration");
        var configArg = string.IsNullOrWhiteSpace(config) ? "Debug" : config;
        var filter = ToolJson.GetStringTrimmed(root, "filter");

        var line = $"test \"{resolvedPath}\" -c {configArg} -v minimal --nologo";
        if (!string.IsNullOrWhiteSpace(filter))
            line += $" --filter \"{filter}\"";
        return line;
    }
}
