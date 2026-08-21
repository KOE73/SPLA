using System.Text;
using SPLA.Agent;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Permissions;
using SPLA.Runtime;

namespace SPLA.CLI.Batch;

/// <summary>One prompt to run, named for output files/labels — inline text gets <c>text1</c>,
/// <c>text2</c>, …, a <c>--prompt-file</c> gets its own base name.</summary>
public sealed record PromptItem(string Name, string Text)
{
    /// <summary>Where the prompt came from — the file path, or null for <c>--prompt</c> text. Reported
    /// in the run statistics: "which prompt produced this" is unanswerable from a name like
    /// <c>text1</c> once the output has left the machine that ran it.</summary>
    public string? Source { get; init; }
}

/// <summary>One matrix cell: a prompt against one model entry.</summary>
public sealed record BatchCell(PromptItem Prompt, ResolvedModelEntry Model);

public sealed record CellResult(string Status, string? Text, string? Note, TimeSpan Elapsed, RunStats Stats);

/// <summary>
/// Runs a prompt-by-model matrix headlessly: one fresh chat per cell, no permission prompts (every
/// tool call is denied — there is nobody at the keyboard to ask), no clarify. Mirrors the pattern in
/// <c>demo/workers/Summarizer/Runner.cs</c> but generalized: no document, no LM Studio load/unload —
/// just "this prompt, against this model, in isolation".
/// <para>
/// Anything the run adds to the prompt (<c>--sys-prompt</c>, the <c>--md-clean</c> directive) rides on
/// <see cref="CliContributor"/>, registered once into the runtime's composer by the caller — this
/// class never touches <see cref="ResolvedSettings.CustomPrompt"/>, so the project's own prompt is
/// never clobbered by a one-off batch run.
/// </para>
/// </summary>
public sealed class BatchRunner(AgentRuntime runtime, ResolvedSettings settings)
{
    public double? Temperature { get; init; }
    public string? ReasoningLevel { get; init; }
    public int? TimeoutSeconds { get; init; }
    public bool Stream { get; init; }

    /// <summary>Skill handed to every cell's chat before its prompt runs, the same "given to it by a
    /// person" path <see cref="SPLA.Runtime.ChatRuntime.ActivateSkill"/> serves elsewhere — not the
    /// REPL's <c>/skills load</c> message-injection shortcut.</summary>
    public string? SkillId { get; init; }

    /// <summary>Reported in the statistics, not acted upon — the flags themselves are already applied
    /// by the caller through the prompt composer. A report that omits them cannot explain why two runs
    /// of the same prompt against the same model differ.</summary>
    public bool MdClean { get; init; }

    /// <summary>Human-readable note of the extra system prompt this run carried ("--sys-prompt-file
    /// x.md"), or null when it carried none.</summary>
    public string? SystemPromptExtra { get; init; }

    public async Task<CellResult> RunOneAsync(BatchCell cell, CancellationToken ct)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();

        if (Temperature is { } t) settings.Temperature = t;
        if (ReasoningLevel is { } r) settings.ReasoningLevel = r;

        // Built before the run so that even a failure reports which model, endpoint and settings the
        // failure belongs to — the case where a report is worth most.
        var stats = RunStats.For(cell, settings, this);

        var chat = new ChatRegistry(runtime).CreateNew($"{cell.Prompt.Name} · {cell.Model.DisplayName}");
        chat.ApplySettings(mode: null, modelId: cell.Model.Id);

        if (SkillId is { Length: > 0 } skillId && chat.ActivateSkill(skillId) is { } skillError)
            return Finish(stats, clock, "error", null, $"skill '{skillId}': {skillError}");

        var answer = new StringBuilder();
        var stream = new StringBuilder();

        var callbacks = new AgentCallbacks
        {
            OnDelta = chunk =>
            {
                stream.Append(chunk);
                if (Stream) Console.Write(chunk);
                return Task.CompletedTask;
            },
            OnAssistantMessage = m =>
            {
                if (!string.IsNullOrWhiteSpace(m.Content)) answer.Append(m.Content);
                return Task.CompletedTask;
            },
            OnLlmTurn = stats.Record
        };

        Func<ToolFunctionDefinition, string, Task<PermissionDecision>> denyAll =
            (_, _) => Task.FromResult(PermissionDecision.Deny);
        Func<ClarifyRequest, Task<string?>> noClarify = _ => Task.FromResult<string?>(null);

        using var runCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (TimeoutSeconds is > 0) runCts.CancelAfter(TimeSpan.FromSeconds(TimeoutSeconds.Value));

        try
        {
            await chat.SendAsync(cell.Prompt.Text, callbacks, denyAll, noClarify, runCts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return Finish(stats, clock, "timeout", null, $"no response in {TimeoutSeconds}s");
        }
        catch (Exception ex)
        {
            return Finish(stats, clock, "error", null, ex.Message);
        }

        var text = (answer.Length > 0 ? answer : stream).ToString().Trim();
        return text.Length == 0
            ? Finish(stats, clock, "empty", null, "model returned no text")
            : Finish(stats, clock, "ok", text, null);
    }

    /// <summary>Stops the clock once, in one place, and seals the statistics with the outcome — so a
    /// timeout and a success are timed and reported on identical terms.</summary>
    private static CellResult Finish(RunStats stats, System.Diagnostics.Stopwatch clock, string status, string? text, string? note)
    {
        clock.Stop();
        stats.Status = status;
        stats.Note = note;
        stats.Elapsed = clock.Elapsed;
        stats.FinishedAt = DateTimeOffset.Now;
        stats.OutputChars = text?.Length ?? 0;
        return new CellResult(status, text, note, clock.Elapsed, stats);
    }
}
