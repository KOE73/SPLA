using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Agent;
using SPLA.UI.Avalonia.ViewModels.Debug;

namespace SPLA.UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>Exposes both KV stores so <see cref="KvDebugWindowViewModel"/> can be constructed
    /// in the code-behind with live references (no DI needed — composition root already owns them).</summary>
    public IKeyValueStore SessionKv => Session.SessionKv;
    public IKeyValueStore ProjectKv => _projectKv.Store;

    /// <summary>Raised when the user clicks the debug-KV button. The View handles the actual
    /// window creation so we stay free of Avalonia types in the ViewModel.</summary>
    public event System.EventHandler? KvDebugRequested;

    [RelayCommand]
    private void OpenKvDebug() => KvDebugRequested?.Invoke(this, System.EventArgs.Empty);

    /// <summary>Live snapshot of the context sent in the most recent LLM request.</summary>
    public ContextSnapshotViewModel ContextSnapshot => Session.ContextSnapshot;

    /// <summary>Raised when the user clicks the debug-context button.</summary>
    public event System.EventHandler? ContextDebugRequested;

    [RelayCommand]
    private void OpenContextDebug() => ContextDebugRequested?.Invoke(this, System.EventArgs.Empty);
}
