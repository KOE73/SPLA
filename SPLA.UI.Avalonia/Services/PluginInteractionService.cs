using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using SPLA.Plugins.Host.Avalonia;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.Services;

public sealed class PluginInteractionService(IActiveConversationAccessor activeConversation) : IPluginInteractionService
{
    public void InsertIntoPrompt(string text)
    {
        if (activeConversation.CurrentInput is { } input)
        {
            input.InsertText(text);
            return;
        }

        _ = CopyToClipboardAsync(text);
    }

    public async Task CopyToClipboardAsync(string text)
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        var clipboard = desktop.MainWindow?.Clipboard;
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(text);
        }
    }
}
