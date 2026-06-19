using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Agent;
using SPLA.Domain.Agent;
using SPLA.Domain.Context;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Permissions;
using SPLA.Observability;
using SPLA.UI.Avalonia.ViewModels.Debug;
using SPLA.UI.Avalonia.ViewModels.Messages;
using SPLA.UI.Avalonia.ViewModels.Status;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.ViewModels.Chat;

/// <summary>
/// One fully-autonomous chat. Owns everything about itself: its messages, input, run-state and
/// cancellation, its working memory / checkpoint / skill session (exposed via <see cref="IAgentSession"/>),
/// its connection choice and behaviour knobs, its live status line, and its own modal prompts
/// (permission + clarify). Multiple instances run concurrently in the background — each opens its own
/// ambient scopes around its conversation loop so tool calls never cross between chats.
/// </summary>
public partial class ChatSessionViewModel : ViewModelBase, IAgentSession
{
    // ── Shared infrastructure + owner ────────────────────────────────────────
    private readonly MainWindowViewModel _owner;
    private readonly ChatServices _services;

    // ── This chat's own agent state (IAgentSession) ──────────────────────────
    private readonly ChatSession _chat;
    private readonly Conversation _conversation = new();
    private readonly KeyValueStore _sessionKv = new("session");
    private readonly CheckpointManager _checkpointManager = new();
    private readonly SkillSession _skillSession = new();

    public IKeyValueStore SessionKv => _sessionKv;
    MarkManager IAgentSession.Checkpoint => _checkpointManager;
    ISkillSession IAgentSession.Skills => _skillSession;

    private CancellationTokenSource? _currentRequestCts;
    private DispatcherTimer? _tokenEstimateTimer;
    private DateTime _tokenEstimateStartedAt;
    private bool _loadStarted;
    private bool _loaded;
    private int _pendingPermissions;

    public string Id => _chat.Id;
    public ChatSession Chat => _chat;

    // ── Per-chat singletons ──────────────────────────────────────────────────
    public StatusViewModel Status { get; } = new();
    public ContextSnapshotViewModel ContextSnapshot { get; } = new();

    /// <summary>Invoked after every save so the sidebar can refresh ordering/titles.</summary>
    public Action? OnChatSaved { get; set; }

    // ── Observable session state ─────────────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<MessageViewModel> _messages = new();

    [ObservableProperty]
    private string _title = "New Chat";

