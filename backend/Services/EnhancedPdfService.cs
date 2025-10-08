using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Geom;
using System.Text;
using System.Text.RegularExpressions;
using DocumentComparer.Models;

namespace DocumentComparer.Services;

public class EnhancedPdfService : IPdfService
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
            var sections = new List<DocumentSection>();
            
            // Reset stream position to beginning
            stream.Position = 0;
            
            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);
            
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var text = PdfTextExtractor.GetTextFromPage(page);
                
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var cleanText = text.Trim();
                    pages.Add(cleanText);
                    sb.AppendLine(cleanText);
                    
                    // Extract sections from this page
                    var pageSections = ExtractSectionsFromPage(cleanText, i);
                    sections.AddRange(pageSections);
                }
            }

            return new Models.PdfDocument
            {
                FileName = fileName,
                FullText = sb.ToString().Trim(),
                Pages = pages,
                Sections = sections
            };
        });
    }

    public async Task<PdfDocumentWithCoordinates> ExtractWithCoordinatesAsync(Stream stream, string fileName)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            var pagesWithCoords = new List<PdfPageWithCoordinates>();
            var sections = new List<DocumentSection>();
            
            // Reset stream position to beginning
            stream.Position = 0;
            
            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);
            
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                
                // Extract text with coordinates
                var strategy = new SimpleTextExtractionStrategy();
                var locationStrategy = new LocationTextExtractionStrategy();
                
                var text = PdfTextExtractor.GetTextFromPage(page, locationStrategy);
                var simpleText = PdfTextExtractor.GetTextFromPage(page, strategy);
                
                if (!string.IsNullOrWhiteSpace(simpleText))
                {
                    var cleanText = simpleText.Trim();
                    sb.AppendLine(cleanText);
                    
                    // Extract sections from this page
                    var pageSections = ExtractSectionsFromPage(cleanText, i);
                    sections.AddRange(pageSections);
                    
                    // Create page with coordinates
                    var pageWithCoords = new PdfPageWithCoordinates
                    {
                        PageNumber = i,
                        Text = cleanText,
                        TextBlocks = ExtractTextBlocks(cleanText),
                        Sections = pageSections
                    };
                    
                    pagesWithCoords.Add(pageWithCoords);
                }
            }

            return new PdfDocumentWithCoordinates
            {
                FileName = fileName,
                FullText = sb.ToString().Trim(),
                Pages = pagesWithCoords,
                Sections = sections
            };
        });
    }

    private List<DocumentSection> ExtractSectionsFromPage(string pageText, int pageNumber)
    {
        var sections = new List<DocumentSection>();
        var lines = pageText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Detect headers/titles (various patterns)
            if (IsLikelyHeader(line))
            {
                sections.Add(new DocumentSection
                {
                    Title = line,
                    PageNumber = pageNumber,
                    LineNumber = i + 1,
                    SectionType = GetSectionType(line),
                    Content = ExtractSectionContent(lines, i)
                });
            }
        }
        
        return sections;
    }

    private bool IsLikelyHeader(string line)
    {
        // Check for common header patterns
        return Regex.IsMatch(line, @"^\d+\.?\s+[A-Z]") || // 1. TITLE or 1 TITLE
               Regex.IsMatch(line, @"^[A-Z][A-Z\s]{2,}$") || // ALL CAPS
               Regex.IsMatch(line, @"^\d+\.\d+") || // 1.1, 2.3, etc.
               line.Length < 100 && line.Count(c => char.IsUpper(c)) > line.Length * 0.6; // Mostly uppercase, short
    }

    private string GetSectionType(string title)
    {
        var upperTitle = title.ToUpper();
        if (upperTitle.Contains("INTRODUCTION") || upperTitle.Contains("OVERVIEW"))
            return "Introduction";
        if (upperTitle.Contains("REQUIREMENT") || upperTitle.Contains("FUNCTIONAL"))
            return "Requirements";
        if (upperTitle.Contains("TECHNICAL") || upperTitle.Contains("ARCHITECTURE"))
            return "Technical";
        if (upperTitle.Contains("CONCLUSION") || upperTitle.Contains("SUMMARY"))
            return "Conclusion";
        
        return "Section";
    }

    private string ExtractSectionContent(string[] lines, int headerIndex)
    {
        var content = new StringBuilder();
        
        // Get content until next header or end
        for (int i = headerIndex + 1; i < lines.Length; i++)
        {
            if (IsLikelyHeader(lines[i]))
                break;
                
            content.AppendLine(lines[i]);
        }
        
        return content.ToString().Trim();
    }

    private List<TextBlock> ExtractTextBlocks(string text)
    {
        var blocks = new List<TextBlock>();
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < lines.Length; i++)
        {
            blocks.Add(new TextBlock
            {
                Text = lines[i].Trim(),
                LineNumber = i + 1,
                X = 0, // Would need more complex parsing for exact coordinates
                Y = i * 20, // Approximate line height
                Width = lines[i].Length * 8, // Approximate character width
                Height = 20
            });
        }
        
        return blocks;
    }
}

// New models for enhanced PDF handling
public class PdfDocumentWithCoordinates
{
    public string FileName { get; set; } = string.Empty;
    public string FullText { get; set; } = string.Empty;
    public List<PdfPageWithCoordinates> Pages { get; set; } = new();
    public List<DocumentSection> Sections { get; set; } = new();
}

public class PdfPageWithCoordinates
{
    public int PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<TextBlock> TextBlocks { get; set; } = new();
    public List<DocumentSection> Sections { get; set; } = new();
}

public class TextBlock
{
    public string Text { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}