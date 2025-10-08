using DocumentComparer.Models;

namespace DocumentComparer.Services;

public interface IDiffService
{
    List<DiffSegment> ComputeInlineDiff(string oldText, string newText);
    List<DiffSegment> ComputePageAwareDiff(PdfDocument docA, PdfDocument docB);
}
