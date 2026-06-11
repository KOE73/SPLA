using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem.Search;

public interface ISearchEngine
{
    Task<List<SearchMatch>> SearchAsync(
        string rootPath,
        string query,
        bool isRegex,
        bool caseSensitive,
        string[]? includePatterns,
        string[]? excludePatterns,
        CancellationToken cancellationToken);
}
