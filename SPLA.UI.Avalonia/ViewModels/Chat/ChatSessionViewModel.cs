using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Agent;
using SPLA.Domain.Agent;
using SPLA.Domain.Context;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.LLM.LMStudio;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Plugins;
using SPLA.Observability;
using SPLA.UI.Avalonia.ViewModels.Debug;
using SPLA.UI.Avalonia.ViewModels.Messages;
using SPLA.UI.Avalonia.ViewModels.Settings;
using SPLA.UI.Avalonia.ViewModels.Status;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.ViewModels.Chat;

public partial class ChatSessionViewModel : ViewModelBase
{
    // ── Dependencies ─────────────────────────────────────────────────────────
    private readonly MainWindowViewModel _owner;
    private readonly LMStudioClient _llmClient;
    private readonly McpHost _mcpHost;
    private readonly SkillManager _skillManager;
    private readonly PluginManager _pluginManager;
    private readonly SkillSession _skillSession;
    private readonly Func<AgentMode, List<ToolDefinition>> _toolFilter;
    private readonly CheckpointManager _checkpointManager;

    private ChatManager? _chatManager;
    private ChatSession? _currentChat;
    private string? _loadedChatId;
    private CancellationTokenSource? _currentRequestCts;
    private DispatcherTimer? _tokenEstimateTimer;
    private DateTime _tokenEstimateStartedAt;
    private readonly Conversation _conversation = new();
    private readonly KeyValueStore _sessionKv = new("session");

    // ── Forwarded singletons ─────────────────────────────────────────────────
    public SettingsViewModel Settings { get; }
    public StatusViewModel Status { get; }
    public ContextSnapshotViewModel ContextSnapshot { get; } = new();
    public IKeyValueStore SessionKv => _sessionKv;

    /// <summary>Invoked after every save so the chat list in the sidebar refreshes.</summary>
    public Action? OnChatSaved { get; set; }

    // ── Observable session state ─────────────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<MessageViewModel> _messages = new();

