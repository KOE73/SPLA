using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using SPLA.Domain.Models;
using SPLA.UI.Avalonia.Helpers;
using SPLA.UI.Avalonia.Services;
using SPLA.UI.Avalonia.ViewModels;
using SPLA.UI.Avalonia.ViewModels.Debug;
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
        vm.KvDebugRequested += VmOnKvDebugRequested;
        vm.ContextDebugRequested += VmOnContextDebugRequested;
        vm.PromptDebugRequested += VmOnPromptDebugRequested;
        vm.ThinClientRequested += VmOnThinClientRequested;
        vm.ActiveChatChanged += VmOnActiveChatChanged;
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

    // TEMP: открыть debug-окно schema-editor (Фаза 1 live-проверка). Убрать после verify.
    private void OpenSchemaEditorDebug_Click(object? sender, RoutedEventArgs e)
        => new Views.SchemaEditorDebugWindow { } .Show(this);

    private void VmOnThinClientRequested(object? sender, System.EventArgs e)
        => new ThinClientWindow().Show(this);

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

    private KvDebugWindow? _kvDebugWindow;

    private void VmOnKvDebugRequested(object? sender, System.EventArgs e)
    {
        // Reuse an already-open window instead of stacking them.
        if (_kvDebugWindow != null)
        {
            _kvDebugWindow.Activate();
            return;
        }
        var vm = (MainWindowViewModel)DataContext!;
        if (vm.SessionKv == null) return;
        var debugVm = new KvDebugWindowViewModel(vm.SessionKv, vm.ProjectKv, vm.Blobs);
        _kvDebugWindow = new KvDebugWindow(debugVm);
        _kvDebugWindow.Closed += (_, _) => _kvDebugWindow = null;
        _kvDebugWindow.Show(this);
    }

    /// <summary>When the active chat changes, re-target any open debug windows at the new chat's
    /// live stores instead of leaving them pinned to the chat that was active when they opened.</summary>
    private void VmOnActiveChatChanged(object? sender, System.EventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext!;

        if (_kvDebugWindow?.DataContext is KvDebugWindowViewModel kvVm && vm.SessionKv != null)
            kvVm.Rebind(vm.SessionKv, vm.Blobs);

        if (_contextDebugWindow != null)
            _contextDebugWindow.DataContext = vm.ContextSnapshot;

        // The prompt preview pulls from vm.SystemPromptSegments live, so a re-pull retargets it.
        if (_promptDebugWindow?.DataContext is PromptDebugWindowViewModel promptVm)
        {
            HookPromptLiveChat(vm.ActiveChat);
            promptVm.Refresh();
        }
    }

    private PromptDebugWindow? _promptDebugWindow;
    private ViewModels.Chat.ChatSessionViewModel? _promptLiveChat;

    private void VmOnPromptDebugRequested(object? sender, System.EventArgs e)
    {
        if (_promptDebugWindow != null)
        {
            _promptDebugWindow.Activate();
            return;
        }
        var vm = (MainWindowViewModel)DataContext!;
        var promptVm = new PromptDebugWindowViewModel(() => vm.SystemPromptSegments);
        _promptDebugWindow = new PromptDebugWindow(promptVm);
        _promptDebugWindow.Closed += (_, _) => { _promptDebugWindow = null; HookPromptLiveChat(null); };
        HookPromptLiveChat(vm.ActiveChat);
        _promptDebugWindow.Show(this);
    }

    /// <summary>Keep the open prompt preview subscribed to the active chat's prompt rebuilds (build-at-send),
    /// so it refreshes live as messages are sent. Re-points on chat switch, detaches on window close.</summary>
    private void HookPromptLiveChat(ViewModels.Chat.ChatSessionViewModel? chat)
    {
        if (ReferenceEquals(_promptLiveChat, chat)) return;
        if (_promptLiveChat != null) _promptLiveChat.SystemPromptChanged -= OnLiveSystemPromptChanged;
        _promptLiveChat = chat;
        if (_promptLiveChat != null) _promptLiveChat.SystemPromptChanged += OnLiveSystemPromptChanged;
    }

    private void OnLiveSystemPromptChanged(object? sender, System.EventArgs e)
    {
        if (_promptDebugWindow?.DataContext is PromptDebugWindowViewModel promptVm)
            global::Avalonia.Threading.Dispatcher.UIThread.Post(promptVm.Refresh);
    }

    private ContextDebugWindow? _contextDebugWindow;

    private void VmOnContextDebugRequested(object? sender, System.EventArgs e)
    {
        // Reuse an already-open window instead of stacking them.
        if (_contextDebugWindow != null)
        {
            _contextDebugWindow.Activate();
            return;
        }
        var vm = (MainWindowViewModel)DataContext!;
        if (vm.ContextSnapshot == null) return;
        _contextDebugWindow = new ContextDebugWindow(vm.ContextSnapshot);
        _contextDebugWindow.Closed += (_, _) => _contextDebugWindow = null;
        _contextDebugWindow.Show(this);
    }

    private async void StatusControl_SettingsRequested(object? sender, System.EventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext!;
        var projectLabel = string.IsNullOrWhiteSpace(App.ProjectFilePath)
            ? "no project"
            : App.ProjectFilePath;
        var settingsWindow = new SettingsWindow
        {
            DataContext = vm.Settings,
            Title = $"Settings — {projectLabel}"
        };
        await settingsWindow.ShowDialog(this);
    }

    private async Task<bool> ConfirmDeleteChatAsync(SPLA.UI.Avalonia.ViewModels.Chat.ChatSessionViewModel chat)
    {
        var dialog = new ConfirmDeleteChatWindow(chat.Title);
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
