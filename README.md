# Angry Narwhal
A C# console app that will crawl all of the publically available pages within a website, capture each page URL, and store that in a CSV.

To set the domain that you want to crawl, change the `baseUrl` value in the `Program.cs` file, e.g.

```c#
using AngryNarwhal;

Console.WriteLine("AngryNarwhal v.0.1.0");

var baseUri = "https://examplesite.com"; // Replace with the website you want to crawl
var outputCsvPath = "sitemap.csv"; // Change this if you want to output the CSV to a location other than the bin folder

var crawler = new WebCrawler(baseUri);
var pages = await crawler.CrawlAsync();

CsvWriterHelper.WriteUrlsToCsv(pages, outputCsvPath);

Console.WriteLine($"Crawling complete. Sitemap saved to {outputCsvPath}");
```

By default, the CSV will be saved in the build folder, so look under `bin` or change the value of the `outputCsvPath` variable to point to a specific location.