using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Roslyn;

/// <summary>
/// <c>roslyn_compile_check</c> — compiles a self-contained C# snippet against the .NET base class
/// library and returns compiler diagnostics. Lets the agent verify generated C# actually compiles
/// instead of guessing. Isolated: it does not see the user's other project files or NuGet packages.
/// </summary>
public sealed class CompileCheckTool : IMcpTool, IToolHelpProvider
{
    public string Name => "roslyn_compile_check";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Compiles a self-contained C# snippet against the .NET base class library and returns compiler diagnostics (CS#### errors/warnings). Use it to verify generated C# compiles before applying it.",
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    code        = new { type = "string", description = "The C# source to compile. A full file (namespace/class) by default, or top-level statements when kind='program'." },
                    kind        = new { type = "string", @enum = new[] { "library", "program" }, description = "'library' (default) for a class/file with no entry point, or 'program' for top-level statements / a Main entry point." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "code" }
            }
        }
    };

    public string? GetHelpText() => """
        tool: roslyn_compile_check

        summary: Compiles C# in isolation against the .NET BCL and reports compiler diagnostics.

        arguments:
          code: the C# source to check (required).
          kind:
            - library (default): a file with namespace/class declarations; no entry point required.
            - program: top-level statements, or code containing a Main entry point.

        limits:
          - Isolated compile: sees only the .NET base class library, NOT the user's other project
            files or NuGet packages. References to types defined elsewhere report as errors.
          - Best for self-contained classes, helpers, algorithms, and script bodies.

        output:
          - First line "ok: true" when there are no errors, otherwise "ok: false".
          - Then one line per diagnostic: "<severity> <id> (<line>,<col>): <message>".

        examples:
          - request: { "code": "public class C { public int Add(int a, int b) => a + b; }" }
          - request: { "code": "Console.WriteLine(\"hi\");", "kind": "program" }
        """;

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var code = ToolJson.GetString(root, "code");
            if (string.IsNullOrWhiteSpace(code))
                return Task.FromResult("Error: Missing 'code' parameter.");

            var kind = ToolJson.GetStringTrimmed(root, "kind")?.ToLowerInvariant() ?? "library";
            var outputKind = kind == "program"
                ? OutputKind.ConsoleApplication
                : OutputKind.DynamicallyLinkedLibrary;

            var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
            var syntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions, cancellationToken: cancellationToken);

            var compilation = CSharpCompilation.Create(
                assemblyName: "RoslynCompileCheck",
                syntaxTrees: new[] { syntaxTree },
                references: ReferenceAssemblies.Bcl,
                options: new CSharpCompilationOptions(outputKind, allowUnsafe: true));

            var diagnostics = compilation.GetDiagnostics(cancellationToken)
                .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
                .OrderBy(d => d.Location.GetLineSpan().StartLinePosition.Line)
                .ThenBy(d => d.Location.GetLineSpan().StartLinePosition.Character)
                .ToList();

            var hasErrors = diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

            var sb = new StringBuilder();
            sb.AppendLine(hasErrors ? "ok: false" : "ok: true");

            if (diagnostics.Count == 0)
            {
                sb.AppendLine("diagnostics: none");
                return Task.FromResult(sb.ToString().TrimEnd());
            }

            sb.AppendLine($"diagnostics: {diagnostics.Count}");
            foreach (var d in diagnostics)
            {
                var pos = d.Location.GetLineSpan().StartLinePosition;
                // 1-based line/column for human/editor alignment.
                sb.AppendLine($"  {d.Severity.ToString().ToLowerInvariant()} {d.Id} ({pos.Line + 1},{pos.Character + 1}): {d.GetMessage()}");
            }

            var result = sb.ToString().TrimEnd();
            var outputTarget = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
            if (outputTarget == OutputTarget.Context) return Task.FromResult(result);
            var blobName = ToolJson.GetStringTrimmed(root, "output_name");
            return Task.FromResult(DataChannel.Route(outputTarget, BlobPayload.OfText(result),
                $"roslyn_compile_check: {(hasErrors ? "errors" : "ok")}, {diagnostics.Count} diagnostics", blobName));
        }
        catch (JsonException)
        {
            return Task.FromResult("Error: Invalid JSON arguments.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Task.FromResult($"Error during compile check: {ex.Message}");
        }
    }
}
