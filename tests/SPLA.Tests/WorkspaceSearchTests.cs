using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.MCP.BasicTools.FileSystem;

namespace SPLA.Tests;

/// <summary>
/// Proves text search works over a VIRTUAL workspace — one whose <see cref="IWorkspace.MapPathToHost"/>
/// returns null, so the disk engines (ripgrep/.NET) have no path to search. Before this, the tool
/// returned "Text search is not available for this workspace." Now <c>WorkspaceSearchEngine</c> walks
/// the workspace's own logical API, so search is available everywhere the fs tools are.
/// </summary>
public sealed class WorkspaceSearchTests
{
    private static IDisposable Scope(IWorkspace ws)
        => AgentSessionScope.Begin(new AgentSession(
            new KeyValueStore("session"), new MarkManager(), new SkillSession(),
            sandbox: new PassthroughSandbox(workspace: ws, shell: null)));

    private static string Search(string query, string? path = null, string? include = null)
    {
        var inc = include is null ? "null" : $"[\"{include}\"]";
        return $$"""
        {"query":"{{query}}","path":{{(path is null ? "null" : $"\"{path}\"")}},"regex":null,
         "case_sensitive":null,"max_results":null,"include_patterns":{{inc}},
         "exclude_patterns":null,"output":null,"output_name":null}
        """;
    }

    [Fact]
    public async Task Finds_matches_by_walking_a_virtual_workspace()
    {
        var ws = new VirtualWorkspace();
        await ws.WriteAllTextAsync("/proj/a.cs", "class Foo {}\nvar x = TARGET;");
        await ws.WriteAllTextAsync("/proj/src/b.cs", "// nothing here");
        await ws.WriteAllTextAsync("/proj/src/deep/c.cs", "found TARGET again");
        using var _ = Scope(ws);

        var json = await new FsSearchTextTool().ExecuteAsync(Search("TARGET", "/proj"));
        var result = JsonSerializer.Deserialize<SearchTextResult>(json, Web)!;

        Assert.Equal(2, result.TotalMatches);   // a.cs line 2, deep/c.cs line 1
        Assert.Contains(result.Matches, m => m.File.EndsWith("a.cs") && m.Line == 2);
        Assert.Contains(result.Matches, m => m.File.EndsWith("c.cs") && m.Line == 1);
    }

    [Fact]
    public async Task Respects_include_glob_over_a_virtual_workspace()
    {
        var ws = new VirtualWorkspace();
        await ws.WriteAllTextAsync("/proj/keep.cs", "hit");
        await ws.WriteAllTextAsync("/proj/skip.md", "hit");
        using var _ = Scope(ws);

        var json = await new FsSearchTextTool().ExecuteAsync(Search("hit", "/proj", include: "*.cs"));
        var result = JsonSerializer.Deserialize<SearchTextResult>(json, Web)!;

        Assert.Equal(1, result.TotalMatches);
        Assert.EndsWith("keep.cs", result.Matches.Single().File);
    }

    [Fact]
    public async Task Skips_ignored_folders_in_a_virtual_workspace()
    {
        var ws = new VirtualWorkspace();
        await ws.WriteAllTextAsync("/proj/real.cs", "needle");
        await ws.WriteAllTextAsync("/proj/bin/generated.cs", "needle");   // under bin/ → ignored
        using var _ = Scope(ws);

        var json = await new FsSearchTextTool().ExecuteAsync(Search("needle", "/proj"));
        var result = JsonSerializer.Deserialize<SearchTextResult>(json, Web)!;

        Assert.Equal(1, result.TotalMatches);
        Assert.EndsWith("real.cs", result.Matches.Single().File);
    }

    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    /// <summary>In-memory workspace with NO host mapping — MapPathToHost returns null, the defining
    /// trait of a virtual workspace. Keyed by exact logical path; dirs are implicit.</summary>
    private sealed class VirtualWorkspace : IWorkspace
    {
        private readonly Dictionary<string, string> _files = new();

        public string? MapPathToHost(string logicalPath) => null;   // virtual: no disk path
        public string? MapPathToProject(string hostPath) => null;

        public bool FileExists(string path) => _files.ContainsKey(path);
        public bool DirectoryExists(string path)
        {
            var dir = path.TrimEnd('/');
            return dir.Length == 0 || _files.Keys.Any(k => k.StartsWith(dir + "/"));
        }

        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken ct = default)
            => Task.FromResult(_files.TryGetValue(path, out var v) ? v.Split('\n') : System.Array.Empty<string>());

        public Task<string> ReadAllTextAsync(string path, CancellationToken ct = default)
            => Task.FromResult(_files.TryGetValue(path, out var v) ? v : string.Empty);

        public Task WriteAllTextAsync(string path, string content, CancellationToken ct = default)
        {
            _files[path] = content;
            return Task.CompletedTask;
        }

        public void DeleteFile(string path) => _files.Remove(path);
        public void CreateDirectory(string path) { }

        private static string Parent(string path)
        {
            var i = path.LastIndexOf('/');
            return i <= 0 ? "/" : path.Substring(0, i);
        }

        public IReadOnlyList<string> GetFiles(string path)
        {
            var dir = path.TrimEnd('/');
            if (dir.Length == 0) dir = "";
            return _files.Keys.Where(k => Parent(k) == (dir.Length == 0 ? "/" : dir)).ToList();
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
