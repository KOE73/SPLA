using Avalonia.Controls;

namespace SPLA.Plugins.Host.Avalonia;

public interface IAvaloniaPlugin
{
    IEnumerable<AvaloniaPluginPanelDescriptor> GetPanels(AvaloniaPluginContext context);
}

public sealed class AvaloniaPluginContext
{
    public required string WorkspacePath { get; init; }
    public required IPluginInteractionService Interaction { get; init; }
}

public sealed class AvaloniaPluginPanelContext
{
    public required string WorkspacePath { get; init; }
    public required IPluginInteractionService Interaction { get; init; }
    public object? Payload { get; init; }
}

public sealed class AvaloniaPluginPanelDescriptor
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public string? Description { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = [];
    public required Func<AvaloniaPluginPanelContext, Control> CreateContent { get; init; }
}

public interface IPluginInteractionService
{
    void InsertIntoPrompt(string text);
    Task CopyToClipboardAsync(string text);
}
