namespace DocumentComparer.Models;

public class PdfDocument
{
    public string FullText { get; set; } = string.Empty;
    public List<string> Pages { get; set; } = new();
}