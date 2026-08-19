using SPLA.Domain.Llm;
using SPLA.Domain.Settings;

namespace SPLA.CLI.Batch;

/// <summary>One named group of measurements, in the order a reader wants them. Sections exist so a
/// renderer never has to know what a field means — it walks titles and pairs and nothing else, which
/// is what makes a new output format a renderer and not a rewrite.</summary>
public sealed record StatSection(string Title, IReadOnlyList<KeyValuePair<string, object?>> Items);

/// <summary>
/// Everything worth knowing about one finished cell — which model actually answered, on what
/// endpoint, with which settings, how long it took and what it cost in tokens.
/// <para>
/// Deliberately a summary and not a trace: a cell may take several LLM calls (an agent loop), and
/// what a run report needs is the total, with the number of calls named so the total is readable.
/// The per-call detail exists on <see cref="AgentCallbacks.OnLlmTurn"/> for anything that wants it.
/// </para>
/// </summary>
public sealed class RunStats
{
    public required string PromptName { get; init; }
    public string? PromptSource { get; init; }
    public int PromptChars { get; init; }

    /// <summary>The model entry as configured — its id and the name that goes on the wire.</summary>
    public required string ModelId { get; init; }
    public string? ModelRequested { get; init; }

    /// <summary>What the provider says answered. Differs from <see cref="ModelRequested"/> whenever
    /// the server resolves the name itself (<c>auto</c>) or a cloud substitutes a dated build — which
    /// is the whole reason this report exists.</summary>
    public string? ModelReported { get; set; }

    /// <summary>Every distinct name the provider reported across the cell's calls. Normally one; more
    /// than one means the run was not answered by a single model and the summary must say so rather
    /// than quietly keeping the last.</summary>
    public List<string> ModelsSeen { get; } = [];

    public string? Connection { get; init; }
    public string? Provider { get; init; }
    public string? Endpoint { get; init; }
    public int? ContextLength { get; init; }

    public double Temperature { get; init; }
    public string? ReasoningRequested { get; init; }
    /// <summary>What the reasoning lever actually became on the wire, as reported by the provider
    /// client. "(nothing sent)" is a real and common answer — see the client's <c>DescribeReasoning</c>.</summary>
    public string? ReasoningWire { get; set; }
    public int? MaxTokens { get; init; }
    public double? TopP { get; init; }
    public double? MinP { get; init; }
    public double PresencePenalty { get; init; }
    public double FrequencyPenalty { get; init; }
    public double RepeatPenalty { get; init; }
    public int? TimeoutSeconds { get; init; }
    public string? Skill { get; init; }
    public bool MdClean { get; init; }
    public string? SystemPromptExtra { get; init; }

    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset FinishedAt { get; set; }
    public TimeSpan Elapsed { get; set; }

