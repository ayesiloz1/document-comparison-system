namespace DocumentComparer.Models;

public class PdfDocument
{
    public string FileName { get; set; } = string.Empty;
    public string FullText { get; set; } = string.Empty;
    public List<string> Pages { get; set; } = new();
    public List<DocumentSection> Sections { get; set; } = new();
}