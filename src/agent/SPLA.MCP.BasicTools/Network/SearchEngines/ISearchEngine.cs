using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.Network.SearchEngines;

public interface ISearchEngine
{
    string Name { get; }
    Task<List<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken);
}

public class SearchResult
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string Snippet { get; set; } = "";
}
