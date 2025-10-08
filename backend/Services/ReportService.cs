using DocumentComparer.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace DocumentComparer.Services;

public class ReportService : IReportService
{
    public byte[] GeneratePdfReport(ComparisonResult result)
    {
        // Create ascii-safe summary to include in report
        var summary = result.Summary ?? "";

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(25);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Text($"Document Comparison Report").SemiBold().FontSize(18);
                page.Content().Column(col =>
                {
                    col.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(10);
                    col.Item().Text("Summary:").Bold();
                    col.Item().Text(summary).FontSize(11);
                    col.Item().Text($"Similarity score: {result.SimilarityScore:0.00}").FontSize(11);

                    col.Item().Text("Detailed differences:").Bold();

                    // For performance, limit to first 500 segments in the PDF
                    var segments = result.DiffSegments.Take(500).ToList();

                    foreach (var seg in segments)
                    {
                        var label = seg.Type.ToString();
                        var sev = seg.Severity.ToString();
                        col.Item().Text($"{label} [{sev}] - {Truncate(seg.Text, 400)}").FontSize(9);
                    }

                    if (result.DiffSegments.Count > 500)
                    {
                        col.Item().Text($"... and {result.DiffSegments.Count - 500} more segments omitted").FontSize(9);
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text($"DocumentComparer • {DateTime.UtcNow:yyyy-MM-dd}").FontSize(9);
            });
        });

        using var ms = new MemoryStream();
        doc.GeneratePdf(ms);
        return ms.ToArray();
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s ?? "";
        return s.Length <= max ? s : s.Substring(0, max) + "…";
    }
}
