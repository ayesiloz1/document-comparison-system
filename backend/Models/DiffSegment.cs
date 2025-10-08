namespace DocumentComparer.Models;

public class DiffSegment
{
    public ChangeType Type { get; set; }
    public string Text { get; set; } = string.Empty;

    // If helpful, store approximate page or character offsets (optional)
    public int? PageNumberA { get; set; } // page in doc A (if applicable)
    public int? PageNumberB { get; set; } // page in doc B (if applicable)

    // Local severity
    public Severity Severity { get; set; } = Severity.Minor;
}
