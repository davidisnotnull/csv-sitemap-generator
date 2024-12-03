using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace AngryNarwhal;

public class WebCrawler
{
    private readonly HttpClient _httpClient;
    private readonly HashSet<string> _visitedUrls;
    private readonly List<UrlRecord> _iframes;
    private readonly string _baseUri;

    public WebCrawler(string baseUri)
    {
        _httpClient = new HttpClient();
        _visitedUrls = new HashSet<string>();
        _iframes = new List<UrlRecord>();
        _baseUri = baseUri.TrimEnd('/');
    }

    public async Task<CrawlResults> CrawlAsync()
    {
        var crawlResults = new CrawlResults()
        {
            Pages = new List<UrlRecord>(),
            MetaDescription = new List<UrlRecord>(),
            PageTitle = new List<UrlRecord>(),
            Iframes = new List<UrlRecord>(),
            ContentFinder = new List<MatchResult>()
        };
        await CrawlPageAsync(_baseUri, crawlResults);
        crawlResults.Iframes = _iframes;
        return crawlResults;
    }

    private async Task CrawlPageAsync(string url, CrawlResults crawlResults)
    {
        if (_visitedUrls.Contains(url)) return;
        _visitedUrls.Add(url);

        crawlResults.Pages.Add(new UrlRecord(url));

        Console.WriteLine($"Crawling: {url}");

        var content = await FetchContentAsync(url);
        if (string.IsNullOrWhiteSpace(content)) return;

        var links = ExtractLinks(content, url);
        CapturePageIFrames(content, url);

        var pageTitle = GetPageTitle(content);
        crawlResults.PageTitle.Add(new UrlRecord { Url = url, Attribute = pageTitle });

        var metaDescription = GetPageMetaDescription(content);
        crawlResults.MetaDescription.Add(new UrlRecord { Url = url, Attribute = metaDescription });
        
        var searchParameters = new string[]
        {
            @"Hackney\s+Money\s+Hub",
            @"the\s+Money\s+Hub",
            @"Hackney\s+(?:\w+\s+)*Here\s+To\s+Help"
        };

        var contentFinderResults = CaptureSpecificContent(content, url, searchParameters);
        crawlResults.ContentFinder.AddRange(contentFinderResults);
        
        // var hasContactInfo = CheckIfPageHasContactInfo(content);
        // crawlResults.HasContactInfo.Add(new UrlRecord { Url = url, Attribute = hasContactInfo.ToString() });

        foreach (var link in links)
        {
            await CrawlPageAsync(link, crawlResults);
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

    private string GetPageTitle(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var titleNode = doc.DocumentNode.SelectSingleNode("//title");

        return titleNode == null ? "No Title found" : titleNode.InnerText;
    }

    private string GetPageMetaDescription(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var metaDescriptionNode = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");
        var metaDescription = metaDescriptionNode?.GetAttributeValue("content", string.Empty) ?? string.Empty;

        return !string.IsNullOrEmpty(metaDescription) ? metaDescription : "No meta description found";
    }

    private void CapturePageIFrames(string htmlContent, string pageUrl)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var iframeTags = doc.DocumentNode.SelectNodes("//iframe[@src]");

        try
        {
            foreach (var iframe in iframeTags)
            {
                try
                {
                    var src = iframe.GetAttributeValue("src", "").Trim();
                    _iframes.Add(new UrlRecord(pageUrl) { Attribute = src });
                }
                catch
                {

                }
            }
        }
        catch
        {
            
        }
    }

    private bool CheckIfPageHasContactInfo(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        var textContent = doc.DocumentNode.SelectSingleNode("//body").InnerText;

        // Regular expressions for email and phone
        var emailPattern = @"[a-zA-Z0-9\.\-_]+@[a-zA-Z0-9\.\-]+\.[a-zA-Z]{2,}";
        var phonePattern = @"(\+?\d{1,3}[-.\s]?)?(\(?\d{1,4}\)?[-.\s]?){1,3}\d{1,9}";

        var emailMatches = Regex.Matches(textContent, emailPattern);
        var phoneMatches = Regex.Matches(textContent, phonePattern);

        if (emailMatches.Count > 0 || phoneMatches.Count > 0)
        {
            return true;
        }

        return false;
    }

    private List<MatchResult> CaptureSpecificContent(string htmlContent, string currentUrl, string[] searchParameters)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        
        // Remove script and style nodes
        doc.DocumentNode.Descendants()
            .Where(n => n.Name == "script" || n.Name == "style")
            .ToList()
            .ForEach(n => n.Remove());
        
        var textContent = doc.DocumentNode.InnerText;
        
        var results = new List<MatchResult>();
        
        foreach (var searchPattern in searchParameters)
        {
            var regex = new Regex(searchPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = regex.Matches(textContent);

            foreach (Match match in matches)
            {
                // Get surrounding context
                var contextRadius = 50;
                var start = Math.Max(match.Index - contextRadius, 0);
                var length = Math.Min(match.Length + 2 * contextRadius, textContent.Length - start);
                var context = textContent.Substring(start, length);

                results.Add(new MatchResult
                {
                    Url = currentUrl,
                    MatchedPhrase = match.Value,
                    Context = context
                });

                Console.WriteLine($"Found '{match.Value}' in {currentUrl}");
            }
        }
        
        return results;
    }

    private string GetAbsoluteUrl(string href, string currentUrl)
    {
        if (Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        if (Uri.TryCreate(new Uri(currentUrl), href, out var relativeUri))
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