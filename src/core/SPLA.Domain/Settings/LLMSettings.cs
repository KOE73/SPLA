using SPLA.Domain.Models;

namespace SPLA.Domain.Settings;

public class LLMSettings
{
    public string BaseUrl { get; set; } = "http://127.0.0.1:1234/v1/";
    public string ModelName { get; set; } = "local-model";
    public string ApiKey { get; set; } = "lm-studio";
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
}
