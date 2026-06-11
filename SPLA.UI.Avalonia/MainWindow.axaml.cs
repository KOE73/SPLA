using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using SPLA.Domain.Models;
using SPLA.UI.Avalonia.Helpers;
using SPLA.UI.Avalonia.Services;
using SPLA.UI.Avalonia.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainWindowViewModel();
        vm.ConfirmDeleteChatAsync = ConfirmDeleteChatAsync;
        vm.SelectProjectFolderAsync = SelectProjectFolderAsync;
        DataContext = vm;
        App.Services.GetRequiredService<IActiveConversationAccessor>().CurrentInput = new MainWindowConversationInput(vm);
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        var settings = App.ResolvedSettings;
        var icon = IconGenerator.GenerateIcon(settings.WorkspacePath, settings.ProjectName);
        if (icon != null)
        {
            Icon = icon;
        }
    }

    private async void StatusControl_SettingsRequested(object? sender, System.EventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext!;
        var settingsWindow = new SettingsWindow
        {
            DataContext = vm.Settings
        };
        await settingsWindow.ShowDialog(this);
        
        // Update status UI values after closing settings
        vm.Status.Endpoint = vm.Settings.BaseUrl;
        vm.Status.ModelName = vm.Settings.SelectedModel ?? "local-model";
    }

    private async Task<bool> ConfirmDeleteChatAsync(ChatSession chat)
    {
        var dialog = new Window
        {
            Title = "Delete chat",
            Width = 360,
            Height = 170,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = Background,
            CanResize = false,
            Content = new StackPanel
            {
                Margin = new global::Avalonia.Thickness(16),
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Delete this chat?",
                        FontWeight = global::Avalonia.Media.FontWeight.Bold,
                        FontSize = 16
                    },
                    new TextBlock
                    {
                        Text = chat.Title,
                        TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
                    },
                    new StackPanel
                    {
                        Orientation = global::Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Right,
                        Spacing = 8,
                        Children =
                        {
                            new Button
                            {
                                Content = "Cancel",
                                MinWidth = 80,
                                Padding = new global::Avalonia.Thickness(10, 5)
                            },
                            new Button
                            {
                                Content = "Delete",
                                MinWidth = 80,
                                Padding = new global::Avalonia.Thickness(10, 5)
                            }
                        }
                    }
                }
            }
        };

        var buttons = ((StackPanel)((StackPanel)dialog.Content!).Children[2]).Children;
        ((Button)buttons[0]).Click += (_, _) => dialog.Close(false);
        ((Button)buttons[1]).Click += (_, _) => dialog.Close(true);

        return await dialog.ShowDialog<bool>(this);
    }

    private async Task<string?> SelectProjectFolderAsync()
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select SPLA project folder",
            AllowMultiple = false
        });

        return folders.FirstOrDefault()?.TryGetLocalPath();
    }
}
