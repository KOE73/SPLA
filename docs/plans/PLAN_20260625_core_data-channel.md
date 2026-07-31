# Data channel (blob) — rollout plan

Goal: let tools move **bulk data** between each other without passing it through the model's context.
A producing tool stores its payload in a per-chat blob store and returns only a short summary + a
`blob:<handle>`; a consuming tool takes that handle back in and reads the data directly.

This solves "pull all DDL/sources from SQL straight into files" and similar pipelines (e.g. read file →
unzip → model), without flooding context.

## Foundation (DONE — do not redo)

- `SPLA.Domain/Agent/IBlobStore.cs` — `IBlobStore`, `BlobPayload` (Text | Bytes, in-memory only,
  materialisation — no streaming), `BlobEntry`, `BlobKind`.
- `SPLA.Domain/Agent/BlobStore.cs` — in-memory, transient (not persisted), thread-safe. Handles are
  `blob:<name|id>`.
- `IAgentSession.Blobs` added (`AgentSessionScope.cs`); `AgentSession` ctor takes optional `IBlobStore`
  (defaults to a fresh `BlobStore`). `ChatSessionViewModel` exposes `Blobs`. Spawned sub-agents and
  tests get a default store automatically.
- `SPLA.MCP.Core/Tools/DataChannel.cs` — the single routing point. Tools never implement routing
  themselves:
  - Producing: compute payload + 1-line summary, then `DataChannel.Route(target, payload, summary, name)`.
  - Consuming: `DataChannel.ResolveText(value, out text, out error)` / `ResolveBytes(...)` on any input
    that might be a `blob:` handle (a literal value resolves to itself).
  - `SchemaParts.Output` / `SchemaParts.OutputName` — drop these into a producing tool's `properties`
    so the standard `output` (context|blob|both) and `output_name` params are declared once.

### Reference conversions (DONE — copy these patterns)

- **Producing**: `sql_query` (`SPLA.Plugins.Sql/Tools/SqlQueryTool.cs`) — added `output`/`output_name`;
  default row limit is uncapped when `output != context` so it dumps the full result set into a blob.
- **Consuming**: `system_write_file` (`SPLA.MCP.BasicTools/FileSystem/FsWriteTool.cs`) — `content`
  accepts a `blob:` handle and writes it straight to disk.

Demo pipeline now works:
`sql_query(sql="SELECT name, definition ...", output="blob")` → `blob:x`
→ `system_write_file(path=..., content="blob:x")` — data never enters context.

## Conventions for converting a tool

1. **Producing tool** (returns potentially-large data the model often doesn't need verbatim):
   - Add `output = SchemaParts.Output` and `output_name = SchemaParts.OutputName` to `properties`.
     If the tool uses `StrictSchema = true`, also add `"output"`/`"output_name"` to `required`
     (both fragments are already nullable, so strict stays valid).
   - Build the payload as today, then:
     ```csharp
     var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
     if (target == OutputTarget.Context) return resultString;       // legacy path unchanged
     var name = ToolJson.GetStringTrimmed(root, "output_name");
     return DataChannel.Route(target, BlobPayload.OfText(resultString), summary, name);
     ```
   - For tools whose output is naturally capped/paged (sql_query limit, fs_search_text top-N): when
     `output != context`, lift the cap so the blob captures the full set.
   - Keep `Context` as the default so existing behaviour is untouched.

2. **Consuming tool** (takes a content/body input):
   - Don't add a new param. Just resolve the existing one through `DataChannel.ResolveText`
     (or `ResolveBytes` for binary) right after reading it; mention `blob:<handle>` in the param
     description.

3. Do **not** touch meta-only/control tools (memory, skills, marks, checkpoints, spawn, connection
   management, single-shot probes). They have no bulk payload.

## Tool inventory & priority

Legend: 🟢 done · ⭐ high-value next · ◽ optional later · — leave alone.

### Producing (add `output` param)
| Tool | File | Priority |
|---|---|---|
| sql_query | Plugins.Sql/Tools/SqlQueryTool.cs | 🟢 |
| sql_schema | Plugins.Sql/Tools/SqlSchemaTool.cs | 🟢 |
| system_read_file | MCP.BasicTools/FileSystem/FsReadTool.cs | 🟢 |
| web_fetch | MCP.BasicTools/Network/WebFetchTool.cs | 🟢 |
| run_command | MCP.BasicTools/System/RunCommandTool.cs | 🟢 |
| fs_search_text | MCP.BasicTools/FileSystem/FsSearchTextTool.cs | 🟢 |
| fs_find_files | MCP.BasicTools/FileSystem/FsFindFilesTool.cs | 🟢 |
| dotnet_build | MCP.BasicTools/System/DotnetBuildTool.cs | 🟢 |
| dotnet_test | MCP.BasicTools/System/DotnetTestTool.cs | 🟢 |
| web_search | MCP.BasicTools/Network/WebSearchTool.cs | 🟢 |
| compile_check | Plugins.Roslyn/CompileCheckTool.cs | 🟢 |
| script_run | Plugins.Roslyn/ScriptRunTool.cs | 🟢 |
| sql_query_plan | Plugins.Sql/Tools/SqlQueryPlanTool.cs | 🟢 |
| OneC: get_object / explain_object / get_dependencies / get_reverse_dependencies / find_readers / find_writers / find_references / find_object | Plugins.OneC/Tools/* | 🟢 |
| Network: lan_scan / port_scan / trace_route / host_network_info / dns_propagation | Plugins.Network/Tools/* | 🟢 |

### Consuming (resolve `blob:` handle on existing input)
| Tool | File | Priority |
|---|---|---|
| system_write_file (content) | MCP.BasicTools/FileSystem/FsWriteTool.cs | 🟢 |
| system_create_file (content) | MCP.BasicTools/FileSystem/FsCreateTool.cs | 🟢 |
| system_patch_file (new_text) | MCP.BasicTools/FileSystem/FsPatchTool.cs | 🟢 |
| sql_execute (sql) | Plugins.Sql/Tools/SqlExecuteTool.cs | 🟢 |

### Leave alone (meta-only / control)
agent_memory_* · agent_spawn / agent_spawn_batch · agent_info · agent_clarify · skill_activate /
skill_deactivate · mark_set / mark_rollback · context_checkpoint_set / context_checkpoint_restore ·
get_context · get_current_datetime · sql_connections · sql_test_connection · sql_verify_context ·
sql_manage_connection · single-shot Network probes (ping, ping_stats, port_check, dns_query, nslookup,
reverse_dns, route, whois, arp, wake, http_head, http_get, http_post, http_redirects, ssl_check,
smtp_probe, tcp_probe, udp_probe) · OneCGraphOpen · index_configuration · plugin_command_run · Test ping.

## Follow-ups (not yet done)

- **Debug view**: optional — surface blobs in the KV debug window (there's already
  `IBlobStore.Changed`); mirror `KvDebugWindowViewModel`.
- **`both` semantics**: currently `both` appends text after the summary for `Text` payloads only;
  decide if bytes need a different treatment.

## Completed

- 🟢 Foundation: `IBlobStore`, `BlobStore`, `DataChannel`, `SchemaParts`, `AgentSessionScope.Blobs`
- 🟢 All producing tools converted (see table above)
- 🟢 All consuming tools converted (see table above)
- 🟢 `SPLA.Tests/BlobStoreTests.cs` — 14 unit tests covering BlobStore and DataChannel
- 🟢 `SPLA.Agent/main_sys_prompt.md` — data channel section added; SPLA.Agent rebuilt
