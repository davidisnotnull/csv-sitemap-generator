namespace AngryNarwhal;

public class UrlRecord()
{
    public UrlRecord(string url) : this()
    {
        Url = url;    
    }
    
    public string Url { get; set; }
    
    public string Attribute { get; set; }
}