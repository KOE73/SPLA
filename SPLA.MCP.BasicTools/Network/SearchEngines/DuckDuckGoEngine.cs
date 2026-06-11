using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.Network.SearchEngines;

public class DuckDuckGoEngine : ISearchEngine
{
    private readonly HttpClient _httpClient;

    public string Name => "duckduckgo";

    public DuckDuckGoEngine(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var url = "https://html.duckduckgo.com/html/?q=" + Uri.EscapeDataString(query);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        if (html.Contains("bots use DuckDuckGo too"))
        {
            throw new Exception("DuckDuckGo returned a bot protection challenge (CAPTCHA).");
        }

        var matches = Regex.Matches(
            html,
            "<a[^>]*class=\"result__a\"[^>]*href=\"(?<url>[^\"]+)\"[^>]*>(?<title>.*?)</a>.*?<a[^>]*class=\"result__snippet\"[^>]*>(?<snippet>.*?)</a>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        var results = new List<SearchResult>();
        foreach (Match match in matches)
        {
            results.Add(new SearchResult
            {
                Title = CleanHtml(match.Groups["title"].Value),
                Url = DecodeDuckDuckGoUrl(WebUtility.HtmlDecode(match.Groups["url"].Value)),
                Snippet = CleanHtml(match.Groups["snippet"].Value)
            });
        }

        return results;
    }

    private static string CleanHtml(string html)
    {
        var text = Regex.Replace(html, "<.*?>", " ", RegexOptions.Singleline);
        text = WebUtility.HtmlDecode(text);
        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private static string DecodeDuckDuckGoUrl(string url)
    {
        if (url.StartsWith("//", StringComparison.Ordinal))
        {
            url = "https:" + url;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return url;
        }

        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in query)
        {
            var pieces = part.Split('=', 2);
            if (pieces.Length == 2 && pieces[0] == "uddg")
            {
                return Uri.UnescapeDataString(pieces[1]);
            }
        }

        return url;
    }
}
