using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using SPLA.Domain.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Roslyn;

public class RoslynPlugin : ISplaPlugin, ISplaPluginSelfCheck
{
    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        // The project-level tools shell out to the dotnet SDK against real files, so they need a root to
        // resolve paths against and to contain execution within. WorkspacePath is absolute after
        // ConfigLoader resolution; fall back to the current directory when running without a project.
        var workspaceRoot = string.IsNullOrWhiteSpace(settings.WorkspacePath)
            ? System.IO.Directory.GetCurrentDirectory()
            : settings.WorkspacePath;

        return new IMcpTool[]
        {
            new CompileCheckTool(),
            new ScriptRunTool(),
            new ProjectBuildTool(workspaceRoot),
            new ProjectRunTool(workspaceRoot),
            new ProjectTestTool(workspaceRoot),
        };
    }

    /// <summary>
    /// Verifies the C# scripting runtime actually executes in this load context. The scripting path
    /// broke once with an <c>InvalidCastException</c> on the globals type that was invisible until the
    /// model called <c>roslyn_script_run</c> at runtime (see <see cref="ScriptRunTool"/>); this probe
    /// exercises the same compile-and-run path once at load so such a regression surfaces immediately
    /// as a Degraded plugin state instead of a failed tool call mid-conversation.
    /// </summary>
    public PluginHealth CheckHealth()
    {
        try
        {
            var log = new StringBuilder();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var ctx = new ScriptContext(NoOpToolHost.Instance, AgentMode.Agent, log, cts.Token);

            var options = ScriptOptions.Default
                .WithReferences(ReferenceAssemblies.Bcl)
                .WithImports("System");

            // Same wiring as ScriptRunTool: pin the globals assembly so the compiled script binds to
            // THIS (already-loaded) plugin copy rather than a fresh reload — the crux of the earlier bug.
            using var loader = new InteractiveAssemblyLoader();
            loader.RegisterDependency(typeof(ScriptContext).Assembly);
            var script = CSharpScript.Create<object>("ctx.Log(\"ok\");", options, typeof(ScriptContext), loader);
            script.RunAsync(ctx, cts.Token).GetAwaiter().GetResult();

            return log.ToString().Contains("ok")
                ? PluginHealth.Ok
                : PluginHealth.Degraded("C# scripting produced no output; roslyn_script_run may not work.");
        }
        catch (Exception ex)
        {
            return PluginHealth.Degraded(
                $"C# scripting self-check failed ({ex.GetType().Name}: {ex.Message}). "
                + "roslyn_script_run will not work in this load context; roslyn_compile_check is unaffected.");
        }
    }

    /// <summary>A tool host that is never actually invoked — the self-check script only calls
    /// <c>ctx.Log</c>, not <c>ctx.Run</c>. Present because <see cref="ScriptContext"/> requires one.</summary>
    private sealed class NoOpToolHost : IToolHost
    {
        public static readonly NoOpToolHost Instance = new();
        public IEnumerable<ToolDefinition> GetToolDefinitions() => Array.Empty<ToolDefinition>();
        public Task<string> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson, CancellationToken ct = default)
            => Task.FromResult(string.Empty);
    }
}
