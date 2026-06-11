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
- **Allowed**: Read files/search (`system.fs.read`, `system.fs.search`), List directories (`system.fs.list`), Context (`GetContextTool`).
- **Ask**: Internet access.
- **Denied**: File writing/patching/deleting, Shell commands.
- **Use case**: "Analyze my current project directory and find where the database connection is established."

### 3. Edit (`Edit`)
**"Standard pair programming"**
- **Allowed**: Read/search files, List directories, Internet access.
- **Ask**: Write/create/patch/delete files (`system.fs.create`, `system.fs.patch`, `system.fs.write`, `system.fs.delete`), Execute safe shell commands.
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
| `system.fs.list` | Project | Read | Low |
| `system.fs.read` | Project | Read | Low |
| `system.fs.search`| Project | Read | Low |
| `GetContextTool` | Local | Read | Low |
| `WebFetchTool` | Internet| Read | Low |
| `system.fs.create`| Project | Write | Medium |
| `system.fs.patch` | Project | Write | Medium |
| `system.fs.write` | Project | Write | Medium |
| `system.fs.delete`| Project | Write | Medium |
| `RunCommandTool`| Shell | Execute| High |

*The `PermissionManager` dynamically evaluates the current `AgentMode` against the tool's `Scope`, `Effect`, and `Risk` to return `Allow`, `Deny`, or `Ask`.*
