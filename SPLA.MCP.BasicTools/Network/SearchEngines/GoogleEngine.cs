using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.Network.SearchEngines;

public class GoogleEngine : ISearchEngine
{
    private readonly HttpClient _httpClient;

    public string Name => "google";

    public GoogleEngine(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var url = "https://www.google.com/search?q=" + Uri.EscapeDataString(query);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        if (html.Contains("detected unusual traffic") || html.Contains("captcha") || html.Contains("consent.google.com"))
        {
            throw new Exception("Google returned a bot protection challenge (CAPTCHA/Consent).");
        }

        // Parse Google results
        // Google has h3 for title, and it is usually wrapped inside <a href="/url?q=..."> or direct links.
        var matches = Regex.Matches(
            html,
            "<a[^>]*href=\"(?<url>(?:https?://|/url\\?q=)[^\"]+)\"[^>]*>.*?<h3[^>]*>(?<title>.*?)</h3>.*?</a>.*?<div[^>]*class=\"[^\"]*(?:VwiC3b|yDtyg|IsZvec|BNeawe|MUFPAc)[^\"]*\"[^>]*>(?<snippet>.*?)</div>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Fallback match if snippet class is different or missing
        if (matches.Count == 0)
        {
            matches = Regex.Matches(
                html,
                "<a[^>]*href=\"(?<url>(?:https?://|/url\\?q=)[^\"]+)\"[^>]*>.*?<h3[^>]*>(?<title>.*?)</h3>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        var results = new List<SearchResult>();
        foreach (Match match in matches)
        {
            var rawUrl = match.Groups["url"].Value;
            var title = CleanHtml(match.Groups["title"].Value);
            var snippet = match.Groups["snippet"].Success ? CleanHtml(match.Groups["snippet"].Value) : "...";

            if (string.IsNullOrWhiteSpace(title)) continue;

            results.Add(new SearchResult
            {
                Title = title,
                Url = DecodeGoogleUrl(rawUrl),
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

    private static string DecodeGoogleUrl(string url)
    {
        if (url.StartsWith("/url?q=", StringComparison.OrdinalIgnoreCase))
        {
            var target = url.Substring(7);
            var index = target.IndexOf('&');
            if (index >= 0)
            {
                target = target.Substring(0, index);
            }
            return Uri.UnescapeDataString(target);
        }
        return url;
    }
}
