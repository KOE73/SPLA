using SPLA.Domain.Models;

namespace SPLA.Domain.Settings;

public class LLMSettings
{
    /// <summary>
    /// Which provider serves this turn — the connection's <c>provider</c> field, and the key the
    /// client resolver dispatches on. Empty/null means "the default provider", which keeps a project
    /// that never named one working.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// The connection this turn runs on. Carried alongside the model because rate limits, balance and
    /// reachability are properties of the credential, not of the model — anything recording those has
    /// to key by connection or five models under one key would report five different budgets.
    /// </summary>
    public string? ConnectionId { get; set; }

    public string BaseUrl { get; set; } = "http://127.0.0.1:1234/v1/";
    public string ModelName { get; set; } = "local-model";
    public string ApiKey { get; set; } = "lm-studio";

    /// <summary>
    /// Manual context-window override in tokens from the connection (<c>context_length</c> in .spla).
    /// Null = not configured; the runtime then auto-detects the operative window from the provider
    /// (LM Studio native API loaded-instance config, vLLM <c>max_model_len</c>) where possible.
    /// </summary>
    public int? ContextLength { get; set; }

    public double Temperature { get; set; } = 0.7;
    public AgentMode Mode { get; set; } = AgentMode.Edit;
    public string Theme { get; set; } = "Dark";

    /// <summary>
    /// Selected reasoning option for the active model. Interpreted per the model's advertised
    /// options: "off"/"on" map to <c>chat_template_kwargs.enable_thinking</c>; graded values
    /// (e.g. "low"/"medium"/"high") map to <c>reasoning_effort</c>. Empty/null = model default.
    /// </summary>
    public string? ReasoningLevel { get; set; }

    /// <summary>
    /// Subtracts a fixed value from the logit of any token already seen (0 = off).
    /// count=1 → penalty X; count=5 → still X. Pushes model toward new words/topics.
    /// Safe range: 0.0–0.2. Above 0.3 model may avoid necessary terms, variable names, JSON keys.
    /// </summary>
    public double PresencePenalty { get; set; } = 0.0;

    /// <summary>
    /// Subtracts count × value from the logit of each repeated token (0 = off).
    /// count=1 → X; count=5 → 5X. Grows with each repetition — best against verbatim loops.
    /// Safe range: 0.0–0.2. Above 0.3 code, JSON, and technical terms start to break.
    /// </summary>
    public double FrequencyPenalty { get; set; } = 0.0;

    /// <summary>
    /// Multiplicative logit penalty for tokens seen in the recent context window (llama.cpp / LM Studio).
    /// 1.0 = off (default). 1.05–1.1 = mild; 1.2+ = aggressive. Below 1.0 increases repetition.
    /// Works independently of presence/frequency penalties. Do not set all three high simultaneously.
    /// </summary>
    public double RepeatPenalty { get; set; } = 1.0;

    /// <summary>
    /// Hard ceiling on generated tokens for this turn. Null = do not send the field, so the server's
    /// own default (or lack of one) applies — a runaway generation otherwise has no ceiling of ours
    /// and can run until the provider's own limit, if any. Safe range: provider/model dependent;
    /// a few thousand is typical for a single turn.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Nucleus sampling: keep only the smallest set of tokens whose cumulative probability reaches
    /// this value. Null = do not send the field, server default applies. Safe range: 0.9–1.0;
    /// lower values narrow the vocabulary and can push the model toward repetition.
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    /// Minimum-probability sampling: discards tokens whose probability falls below this fraction of
    /// the most likely token's probability. Null = do not send the field, server default applies.
    /// Safe range: 0.0–0.1; it trims the improbable tail that a loop can otherwise wander into
    /// without touching the head of the distribution the way top_p does.
    /// </summary>
    public double? MinP { get; set; }
}
