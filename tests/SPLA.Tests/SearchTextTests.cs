using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Host;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.BasicTools.FileSystem.Search;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Engine-level tests over a real directory. These drive <see cref="WorkspaceSearchEngine"/>, which
/// since ADR_20260831 is the single non-ripgrep engine: the old direct-disk .NET engine was removed
/// because a disk workspace is an <see cref="IWorkspace"/> too, and a third implementation of every
/// feature bought nothing but a third set of behaviours to keep in sync.
/// </summary>
public class SearchTextTests : IDisposable
{
    private readonly string _testDir;
    private readonly WorkspaceSearchEngine _engine = new(new LocalWorkspace());

    public SearchTextTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "SplaSearchTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);

        File.WriteAllText(Path.Combine(_testDir, "File1.cs"),
@"using System;
namespace TestNamespace;
public class TargetClass
{
    // ExactMatchQuery
    public void MyMethod()
    {
        string dummy = ""ExactMatchQuery in string"";
    }
}");

        File.WriteAllText(Path.Combine(_testDir, "File2.txt"), "exactmatchquery is here in lowercase\nAnother line.");
        File.WriteAllText(Path.Combine(_testDir, "File3.log"), "Some logs that shouldn't be matched usually.");
    }

    public void Dispose()
    {
        try { Directory.Delete(_testDir, true); }
        catch { /* ignore clean up errors */ }
    }

    private SearchRequest Request(
        string query, bool isRegex = false, bool caseSensitive = false,
        string[]? include = null, string[]? exclude = null,
        int context = 0, bool multiline = false,
        SearchOutputMode mode = SearchOutputMode.Content)
        => new(_testDir, query, isRegex, caseSensitive, include, exclude, context, context, multiline, mode);

    [Fact]
    public async Task TestExactSearch()
    {
        var outcome = await _engine.SearchAsync(Request("ExactMatchQuery", caseSensitive: true), CancellationToken.None);

        Assert.NotEmpty(outcome.Matches);
        Assert.All(outcome.Matches, m => Assert.Contains("ExactMatchQuery", m.Preview));
        Assert.DoesNotContain(outcome.Matches, m => m.File.EndsWith("File2.txt"));
    }

    [Fact]
    public async Task TestCaseInsensitiveSearch()
    {
        var outcome = await _engine.SearchAsync(Request("ExactMatchQuery"), CancellationToken.None);

        Assert.True(outcome.Matches.Count >= 2);
        Assert.Contains(outcome.Matches, m => m.File.EndsWith("File1.cs"));
        Assert.Contains(outcome.Matches, m => m.File.EndsWith("File2.txt"));
    }

    [Fact]
    public async Task TestRegexSearch()
    {
        var outcome = await _engine.SearchAsync(
            Request(@"Target[A-Z]\w+", isRegex: true, caseSensitive: true), CancellationToken.None);

        Assert.Single(outcome.Matches);
        Assert.Contains("TargetClass", outcome.Matches[0].Preview);
    }

    [Fact]
    public async Task TestIncludePatterns()
    {
        var outcome = await _engine.SearchAsync(
            Request("ExactMatchQuery", include: new[] { "*.cs" }), CancellationToken.None);

        Assert.NotEmpty(outcome.Matches);
        Assert.All(outcome.Matches, m => Assert.EndsWith(".cs", m.File));
    }

    [Fact]
    public async Task TestExcludePatterns()
    {
        var outcome = await _engine.SearchAsync(
            Request("ExactMatchQuery", exclude: new[] { "*.txt" }), CancellationToken.None);

        Assert.DoesNotContain(outcome.Matches, m => m.File.EndsWith(".txt"));
    }

    [Fact]
    public async Task Context_lines_surround_the_match()
    {
        var outcome = await _engine.SearchAsync(
            Request("ExactMatchQuery", caseSensitive: true, include: new[] { "*.cs" }, context: 2),
            CancellationToken.None);

        var first = outcome.Matches.First(m => m.Line == 5);   // the "// ExactMatchQuery" comment
        Assert.NotNull(first.Before);
        Assert.NotNull(first.After);
        Assert.Contains(first.Before!, l => l.Contains("public class TargetClass"));
        Assert.Contains(first.After!, l => l.Contains("MyMethod"));
    }

