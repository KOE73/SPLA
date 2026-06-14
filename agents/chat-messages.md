# Chat Message Architecture

This document defines the canonical design for the SPLA chat message system in `SPLA.UI.Avalonia`.
All agents and contributors must follow these conventions when adding, modifying, or rendering chat messages.

## 1. Core problem statement

The original implementation had three critical flaws:

**Flaw 1 — Type via boolean flags (anti-pattern)**
Message type was determined at runtime by inspecting string content:
```csharp
public bool IsError => Content?.StartsWith("Error:") ?? false;
public bool IsToolCallNotice => Role == ChatRole.Assistant && Content?.StartsWith("[Tool call:") ?? false;
public bool IsPlainText => IsSystemOrTool || IsError || IsToolCallNotice;
```
This is brittle: any refactor of string prefixes silently breaks rendering. It also means the view
cannot know at bind-time what template to use without evaluating all booleans.

**Flaw 2 — Streaming scroll bug**
All three native views subscribed to `CollectionChanged` for auto-scroll to end.
When a `StreamingMessageViewModel` receives a chunk via `Append()`, the collection does NOT change —
only a property on an existing item changes. The scroll does not follow, causing the view to freeze
at an arbitrary position while content grows below. Simultaneously, `MarkdownScrollViewer` forces
a full layout pass on each `PropertyChanged`, producing visible jitter.

**Flaw 3 — No tool execution progress model**
Tool calls existed only as a `List<ToolCall>` on `AssistantMessageViewModel`. There was no
way to represent a running tool, stream progress lines from it, or show final status.
Any such display required abusing the message content string.

**Flaw 4 — Web view full re-render on each streaming chunk**
`ChatWebView` called `ChatHtmlRenderer.RenderToFile()` with a 120 ms debounce on every change.
On long chats this rebuilds the entire HTML document, causes DOM flicker, and discards scroll position.

---

## 2. Message type hierarchy

Use explicit discriminated subclasses. Never determine message type from string content.

```
ChatMessageViewModel            (abstract base)
├── UserMessageViewModel        user turn
├── AssistantMessageViewModel   completed assistant turn
│   └── StreamingAssistantMessageViewModel   live streaming turn
├── SystemMessageViewModel      context / instruction
├── ToolCallMessageViewModel    one tool invocation
└── PermissionMessageViewModel  awaiting user allow/deny
```

`ToolResultMessageViewModel` is intentionally not a top-level visible message.
It is owned by `ToolCallMessageViewModel` as a property (see section 4).

### 2.1 Base class contract

`ChatMessageViewModel` exposes:

| Property | Type | Purpose |
|---|---|---|
| `Id` | `Guid` | Stable identifier for JS bridge and list operations |
| `RetentionPolicy` | `MessageRetentionPolicy` | Context-window inclusion rules |
| `Timestamp` | `DateTimeOffset` | Creation time |

No content, no role — those are defined per subclass only.

### 2.2 Why not a single class with a `MessageKind` enum?

A discriminated subclass hierarchy gives:
- compile-time exhaustiveness in `switch` expressions;
- no nullable fields — each class carries exactly the data it needs;
- DataTemplate selector in Avalonia uses type directly, no converter needed;
- adding a new message kind does not require touching existing classes.

A `MessageKind` enum with a monolithic class is a source of `NullReferenceException` and
forgotten `case` branches. It is prohibited.

---

## 3. Display profile system

A `ChatDisplayProfile` defines how each message type renders inside a native chat view.
Profiles are selectable at runtime and persist per session.

### 3.1 Built-in profiles

| Profile | UserMessage | AssistantMessage | ToolCallMessage | SystemMessage |
|---|---|---|---|---|
| **Bubble** | Bubble, right-aligned | Bubble, left-aligned | Card, center | Hidden |
| **Classic** | Header + markdown body | Header + markdown body | Inline block | Small italic |
| **Debug** | Header + body + all metadata | Header + body + all metadata | Full log with args | Full, visible |

### 3.2 Why profiles instead of three separate views?

Three separate views (Bubble / Classic / Diagnostic) duplicated DataTemplate definitions for every
message type across three AXAML files. Adding a new message kind required editing three files.
Adding a new rendering variant required creating a fourth file with full duplication.

