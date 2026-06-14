using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Input.Platform;

namespace SPLA.UI.Avalonia.ViewModels.Messages;

public partial class MessageViewModel : ViewModelBase
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;

    public ChatRole Role { get; }
    private string _content;
    public virtual string Content
    {
        get => _content;
        set
        {
            if (SetProperty(ref _content, value))
            {
                OnPropertyChanged(nameof(HasContent));
                OnPropertyChanged(nameof(IsVisibleMessage));
                OnPropertyChanged(nameof(IsError));
                OnPropertyChanged(nameof(IsToolCallNotice));
                OnPropertyChanged(nameof(IsPlainText));
                OnPropertyChanged(nameof(IsMarkdown));
            }
        }
    }

    public string? ToolCallId { get; set; }
    public List<ToolCall>? ToolCalls { get; set; }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RetentionIcon))]
    [NotifyPropertyChangedFor(nameof(RetentionDescription))]
    private ContextRetention _retentionPolicy = ContextRetention.Persistent;

    public string? ReplacementKey { get; set; }

    public string RetentionIcon => RetentionPolicy switch
    {
        ContextRetention.Persistent => "📌",
        ContextRetention.UntilSuperseded => "🔄",
        ContextRetention.NextStepOnly => "➡️",
        ContextRetention.Never => "❌",
        ContextRetention.UntilResolved => "🛑",
        _ => "?"
    };

    public string RetentionDescription => RetentionPolicy switch
    {
        ContextRetention.Persistent => "Always sent in subsequent requests (📌)",
        ContextRetention.UntilSuperseded => "Sent until superseded by a newer version (🔄)",
        ContextRetention.NextStepOnly => "Sent in the next request only (➡️)",
        ContextRetention.Never => "Excluded from requests (❌)",
        ContextRetention.UntilResolved => "Sent until the issue/permission is resolved (🛑)",
        _ => "Unknown policy"
    };

    [RelayCommand]
    private void SetRetention(object parameter)
    {
        if (parameter is ContextRetention policy)
        {
            RetentionPolicy = policy;
        }
        else if (parameter is string str && Enum.TryParse<ContextRetention>(str, out var parsed))
        {
            RetentionPolicy = parsed;
        }
    }
    
    public bool IsUser => Role == ChatRole.User;
    public bool IsAssistant => Role == ChatRole.Assistant;
    public bool IsSystem => Role == ChatRole.System;
    public virtual bool HasContent => !string.IsNullOrWhiteSpace(Content);
    public bool IsVisibleMessage => HasContent;
    public bool IsSystemOrTool => Role == ChatRole.System || Role == ChatRole.Tool;
    public bool IsTool => Role == ChatRole.Tool;
    public virtual bool IsError => Content != null && Content.StartsWith("Error:", StringComparison.OrdinalIgnoreCase);
    public virtual bool IsToolCallNotice => Role == ChatRole.Assistant && Content != null && Content.StartsWith("[Tool call:", StringComparison.Ordinal);
    public virtual bool IsPlainText => IsSystemOrTool || IsError || IsToolCallNotice;
    public virtual bool IsMarkdown => !IsPlainText;

    public MessageViewModel(ChatRole role, string content)
    {
        Role = role;
        _content = content;
    }

    [RelayCommand]
    private async Task CopyAsync()
    {
        var lifetime = global::Avalonia.Application.Current?.ApplicationLifetime as global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
        var clipboard = lifetime?.MainWindow?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(Content);
        }
    }
}
