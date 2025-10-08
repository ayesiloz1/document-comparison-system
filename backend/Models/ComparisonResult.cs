namespace DocumentComparer.Models;

public class ComparisonResult
{
    public string Summary { get; set; } = string.Empty;
    public double SimilarityScore { get; set; } = 0.0; // 0..1
    public List<DiffSegment> DiffSegments { get; set; } = new();
    public AIInsight? AIInsights { get; set; }
}