    /// <summary>Number of LLM calls the cell took. One for a plain answer; more when the agent looped.</summary>
    public int LlmCalls { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens => PromptTokens + CompletionTokens;

    /// <summary>Provider counters summed across the cell's calls, under their own wire names — so a
    /// counter this build has never heard of (cache reads, reasoning tokens) is still reported.</summary>
    public Dictionary<string, long> RawUsage { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Last-seen provider observations other than the reasoning one — rate-limit budget and
    /// the like. Last-seen, not summed: they describe the key's standing, not this call.</summary>
    public Dictionary<string, string> ProviderSignals { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string Status { get; set; } = "ok";
    public string? Note { get; set; }
    public int OutputChars { get; set; }
    public string? OutputFile { get; set; }

    /// <summary>Folds one finished LLM call into the totals. Called once per call, from the agent's
    /// own accounting choke point.</summary>
    public void Record(LlmTurnResult turn)
    {
        LlmCalls++;

        if (turn.ModelReported is { Length: > 0 } reported)
        {
            ModelReported = reported;
            if (!ModelsSeen.Contains(reported)) ModelsSeen.Add(reported);
        }

        foreach (var (key, value) in turn.RawUsage)
            RawUsage[key] = RawUsage.TryGetValue(key, out var running) ? running + value : value;

        if (turn.Message.PromptTokens is { } p) PromptTokens += p;
        if (turn.Message.CompletionTokens is { } c) CompletionTokens += c;

        foreach (var fact in turn.Signals)
        {
            if (fact.Key == "reasoning.wire") ReasoningWire = fact.Value;
            else ProviderSignals[fact.Label] = string.IsNullOrEmpty(fact.Unit) ? fact.Value : $"{fact.Value} {fact.Unit}";
        }
    }

    public static RunStats For(BatchCell cell, ResolvedSettings settings, BatchRunner runner) => new()
    {
        PromptName        = cell.Prompt.Name,
        PromptSource      = cell.Prompt.Source,
        PromptChars       = cell.Prompt.Text.Length,
        ModelId           = cell.Model.Id,
        ModelRequested    = cell.Model.Model,
        Connection        = cell.Model.Connection.DisplayName,
        Provider          = cell.Model.Provider,
        Endpoint          = cell.Model.Endpoint,
        ContextLength     = cell.Model.ContextLength,
        Temperature       = runner.Temperature ?? settings.Temperature,
        ReasoningRequested= runner.ReasoningLevel ?? settings.ReasoningLevel,
        MaxTokens         = settings.MaxTokens,
        TopP              = settings.TopP,
        MinP              = settings.MinP,
        PresencePenalty   = settings.PresencePenalty,
        FrequencyPenalty  = settings.FrequencyPenalty,
        RepeatPenalty     = settings.RepeatPenalty,
        TimeoutSeconds    = runner.TimeoutSeconds,
        Skill             = runner.SkillId,
        MdClean           = runner.MdClean,
        SystemPromptExtra = runner.SystemPromptExtra
    };

    /// <summary>The report as ordered sections — the single shape every format renders.</summary>
    public IReadOnlyList<StatSection> ToSections()
    {
        var sections = new List<StatSection>
        {
            new("run", [
                Pair("status", Status),
                Pair("note", Note),
                Pair("started_at", StartedAt.ToString("O")),
                Pair("finished_at", FinishedAt == default ? null : FinishedAt.ToString("O")),
                Pair("elapsed", Elapsed.ToString(@"hh\:mm\:ss\.fff")),
                Pair("elapsed_seconds", Math.Round(Elapsed.TotalSeconds, 3)),
                Pair("llm_calls", LlmCalls),
                Pair("spla_version", typeof(RunStats).Assembly.GetName().Version?.ToString())
            ]),
            new("prompt", [
                Pair("name", PromptName),
                Pair("source", PromptSource),
                Pair("chars", PromptChars)
            ]),
            new("model", [
                Pair("entry_id", ModelId),
                Pair("requested", ModelRequested),
                Pair("reported", ModelReported),
                // Only when it actually varied — a one-element list every time trains a reader to
                // stop looking at the field that exists to catch the rare case.
                Pair("reported_all", ModelsSeen.Count > 1 ? string.Join(", ", ModelsSeen) : null),
                Pair("connection", Connection),
                Pair("provider", Provider),
                Pair("endpoint", Endpoint),
                Pair("context_length", ContextLength)
            ]),
            new("settings", [
                Pair("temperature", Temperature),
                Pair("reasoning_requested", ReasoningRequested ?? "(default)"),
                Pair("reasoning_wire", ReasoningWire),
                Pair("max_tokens", MaxTokens),
                Pair("top_p", TopP),
                Pair("min_p", MinP),
                Pair("presence_penalty", PresencePenalty),
                Pair("frequency_penalty", FrequencyPenalty),
                Pair("repeat_penalty", RepeatPenalty),
                Pair("timeout_seconds", TimeoutSeconds),
                Pair("skill", Skill),
                Pair("md_clean", MdClean ? true : null),
                Pair("system_prompt_extra", SystemPromptExtra)
            ]),
            new("tokens", [
                Pair("prompt", PromptTokens),
                Pair("completion", CompletionTokens),
                Pair("total", TotalTokens),
                .. RawUsage.Where(u => u.Key is not ("prompt_tokens" or "completion_tokens"))
                           .Select(u => Pair($"raw.{u.Key}", u.Value))
            ]),
            new("output", [
                Pair("file", OutputFile),
                Pair("chars", OutputChars)
            ])
        };

        if (ProviderSignals.Count > 0)
            sections.Add(new StatSection("provider", ProviderSignals.Select(s => Pair(s.Key, (object?)s.Value)).ToList()));

        // Nulls and blanks mean "not applicable to this run" and are dropped rather than rendered as
        // empty rows: a report full of blanks is read as a broken report. An empty model name is the
        // ordinary case, not an anomaly — it is how a project says "whatever the server has loaded".
        return sections
            .Select(s => new StatSection(s.Title, s.Items.Where(i => i.Value is not (null or "")).ToList()))
            .Where(s => s.Items.Count > 0)
            .ToList();
    }

    private static KeyValuePair<string, object?> Pair(string key, object? value) => new(key, value);
}
