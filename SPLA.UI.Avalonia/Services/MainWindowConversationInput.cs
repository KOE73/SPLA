using SPLA.UI.Avalonia.ViewModels;
using System;

namespace SPLA.UI.Avalonia.Services;

public sealed class MainWindowConversationInput(MainWindowViewModel viewModel) : IConversationInput
{
    public string Text
    {
        get => viewModel.InputText;
        set => viewModel.InputText = value;
    }

    public void InsertText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        Text = string.IsNullOrWhiteSpace(Text)
            ? text
            : $"{Text}{Environment.NewLine}{text}";
    }
}
