using AngryNarwhal;

Console.WriteLine("AngryNarwhal v.0.1.0");

var baseUri = "https://hackney.gov.uk/"; // Replace with the website you want to crawl
var outputCsvPath = "sitemap.csv"; // Change this if you want to output the CSV to a location other than the bin folder
var outputMetadataPath = "metadata.csv";
var outputPageTitlePath = "pagetitles.csv";
var outputIframePath = "iframes.csv";
var outputContentPath = "content.csv";

var crawler = new WebCrawler(baseUri);
var results = await crawler.CrawlAsync();

CsvWriterHelper.WriteUrlRecordsToCsv(results.Pages, outputCsvPath);
CsvWriterHelper.WriteUrlRecordsToCsv(results.PageTitle, outputPageTitlePath);
CsvWriterHelper.WriteUrlRecordsToCsv(results.MetaDescription, outputMetadataPath);
CsvWriterHelper.WriteUrlRecordsToCsv(results.Iframes, outputIframePath);
CsvWriterHelper.WriteMatchResultsToCsv(results.ContentFinder, outputContentPath);

Console.WriteLine($"Crawling complete.");