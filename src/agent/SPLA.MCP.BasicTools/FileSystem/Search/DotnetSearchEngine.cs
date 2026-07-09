using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem.Search;

public class DotnetSearchEngine : ISearchEngine
{
    public async Task<List<SearchMatch>> SearchAsync(
        string rootPath,
        string query,
        bool isRegex,
        bool caseSensitive,
        string[]? includePatterns,
        string[]? excludePatterns,
        CancellationToken cancellationToken)
    {
        var matches = new List<SearchMatch>();
        var rootDir = new DirectoryInfo(rootPath);
        if (!rootDir.Exists) return matches;

        var includeRegexes = includePatterns?.Select(GlobToRegex).ToList();
        var excludeRegexes = excludePatterns?.Select(GlobToRegex).ToList();

        // Standard ignore folders
        var ignoreFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".git", "bin", "obj", "node_modules"
        };

        var files = GetFilesRecursive(rootDir, ignoreFolders, cancellationToken);

        Regex? queryRegex = null;
        if (isRegex)
        {
            var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            queryRegex = new Regex(query, options | RegexOptions.Compiled);
        }

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(rootPath, file.FullName);

            // Filter include/exclude patterns
            if (includeRegexes != null && includeRegexes.Count > 0)
            {
                if (!includeRegexes.Any(r => r.IsMatch(relativePath))) continue;
            }

            if (excludeRegexes != null && excludeRegexes.Count > 0)
            {
                if (excludeRegexes.Any(r => r.IsMatch(relativePath))) continue;
            }

            if (IsBinaryFile(file.FullName)) continue;

            try
            {
                // Read lines safely
                var lines = await File.ReadAllLinesAsync(file.FullName, cancellationToken);
                for (int i = 0; i < lines.Length; i++)
                {
                    var lineText = lines[i];
                    if (isRegex && queryRegex != null)
                    {
                        var regexMatch = queryRegex.Match(lineText);
                        if (regexMatch.Success)
                        {
                            matches.Add(new SearchMatch
                            {
                                File = file.FullName,
                                Line = i + 1,
                                Column = regexMatch.Index + 1,
                                Preview = lineText
                            });
                        }
                    }
                    else
                    {
                        var comp = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                        int idx = lineText.IndexOf(query, comp);
                        if (idx >= 0)
                        {
                            matches.Add(new SearchMatch
                            {
                                File = file.FullName,
                                Line = i + 1,
                                Column = idx + 1,
                                Preview = lineText
                            });
                        }
                    }
                }
            }
            catch
            {
                // Skip files that cannot be read (locked, permission issues, etc.)
            }
        }

        return matches;
    }

    private List<FileInfo> GetFilesRecursive(DirectoryInfo dir, HashSet<string> ignoreFolders, CancellationToken cancellationToken)
    {
        var result = new List<FileInfo>();
        try
        {
            foreach (var subDir in dir.GetDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (ignoreFolders.Contains(subDir.Name)) continue;
                result.AddRange(GetFilesRecursive(subDir, ignoreFolders, cancellationToken));
            }

            foreach (var file in dir.GetFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();
                result.Add(file);
            }
        }
        catch
        {
            // Skip unaccessible directories
        }
        return result;
    }

    private static Regex GlobToRegex(string glob)
    {
        // Replace Windows backslashes with forward slashes to normalize
        var pattern = glob.Replace("\\", "/");
        
        // Escape special regex chars, translate wildcards:
        // ** -> .*
        // * -> [^/]*
        // ? -> [^/]
        var escaped = Regex.Escape(pattern);
        escaped = escaped.Replace("\\*\\*", ".*")
                         .Replace("\\*", "[^/]*")
                         .Replace("\\?", "[^/]");

        return new Regex("^" + escaped + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    private bool IsBinaryFile(string path)
    {
        var ext = Path.GetExtension(path).ToLower();
        var binaryExtensions = new[] { ".exe", ".dll", ".pdb", ".bin", ".zip", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".pdf", ".mp3", ".mp4", ".git" };
        if (binaryExtensions.Contains(ext)) return true;
        if (path.Contains("\\bin\\") || path.Contains("\\obj\\") || path.Contains("\\.git\\")) return true;
        return false;
    }
}