    [ObservableProperty]
    private string _inputText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(IsRunning))]
    [NotifyPropertyChangedFor(nameof(StateGlyph))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveClarify))]
    [NotifyPropertyChangedFor(nameof(IsAwaitingUser))]
    [NotifyPropertyChangedFor(nameof(StateGlyph))]
    private ClarifyViewModel? _activeClarify;

    public bool HasActiveClarify => ActiveClarify is not null;

    // ── Sidebar state indicator ──────────────────────────────────────────────
    public bool IsRunning => IsBusy;
    public bool IsAwaitingUser => HasActiveClarify || _pendingPermissions > 0;

    /// <summary>Glyph the chat list shows next to this chat: "❓" waiting on the user, "●" working.</summary>
    public string StateGlyph => IsAwaitingUser ? "❓" : IsBusy ? "●" : "";

    // ── Connection + behaviour knobs (per chat) ──────────────────────────────
    public ObservableCollection<SplaConnectionSection> Connections { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedModelSupportsReasoning))]
    [NotifyPropertyChangedFor(nameof(SelectedModelReasoningOptions))]
    private SplaConnectionSection? _selectedConnection;

    [ObservableProperty]
    private double _temperature = 0.7;

    [ObservableProperty]
    private string _reasoningLevel = "";

    private ModelInfo? CurrentModelInfo =>
        _services.ModelCatalog().FirstOrDefault(m => m.Id == SelectedConnection?.Model);

    public bool SelectedModelSupportsReasoning => CurrentModelInfo?.SupportsReasoning ?? false;
    public ObservableCollection<string> SelectedModelReasoningOptions =>
        new(CurrentModelInfo?.ReasoningOptions ?? new());

    partial void OnSelectedConnectionChanged(SplaConnectionSection? value)
    {
        if (value != null) _chat.ConnectionId = value.Id;
        // Snap reasoning to the new model's default (or clear when unsupported).
        var info = CurrentModelInfo;
        ReasoningLevel = info is { SupportsReasoning: true }
            ? (string.IsNullOrEmpty(info.ReasoningDefault) ? info.ReasoningOptions[0] : info.ReasoningDefault)
            : "";
        if (_loaded) SaveChat();
    }

    partial void OnTemperatureChanged(double value) { if (_loaded) SaveChat(); }
    partial void OnReasoningLevelChanged(string value) { if (_loaded) SaveChat(); }

    // ── Render mode + display profile (per chat) ─────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNativeChatViewSelected))]
    [NotifyPropertyChangedFor(nameof(IsWebChatViewSelected))]
    private string _activeRenderMode = "native";

    public bool IsNativeChatViewSelected => ActiveRenderMode == "native";
    public bool IsWebChatViewSelected    => ActiveRenderMode == "web";

    [ObservableProperty]
    private ChatProfileViewModel? _selectedProfile;

    public ChatDisplayProfile? ActiveProfile  => SelectedProfile?.Profile;
    public bool ActiveProfileUsesBubbles      => SelectedProfile?.Profile.UseBubbleLayout == true;
    public bool ActiveProfileUsesLinear       => SelectedProfile?.Profile.UseBubbleLayout != true;

    partial void OnSelectedProfileChanged(ChatProfileViewModel? value)
    {
        OnPropertyChanged(nameof(ActiveProfile));
        OnPropertyChanged(nameof(ActiveProfileUsesBubbles));
        OnPropertyChanged(nameof(ActiveProfileUsesLinear));
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

    // ────────────────────────────────────────────────────────────────────────

    public ChatSessionViewModel(MainWindowViewModel owner, ChatServices services, ChatSession chat)
    {
        _owner    = owner;
        _services = services;
        _chat     = chat;
        _title    = chat.Title;

        Status.AttachSkillSession(_skillSession);

        // Persist per-chat mode when the user changes it via the bottom-bar combo.
        Status.PropertyChanged += (_, e) =>
        {
            if (_loaded && e.PropertyName == nameof(StatusViewModel.Mode)) SaveChat();
        };

        owner.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(MainWindowViewModel.AvailableProfiles))
                OnPropertyChanged(nameof(AvailableProfiles));
        };
    }

    /// <summary>Builds the LLM request settings from this chat's selected connection + knobs.</summary>
    private LLMSettings BuildLlmSettings()
    {
        var conn  = SelectedConnection;
        var model = conn?.Model;
        return new LLMSettings
        {
            BaseUrl        = string.IsNullOrWhiteSpace(conn?.Endpoint) ? "http://127.0.0.1:1234/v1/" : conn!.Endpoint!,
            ApiKey         = string.IsNullOrWhiteSpace(conn?.ApiKey) ? "lm-studio" : conn!.ApiKey!,
            ModelName      = string.IsNullOrWhiteSpace(model) || model == "auto" ? "local-model" : model!,
            Temperature    = Temperature,
            Mode           = Status.Mode,
            Theme          = App.ResolvedSettings.Theme,
            ReasoningLevel = string.IsNullOrEmpty(ReasoningLevel) ? null : ReasoningLevel
        };
    }

    // ── Loading ──────────────────────────────────────────────────────────────

    /// <summary>Loads this chat's persisted state into the view-model. Idempotent.</summary>
    public void Load()
    {
        // _loadStarted guards re-entry; _loaded stays false through the body so the property setters
        // below (connection/temp/mode) do NOT trigger SaveChat before messages are restored.
        if (_loadStarted) return;
        _loadStarted = true;

        var resolved = App.ResolvedSettings;

        // Connection list + this chat's selection (live reference; fall back to the first).
        Connections.Clear();
        foreach (var c in resolved.Connections) Connections.Add(c);
        SelectedConnection = Connections.FirstOrDefault(c => c.Id == _chat.ConnectionId)
                          ?? Connections.FirstOrDefault();

        // Behaviour knobs.
        Temperature   = _chat.Model?.Temperature ?? resolved.Temperature;
        ReasoningLevel = _chat.Model?.ReasoningLevel ?? resolved.ReasoningLevel ?? "";
        Status.Mode = _chat.Agent?.Mode != null && Enum.TryParse<AgentMode>(_chat.Agent.Mode, true, out var m)
            ? m : resolved.Mode;

        // Render mode + profile default from resolved settings (per-chat, in-memory).
        ActiveRenderMode = resolved.ChatRenderMode;
        var targetProfile = AvailableProfiles.FirstOrDefault(p => p.Id == resolved.ActiveProfileId)
                         ?? AvailableProfiles.FirstOrDefault();
        if (targetProfile != null)
        {
            targetProfile.IsSelected = true;
            SelectedProfile = targetProfile;
        }

        Title = _chat.Title;

        // Restore the chat from disk.
        var loaded = _services.ChatManager.LoadChat(_chat.Id);
        if (loaded != null)
        {
            _chat.Messages  = loaded.Messages;
            _chat.Title     = loaded.Title;
            _chat.UpdatedAt = loaded.UpdatedAt;
            Title = loaded.Title;
        }

        _sessionKv.LoadFrom(loaded?.Kv ?? _chat.Kv);

        if (resolved.ProjectName != null)
            Messages.Add(new SystemMessageViewModel($"Project: {resolved.ProjectName} | Mode: {Status.Mode}"));

        var promptBuilder = new SystemPromptBuilder(_services.SkillManager, _services.PluginManager, _skillSession);
        var systemPrompt  = promptBuilder.Build(resolved, Directory.GetCurrentDirectory());

        var sysMsg = new ChatMessage { Role = ChatRole.System, Content = systemPrompt };
        _conversation.Add(sysMsg);
        Messages.Add(new SystemMessageViewModel(systemPrompt, isSystemPrompt: true) { Domain = sysMsg });

        foreach (var msg in _chat.Messages)
        {
            var role = msg.Role.ToLower() switch
            {
                "user"      => ChatRole.User,
                "assistant" => ChatRole.Assistant,
                "tool"      => ChatRole.Tool,
                _           => ChatRole.System
            };
            var domainMsg = new ChatMessage
            {
                Role      = role,
                Content   = msg.Content,
                Reasoning = string.IsNullOrEmpty(msg.Reasoning) ? null : msg.Reasoning
            };
            _conversation.Add(domainMsg);

            MessageViewModel vm = msg.Role.ToLower() switch
            {
                "user"      => new UserMessageViewModel(msg.Content),
                "assistant" => new AssistantMessageViewModel(msg.Content),
                "tool"      => new MessageViewModel(ChatRole.Tool, msg.Content),
                _           => new SystemMessageViewModel(msg.Content)
            };
            if (!string.IsNullOrEmpty(msg.Reasoning)) vm.Reasoning = msg.Reasoning;
            vm.Domain = domainMsg;
            Messages.Add(vm);
        }

        // From here on, user-driven knob changes persist.
        _loaded = true;
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
            var all   = _services.SkillManager.GetAll();
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
            var body = _services.SkillManager.LoadBody(id);
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
            ConversationId: _chat.Id,
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

    /// <summary>Cancels any in-flight run and stops timers. Called when the chat is deleted.</summary>
    internal void Shutdown()
    {
        _currentRequestCts?.Cancel();
        StopTokenEstimate();
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
            if (message.Domain.MsgId == _checkpointManager.RestoreAnchorId)
                _checkpointManager.Confirm();
            _conversation.Remove(message.Domain);
        }

        SaveChat();
    }

    [RelayCommand]
    private async Task CompactContextAsync()
    {
        Messages.Add(new MessageViewModel(ChatRole.System, "Compacting context..."));

        try
        {
            _services.ChatManager.SaveBackup(BuildVisibleChatSnapshot(), "before_compact");

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

            var responseMsg = await _services.Llm.SendMessageAsync(coreMessages, BuildLlmSettings(), new List<ToolDefinition>());
            var summary     = responseMsg.Content ?? "Compression failed.";
            _services.ChatManager.SaveSummary(_chat.Id, summary);

            var resolved     = App.ResolvedSettings;
            var tailMessages = Messages.Where(m => m.IsUser || m.IsAssistant).TakeLast(resolved.CompactTailMessages).ToList();

            ClearInternal();

            if (resolved.ProjectName != null)
                Messages.Add(new SystemMessageViewModel($"Project: {resolved.ProjectName} | Mode: {Status.Mode}"));

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

    // ── Persistence ──────────────────────────────────────────────────────────

    internal void SaveChat()
    {
        _chat.ConnectionId = SelectedConnection?.Id ?? _chat.ConnectionId;
        _chat.Model = new SplaLlmSection
        {
            Temperature    = Temperature,
            ReasoningLevel = string.IsNullOrEmpty(ReasoningLevel) ? null : ReasoningLevel
        };
        _chat.Agent = new SplaAgentSection { Mode = Status.Mode.ToString() };
        _chat.Title = Title;
        _chat.Kv = _sessionKv.Snapshot();

        _chat.Messages.Clear();
        foreach (var msg in _conversation.Persistable)
        {
            _chat.Messages.Add(new ChatSessionMessage
            {
                Role      = msg.Role.ToString().ToLower(),
                Content   = msg.Content ?? "",
                Reasoning = string.IsNullOrEmpty(msg.Reasoning) ? null : msg.Reasoning
            });
        }

        _services.ChatManager.SaveChat(_chat);
        OnChatSaved?.Invoke();
    }

    private ChatSession BuildVisibleChatSnapshot()
    {
        var snapshot = new ChatSession
        {
            Id        = _chat.Id,
            Title     = _chat.Title,
            CreatedAt = _chat.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            Workspace = _chat.Workspace,
            Model     = _chat.Model,
            Agent     = _chat.Agent,
            Context   = _chat.Context
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
            .Concat(_services.ProjectKv.List().Select(kv => (_services.ProjectKv.Scope, kv.Key, kv.Value)))
            .ToList();

    // ── Modal prompts (per chat) ─────────────────────────────────────────────

    private async Task<PermissionDecision> HandlePermissionRequestAsync(ToolFunctionDefinition def, string args)
    {
        var tcs = new TaskCompletionSource<PermissionDecision>();
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _pendingPermissions++;
            OnPropertyChanged(nameof(IsAwaitingUser));
            OnPropertyChanged(nameof(StateGlyph));
            Messages.Add(new PermissionMessageViewModel(def, args, tcs));
        });

        try
        {
            var decision = await tcs.Task;
            if (decision is PermissionDecision.AllowRemember or PermissionDecision.Deny)
                _services.PersistPermission(def, args, decision);
            return decision;
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_pendingPermissions > 0) _pendingPermissions--;
                OnPropertyChanged(nameof(IsAwaitingUser));
                OnPropertyChanged(nameof(StateGlyph));
            });
        }
    }

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
            var orchestrator = new ConversationOrchestrator(_services.Llm, _services.McpHost)
            {
                ToolFilter    = (_, mode) => _services.ToolFilter(mode),
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

            // Route this chat's tool state and permission prompts to itself, even when several
            // chats run concurrently in the background (AsyncLocal flows on this run's async chain).
            using var agentScope      = AgentSessionScope.Begin(this);
            using var permissionScope = PermissionScope.Begin(HandlePermissionRequestAsync);

            await orchestrator.RunAsync(_conversation, BuildLlmSettings(), Status.Mode, callbacks, cancellationToken);
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
