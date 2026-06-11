using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.Services.Plugins;

public sealed class PluginCommandRunTool(
    IReadOnlyList<SplaPluginUiCommand> commands,
    IPluginPanelHostService panelHost,
    SPLA.Plugins.Host.Avalonia.IPluginInteractionService interaction) : IMcpTool, IToolHelpProvider
{
    public string Name => "plugin.command.run";

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "[H] Runs an environment-independent plugin command by id, such as opening a plugin panel.",
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    commandId = new { type = "string", description = "Plugin command id, for example onec.view.open." }
                },
                required = new[] { "commandId" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var commandId = doc.RootElement.TryGetProperty("commandId", out var idElement)
            ? idElement.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(commandId))
        {
            return AvailableCommands("error: missing commandId");
        }

        var command = commands.FirstOrDefault(c => string.Equals(c.Id, commandId, StringComparison.OrdinalIgnoreCase));
        if (command is null)
        {
            return AvailableCommands($"error: plugin command not found: {commandId}");
        }

        return command.Kind switch
        {
            SplaPluginUiCommandKind.OpenPanel => await OpenPanelAsync(command),
            SplaPluginUiCommandKind.OpenUrl or SplaPluginUiCommandKind.OpenFile => OpenExternal(command),
            SplaPluginUiCommandKind.CopyText => await CopyTextAsync(command),
            SplaPluginUiCommandKind.InsertText => InsertText(command),
            _ => $"error: unsupported plugin command kind: {command.Kind}"
        };
    }

    public string? GetHelpText() =>
        $"""
        tool: plugin.command.run

        summary: Runs a registered plugin command by id. Use this for UI commands exposed by plugins, for example opening an Avalonia plugin panel.

        arguments:
          commandId: exact plugin command id.

        available_commands:
        {string.Join(Environment.NewLine, commands.Select(c => $"  - {c.Id}: {c.DisplayName} ({c.Kind})"))}

        examples:
          - request:
              commandId: onec.view.open
        """;

    private async Task<string> OpenPanelAsync(SplaPluginUiCommand command)
    {
        var opened = await panelHost.OpenPanelAsync(command.Target);
        return opened
            ? $"opened: true\ncommand: {command.Id}\npanel: {command.Target}"
            : $"opened: false\ncommand: {command.Id}\nreason: panel not found\npanel: {command.Target}";
    }

    private static string OpenExternal(SplaPluginUiCommand command)
    {
        if (command.Kind == SplaPluginUiCommandKind.OpenFile && !File.Exists(command.Target))
        {
            return $"opened: false\ncommand: {command.Id}\nreason: file not found\ntarget: {command.Target}";
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = command.Target,
            UseShellExecute = true
        });

        return $"opened: true\ncommand: {command.Id}\ntarget: {command.Target}";
    }

    private async Task<string> CopyTextAsync(SplaPluginUiCommand command)
    {
        await interaction.CopyToClipboardAsync(command.Target);
        return $"copied: true\ncommand: {command.Id}";
    }

    private string InsertText(SplaPluginUiCommand command)
    {
        interaction.InsertIntoPrompt(command.Target);
        return $"inserted: true\ncommand: {command.Id}";
    }

    private string AvailableCommands(string header) =>
        $"""
        {header}
        available:
        {string.Join(Environment.NewLine, commands.Select(c => $"  - {c.Id}: {c.DisplayName} ({c.Kind})"))}
        """;
}
