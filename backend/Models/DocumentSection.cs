namespace DocumentComparer.Models;

public class DocumentSection
{
    public string Title { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int LineNumber { get; set; }
    public string SectionType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