A profile is a dictionary from message type → template key. The view hosts a single
`ItemsControl` with a `MessageTemplateSelector` that resolves the correct template from the
active profile. Adding a new message kind requires one new DataTemplate resource.
Adding a new visual variant requires one new profile definition.

### 3.3 Extension rule

When adding a new built-in profile, define entries for all known message types.
A profile must never silently omit a message type — add a fallback "plain text" entry instead.

---

## 4. Tool call message and progress streaming

`ToolCallMessageViewModel` is the primary container for one tool execution lifecycle.

```csharp
public sealed class ToolCallMessageViewModel : ChatMessageViewModel
{
    public string ToolName { get; }
    public string ArgumentsJson { get; }
    public ToolCallStatus Status { get; set; }          // Pending / Running / Complete / Error
    public ObservableCollection<ToolProgressEntry> ProgressItems { get; }
    public string? ResultSummary { get; set; }
    public bool IsExpanded { get; set; }
}

public sealed record ToolProgressEntry(string Text, ToolProgressKind Kind);

public enum ToolProgressKind { Info, Found, Warning, Error }
```

The tool implementation writes progress by calling:
```csharp
toolCallMessage.AppendProgress("Scanning subnet 192.168.1.0/24 ...", ToolProgressKind.Info);
toolCallMessage.AppendProgress($"Found {count} hosts", ToolProgressKind.Found);
toolCallMessage.SetComplete(resultSummary);
```

The UI binds `ProgressItems` directly. No string parsing. No timer polling.

### 4.1 Why ProgressItems on ToolCallMessage, not separate messages?

Tool progress is ephemeral and belongs to its parent tool invocation.
Mixing progress lines as independent `ChatMessageViewModel` items pollutes the message list
and makes grouping, collapsing, and retention logic impossible.
A dedicated `ProgressItems` collection on `ToolCallMessageViewModel` allows the view to
render a collapsible card with live-updating content while the tool runs.

### 4.2 Progress streaming contract

A tool is allowed to:
- append any number of `ToolProgressEntry` items while `Status == Running`;
- call `SetComplete(summary)` exactly once, which sets `Status = Complete`;
- call `SetError(message)` exactly once on failure, which sets `Status = Error`.

A tool must not append progress after calling `SetComplete` or `SetError`.

---

## 5. Streaming assistant message fix

### 5.1 Root cause

`ScrollViewer.ScrollToEnd()` was called only on `ObservableCollection.CollectionChanged`.
`StreamingAssistantMessageViewModel.Append()` triggers `PropertyChanged("Content")`, not a
collection change. The scroll therefore does not follow streamed content.

### 5.2 Fix

The native chat view code-behind subscribes to the streaming message's `PropertyChanged`
while it is the last item in the collection:

```csharp
// In ChatNativeView.axaml.cs
void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
{
    if (e.NewItems?[^1] is StreamingAssistantMessageViewModel streaming)
    {
        _activeStreamingMessage = streaming;
        streaming.PropertyChanged += OnStreamingPropertyChanged;
    }
    ScrollToEnd();
}

void OnStreamingPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(StreamingAssistantMessageViewModel.Content))
        ScrollToEnd();
}
```

Unsubscribe from `_activeStreamingMessage` when a new collection change arrives or
when `StreamingAssistantMessageViewModel.IsComplete` becomes `true`.

### 5.3 Markdown jitter fix

Do not render markdown on every `Append()` call. `MarkdownScrollViewer` must receive
content updates at most every 80 ms. Buffer chunks in `StreamingAssistantMessageViewModel`
and fire `PropertyChanged` on a timer, not per chunk:

```csharp
// StreamingAssistantMessageViewModel
private readonly Timer _notifyTimer;

public void Append(string delta)
{
    _buffer.Append(delta);
    _notifyTimer.Change(dueTime: 80, period: Timeout.Infinite);
}

private void OnTimerElapsed()
    => OnPropertyChanged(nameof(Content));

public override string Content => _buffer.ToString();
```

`SetFinal(text)` cancels the timer and fires `PropertyChanged` immediately.

