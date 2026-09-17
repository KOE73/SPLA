using SPLA.Runtime;
using SPLA.Domain.Models;
using SPLA.Service;

namespace SPLA.CLI;

/// <summary>Console-side wiring shared by the interactive REPL and the parallel serve REPL: the
/// streaming subscribers that render a turn to stdout, and the permission/clarify prompts that read
/// a decision from stdin.
/// <para>
/// ADR_20260910-2 wave 3: rendering is a <see cref="ChatFeed"/> subscription, not caller-supplied
/// <c>AgentCallbacks</c> — <see cref="ChatRuntime.SendAsync"/> no longer takes any. Subscribe before
/// the turn starts, dispose once it (and the caller's own <c>await</c>) is done, so the console never
/// misses the turn's own last events (the assistant message, the token line) before the command
/// returns.
/// </para>
/// </summary>
internal static class ConsoleHandlers
{
    /// <summary>Rich rendering for the primary REPL: streamed text, tool call/progress/result lines,
    /// notices, and per-turn + cumulative token accounting. Subscribes <paramref name="feed"/>
    /// immediately; dispose the result once the turn has finished.</summary>
    public static IDisposable SubscribeRich(ChatFeed feed, AgentRuntime runtime)
    {
        var lastProgress = DateTime.MinValue;
        return feed.Subscribe(e =>
        {
            switch (e)
            {
                case ChatDelta d:
                    Console.Write(d.Text);
                    break;
                case ChatAssistantMessage:
                    Console.WriteLine();
                    break;
                case ChatToolStarted t:
                    Console.WriteLine($" -> Call: {t.Call.Function.Name}");
                    break;
                case ChatToolProgress p:
                {
                    var now = DateTime.UtcNow;
                    if ((now - lastProgress).TotalMilliseconds < 150 && p.Progress.Fraction < 1.0) break;
                    lastProgress = now;
                    var pct = p.Progress.Fraction is double f ? $" {f * 100:0}%" : "";
                    var detail = p.Progress.Details is { Count: > 0 }
                        ? "  " + string.Join("  ", p.Progress.Details.Select(dt => $"{dt.Label}: {dt.Value}"))
                        : "";
                    Console.Write($"\r    {p.Call.Function.Name}{pct} ({p.Progress.Current}/{p.Progress.Total}){detail}        ");
                    break;
                }
                case ChatToolResult r:
                {
                    var mark = r.Result.Outcome switch
                    {
                        ToolOutcome.Failed => "Failed",
                        ToolOutcome.Refused => "Refused",
                        _ => "Result received"
                    };
                    Console.WriteLine($"\r -> {mark} ({r.Result.TextContent.Length} chars).            ");
                    break;
                }
                case ChatNotice n:
                    Console.WriteLine($"\n{n.Text}");
                    break;
                // A generation thrown away mid-stream. Said out loud rather than left to the log: the
                // text already on screen came from it, and without this line the retry's answer would
                // appear to continue the loop the reader was just watching.
                case ChatAttempt a:
                    Console.WriteLine(
                        $"\n   [attempt {a.Attempt.Index} discarded] {a.Attempt.Note} · {a.Attempt.Chars:N0} chars in {a.Attempt.Duration.TotalSeconds:F1}s");
                    break;
                // Reads the tallies, never writes them: the pipeline has already recorded this call by
                // the time the event fires, so the totals printed here are the ones on disk.
                case ChatLlmTurn lt:
                {
                    var (prompt, completion) = (lt.Turn.Message.PromptTokens, lt.Turn.Message.CompletionTokens);
                    if (prompt is null && completion is null) break;

                    var t = runtime.TokenUsageProject.Total;
                    var g = runtime.TokenUsageGlobal.Total;
                    Console.WriteLine(
                        $"   [tokens] turn in:{prompt?.ToString() ?? "?"} out:{completion?.ToString() ?? "?"}" +
                        $"  ·  {lt.Turn.ModelReported}" +
                        $"  ·  project Σ {t.TotalTokens:N0} (in {t.PromptTokens:N0}/out {t.CompletionTokens:N0})" +
                        $"  ·  machine Σ {g.TotalTokens:N0}");
                    break;
                }
            }
        });
    }

    /// <summary>Minimal rendering for the parallel serve REPL (no progress bar / token line).</summary>
    public static IDisposable SubscribeBasic(ChatFeed feed) => feed.Subscribe(e =>
    {
        switch (e)
        {
            case ChatDelta d:
                Console.Write(d.Text);
                break;
            case ChatAssistantMessage:
                Console.WriteLine();
                break;
            case ChatToolStarted t:
                Console.WriteLine($" -> Call: {t.Call.Function.Name}");
                break;
            case ChatToolResult r:
            {
                var mark = r.Result.IsError ? $"{r.Result.Outcome}" : "Result";
                Console.WriteLine($" -> {mark} ({r.Result.TextContent.Length} chars)");
                break;
            }
            case ChatNotice n:
                Console.WriteLine($"\n{n.Text}");
                break;
            case ChatAttempt a:
                Console.WriteLine($"\n [attempt {a.Attempt.Index} discarded] {a.Attempt.Note}");
                break;
        }
    });

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
