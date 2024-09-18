using AngryNarwhal;

Console.WriteLine("AngryNarwhal v.0.1.0");

var baseUri = "https://examplesite.com"; // Replace with the website you want to crawl
var outputCsvPath = "sitemap.csv"; // Change this if you want to output the CSV to a location other than the bin folder

var crawler = new WebCrawler(baseUri);
var pages = await crawler.CrawlAsync();

CsvWriterHelper.WriteUrlsToCsv(pages, outputCsvPath);

Console.WriteLine($"Crawling complete. Sitemap saved to {outputCsvPath}");