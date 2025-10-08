namespace DocumentComparer.Models;

public class SectionComparisonResult
{
    public string DocumentAName { get; set; } = string.Empty;
    public string DocumentBName { get; set; } = string.Empty;
    public List<DocumentSection> DocumentASections { get; set; } = new();
    public List<DocumentSection> DocumentBSections { get; set; } = new();
    public List<SectionComparison> SectionComparisons { get; set; } = new();
    public double OverallSimilarity { get; set; }
    public AISectionInsights AISectionInsights { get; set; } = new();
    public DateTime ComparisonTimestamp { get; set; }
}

public class SectionComparison
{
    public string ComparisonId { get; set; } = string.Empty;
    public DocumentSection? SectionA { get; set; }
    public DocumentSection? SectionB { get; set; }
    public SectionChangeType ChangeType { get; set; }
    public double? SimilarityScore { get; set; }
    public int? PageNumberA { get; set; }
    public int? PageNumberB { get; set; }
    public string AISummary { get; set; } = string.Empty;
    public ChangeSeverity Severity { get; set; } = ChangeSeverity.Low;
}

public class AISectionInsights
{
    public string OverallSummary { get; set; } = string.Empty;
    public List<SectionComparison> KeyChanges { get; set; } = new();
    public ChangeStatistics ChangeStatistics { get; set; } = new();
}

public class ChangeStatistics
{
    public int AddedSections { get; set; }
    public int DeletedSections { get; set; }
    public int ModifiedSections { get; set; }
    public int UnchangedSections { get; set; }
    
    public int TotalSections => AddedSections + DeletedSections + ModifiedSections + UnchangedSections;
    public double ChangePercentage => TotalSections == 0 ? 0 : 
        (double)(AddedSections + DeletedSections + ModifiedSections) / TotalSections * 100;
}

public enum SectionChangeType
{
    Unchanged,
    Added,
    Deleted,
    Modified
}

public enum ChangeSeverity
{
    Low,
    Medium,
    High,
    Critical
}