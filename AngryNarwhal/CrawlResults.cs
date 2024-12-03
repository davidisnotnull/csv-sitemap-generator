namespace AngryNarwhal;

public class CrawlResults
{
    // Captures a list of all unique pages
    public List<UrlRecord> Pages { get; set; }

    // This is for capturing all of the iframes across the site
    public List<UrlRecord> Iframes { get; set; }

    // Captures a list of all meta descriptions for unique pages
    public List<UrlRecord> MetaDescription { get; set; }

    // Captures a list of all Page Titles for unique pages
    public List<UrlRecord> PageTitle { get; set; }
    
    // Captures a list of Contact Info (emails, phone numbers) across all unique pages
    public List<UrlRecord> HasContactInfo { get; set; }
    
    // Use this to capture pages that have specific words/phrases
    public List<MatchResult> ContentFinder { get; set; }
}