---

## 6. Web view incremental update

`ChatWebView` must not re-render the full HTML document on streaming updates.

### 6.1 Allowed re-render triggers

Full HTML re-render is only allowed when:
- a new non-streaming message is appended to the collection;
- theme changes;
- the active chat session changes.

### 6.2 Streaming incremental update

For `StreamingAssistantMessageViewModel` updates, call a JavaScript function instead:

```javascript
// in the HTML page
function appendMessageDelta(messageId, delta) {
    const el = document.getElementById('msg-' + messageId);
    if (el) el.dataset.raw = (el.dataset.raw || '') + delta;
    scheduleMarkdownRender(el);
}
```

Call from C#:
```csharp
await _webView.InvokeScriptAsync("appendMessageDelta",
    [message.Id.ToString(), delta]);
```

This preserves Mermaid diagrams (which are rendered once and must not be re-initialized),
scroll position, and selection state.

### 6.3 Tool progress incremental update

```javascript
function appendToolProgress(toolCallId, text, kind) {
    const list = document.getElementById('progress-' + toolCallId);
    if (list) {
        const li = document.createElement('li');
        li.className = 'progress-' + kind;
        li.textContent = text;
        list.appendChild(li);
    }
}
```

### 6.4 Why Web view is kept

Web view is the only rendering surface that supports Mermaid diagrams.
Mermaid is a first-class output format for planning, sequence diagrams, and network topology
that SPLA agents produce. Native Avalonia has no equivalent. Web view is mandatory.

---

## 7. Template registry

All DataTemplate resources for message types live in a single resource dictionary:

```
SPLA.UI.Avalonia/
  Themes/
    ChatMessageTemplates.axaml    ← one file, all message type templates
```

Template keys follow the pattern: `{ProfileName}{MessageTypeName}Template`

Examples:
- `BubbleUserMessageTemplate`
- `BubbleAssistantMessageTemplate`
- `BubbleToolCallMessageTemplate`
- `ClassicToolCallMessageTemplate`
- `DebugPermissionMessageTemplate`

`MessageTemplateSelector` reads the active `ChatDisplayProfile` and resolves the correct key.
If a profile does not define a template for a given type, the selector falls back to
`DefaultPlainTextMessageTemplate`.

### 7.1 Adding a new message type checklist

1. Create subclass of `ChatMessageViewModel` in `ViewModels/Messages/`.
2. Add a DataTemplate for each built-in profile in `ChatMessageTemplates.axaml`.
3. Add a fallback entry to the selector.
4. No other files need modification.

### 7.2 Adding a new profile checklist

1. Add a new `ChatDisplayProfile` record with template key entries for all known types.
2. Register in the profile list exposed by the configuration.
3. No DataTemplate files need modification — profiles reference existing template keys or
   introduce new ones in `ChatMessageTemplates.axaml`.

---

## 8. Retention policy

`MessageRetentionPolicy` controls whether a message is included in the API request context window.

| Value | Meaning |
|---|---|
| `Persistent` | Always included |
| `UntilSuperseded` | Included until a newer message with `ReplacementKey` replaces it |
| `NextStepOnly` | Included once, then dropped |
| `Never` | Display only, never sent to model |
| `UntilResolved` | Included until `PermissionMessageViewModel.IsAnswered == true` |

Retention is evaluated by the LLM context builder, not by the views.
Views must display the retention icon but must not filter the visible list by policy.

---

## 9. What is explicitly forbidden

- Determining message type by inspecting `Content` string prefixes. Use subclass type.
- Calling `ScrollToEnd()` only on `CollectionChanged`. Subscribe to streaming `PropertyChanged`.
- Full HTML re-render inside `ChatWebView` on every streaming chunk. Use JS incremental API.
- Defining DataTemplates for message types inside individual view AXAML files.
  All templates live in `ChatMessageTemplates.axaml`.
- Adding tool progress as separate `ChatMessageViewModel` items in the top-level list.
  Progress belongs inside `ToolCallMessageViewModel.ProgressItems`.
- Hardcoding profile-specific rendering logic inside a message ViewModel.
  ViewModels carry data only. All rendering decisions live in templates and the profile.
