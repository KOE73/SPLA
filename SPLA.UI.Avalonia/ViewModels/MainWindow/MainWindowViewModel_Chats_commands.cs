using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SPLA.UI.Avalonia.ViewModels.Chat;
using System.Linq;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Reconciles the sidebar list with the chats on disk: keeps existing (possibly running)
    /// view-models, creates shells for new chats, drops deleted ones, and orders by recency.
    /// Never recreates a live view-model, so background runs survive a refresh.
    /// </summary>
    private void SyncChatsFromDisk()
    {
        if (_chatManager == null || _services == null) return;

        var metas = _chatManager.ListChats();
        var ids = metas.Select(m => m.Id).ToHashSet();

        foreach (var staleId in _chatVms.Keys.Where(k => !ids.Contains(k)).ToList())
            _chatVms.Remove(staleId);

        _isReloadingChats = true;
        try
        {
            // Build the desired ordered VM list, reusing live view-models from the cache.
            var desired = new System.Collections.Generic.List<ChatSessionViewModel>(metas.Count);
            foreach (var meta in metas)
            {
                if (!_chatVms.TryGetValue(meta.Id, out var vm))
                {
                    vm = new ChatSessionViewModel(this, _services, meta) { OnChatSaved = OnChildChatSaved };
                    _chatVms[meta.Id] = vm;
                }
                else
                {
                    vm.Title = meta.Title;
                }
                desired.Add(vm);
            }

            // Reconcile `Chats` to match `desired` IN PLACE — never Clear(). Clearing empties the
            // collection, which nulls the ListBox SelectedItem and writes garbage back through the
            // two-way ActiveChat binding (the active chat would thrash to whichever chat just saved).
            for (int i = Chats.Count - 1; i >= 0; i--)
                if (!ids.Contains(Chats[i].Id))
                    Chats.RemoveAt(i);

            for (int i = 0; i < desired.Count; i++)
            {
                var vm = desired[i];
                var current = Chats.IndexOf(vm);
                if (current < 0)
                    Chats.Insert(i, vm);
                else if (current != i)
                    Chats.Move(current, i);
            }

            BindLog?.LogInformation("[CHATBIND] SyncChatsFromDisk: order=[{Order}] active={Active}",
                string.Join(",", Chats.Select(c => c.Id)), ActiveChat?.Id);
        }
        finally
        {
            _isReloadingChats = false;
        }
    }

    /// <summary>After any chat saves, re-order the sidebar without disturbing running view-models.</summary>
    private void OnChildChatSaved()
        => global::Avalonia.Threading.Dispatcher.UIThread.Post(SyncChatsFromDisk);

    partial void OnActiveChatChanged(ChatSessionViewModel? value)
    {
        BindLog?.LogInformation("[CHATBIND] OnActiveChatChanged -> {Id} (title='{Title}', reloading={R})",
            value?.Id, value?.Title, _isReloadingChats);

        // Debug views read the active chat's stores through computed properties — tell them to re-pull,
        // and let any open debug windows re-target themselves at the new chat's live stores.
        OnPropertyChanged(nameof(SessionKv));
        OnPropertyChanged(nameof(Blobs));
        OnPropertyChanged(nameof(ContextSnapshot));
        ActiveChatChanged?.Invoke(this, System.EventArgs.Empty);

        if (_isReloadingChats) return;
        value?.Load();
    }

    [RelayCommand]
    private async Task DeleteChatAsync(ChatSessionViewModel? chat)
    {
        if (_chatManager == null || chat == null) return;

        if (ConfirmDeleteChatAsync != null)
        {
            var confirmed = await ConfirmDeleteChatAsync(chat);
            if (!confirmed) return;
        }

        var deletingActive = ActiveChat?.Id == chat.Id;
        chat.Shutdown();
        _chatManager.DeleteChat(chat.Id);
        _chatVms.Remove(chat.Id);
        Chats.Remove(chat);

        if (!deletingActive) return;

        if (Chats.Count > 0)
            ActiveChat = Chats.First();
        else
            NewChat();
    }

    [RelayCommand]
    private void SelectChat(ChatSessionViewModel? chat)
    {
        if (chat == null) return;
        ActiveChat = chat;
    }

    [RelayCommand]
    private void ForkChat()
    {
        if (_chatManager == null || ActiveChat == null) return;
        var newChat = _chatManager.DuplicateChat(ActiveChat.Id);
        SyncChatsFromDisk();
        if (_chatVms.TryGetValue(newChat.Id, out var vm))
            ActiveChat = vm;
    }
}
