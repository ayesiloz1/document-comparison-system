using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;
using DocumentComparer.Models;

namespace DocumentComparer.Services;

public class PdfService : IPdfService
{
    public string ExtractText(string filePath)
    {
        var sb = new StringBuilder();
        
        using var pdfReader = new PdfReader(filePath);
        using var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);
        
        for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
        {
            var page = pdfDocument.GetPage(i);
            var text = PdfTextExtractor.GetTextFromPage(page);
            
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text.Trim());
            }
        }

        return sb.ToString().Trim();
    }

    public (string TextA, string TextB) ExtractBoth(string pathA, string pathB)
    {
        return (ExtractText(pathA), ExtractText(pathB));
    }

    public async Task<Models.PdfDocument> ExtractAsync(Stream stream, string fileName)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            var pages = new List<string>();
            
            // Reset stream position to beginning
            stream.Position = 0;
            
            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);
            
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var pageText = PdfTextExtractor.GetTextFromPage(page);
                
                // Clean and filter the extracted text
                var cleanText = CleanPdfText(pageText);
                
                if (!string.IsNullOrWhiteSpace(cleanText))
                {
                    pages.Add(cleanText);
                    sb.AppendLine(cleanText);
                }
            }

            var fullText = sb.ToString().Trim();
            
            // If we got very little text, it might be a scanned PDF or have extraction issues
            if (fullText.Length < 50)
            {
                fullText = "Warning: Limited text extracted. This might be a scanned PDF or contain mostly images.";
            }

            return new Models.PdfDocument
            {
                FullText = fullText,
                Pages = pages
            };
        });
    }

    private static string CleanPdfText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var cleanLines = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Skip obvious PDF structure references and artifacts
            if (string.IsNullOrEmpty(trimmed) ||
                trimmed.Contains(" 0 R") ||  // PDF object references
                trimmed.Contains("<<") ||    // PDF dictionary markers
                trimmed.Contains(">>") ||
                trimmed.Contains("/Type") ||
                trimmed.Contains("/Filter") ||
                trimmed.Contains("/Length") ||
                trimmed.Contains("/Subtype") ||
                trimmed.Contains("/Producer") ||
                trimmed.Contains("/Creator") ||
                trimmed.Length < 2 || // Very short lines are usually artifacts
                trimmed.All(c => char.IsDigit(c) || char.IsWhiteSpace(c) || c == '.')) // Just numbers
            {
                continue;
            }

            cleanLines.Add(trimmed);
        }

        return string.Join("\n", cleanLines);
    }
}
