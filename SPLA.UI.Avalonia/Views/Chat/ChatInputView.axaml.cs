using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SPLA.UI.Avalonia.ViewModels.Chat;

namespace SPLA.UI.Avalonia.Views.Chat;

public partial class ChatInputView : UserControl
{
    public ChatInputView()
    {
        InitializeComponent();
        
        var inputBox = this.FindControl<TextBox>("InputTextBox");
        if (inputBox != null)
        {
            inputBox.AddHandler(InputElement.KeyDownEvent, InputTextBox_KeyDown, RoutingStrategies.Tunnel);
        }
    }

    private void InputTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        var vm = DataContext as ChatSessionViewModel;
        if (vm == null) return;

        if (e.Key == Key.Enter)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                return; // Let Avalonia insert newline
            }
            
            var tb = sender as TextBox;
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                e.Handled = true;
                if (tb != null) vm.InputText = tb.Text ?? "";
                vm.SendCommand.Execute(null);
                return;
            }

            if (tb != null)
            {
                bool hasNewlines = tb.Text != null && (tb.Text.Contains('\n') || tb.Text.Contains('\r'));
                if (!hasNewlines)
                {
                    e.Handled = true;
                    vm.InputText = tb.Text ?? "";
                    vm.SendCommand.Execute(null);
                }
            }
        }
    }
}