    [Fact]
    public async Task Context_is_absent_when_not_asked_for()
    {
        var outcome = await _engine.SearchAsync(Request("TargetClass", caseSensitive: true), CancellationToken.None);

        // Null rather than empty: an always-present empty array is noise in every result that did not
        // want context, and the tool omits nulls from its JSON.
        Assert.All(outcome.Matches, m => Assert.Null(m.Before));
        Assert.All(outcome.Matches, m => Assert.Null(m.After));
    }

    [Fact]
    public async Task Files_with_matches_mode_reports_files_and_skips_line_text()
    {
        var outcome = await _engine.SearchAsync(
            Request("ExactMatchQuery", mode: SearchOutputMode.FilesWithMatches), CancellationToken.None);

        Assert.Equal(2, outcome.Files.Count);
        Assert.Empty(outcome.Matches);   // early exit: no previews built
    }

    [Fact]
    public async Task Count_mode_reports_per_file_counts()
    {
        var outcome = await _engine.SearchAsync(
            Request("ExactMatchQuery", caseSensitive: true, mode: SearchOutputMode.Count), CancellationToken.None);

        var file1 = outcome.Files.Single(f => f.File.EndsWith("File1.cs"));
        Assert.Equal(2, file1.MatchCount);   // the comment and the string literal
        Assert.Empty(outcome.Matches);
    }

    [Fact]
    public async Task Multiline_matches_across_a_line_break()
    {
        var outcome = await _engine.SearchAsync(
            Request(@"TargetClass\s*\{", isRegex: true, caseSensitive: true, multiline: true),
            CancellationToken.None);

        Assert.Single(outcome.Matches);
        Assert.Equal(3, outcome.Matches[0].Line);   // reported at the line the match STARTS on
    }

    [Fact]
    public async Task Same_pattern_without_multiline_does_not_span_lines()
    {
        var outcome = await _engine.SearchAsync(
            Request(@"TargetClass\s*\{", isRegex: true, caseSensitive: true), CancellationToken.None);

        Assert.Empty(outcome.Matches);
    }

    [Fact]
    public async Task Lookaround_is_refused_rather_than_silently_differing()
    {
        // Unsupported on ripgrep's engine class too. Refusing beats one query meaning two things
        // depending on which substrate happened to serve it.
        await Assert.ThrowsAsync<UnsupportedPatternException>(() =>
            _engine.SearchAsync(Request("(?<=class )Target", isRegex: true), CancellationToken.None));
    }

    [Fact]
    public void TestRanking()
    {
        var matches = new[]
        {
            new SearchMatch { File = "A.cs", Line = 1, Column = 1, Preview = "    string val = \"Query\"; // String literal" },
            new SearchMatch { File = "A.cs", Line = 2, Column = 1, Preview = "public class QueryClass" },
            new SearchMatch { File = "A.cs", Line = 3, Column = 1, Preview = "Query" },
            new SearchMatch { File = "A.cs", Line = 4, Column = 1, Preview = "    // Comment with Query" },
            new SearchMatch { File = "A.cs", Line = 5, Column = 1, Preview = "    public void RunQuery()" }
        }.ToList();

        var ranked = SearchRanking.RankAndFilter(matches, "Query", 100);

        Assert.Equal("Query", ranked[0].Preview);
        Assert.Contains("class", ranked[1].Preview);
        Assert.Contains("void", ranked[2].Preview);
    }

    [Fact]
    public void TestLargeResultTruncation()
    {
        var matches = Enumerable.Range(1, 200).Select(i => new SearchMatch
        {
            File = "A.cs",
            Line = i,
            Column = 1,
            Preview = $"Query line {i}"
        }).ToList();

        var ranked = SearchRanking.RankAndFilter(matches, "Query", 50);

        Assert.Equal(50, ranked.Count);
    }

    [Fact]
    public async Task TestFallbackWithoutRg()
    {
        var outcome = await _engine.SearchAsync(Request("TargetClass", caseSensitive: true), CancellationToken.None);

        Assert.Single(outcome.Matches);
        Assert.Equal(3, outcome.Matches[0].Line);
    }
}
