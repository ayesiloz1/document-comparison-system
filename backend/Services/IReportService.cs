using DocumentComparer.Models;

namespace DocumentComparer.Services;

public interface IReportService
{
    byte[] GeneratePdfReport(ComparisonResult result);
}
