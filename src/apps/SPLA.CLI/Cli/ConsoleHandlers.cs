using SPLA.Runtime;
using SPLA.Agent;
using SPLA.Domain.Models;
using SPLA.Service;

namespace SPLA.CLI;

/// <summary>Console-side wiring shared by the interactive REPL and the parallel serve REPL: the
/// streaming callbacks that render a turn to stdout, and the permission/clarify prompts that read
/// a decision from stdin.</summary>
internal static class ConsoleHandlers
{
    /// <summary>Rich turn callbacks for the primary REPL: streamed text, tool call/progress/result
    /// lines, notices, and per-turn + cumulative token accounting.</summary>
    public static AgentCallbacks RichCallbacks(AgentRuntime runtime)
    {
        var lastProgress = DateTime.MinValue;
        return new AgentCallbacks
        {
            OnDelta = chunk => { Console.Write(chunk); return Task.CompletedTask; },
            OnAssistantMessage = _ => { Console.WriteLine(); return Task.CompletedTask; },
            OnToolCallStarted = tc =>
            {
                Console.WriteLine($" -> Call: {tc.Function.Name}");
                return Task.CompletedTask;
            },
            OnToolProgress = (tc, progress) =>
            {
                var now = DateTime.UtcNow;
                if ((now - lastProgress).TotalMilliseconds < 150 && progress.Fraction < 1.0) return;
                lastProgress = now;
                var pct = progress.Fraction is double f ? $" {f * 100:0}%" : "";
                var detail = progress.Details is { Count: > 0 }
                    ? "  " + string.Join("  ", progress.Details.Select(d => $"{d.Label}: {d.Value}"))
                    : "";
                Console.Write($"\r    {tc.Function.Name}{pct} ({progress.Current}/{progress.Total}){detail}        ");
            },
            OnToolResult = (_, result) =>
            {
                Console.WriteLine($"\r -> Result received ({result.Length} chars).            ");
                return Task.CompletedTask;
            },
            OnNotice = note => { Console.WriteLine($"\n{note}"); return Task.CompletedTask; },
            OnTokenUsage = (prompt, completion) =>
            {
                runtime.TokenUsageProject.Record(prompt, completion);
                runtime.TokenUsageGlobal.Record(prompt, completion);
                if (prompt is int || completion is int)
                {
                    var t = runtime.TokenUsageProject.Total;
                    var g = runtime.TokenUsageGlobal.Total;
                    Console.WriteLine(
                        $"   [tokens] turn in:{prompt?.ToString() ?? "?"} out:{completion?.ToString() ?? "?"}" +
                        $"  ·  project Σ {t.TotalTokens:N0} (in {t.PromptTokens:N0}/out {t.CompletionTokens:N0})" +
                        $"  ·  machine Σ {g.TotalTokens:N0}");
                }
            }
        };
    }

    /// <summary>Minimal turn callbacks for the parallel serve REPL (no progress bar / token line).</summary>
    public static AgentCallbacks BasicCallbacks() => new()
    {
        OnDelta = chunk => { Console.Write(chunk); return Task.CompletedTask; },
        OnAssistantMessage = _ => { Console.WriteLine(); return Task.CompletedTask; },
        OnToolCallStarted = tc => { Console.WriteLine($" -> Call: {tc.Function.Name}"); return Task.CompletedTask; },
        OnToolResult = (_, result) => { Console.WriteLine($" -> Result ({result.Length} chars)"); return Task.CompletedTask; },
        OnNotice = note => { Console.WriteLine($"\n{note}"); return Task.CompletedTask; }
    };

    /// <summary>Interactive permission prompt. <paramref name="colored"/> selects the richer
    /// yellow multi-line rendering used by the primary REPL vs. the compact serve-REPL line.</summary>
    public static Func<ToolFunctionDefinition, string, Task<PermissionDecision>> Permission(bool colored) => (def, argsJson) =>
    {
        if (colored)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[PERMISSION] Agent requests: {def.Name}");
            Console.WriteLine($"Arguments: {argsJson}");
            Console.Write("Allow? (y/N): ");
            Console.ResetColor();
        }
        else
        {
            Console.Write($"\n[PERMISSION] {def.Name} {argsJson}\nAllow? (y/N): ");
        }

        var ans = Console.ReadLine();
        return Task.FromResult(ans?.Trim().ToLower().StartsWith("y") == true
            ? PermissionDecision.AllowOnce
            : PermissionDecision.Deny);
    };

    /// <summary>Interactive clarify prompt: lists the options and reads a 1-based choice from stdin.</summary>
    public static Func<ClarifyRequest, Task<string?>> Clarify() => req =>
    {
        Console.WriteLine($"\n[?] {req.Question}");
        for (int i = 0; i < req.Options.Count; i++)
        {
            var o = req.Options[i];
            var desc = o.Description != null ? $" — {o.Description}" : "";
            Console.WriteLine($"  {i + 1}. {o.Label}{desc}");
        }
        Console.Write("Choice (number, or Enter to skip): ");
        var line = Console.ReadLine()?.Trim();
        return Task.FromResult(int.TryParse(line, out var idx) && idx >= 1 && idx <= req.Options.Count
            ? req.Options[idx - 1].Label
            : (string?)null);
    };
}
