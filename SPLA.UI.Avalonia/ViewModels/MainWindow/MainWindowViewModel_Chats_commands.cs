using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    private void LoadChatsList()
    {
        if (_chatManager == null) return;
        
        var activeId = CurrentChat?.Id;
        _isReloadingChats = true;
        try
        {
            Chats.Clear();
            foreach (var c in _chatManager.ListChats())
            {
                Chats.Add(c);
            }
            
            if (activeId != null)
            {
                var matched = Chats.FirstOrDefault(c => c.Id == activeId);
                if (matched != null)
                {
                    CurrentChat = matched;
                }
            }
        }
        finally
        {
            _isReloadingChats = false;
        }
    }

    partial void OnCurrentChatChanged(ChatSession? value)
    {
        if (_isReloadingChats) return;

        if (value == null)
        {
            Session.ClearSession();
            return;
        }

        Session.LoadSession(value);
    }

    [RelayCommand]
    private async Task DeleteChatAsync(ChatSession? chat)
    {
        if (_chatManager == null || chat == null || Session.IsBusy) return;

        if (ConfirmDeleteChatAsync != null)
        {
            var confirmed = await ConfirmDeleteChatAsync(chat);
            if (!confirmed) return;
        }

        var deletingCurrent = CurrentChat?.Id == chat.Id;
        _chatManager.DeleteChat(chat.Id);
        Chats.Remove(chat);

        if (!deletingCurrent) return;

        CurrentChat = null;
        Session.ClearSession();

        LoadChatsList();
        if (Chats.Count > 0)
        {
            SelectChat(Chats.First());
        }
        else
        {
            NewChat();
        }
    }

    [RelayCommand]
    private void SelectChat(ChatSession session)
    {
        if (_chatManager == null) return;
        if (_isReloadingChats) return;
        
        if (CurrentChat != session)
        {
            CurrentChat = session;
        }
        else
        {
            // Force reload even though CurrentChat didn't change reference.
            Session.ClearSession();
            Session.LoadSession(session);
        }
    }


    [RelayCommand]
    private void ForkChat()
    {
        if (_chatManager == null || CurrentChat == null) return;
        var newChat = _chatManager.DuplicateChat(CurrentChat.Id);
        LoadChatsList();
        SelectChat(newChat);
    }
}

