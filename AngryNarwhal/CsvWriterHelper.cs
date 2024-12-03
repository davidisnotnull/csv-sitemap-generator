using System.Globalization;
using CsvHelper;

namespace AngryNarwhal;

public class CsvWriterHelper
{
    public static void WriteUrlRecordsToCsv(List<UrlRecord> results, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteHeader<UrlRecord>();
            csv.NextRecord();

            foreach (var result in results)
            {
                csv.WriteRecord(result);
                csv.NextRecord();
            }
        }
    }

    public static void WriteMatchResultsToCsv(List<MatchResult> results, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteHeader<MatchResult>();
            csv.NextRecord();
            
            foreach (var result in results)
            {
                csv.WriteRecord(result);
                csv.NextRecord();
            }
        }
    }
}