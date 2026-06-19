# SPLA Agent Modes & Capabilities

SPLA is designed to operate safely in your local environment. To balance power and security, SPLA uses a 5-tier permission model. You can switch these modes on the fly via the Status Bar.

## Modes

### 0. Chat (`Chat`)
**"Safe conversation only"**
- **Allowed**: Basic chat, context awareness (if already loaded).
- **Denied**: File reading/writing, shell commands, internet access, MCP tools.
- **Use case**: You just want to ask questions without any risk of the agent reading your files or modifying your system.

### 1. Research (`Research`)
**"Look but don't touch locally"**
- **Allowed**: Web fetch (Internet), Reading local context/files (Ask first).
- **Denied**: Shell commands, File writing.
- **Use case**: "Find documentation on the web", "Read this file and explain it".

### 2. Inspect (`Inspect`)
**"Read-only local access"**
- **Allowed**: Read files/search (`system_read_file`, `system_search_text`), List directories (`system_list_files`), Context (`agent_get_context`).
- **Ask**: Internet access.
- **Denied**: File writing/patching/deleting, Shell commands.
- **Use case**: "Analyze my current project directory and find where the database connection is established."

### 3. Edit (`Edit`)
**"Standard pair programming"**
- **Allowed**: Read/search files, List directories, Internet access.
- **Ask**: Write/create/patch/delete files (`system_create_file`, `system_patch_file`, `system_write_file`, `system_delete_file`), Execute safe shell commands.
- **Denied**: High-risk system commands.
- **Use case**: "Refactor this class", "Add a new feature". The agent will prepare the code and ask for your permission before saving.

### 4. Agent (`Agent`)
**"Full Autonomy"**
- **Allowed**: All read/write/patch tools, Internet access.
- **Ask**: Only high-risk shell commands (`run_command`), depending on safety heuristics.
- **Use case**: "Create a new project from scratch, build it, and fix any compilation errors."

## Tool Matrix

| Tool | Scope | Effect | Risk |
|------|-------|--------|------|
| `system_list_files` | Project | Read | Low |
| `system_read_file` | Project | Read | Low |
| `system_search_text`| Project | Read | Low |
| `agent_get_context` | Local | Read | Low |
| `web_fetch` | Internet| Read | Low |
| `system_create_file`| Project | Write | Medium |
| `system_patch_file` | Project | Write | Medium |
| `system_write_file` | Project | Write | Medium |
| `system_delete_file`| Project | Write | Medium |
| `system_run_shell`| Shell | Execute| High |

*The `PermissionManager` dynamically evaluates the current `AgentMode` against the tool's `Scope`, `Effect`, and `Risk` to return `Allow`, `Deny`, or `Ask`.*
