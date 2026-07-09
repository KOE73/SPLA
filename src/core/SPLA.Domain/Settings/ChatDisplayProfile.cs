using YamlDotNet.Serialization;

namespace SPLA.Domain.Settings;

public enum ToolDisplayMode { Hidden, Collapsed, Expanded }

/// <summary>
/// Defines how chat messages are visually presented — independent of the render engine (Native vs Web).
/// Serialized as YAML in the global defaults or project config under ui.chat_profiles.
/// </summary>
public class ChatDisplayProfile
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = "";

    [YamlMember(Alias = "display_name")]
    public string DisplayName { get; set; } = "";

    // ── Layout ────────────────────────────────────────────────────────────

    [YamlMember(Alias = "use_bubble_layout")]
    public bool UseBubbleLayout { get; set; }

    // ── Message-kind visibility ───────────────────────────────────────────

    [YamlMember(Alias = "show_system_messages")]
    public bool ShowSystemMessages { get; set; } = true;

    /// <summary>
    /// Hidden = not shown at all; Collapsed = minimal chip, expandable; Expanded = full detail.
    /// </summary>
    [YamlMember(Alias = "tool_display_mode")]
    public ToolDisplayMode ToolDisplayMode { get; set; } = ToolDisplayMode.Collapsed;

    // ── Extra info ────────────────────────────────────────────────────────

    [YamlMember(Alias = "show_metadata")]
    public bool ShowMetadata { get; set; }

    [YamlMember(Alias = "show_retention_controls")]
    public bool ShowRetentionControls { get; set; } = true;

    // ── Built-in defaults ─────────────────────────────────────────────────

    public static readonly ChatDisplayProfile Bubbles = new()
    {
        Id = "bubbles", DisplayName = "Bubbles",
        UseBubbleLayout = true, ShowSystemMessages = false,
        ToolDisplayMode = ToolDisplayMode.Collapsed, ShowRetentionControls = false
    };

    public static readonly ChatDisplayProfile Classic = new()
    {
        Id = "classic", DisplayName = "Document",
        ShowSystemMessages = true,
        ToolDisplayMode = ToolDisplayMode.Collapsed, ShowRetentionControls = true
    };

    public static readonly ChatDisplayProfile Diagnostic = new()
    {
        Id = "diagnostic", DisplayName = "Diagnostic",
        ShowSystemMessages = true,
        ToolDisplayMode = ToolDisplayMode.Expanded, ShowMetadata = true, ShowRetentionControls = true
    };

    public static IReadOnlyList<ChatDisplayProfile> BuiltInProfiles => [Bubbles, Classic, Diagnostic];
}
