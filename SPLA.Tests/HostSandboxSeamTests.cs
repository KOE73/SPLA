using System.Collections.Generic;
using System.Linq;
using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.MCP.BasicTools.FileSystem;

namespace SPLA.Tests;

/// <summary>
/// Proves the host-abstraction seam: the fs tools go through <see cref="ISandbox.Workspace"/> from
/// the active chat scope, not <see cref="System.IO"/> directly. A pure in-memory workspace with no
/// disk backing means any file that survives a write→read round-trip could only have gone through
/// the injected sandbox.
/// </summary>
public sealed class HostSandboxSeamTests
{
    private static IDisposable Scope(ISandbox sandbox)
        => AgentSessionScope.Begin(new AgentSession(
            new KeyValueStore("session"), new MarkManager(), new SkillSession(), sandbox: sandbox));

    [Fact]
    public async Task Write_then_read_flows_through_scoped_workspace_not_disk()
    {
        var ws = new MemoryWorkspace();
        using var _ = Scope(new PassthroughSandbox(workspace: ws, shell: null));

        var write = await new FsWriteTool().ExecuteAsync(
            """{"path":"/virtual/note.txt","content":"hello seam"}""");
        Assert.Contains("Successfully wrote", write);

        // Lives only in the in-memory workspace — nothing was written to a real path.
        Assert.False(File.Exists("/virtual/note.txt"));
        Assert.True(ws.FileExists("/virtual/note.txt"));

        var read = await new FsReadTool().ExecuteAsync(
            """{"path":"/virtual/note.txt","start_line":1,"line_count":null,"output":null,"output_name":null}""");
        Assert.Contains("hello seam", read);
    }

    [Fact]
    public async Task Create_patch_delete_flow_through_scoped_workspace()
    {
        var ws = new MemoryWorkspace();
        using var _ = Scope(new PassthroughSandbox(workspace: ws, shell: null));

        var create = await new FsCreateTool().ExecuteAsync(
            """{"path":"/virtual/a.txt","content":"one two three"}""");
        Assert.Contains("Successfully created", create);
        Assert.True(ws.FileExists("/virtual/a.txt"));
        Assert.False(File.Exists("/virtual/a.txt"));

        var patch = await new FsPatchTool().ExecuteAsync(
            """{"path":"/virtual/a.txt","old_text":"two","new_text":"TWO"}""");
        Assert.Contains("status: success", patch);

        var read = await new FsReadTool().ExecuteAsync(
            """{"path":"/virtual/a.txt","start_line":1,"line_count":null,"output":null,"output_name":null}""");
        Assert.Contains("one TWO three", read);

        var delete = await new FsDeleteTool().ExecuteAsync("""{"path":"/virtual/a.txt"}""");
        Assert.Contains("Successfully deleted", delete);
        Assert.False(ws.FileExists("/virtual/a.txt"));
    }

    [Fact]
    public async Task Find_files_recurses_through_scoped_workspace()
    {
        var ws = new MemoryWorkspace();
        await ws.WriteAllTextAsync("/proj/src/a.cs", "x");
        await ws.WriteAllTextAsync("/proj/src/deep/b.cs", "y");
        await ws.WriteAllTextAsync("/proj/src/readme.md", "z");
        using var _ = Scope(new PassthroughSandbox(workspace: ws, shell: null));

        var result = await new FsFindFilesTool().ExecuteAsync(
            """{"path":"/proj","pattern":"*.cs","max_results":null,"exclude_patterns":null,"output":null,"output_name":null}""");

        Assert.Contains("a.cs", result);
        Assert.Contains("b.cs", result);       // found via recursion into /proj/src/deep
        Assert.DoesNotContain("readme.md", result);
    }

    [Fact]
    public async Task Shell_tool_reports_disabled_when_sandbox_has_no_shell()
    {
        // PassthroughSandbox always supplies a shell; a no-execute scenario uses a sandbox whose
        // Shell is genuinely null (the contract's way to forbid execution).
        using var _ = Scope(new NoShellSandbox());

        var result = await new SPLA.MCP.BasicTools.SystemTools.RunCommandTool().ExecuteAsync(
            """{"command":"echo hi","cwd":null,"code_page":null,"output":null,"output_name":null}""");

        Assert.Contains("disabled", result);
    }

    private sealed class NoShellSandbox : ISandbox
    {
        public IWorkspace Workspace { get; } = new MemoryWorkspace();
        public IShell? Shell => null;
        public ICapabilityGate Gate => AllowAllGate.Instance;
    }

    /// <summary>Minimal virtual file system — no disk, keyed by exact logical path.</summary>
    private sealed class MemoryWorkspace : IWorkspace
    {
        private readonly Dictionary<string, string> _files = new();

        // Identity mapping: this store's logical paths are its own address space (no real disk).
        public string? MapPathToHost(string logicalPath) => logicalPath;
        public string? MapPathToProject(string hostPath) => hostPath;

        public bool FileExists(string path) => _files.ContainsKey(path);
        public bool DirectoryExists(string path)
            => _files.Keys.Any(k => k.StartsWith(path.TrimEnd('/') + "/"));

        private static string Parent(string path)
        {
            var i = path.LastIndexOf('/');
            return i <= 0 ? "/" : path.Substring(0, i);
        }

        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken ct = default)
            => Task.FromResult(_files.TryGetValue(path, out var v)
                ? v.Split('\n')
                : System.Array.Empty<string>());

        public Task<string> ReadAllTextAsync(string path, CancellationToken ct = default)
            => Task.FromResult(_files.TryGetValue(path, out var v) ? v : string.Empty);

        public Task WriteAllTextAsync(string path, string content, CancellationToken ct = default)
        {
            _files[path] = content;
            return Task.CompletedTask;
        }

        public void DeleteFile(string path) => _files.Remove(path);
        public void CreateDirectory(string path) { /* virtual: directories are implicit */ }

        // Immediate children only, so recursive walkers behave like a real tree.
        public IReadOnlyList<string> GetFiles(string path)
        {
            var dir = path.TrimEnd('/');
            return _files.Keys.Where(k => Parent(k) == dir).ToList();
        }

        public IReadOnlyList<string> GetDirectories(string path)
        {
            var prefix = path.TrimEnd('/') + "/";
            return _files.Keys
                .Where(k => k.StartsWith(prefix))
                .Select(k => k.Substring(prefix.Length))
                .Where(rest => rest.Contains('/'))
                .Select(rest => prefix + rest.Substring(0, rest.IndexOf('/')))
                .Distinct()
                .ToList();
        }
    }
}
