using DocumentComparer.Models;

namespace DocumentComparer.Services
{
    public interface IPdfService
    {
        string ExtractText(string filePath);
        (string TextA, string TextB) ExtractBoth(string pathA, string pathB);
        Task<PdfDocument> ExtractAsync(Stream stream, string fileName);
    }
}
