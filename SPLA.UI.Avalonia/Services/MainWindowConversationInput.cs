using SPLA.UI.Avalonia.ViewModels;
using System;

namespace SPLA.UI.Avalonia.Services;

public sealed class MainWindowConversationInput(MainWindowViewModel viewModel) : IConversationInput
{
    public string Text
    {
        get => viewModel.ActiveChat?.InputText ?? "";
        set { if (viewModel.ActiveChat != null) viewModel.ActiveChat.InputText = value; }
    }

    public void InsertText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        Text = string.IsNullOrWhiteSpace(Text)
            ? text
            : $"{Text}{Environment.NewLine}{text}";
    }
}
