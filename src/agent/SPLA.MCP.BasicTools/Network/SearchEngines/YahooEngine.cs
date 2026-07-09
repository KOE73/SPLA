using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.Network.SearchEngines;

public class YahooEngine : ISearchEngine
{
    private readonly HttpClient _httpClient;

    public string Name => "yahoo";

    public YahooEngine(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var url = "https://search.yahoo.com/search?p=" + Uri.EscapeDataString(query);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        if (html.Contains("bots use Yahoo too") || html.Contains("verification-challenge") || html.Contains("challenges.cloudflare.com"))
        {
            throw new Exception("Yahoo returned a bot protection challenge (Cloudflare/CAPTCHA).");
        }

        var matches = Regex.Matches(
            html,
            "<a[^>]*href=\"(?<url>https://r.search.yahoo.com/[^\"]+)\"[^>]*>.*?<h3[^>]*class=\"[^\"]*title[^\"]*\"[^>]*>(?<title>.*?)</h3></a>.*?<div[^>]*class=\"[^\"]*compText[^\"]*\"[^>]*>(?<snippet>.*?)</div>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        var results = new List<SearchResult>();
        foreach (Match match in matches)
        {
            results.Add(new SearchResult
            {
                Title = CleanHtml(match.Groups["title"].Value),
                Url = DecodeYahooUrl(match.Groups["url"].Value),
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

    private static string DecodeYahooUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return url;
        }

        var segments = uri.PathAndQuery.Split('/');
        foreach (var segment in segments)
        {
            if (segment.StartsWith("RU=", StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(segment.Substring(3));
            }
        }

        return url;
    }
}
