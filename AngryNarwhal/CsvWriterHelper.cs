using System.Globalization;
using CsvHelper;

namespace AngryNarwhal;

public class CsvWriterHelper
{
    public static void WriteUrlsToCsv(List<string> urls, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteHeader<UrlRecord>();
            csv.NextRecord();

            foreach (var url in urls)
            {
                csv.WriteRecord(new UrlRecord { Url = url });
                csv.NextRecord();
            }
        }
    }
}