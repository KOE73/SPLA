using System.Collections.Generic;

namespace SPLA.MCP.Core.Interfaces;

public interface ISplaPluginUiCommands
{
    IEnumerable<SplaPluginUiCommand> GetUiCommands();
}

public class SplaPluginUiCommand
{
    public string PluginId { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SplaPluginUiCommandKind Kind { get; set; } = SplaPluginUiCommandKind.OpenFile;
    public string Target { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = [];
}

public enum SplaPluginUiCommandKind
{
    OpenFile,
    OpenUrl,
    OpenPanel,
    RunTool,
    CopyText,
    InsertText,
    ExecuteAction
}
