using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Agent;
using SPLA.UI.Avalonia.ViewModels.Debug;

namespace SPLA.UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>Exposes both KV stores so <see cref="KvDebugWindowViewModel"/> can be constructed
    /// in the code-behind with live references (no DI needed — composition root already owns them).
    /// Session KV follows the active chat.</summary>
    public IKeyValueStore?  SessionKv => ActiveChat?.SessionKv;
    public IKeyValueStore   ProjectKv => _projectKv.Store;
    public IBlobStore?      Blobs     => ActiveChat?.Blobs;

    /// <summary>Raised whenever the active chat changes, so open debug windows can re-target
    /// themselves at the new chat's live stores instead of staying pinned to the old one.</summary>
    public event System.EventHandler? ActiveChatChanged;

    /// <summary>Raised when the user clicks the debug-KV button. The View handles the actual
    /// window creation so we stay free of Avalonia types in the ViewModel.</summary>
    public event System.EventHandler? KvDebugRequested;

    [RelayCommand]
    private void OpenKvDebug() => KvDebugRequested?.Invoke(this, System.EventArgs.Empty);

    /// <summary>Live snapshot of the context sent in the most recent LLM request (active chat).</summary>
    public ContextSnapshotViewModel? ContextSnapshot => ActiveChat?.ContextSnapshot;

    /// <summary>Raised when the user clicks the debug-context button.</summary>
    public event System.EventHandler? ContextDebugRequested;

    [RelayCommand]
    private void OpenContextDebug() => ContextDebugRequested?.Invoke(this, System.EventArgs.Empty);

    /// <summary>The active chat's current system-prompt segments (source-tagged), for the live preview.</summary>
    public System.Collections.Generic.IReadOnlyList<SPLA.Agent.PromptSegment>? SystemPromptSegments
        => ActiveChat?.BuildSystemSegments();

    /// <summary>Raised when the user clicks the system-prompt preview button.</summary>
    public event System.EventHandler? PromptDebugRequested;

    [RelayCommand]
    private void OpenPromptDebug() => PromptDebugRequested?.Invoke(this, System.EventArgs.Empty);

    /// <summary>Raised when the user opens the thin-client (web) window. The View creates the window
    /// so the ViewModel stays free of Avalonia types.</summary>
    public event System.EventHandler? ThinClientRequested;

    [RelayCommand]
    private void OpenThinClient() => ThinClientRequested?.Invoke(this, System.EventArgs.Empty);
}
