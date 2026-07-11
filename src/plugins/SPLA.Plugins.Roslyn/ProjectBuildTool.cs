using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Tools;
using System.Text.Json;

namespace SPLA.Plugins.Roslyn;

/// <summary>
/// <c>roslyn_project_build</c> — runs <c>dotnet build</c> on a REAL project/solution in the workspace,
/// unlike <see cref="CompileCheckTool"/> which compiles an isolated snippet against the BCL only. This
/// closes the "scaffold a .csproj → prove it actually builds" loop: it sees project references, NuGet
/// packages and the declared target framework.
/// </summary>
internal sealed class ProjectBuildTool : DotnetProjectTool
{
    public ProjectBuildTool(string workspaceRoot) : base(workspaceRoot) { }

    public override string Name => "roslyn_project_build";
    protected override int DefaultTimeoutSeconds => 180;
    protected override string Verb => "build";

    public override ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Runs 'dotnet build' on a REAL .csproj/.sln/.slnx (or a folder) in the workspace and returns the build result and MSBuild/compiler diagnostics. "
                + "Unlike roslyn_compile_check (isolated, BCL-only), this builds the actual project with its references and NuGet packages — use it to verify a scaffolded project compiles for real. "
                + "'path' is relative to the workspace; omit it to build the project/solution in the workspace root.",
            Scope = ToolScope.Shell,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.High,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path            = new { type = "string", description = "Path (relative to the workspace) to a .csproj/.sln/.slnx or a folder containing one. Omit to build whatever is in the workspace root." },
                    configuration   = new { type = "string", description = "Build configuration: 'Debug' (default) or 'Release'." },
                    timeout_seconds = new { type = "integer", description = "Hard timeout in seconds (default 180, max 600). The process tree is killed on timeout." },
                    output          = SchemaParts.Output,
                    output_name     = SchemaParts.OutputName
                },
                required = System.Array.Empty<string>()
            }
        }
    };

    public override string? GetHelpText() => """
        tool: roslyn_project_build

        summary: Runs 'dotnet build' on a real project/solution in the workspace and reports the result.

        arguments:
          path: relative path to a .csproj/.sln/.slnx or folder. Omit to build the workspace root.
          configuration: Debug (default) or Release.
          timeout_seconds: hard timeout (default 180, max 600).

        difference from roslyn_compile_check:
          - compile_check is an ISOLATED in-memory compile against the BCL only — no project refs, no NuGet.
          - project_build runs the real dotnet SDK: MSBuild, NuGet restore, target framework, references.

        output:
          - "ok: true/false" + "exit_code: N", then captured stdout, then stderr if any.

        example:
          - request: { "path": "src/Calculator/Calculator.csproj", "configuration": "Release" }
        """;

    protected override string BuildArguments(string resolvedPath, JsonElement root)
    {
        var config = SPLA.MCP.Core.Json.ToolJson.GetStringTrimmed(root, "configuration");
        var configArg = string.IsNullOrWhiteSpace(config) ? "Debug" : config;
        // -clp:NoSummary keeps the tail terse; -v minimal keeps successful builds short but still lists errors.
        return $"build \"{resolvedPath}\" -c {configArg} -v minimal --nologo";
    }
}
