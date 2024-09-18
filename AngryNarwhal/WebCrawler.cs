using HtmlAgilityPack;

namespace AngryNarwhal;

public class WebCrawler
{
    private readonly HttpClient _httpClient;
    private readonly HashSet<string> _visitedUrls;
    private readonly string _baseUri;
    
    public WebCrawler(string baseUri)
    {
        _httpClient = new HttpClient();
        _visitedUrls = new HashSet<string>();
        _baseUri = baseUri.TrimEnd('/');
    }
    
    public async Task<List<string>> CrawlAsync()
    {
        var pages = new List<string>();
        await CrawlPageAsync(_baseUri, pages);
        return pages;
    }
    
    private async Task CrawlPageAsync(string url, List<string> pages)
    {
        if (_visitedUrls.Contains(url)) return;
        _visitedUrls.Add(url);
        pages.Add(url);
        
        Console.WriteLine($"Crawling: {url}");

        string content = await FetchContentAsync(url);
        if (content == null) return;

        var links = ExtractLinks(content, url);
        foreach (var link in links)
        {
            await CrawlPageAsync(link, pages);
        }
    }
    
    private async Task<string> FetchContentAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching {url}: {ex.Message}");
        }
        return null;
    }
    
    private List<string> ExtractLinks(string htmlContent, string currentUrl)
    {
        var links = new List<string>();
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var anchorTags = doc.DocumentNode.SelectNodes("//a[@href]");
        if (anchorTags == null) return links;

        foreach (var node in anchorTags)
        {
            var hrefValue = node.GetAttributeValue("href", string.Empty);
            var absoluteUrl = GetAbsoluteUrl(hrefValue, currentUrl);
            if (IsValidUrl(absoluteUrl))
            {
                links.Add(absoluteUrl);
            }
        }
        return links;
    }
    
    private string GetAbsoluteUrl(string href, string currentUrl)
    {
        if (Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }
        else if (Uri.TryCreate(new Uri(currentUrl), href, out var relativeUri))
        {
            return relativeUri.ToString();
        }
        return null;
    }
    
    private bool IsValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return false;

        return url.StartsWith(_baseUri) && !url.Contains("#") && !url.Contains("mailto:");
    }
}