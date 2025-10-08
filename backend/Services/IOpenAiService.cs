using DocumentComparer.Models;

namespace DocumentComparer.Services;

public record OpenAiOptions
{
    public string Endpoint { get; init; } = "";
    public string ApiKey { get; init; } = "";
    public string DeploymentOrModelName { get; init; } = "gpt-4o-mini";
}

public interface IOpenAiService
{
    /// <summary>
    /// Ask Azure OpenAI to produce a concise semantic summary and similarity score.
    /// </summary>
    Task<ComparisonSummary> SummarizeChangesAsync(string oldText, string newText, CancellationToken cancellationToken = default);

    /// <summary>
    /// (Optional) Use OpenAI to classify a set of diff segments into severity categories.
    /// </summary>
    Task<List<(int index, string severity)>> ClassifySegmentsAsync(List<string> segmentTexts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate comprehensive AI insights about document changes
    /// </summary>
    Task<AIInsight> GenerateInsightsAsync(string oldText, string newText, List<DiffSegment> diffSegments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate detailed report content for export
    /// </summary>
    string GenerateDetailedReport(string context);
}
