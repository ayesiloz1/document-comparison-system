namespace DocumentComparer.Models;

public class ComparisonSummary
{
    public string SummaryText { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
}
