using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DocumentComparer.Models;

namespace DocumentComparer.Services;

public class DiffService : IDiffService
{
    public List<DiffSegment> ComputeInlineDiff(string oldText, string newText)
    {
        var builder = new InlineDiffBuilder(new Differ());
        var model = builder.BuildDiffModel(oldText ?? string.Empty, newText ?? string.Empty);

        var segments = new List<DiffSegment>();

        foreach (var line in model.Lines)
        {
            var seg = new DiffSegment
            {
                Type = MapChangeType(line.Type),
                Text = line.Text ?? string.Empty,
                PageNumberA = null,
                PageNumberB = null
            };
            segments.Add(seg);
        }

        return segments;
    }

    public List<DiffSegment> ComputePageAwareDiff(Models.PdfDocument docA, Models.PdfDocument docB)
    {
        var segments = new List<DiffSegment>();
        var maxPages = Math.Max(docA.Pages.Count, docB.Pages.Count);

        for (int pageIndex = 0; pageIndex < maxPages; pageIndex++)
        {
            var pageA = pageIndex < docA.Pages.Count ? docA.Pages[pageIndex] : string.Empty;
            var pageB = pageIndex < docB.Pages.Count ? docB.Pages[pageIndex] : string.Empty;
            var pageNumber = pageIndex + 1;

            if (string.IsNullOrWhiteSpace(pageA) && string.IsNullOrWhiteSpace(pageB))
                continue;

            // If both pages exist, do line-by-line comparison
            if (!string.IsNullOrWhiteSpace(pageA) && !string.IsNullOrWhiteSpace(pageB))
            {
                var builder = new InlineDiffBuilder(new Differ());
                var model = builder.BuildDiffModel(pageA, pageB);

                foreach (var line in model.Lines)
                {
                    if (string.IsNullOrWhiteSpace(line.Text))
                        continue;

                    var seg = new DiffSegment
                    {
                        Type = MapChangeType(line.Type),
                        Text = line.Text ?? string.Empty,
                        PageNumberA = line.Type != DiffPlex.DiffBuilder.Model.ChangeType.Inserted ? pageNumber : null,
                        PageNumberB = line.Type != DiffPlex.DiffBuilder.Model.ChangeType.Deleted ? pageNumber : null
                    };
                    segments.Add(seg);
                }
            }
            // Page only in document A (deleted)
            else if (!string.IsNullOrWhiteSpace(pageA))
            {
                var lines = pageA.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    segments.Add(new DiffSegment
                    {
                        Type = DocumentComparer.Models.ChangeType.Deleted,
                        Text = line.Trim(),
                        PageNumberA = pageNumber,
                        PageNumberB = null
                    });
                }
            }
            // Page only in document B (inserted)
            else if (!string.IsNullOrWhiteSpace(pageB))
            {
                var lines = pageB.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    segments.Add(new DiffSegment
                    {
                        Type = DocumentComparer.Models.ChangeType.Inserted,
                        Text = line.Trim(),
                        PageNumberA = null,
                        PageNumberB = pageNumber
                    });
                }
            }
        }

        return segments;
    }

    // Explicitly qualify DiffPlex type name to avoid ambiguity
    private DocumentComparer.Models.ChangeType MapChangeType(DiffPlex.DiffBuilder.Model.ChangeType diffType)
    {
        return diffType switch
        {
            DiffPlex.DiffBuilder.Model.ChangeType.Inserted => DocumentComparer.Models.ChangeType.Inserted,
            DiffPlex.DiffBuilder.Model.ChangeType.Deleted => DocumentComparer.Models.ChangeType.Deleted,
            DiffPlex.DiffBuilder.Model.ChangeType.Modified => DocumentComparer.Models.ChangeType.Modified,
            DiffPlex.DiffBuilder.Model.ChangeType.Unchanged => DocumentComparer.Models.ChangeType.Unchanged,
            _ => DocumentComparer.Models.ChangeType.Modified
        };
    }
}
