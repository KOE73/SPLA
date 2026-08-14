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
public sealed record PromptItem(string Name, string Text);

/// <summary>One matrix cell: a prompt against one model entry.</summary>
public sealed record BatchCell(PromptItem Prompt, ResolvedModelEntry Model);

public sealed record CellResult(string Status, string? Text, string? Note, TimeSpan Elapsed);

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

    public async Task<CellResult> RunOneAsync(BatchCell cell, CancellationToken ct)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();

        if (Temperature is { } t) settings.Temperature = t;
        if (ReasoningLevel is { } r) settings.ReasoningLevel = r;

        var chat = new ChatRegistry(runtime).CreateNew($"{cell.Prompt.Name} · {cell.Model.DisplayName}");
        chat.ApplySettings(mode: null, modelId: cell.Model.Id);

        if (SkillId is { Length: > 0 } skillId && chat.ActivateSkill(skillId) is { } skillError)
            return new CellResult("error", null, $"skill '{skillId}': {skillError}", clock.Elapsed);

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
            OnTokenUsage = (p, c) =>
            {
                runtime.TokenUsageProject.Record(p, c);
                runtime.TokenUsageGlobal.Record(p, c);
            }
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
            return new CellResult("timeout", null, $"no response in {TimeoutSeconds}s", clock.Elapsed);
        }
        catch (Exception ex)
        {
            return new CellResult("error", null, ex.Message, clock.Elapsed);
        }

        var text = (answer.Length > 0 ? answer : stream).ToString().Trim();
        clock.Stop();
        return text.Length == 0
            ? new CellResult("empty", null, "model returned no text", clock.Elapsed)
            : new CellResult("ok", text, null, clock.Elapsed);
    }
}
