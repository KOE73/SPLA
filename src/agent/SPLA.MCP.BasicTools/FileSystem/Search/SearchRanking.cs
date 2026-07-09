using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SPLA.MCP.BasicTools.FileSystem.Search;

public static class SearchRanking
{
    public static List<SearchMatch> RankAndFilter(List<SearchMatch> matches, string query, int maxResults)
    {
        // 1. Duplicate reduction: group by file and trimmed preview to avoid repeated lines
        var filteredMatches = new List<SearchMatch>();
        var seenCounts = new Dictionary<(string File, string CleanedPreview), int>();

        foreach (var match in matches)
        {
            var cleanPreview = match.Preview.Trim();
            var key = (match.File, cleanPreview);
            if (!seenCounts.ContainsKey(key))
            {
                seenCounts[key] = 0;
            }

            if (seenCounts[key] < 3) // Keep at most 3 identical preview lines per file
            {
                seenCounts[key]++;
                filteredMatches.Add(match);
            }
        }

        // 2. Score and rank matches
        var scored = filteredMatches.Select(m => new { Match = m, Score = GetScore(m, query) })
                                    .OrderByDescending(x => x.Score)
                                    .ThenBy(x => x.Match.File)
                                    .ThenBy(x => x.Match.Line)
                                    .Select(x => x.Match)
                                    .Take(maxResults)
                                    .ToList();

        return scored;
    }

    private static int GetScore(SearchMatch match, string query)
    {
        var preview = match.Preview.Trim();

        // 1. Exact full-line match gets top priority
        if (preview.Equals(query, StringComparison.Ordinal))
        {
            return 1000;
        }
        if (preview.Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            return 900;
        }

        int baseScore = 0;
        if (preview.Contains(query, StringComparison.Ordinal))
        {
            baseScore = 500;
        }
        else if (preview.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            baseScore = 300;
        }
        else
        {
            baseScore = 100; // Matches via regex or partial matches
        }

        // Context check:
        // A comment?
        if (preview.StartsWith("//") || preview.StartsWith("/*") || preview.StartsWith("*") || preview.Contains(" //") || preview.Contains(" /*"))
        {
            return baseScore + 40; // Ranking #5: comments
        }

        // A string literal?
        if (preview.Contains("\"") || preview.Contains("'"))
        {
            return baseScore + 20; // Ranking #6: string literals
        }

        // Class/Struct/Interface/Enum declaration?
        if (Regex.IsMatch(preview, @"\b(class|struct|interface|record|enum)\b", RegexOptions.IgnoreCase))
        {
            return baseScore + 90; // Ranking #2: class declarations
        }

        // Method/Function declaration?
        if (Regex.IsMatch(preview, @"\b(public|private|protected|internal|static|async|override|virtual)\b.*\b[A-Za-z0-9_]+\s*\(", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(preview, @"\b[A-Za-z0-9_]+\s*\(.*\)\s*$", RegexOptions.IgnoreCase))
        {
            return baseScore + 80; // Ranking #3: method declarations
        }

        // Symbol reference (default code line)
        return baseScore + 60; // Ranking #4: symbol references
    }
}
