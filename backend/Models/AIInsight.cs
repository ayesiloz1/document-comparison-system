namespace DocumentComparer.Models;

public class AIInsight
{
    public string Summary { get; set; } = string.Empty;
    public List<string> KeyChanges { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string Impact { get; set; } = string.Empty;
}