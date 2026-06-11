# MainWindowViewModel split map

`MainWindowViewModel` is intentionally split into partial files by UI/runtime domain. When changing this view model, open only the file for the target behavior first.

- `MainWindowViewModel.cs` - shared state, observable properties, constructor, tool registration.
- `MainWindowViewModel_Initialization.cs` - startup settings, model display, initial chat/project loading.
- `MainWindowViewModel_Settings.cs` - reactions to settings and status changes.
- `MainWindowViewModel_Permissions.cs` - permission manager setup and remembered tool decisions.
- `MainWindowViewModel_Tools.cs` - tool filtering by `AgentMode`.
- `MainWindowViewModel_Sidebar_commands.cs` - chat view picker, recent projects, plugin UI commands, project creation.
- `MainWindowViewModel_Chats_commands.cs` - chat list loading, select/new/delete/fork chat, system prompt construction.
- `MainWindowViewModel_Messages_commands.cs` - message clearing/deletion, persistence filtering, runtime context building.
- `MainWindowViewModel_Conversation_commands.cs` - send/stop commands and the LLM/tool-call processing loop.
- `MainWindowViewModel_TokenEstimate.cs` - active token/elapsed estimate timer.
- `MainWindowViewModel_Compact_commands.cs` - `/compact`, summary saving, visible chat snapshot.

Naming convention: keep new files in this folder as `MainWindowViewModel_[Domain].cs` or `MainWindowViewModel_[Domain]_commands.cs` when they contain RelayCommand methods.
