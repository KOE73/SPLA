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
}
