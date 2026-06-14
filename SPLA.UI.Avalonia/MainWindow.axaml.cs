using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
    private const double SplitterColumnWidth = 5;
    private double _savedSidebarWidth = 240;

    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainWindowViewModel();
        vm.ConfirmDeleteChatAsync = ConfirmDeleteChatAsync;
        vm.SelectProjectFolderAsync = SelectProjectFolderAsync;
        DataContext = vm;
        App.Services.GetRequiredService<IActiveConversationAccessor>().CurrentInput = new MainWindowConversationInput(vm);

        // Sidebar starts closed — collapse cols 2 and 3
        SetSidebarColumns(false);

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.IsSidebarOpen))
                SetSidebarColumns(vm.IsSidebarOpen);
        };

        Loaded += MainWindow_Loaded;
    }

    private void SetSidebarColumns(bool open)
    {
        var cols = RootGrid.ColumnDefinitions;
        if (open)
        {
            cols[2].Width = new GridLength(SplitterColumnWidth);
            cols[3].Width = new GridLength(_savedSidebarWidth, GridUnitType.Pixel);
            cols[3].MinWidth = 140;
            cols[3].MaxWidth = 560;
        }
        else
        {
            var current = cols[3].Width;
            if (current.IsAbsolute && current.Value > 0)
                _savedSidebarWidth = current.Value;
            cols[2].Width = new GridLength(0);
            cols[3].Width = new GridLength(0);
            cols[3].MinWidth = 0;
        }
    }

    // ── Custom title bar ─────────────────────────────────────────────────────

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        if (MaximizeButton != null)
            MaximizeButton.Content = WindowState == WindowState.Maximized ? "❐" : "□";
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
        => Close();

    // ── Existing handlers ────────────────────────────────────────────────────

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