    [ObservableProperty]
    private string _inputText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveClarify))]
    private ClarifyViewModel? _activeClarify;

    public bool HasActiveClarify => ActiveClarify is not null;

    // ── Render mode and display profile (per-session) ─────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNativeChatViewSelected))]
    [NotifyPropertyChangedFor(nameof(IsWebChatViewSelected))]
    private string _activeRenderMode = "native";

    public bool IsNativeChatViewSelected => ActiveRenderMode == "native";
    public bool IsWebChatViewSelected    => ActiveRenderMode == "web";

    partial void OnActiveRenderModeChanged(string value)
    {
        if (Settings.ChatRenderMode != value)
        {
            Settings.ChatRenderMode = value;
            _ = Settings.SaveSettingsAsync();
        }
    }

    [ObservableProperty]
    private ChatProfileViewModel? _selectedProfile;

    public ChatDisplayProfile? ActiveProfile         => SelectedProfile?.Profile;
    public bool ActiveProfileUsesBubbles             => SelectedProfile?.Profile.UseBubbleLayout == true;
    public bool ActiveProfileUsesLinear              => SelectedProfile?.Profile.UseBubbleLayout != true;

    partial void OnSelectedProfileChanged(ChatProfileViewModel? value)
    {
        OnPropertyChanged(nameof(ActiveProfile));
        OnPropertyChanged(nameof(ActiveProfileUsesBubbles));
        OnPropertyChanged(nameof(ActiveProfileUsesLinear));
        if (value != null && Settings.ActiveProfileId != value.Id)
        {
            Settings.ActiveProfileId = value.Id;
            _ = Settings.SaveSettingsAsync();
        }
    }

    public ObservableCollection<ChatProfileViewModel> AvailableProfiles => _owner.AvailableProfiles;
    public IRelayCommand ForkChatCommand => _owner.ForkChatCommand;

    [RelayCommand]
    private void SelectProfile(ChatProfileViewModel? profile)
    {
        if (profile == null || profile == SelectedProfile) return;
        if (SelectedProfile != null) SelectedProfile.IsSelected = false;
        profile.IsSelected = true;
        SelectedProfile = profile;
    }

    [RelayCommand]
    private void SelectRenderMode(string? mode)
    {
        if (string.IsNullOrEmpty(mode) || mode == ActiveRenderMode) return;
        ActiveRenderMode = mode;
    }

    public void InitializeSessionDefaults()
    {
        ActiveRenderMode = Settings.ChatRenderMode;

        var targetProfile = AvailableProfiles.FirstOrDefault(p => p.Id == Settings.ActiveProfileId)
                         ?? AvailableProfiles.FirstOrDefault();
        if (targetProfile != null)
        {
            targetProfile.IsSelected = true;
            SelectedProfile = targetProfile;
        }
    }

    // ────────────────────────────────────────────────────────────────────────

    public ChatSessionViewModel(
        MainWindowViewModel owner,
        LMStudioClient llmClient,
        McpHost mcpHost,
        SkillManager skillManager,
        PluginManager pluginManager,
        SkillSession skillSession,
        SettingsViewModel settings,
        StatusViewModel status,
        Func<AgentMode, List<ToolDefinition>> toolFilter,
        CheckpointManager checkpointManager)
    {
        _owner        = owner;
        _llmClient    = llmClient;
        _mcpHost      = mcpHost;
        _skillManager = skillManager;
        _pluginManager = pluginManager;
        _skillSession = skillSession;
        Settings      = settings;
        Status        = status;
        _toolFilter   = toolFilter;
        _checkpointManager = checkpointManager;

        owner.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(MainWindowViewModel.AvailableProfiles))
                OnPropertyChanged(e.PropertyName);
        };
    }

    public void SetChatManager(ChatManager chatManager) => _chatManager = chatManager;

    // ── Session loading ──────────────────────────────────────────────────────

    public void LoadSession(ChatSession session)
    {
        if (_loadedChatId == session.Id && Messages.Count > 0) return;

        _loadedChatId = session.Id;
        _currentChat  = session;
        ClearInternal();

        var loaded = _chatManager?.LoadChat(session.Id);
        if (loaded != null)
        {
            session.Messages  = loaded.Messages;
            session.Title     = loaded.Title;
            session.UpdatedAt = loaded.UpdatedAt;
        }

        _sessionKv.LoadFrom(loaded?.Kv ?? new Dictionary<string, string>());

        var resolved = App.ResolvedSettings;
        if (resolved.ProjectName != null)
            Messages.Add(new SystemMessageViewModel($"Project: {resolved.ProjectName} | Mode: {resolved.Mode}"));

        var promptBuilder = new SystemPromptBuilder(_skillManager, _pluginManager, _skillSession);
        var systemPrompt  = promptBuilder.Build(resolved, Directory.GetCurrentDirectory());

        var sysMsg = new ChatMessage { Role = ChatRole.System, Content = systemPrompt };
        _conversation.Add(sysMsg);
        Messages.Add(new SystemMessageViewModel(systemPrompt, isSystemPrompt: true) { Domain = sysMsg });

        foreach (var m in session.Messages)
        {
            var role = m.Role.ToLower() switch
            {
                "user"      => ChatRole.User,
                "assistant" => ChatRole.Assistant,
                "tool"      => ChatRole.Tool,
                _           => ChatRole.System
            };
            var domainMsg = new ChatMessage
            {
                Role      = role,
                Content   = m.Content,
                Reasoning = string.IsNullOrEmpty(m.Reasoning) ? null : m.Reasoning
            };
            _conversation.Add(domainMsg);

            MessageViewModel vm = m.Role.ToLower() switch
            {
                "user"      => new UserMessageViewModel(m.Content),
                "assistant" => new AssistantMessageViewModel(m.Content),
                "tool"      => new MessageViewModel(ChatRole.Tool, m.Content),
                _           => new SystemMessageViewModel(m.Content)
            };
            if (!string.IsNullOrEmpty(m.Reasoning)) vm.Reasoning = m.Reasoning;
            vm.Domain = domainMsg;
            Messages.Add(vm);
        }
    }

    public void ClearSession()
    {
        _currentChat  = null;
        _loadedChatId = null;
        ClearInternal();
    }

    private void ClearInternal()
    {
        foreach (var m in Messages)
        {
            if (m is StreamingMessageViewModel sm) sm.SetFinal(string.Empty);
            else m.Content = string.Empty;
        }
        Messages.Clear();
        _conversation.Clear();
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsBusy) return;

        var userText = InputText;
        InputText = "";
        IsBusy = true;

        if (userText.Trim().ToLower() == "/compact")
        {
            await CompactContextAsync();
            IsBusy = false;
            return;
        }

        if (userText.Trim().Equals("/skills", StringComparison.OrdinalIgnoreCase))
        {
            var all   = _skillManager.GetAll();
            var lines = all.Count == 0
                ? "No skills available."
                : string.Join("\n", all.Select(s => $"  [{(s.IsEnabled ? "on " : "off")}] {s.Id} — {s.Description}"));
            Messages.Add(new SystemMessageViewModel($"**Skills:**\n{lines}"));
            IsBusy = false;
            return;
        }

        if (userText.TrimStart().StartsWith("/skills load ", StringComparison.OrdinalIgnoreCase))
        {
            var id   = userText.TrimStart()["/skills load ".Length..].Trim();
            var body = _skillManager.LoadBody(id);
            if (body == null)
            {
                Messages.Add(new SystemMessageViewModel($"Skill '{id}' not found."));
            }
            else
            {
                var skillContent = $"[Skill loaded: {id}]\n\n{body}";
                var skillMsg     = new ChatMessage { Role = ChatRole.User, Content = skillContent };
                _conversation.Add(skillMsg);
                Messages.Add(new UserMessageViewModel(skillContent) { Domain = skillMsg });
                SaveChat();
                Messages.Add(new SystemMessageViewModel($"Skill '{id}' loaded into context."));
            }
            IsBusy = false;
            return;
        }

        var userMsg = new ChatMessage { Role = ChatRole.User, Content = userText };
        _conversation.Add(userMsg);
        Messages.Add(new UserMessageViewModel(userText) { Domain = userMsg });
        SaveChat();

        _currentRequestCts?.Dispose();
        _currentRequestCts = new CancellationTokenSource();

        using var telemetryContext = SplaTelemetry.PushContext(new(
            ConversationId: _currentChat?.Id,
            RequestId:      Guid.NewGuid().ToString("N"),
            ProjectId:      App.ResolvedSettings.ProjectName,
            WorkspacePath:  App.ResolvedSettings.WorkspacePath));

        await Task.Run(async () => await ProcessConversationAsync(_currentRequestCts.Token));
    }

    [RelayCommand]
    private void Stop()
    {
        if (!IsBusy) return;
        _currentRequestCts?.Cancel();
    }

    [RelayCommand]
    private async Task DeleteMessageAsync(MessageViewModel? message)
    {
        if (message == null || !Messages.Contains(message)) return;

        if (message is StreamingMessageViewModel sm) sm.SetFinal(string.Empty);
        else message.Content = string.Empty;

        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        if (!Messages.Remove(message)) return;
        if (message.Domain != null)
        {
            // If the deleted message was a checkpoint anchor, clear it so stale rollbacks don't fire.
            if (message.Domain.MsgId == _checkpointManager.RestoreAnchorId)
                _checkpointManager.Confirm(); // clears RestoreRequested; AnchorId remains but stack pop already happened
            _conversation.Remove(message.Domain);
        }

        SaveChat();
    }

    [RelayCommand]
    private async Task CompactContextAsync()
    {
        if (_chatManager == null || _currentChat == null) return;

        Messages.Add(new MessageViewModel(ChatRole.System, "Compacting context..."));

        try
        {
            _chatManager.SaveBackup(BuildVisibleChatSnapshot(), "before_compact");

            var compressionPrompt = new ChatMessage
            {
                Role    = ChatRole.System,
                Content = "Compress the conversation into a working summary to continue development.\nKeep only facts, decisions, constraints, open questions, modified files, commands, and the next step.\nDo not keep chit-chat. Respond in Markdown format."
            };

            var coreMessages = Messages
                .Where(m => !m.IsSystemOrTool && !m.IsToolCallNotice && m.HasContent)
                .Select(m => new ChatMessage { Role = m.Role, Content = m.Content })
                .ToList();
            coreMessages.Insert(0, compressionPrompt);

            var responseMsg = await _llmClient.SendMessageAsync(coreMessages, Settings.GetSettings(), new List<ToolDefinition>());
            var summary     = responseMsg.Content ?? "Compression failed.";
            _chatManager.SaveSummary(_currentChat.Id, summary);

            var resolved     = App.ResolvedSettings;
            var tailMessages = Messages.Where(m => m.IsUser || m.IsAssistant).TakeLast(resolved.CompactTailMessages).ToList();

            ClearInternal();

            if (resolved.ProjectName != null)
                Messages.Add(new SystemMessageViewModel($"Project: {resolved.ProjectName} | Mode: {resolved.Mode}"));

            var summaryContent = "--- Compressed Context (Summary) ---\n" + summary;
            var summaryDomain  = new ChatMessage { Role = ChatRole.System, Content = summaryContent };
            _conversation.Add(summaryDomain);
            Messages.Add(new MessageViewModel(ChatRole.System, summaryContent) { Domain = summaryDomain });

            foreach (var tm in tailMessages)
            {
                Messages.Add(tm);
                if (tm.Domain != null) _conversation.Add(tm.Domain);
            }

            Messages.Add(new SystemMessageViewModel("Context compacted. Full history saved in YAML. The agent will use the summary and recent messages going forward."));
            SaveChat();
        }
        catch (Exception ex)
        {
            Messages.Add(new MessageViewModel(ChatRole.System, $"Compression error: {ex.Message}"));
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    internal void SaveChat()
    {
        if (_chatManager == null || _currentChat == null) return;

        _currentChat.Kv = _sessionKv.Snapshot();
        _currentChat.Messages.Clear();
        foreach (var msg in _conversation.Persistable)
        {
            _currentChat.Messages.Add(new ChatSessionMessage
            {
                Role      = msg.Role.ToString().ToLower(),
                Content   = msg.Content ?? "",
                Reasoning = string.IsNullOrEmpty(msg.Reasoning) ? null : msg.Reasoning
            });
        }

        _chatManager.SaveChat(_currentChat);
        OnChatSaved?.Invoke();
    }

    private ChatSession BuildVisibleChatSnapshot()
    {
        var snapshot = new ChatSession
        {
            Id        = _currentChat?.Id ?? "",
            Title     = _currentChat?.Title ?? "Chat",
            CreatedAt = _currentChat?.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Workspace = _currentChat?.Workspace ?? App.ResolvedSettings.WorkspacePath,
            Model     = _currentChat?.Model,
            Agent     = _currentChat?.Agent,
            Context   = _currentChat?.Context
        };

        foreach (var message in Messages.Where(m => m.HasContent && m is not PermissionMessageViewModel))
        {
            snapshot.Messages.Add(new ChatSessionMessage
            {
                Role    = message.Role.ToString().ToLowerInvariant(),
                Content = message.Content
            });
        }

        return snapshot;
    }

    private IReadOnlyList<(string scope, string key, string value)> CollectWorkingMemory()
        => _sessionKv.List().Select(kv => (_sessionKv.Scope, kv.Key, kv.Value))
            .Concat(_owner.ProjectKv.List().Select(kv => (_owner.ProjectKv.Scope, kv.Key, kv.Value)))
            .ToList();

    // ── Conversation loop ────────────────────────────────────────────────────

    private async Task ProcessConversationAsync(CancellationToken cancellationToken)
    {
        StreamingMessageViewModel? streamingVm = null;

        const int FlushIntervalMs = 80;
        var pendingDelta     = new System.Text.StringBuilder();
        var pendingReasoning = new System.Text.StringBuilder();
        var lastFlush          = DateTime.UtcNow;
        var lastReasoningFlush = DateTime.UtcNow;

        int toolBatchTotal = 0;
        int toolBatchIdx   = 0;

        const int ProgressFlushIntervalMs = 120;
        var lastProgressFlush = DateTime.MinValue;

        try
        {
            var orchestrator = new ConversationOrchestrator(_llmClient, _mcpHost)
            {
                ToolFilter    = (_, mode) => _toolFilter(mode),
                WorkingMemory = CollectWorkingMemory,
                Checkpoint    = _checkpointManager
            };

            var callbacks = new AgentCallbacks
            {
                OnLlmTurnStart = async ctx =>
                {
                    ContextSnapshot.Capture(ctx, _conversation.Messages);
                    pendingDelta     = new System.Text.StringBuilder();
                    pendingReasoning = new System.Text.StringBuilder();
                    lastFlush          = DateTime.UtcNow;
                    lastReasoningFlush = DateTime.UtcNow;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        StartTokenEstimate(ctx.Select(m => m.Content ?? ""));
                        streamingVm = new StreamingMessageViewModel();
                        Messages.Add(streamingVm);
                    });
                },

                OnReasoning = async chunk =>
                {
                    pendingReasoning.Append(chunk);
                    if ((DateTime.UtcNow - lastReasoningFlush).TotalMilliseconds >= FlushIntervalMs)
                    {
                        var toFlush = pendingReasoning.ToString();
                        pendingReasoning.Clear();
                        lastReasoningFlush = DateTime.UtcNow;
                        var vm = streamingVm;
                        if (vm != null)
                            await Dispatcher.UIThread.InvokeAsync(() => vm.AppendReasoning(toFlush));
                    }
                },

                OnDelta = async chunk =>
                {
                    pendingDelta.Append(chunk);
                    if ((DateTime.UtcNow - lastFlush).TotalMilliseconds >= FlushIntervalMs)
                    {
                        var toFlush = pendingDelta.ToString();
                        pendingDelta.Clear();
                        lastFlush = DateTime.UtcNow;
                        var vm = streamingVm;
                        if (vm != null)
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                vm.Append(toFlush);
                                Status.UpdateActiveCompletionTokens(EstimateTokens(new[] { vm.StreamingContent }));
                            });
                    }
                },

                OnAssistantMessage = async msg =>
                {
                    toolBatchTotal = msg.ToolCalls?.Count ?? 0;
                    toolBatchIdx   = 0;

                    var vm = streamingVm;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (vm != null)
                        {
                            if (pendingDelta.Length > 0)     { vm.Append(pendingDelta.ToString());             pendingDelta.Clear(); }
                            if (pendingReasoning.Length > 0) { vm.AppendReasoning(pendingReasoning.ToString()); pendingReasoning.Clear(); }
                        }

                        StopTokenEstimate();
                        Status.AddTokens(msg.PromptTokens, msg.CompletionTokens);

                        if (vm != null)
                        {
                            vm.Domain = msg;
                            if (!string.IsNullOrEmpty(msg.Content))   vm.SetFinal(msg.Content);
                            if (!string.IsNullOrEmpty(msg.Reasoning)) vm.SetFinalReasoning(msg.Reasoning);
                            if (!vm.HasContent && !vm.HasReasoning) Messages.Remove(vm);
                            else SaveChat();
                        }
                        streamingVm = null;
                    });
                },

                OnToolCallStarted = async tc =>
                {
                    toolBatchIdx++;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        var label = toolBatchTotal > 1
                            ? $"⏳ {tc.Function.Name}  {toolBatchIdx}/{toolBatchTotal}"
                            : $"⏳ {tc.Function.Name}";
                        Status.SetActiveOperation(label);
                        RestartElapsedTimer();
                        lastProgressFlush = DateTime.MinValue;
                        Messages.Add(new MessageViewModel(ChatRole.Assistant, $"[Tool call: {tc.Function.Name}]")
                        {
                            ToolCalls = new List<ToolCall> { tc }
                        });
                    });
                },

                OnToolProgress = (tc, progress) =>
                {
                    var now      = DateTime.UtcNow;
                    var complete = progress.Fraction >= 1.0;
                    if (!complete && (now - lastProgressFlush).TotalMilliseconds < ProgressFlushIntervalMs) return;
                    lastProgressFlush = now;
                    Dispatcher.UIThread.Post(() => Status.ReportProgress(tc.Function.Name, progress));
                },

                OnToolResult = async (tc, result) =>
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        var remaining = toolBatchTotal - toolBatchIdx;
                        var label     = remaining > 0
                            ? $"✓ {tc.Function.Name}  ·  {remaining} left"
                            : $"✓ {tc.Function.Name}";
                        Status.SetActiveOperation(label);
                        var domainMsg = _conversation.Messages.LastOrDefault(
                            m => m.Role == ChatRole.Tool && m.ToolCallId == tc.Id);
                        Messages.Add(new MessageViewModel(ChatRole.Tool, result) { ToolCallId = tc.Id, Domain = domainMsg });
                        SaveChat();
                    });
                },

                OnNotice = async note =>
                {
                    await Dispatcher.UIThread.InvokeAsync(() => Messages.Add(new SystemMessageViewModel(note)));
                }
            };

            using var clarifyScope = ClarifyScope.Begin(async request =>
            {
                var vm = new ClarifyViewModel(request);
                await Dispatcher.UIThread.InvokeAsync(() => ActiveClarify = vm);
                try   { return await vm.AnswerTask; }
                finally { await Dispatcher.UIThread.InvokeAsync(() => ActiveClarify = null); }
            });

            await orchestrator.RunAsync(_conversation, Settings.GetSettings(), Settings.Mode, callbacks, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (streamingVm != null && !streamingVm.HasContent) Messages.Remove(streamingVm);
                Messages.Add(new SystemMessageViewModel("Stopped."));
                SaveChat();
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (streamingVm != null && !streamingVm.HasContent) Messages.Remove(streamingVm);
                Messages.Add(new SystemMessageViewModel($"Error: {ex.Message}"));
            });
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StopTokenEstimate();
                IsBusy = false;
                _currentRequestCts?.Dispose();
                _currentRequestCts = null;
            });
        }
    }

    // ── Token estimation ─────────────────────────────────────────────────────

    private void StartTokenEstimate(IEnumerable<string> promptContents)
    {
        Status.BeginActiveTokens(EstimateTokens(promptContents));
        RestartElapsedTimer();
    }

    private void RestartElapsedTimer()
    {
        _tokenEstimateStartedAt = DateTime.UtcNow;
        _tokenEstimateTimer?.Stop();
        _tokenEstimateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _tokenEstimateTimer.Tick += TokenEstimateTimer_Tick;
        _tokenEstimateTimer.Start();
    }

    private void TokenEstimateTimer_Tick(object? sender, EventArgs e)
    {
        if (!IsBusy) return;
        Status.UpdateActiveElapsed(DateTime.UtcNow - _tokenEstimateStartedAt);
    }

    private void StopTokenEstimate()
    {
        if (_tokenEstimateTimer != null)
        {
            _tokenEstimateTimer.Stop();
            _tokenEstimateTimer.Tick -= TokenEstimateTimer_Tick;
            _tokenEstimateTimer = null;
        }
        Status.ClearActiveTokens();
    }

    private static int EstimateTokens(IEnumerable<string> contents)
    {
        var chars = contents.Sum(x => string.IsNullOrEmpty(x) ? 0 : x.Length);
        return Math.Max(1, (int)Math.Ceiling(chars / 4.0));
    }
}
