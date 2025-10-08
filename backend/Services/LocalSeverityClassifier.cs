using DocumentComparer.Models;

namespace DocumentComparer.Services;

/// <summary>
/// Lightweight heuristic severity classifier. Fast, offline.
/// You can replace or augment with an OpenAI-based classification if needed.
/// </summary>
public class LocalSeverityClassifier : ISeverityClassifier
{
    public List<DiffSegment> Classify(List<DiffSegment> diffSegments)
    {
        var result = new List<DiffSegment>();
        foreach (var seg in diffSegments)
        {
            seg.Severity = EstimateSeverity(seg);
            result.Add(seg);
        }
        return result;
    }

    private Severity EstimateSeverity(DiffSegment seg)
    {
        if (seg.Type == ChangeType.Unchanged)
            return Severity.Minor;

        var text = seg.Text ?? "";
        var len = text.Length;

        // Heuristics: big insertions/deletions => more severe; numbers or specific tokens increase severity
        var numericChange = text.Any(c => char.IsDigit(c)) ? 1 : 0;
        var legalKeywords = new[] { "shall", "must", "notwithstanding", "liability", "indemnif", "warrant", "penalty", "terminate", "termination" };
        var containsLegal = legalKeywords.Any(k => text.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0) ? 1 : 0;

        var score = 0;
        if (len > 300) score += 2;
        else if (len > 100) score += 1;

        score += numericChange;
        score += containsLegal * 2;

        if (score >= 4) return Severity.Major;
        if (score >= 2) return Severity.Moderate;
        return Severity.Minor;
    }
}
