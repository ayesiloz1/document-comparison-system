namespace DocumentComparer.Utils;

public static class LocalSimilarityEstimator
{
    // Simple heuristic similarity based on Levenshtein-like ratio using lengths and shared tokens
    public static double EstimateSimilarityFromText(string a, string b)
    {
        return EstimateSimilarity(a, b);
    }

    public static double EstimateSimilarity(List<DocumentComparer.Models.DiffSegment> segments)
    {
        if (segments == null || segments.Count == 0) return 1.0;
        // Count unchanged lines vs total lines
        var unchanged = segments.Count(s => s.Type == DocumentComparer.Models.ChangeType.Unchanged);
        var total = segments.Count;
        return (double)unchanged / Math.Max(1, total);
    }

    private static double EstimateSimilarity(string a, string b)
    {
        if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return 1.0;
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0.0;

        var sa = Tokenize(a);
        var sb = Tokenize(b);
        var intersect = sa.Intersect(sb).Count();
        var union = sa.Union(sb).Count();
        return union == 0 ? 0.0 : (double)intersect / union;
    }

    private static HashSet<string> Tokenize(string s)
    {
        var tokens = s
            .ToLowerInvariant()
            .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', ';', ':', '(', ')', '/', '\\', '"', '\'' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 2)
            .ToHashSet();
        return tokens;
    }
}

public static class FileHelpers
{
    public static bool IsPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;
            
        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
            return false;
            
        // Check content type
        return file.ContentType == "application/pdf";
    }
}
