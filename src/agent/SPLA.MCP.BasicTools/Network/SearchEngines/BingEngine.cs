using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.Network.SearchEngines;

public class BingEngine : ISearchEngine
{
    private readonly HttpClient _httpClient;

    public string Name => "bing";

    public BingEngine(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var url = "https://www.bing.com/search?q=" + Uri.EscapeDataString(query);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        if (html.Contains("challenges.cloudflare.com") || html.Contains("Turnstile") || html.Contains("verification-challenge"))
        {
            throw new Exception("Bing returned a bot protection challenge (Cloudflare/Turnstile).");
        }

        // Bing has b_algo class for result items, and h2 containing an anchor for title.
        var matches = Regex.Matches(
            html,
            "<li[^>]*class=\"[^\"]*b_algo[^\"]*\"[^>]*>.*?<h2><a[^>]*href=\"(?<url>[^\"]+)\"[^>]*>(?<title>.*?)</a></h2>.*?<div[^>]*class=\"[^\"]*b_caption[^\"]*\"[^>]*>.*?<p[^>]*>(?<snippet>.*?)</p></div>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (matches.Count == 0)
        {
            matches = Regex.Matches(
                html,
                "<h2><a[^>]*href=\"(?<url>[^\"]+)\"[^>]*>(?<title>.*?)</a></h2>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        var results = new List<SearchResult>();
        foreach (Match match in matches)
        {
            var title = CleanHtml(match.Groups["title"].Value);
            var snippet = match.Groups["snippet"].Success ? CleanHtml(match.Groups["snippet"].Value) : "...";

            if (string.IsNullOrWhiteSpace(title)) continue;

            results.Add(new SearchResult
            {
                Title = title,
                Url = WebUtility.HtmlDecode(match.Groups["url"].Value),
                Snippet = snippet
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
}
