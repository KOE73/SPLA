using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.BasicTools.FileSystem.Search;
using Xunit;

namespace SPLA.Tests;

public class SearchTextTests : IDisposable
{
    private readonly string _testDir;

    public SearchTextTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "SplaSearchTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);

        // Populate with dummy files
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
        try
        {
            Directory.Delete(_testDir, true);
        }
        catch
        {
            // Ignore clean up errors
        }
    }

    [Fact]
    public async Task TestExactSearch()
    {
        var engine = new DotnetSearchEngine();
        var matches = await engine.SearchAsync(_testDir, "ExactMatchQuery", isRegex: false, caseSensitive: true, null, null, CancellationToken.None);
        
        Assert.NotEmpty(matches);
        Assert.All(matches, m => Assert.Contains("ExactMatchQuery", m.Preview));
        
        // Casing is sensitive, so lowercase in File2 should NOT match
        Assert.DoesNotContain(matches, m => m.File.EndsWith("File2.txt"));
    }

    [Fact]
    public async Task TestCaseInsensitiveSearch()
    {
        var engine = new DotnetSearchEngine();
        var matches = await engine.SearchAsync(_testDir, "ExactMatchQuery", isRegex: false, caseSensitive: false, null, null, CancellationToken.None);
        
        Assert.True(matches.Count >= 2);
        Assert.Contains(matches, m => m.File.EndsWith("File1.cs"));
        Assert.Contains(matches, m => m.File.EndsWith("File2.txt"));
    }

    [Fact]
    public async Task TestRegexSearch()
    {
        var engine = new DotnetSearchEngine();
        var matches = await engine.SearchAsync(_testDir, @"Target[A-Z]\w+", isRegex: true, caseSensitive: true, null, null, CancellationToken.None);
        
        Assert.Single(matches);
        Assert.Contains("TargetClass", matches[0].Preview);
    }

    [Fact]
    public async Task TestIncludePatterns()
    {
        var engine = new DotnetSearchEngine();
        var matches = await engine.SearchAsync(_testDir, "ExactMatchQuery", isRegex: false, caseSensitive: false, 
            includePatterns: new[] { "*.cs" }, excludePatterns: null, CancellationToken.None);
        
        Assert.All(matches, m => Assert.EndsWith(".cs", m.File));
    }

    [Fact]
    public async Task TestExcludePatterns()
    {
        var engine = new DotnetSearchEngine();
        var matches = await engine.SearchAsync(_testDir, "ExactMatchQuery", isRegex: false, caseSensitive: false, 
            includePatterns: null, excludePatterns: new[] { "*.txt" }, CancellationToken.None);
        
        Assert.DoesNotContain(matches, m => m.File.EndsWith(".txt"));
    }

    [Fact]
    public void TestRanking()
    {
        var matches = new[]
        {
            new SearchMatch { File = "A.cs", Line = 1, Column = 1, Preview = "    string val = \"Query\"; // String literal" },
            new SearchMatch { File = "A.cs", Line = 2, Column = 1, Preview = "public class QueryClass" }, // Class declaration
            new SearchMatch { File = "A.cs", Line = 3, Column = 1, Preview = "Query" }, // Exact match line
            new SearchMatch { File = "A.cs", Line = 4, Column = 1, Preview = "    // Comment with Query" }, // Comment
            new SearchMatch { File = "A.cs", Line = 5, Column = 1, Preview = "    public void RunQuery()" } // Method declaration
        }.ToList();

        var ranked = SearchRanking.RankAndFilter(matches, "Query", 100);

        // Expected Ranking:
        // 1. Exact Match ("Query") -> score 100
        // 2. Class declaration ("public class QueryClass") -> score 85 (or 70 if not exact)
        // 3. Method declaration ("public void RunQuery()") -> score 60
        // 4. Comment ("// Comment with Query") -> score 40
        // 5. String literal -> score 30
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
        // Explicitly test DotnetSearchEngine to make sure it functions correctly without Ripgrep
        var engine = new DotnetSearchEngine();
        var matches = await engine.SearchAsync(_testDir, "TargetClass", isRegex: false, caseSensitive: true, null, null, CancellationToken.None);
        
        Assert.Single(matches);
        Assert.Equal(3, matches[0].Line);
    }
}
