using DocumentComparer.Models;
using DocumentComparer.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text.RegularExpressions;

namespace DocumentComparer.Services;

public interface ISectionComparisonService
{
    Task<SectionComparisonResult> CompareDocumentSectionsAsync(IFormFile file1, IFormFile file2);
}

public class SectionComparisonService : ISectionComparisonService
{
    private readonly IOpenAiService _openAiService;
    private readonly IPdfService _pdfService;

    public SectionComparisonService(IOpenAiService openAiService, IPdfService pdfService)
    {
        _openAiService = openAiService;
        _pdfService = pdfService;
    }

    public async Task<SectionComparisonResult> CompareDocumentSectionsAsync(IFormFile file1, IFormFile file2)
    {
        // Extract sections from both documents
        var sectionsA = await ExtractDocumentSectionsAsync(file1, "Document A");
        var sectionsB = await ExtractDocumentSectionsAsync(file2, "Document B");

        // Compare sections intelligently
        var comparisons = await CompareSectionsAsync(sectionsA, sectionsB);

        // Calculate overall similarity
        var overallSimilarity = CalculateOverallSimilarity(comparisons);

        // Generate AI insights
        var aiInsights = await GenerateAISectionInsightsAsync(comparisons);

        return new SectionComparisonResult
        {
            DocumentAName = file1.FileName,
            DocumentBName = file2.FileName,
            DocumentASections = sectionsA,
            DocumentBSections = sectionsB,
            SectionComparisons = comparisons,
            OverallSimilarity = overallSimilarity,
            AISectionInsights = aiInsights,
            ComparisonTimestamp = DateTime.UtcNow
        };
    }

    private async Task<List<DocumentSection>> ExtractDocumentSectionsAsync(IFormFile file, string documentName)
    {
        var sections = new List<DocumentSection>();
        
        using var stream = file.OpenReadStream();
        using var pdfReader = new PdfReader(stream);
        using var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);

        for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
        {
            var page = pdfDocument.GetPage(pageNum);
            
            // Try multiple extraction strategies for better text recovery
            string pageText;
            try
            {
                // First try LocationTextExtractionStrategy for better formatting
                var locationStrategy = new LocationTextExtractionStrategy();
                pageText = PdfTextExtractor.GetTextFromPage(page, locationStrategy);
            }
            catch
            {
                // Fallback to SimpleTextExtractionStrategy
                var simpleStrategy = new SimpleTextExtractionStrategy();
                pageText = PdfTextExtractor.GetTextFromPage(page, simpleStrategy);
            }
            
            // Fix ligature issues in extracted text
            pageText = FixLigatureIssues(pageText);

            // Extract sections from this page
            var pageSections = ExtractSectionsFromPageText(pageText, pageNum, documentName);
            sections.AddRange(pageSections);
        }

        // Post-process sections to merge related content
        return MergeRelatedSections(sections);
    }

    private List<DocumentSection> ExtractSectionsFromPageText(string pageText, int pageNumber, string documentName)
    {
        var sections = new List<DocumentSection>();
        var lines = pageText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        var currentSection = new DocumentSection
        {
            SectionId = Guid.NewGuid().ToString(),
            PageNumber = pageNumber,
            DocumentName = documentName
        };

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Detect section headers (various patterns)
            if (IsLikelyHeader(line))
            {
                // Save previous section if it has content
                if (!string.IsNullOrEmpty(currentSection.Content))
                {
                    sections.Add(currentSection);
                }

                // Start new section
                currentSection = new DocumentSection
                {
                    SectionId = Guid.NewGuid().ToString(),
                    Title = line,
                    SectionType = DetermineSectionType(line),
                    PageNumber = pageNumber,
                    DocumentName = documentName,
                    Content = line + "\n"
                };
            }
            else
            {
                // Add to current section
                currentSection.Content += line + "\n";
            }
        }

        // Add the last section
        if (!string.IsNullOrEmpty(currentSection.Content))
        {
            sections.Add(currentSection);
        }

        return sections;
    }

    private bool IsLikelyHeader(string line)
    {
        // Check for common header patterns
        if (line.Length > 100) return false; // Too long to be a header
        
        // Numbered sections (1., 1.1, I., A., etc.)
        if (Regex.IsMatch(line, @"^\s*(?:\d+\.|\d+\.\d+\.?|[IVX]+\.?|[A-Z]\.)\s*[A-Z]", RegexOptions.IgnoreCase))
            return true;
            
        // All caps headers
        if (line.Length > 3 && line.ToUpper() == line && line.Any(char.IsLetter))
            return true;
            
        // Title case with limited words
        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= 8 && words.All(w => char.IsUpper(w[0]) || IsCommonWord(w.ToLower())))
            return true;
            
        return false;
    }

    private bool IsCommonWord(string word)
    {
        var commonWords = new[] { "and", "or", "of", "the", "a", "an", "in", "on", "at", "to", "for", "with" };
        return commonWords.Contains(word);
    }

    private string DetermineSectionType(string title)
    {
        var titleLower = title.ToLower();
        
        if (titleLower.Contains("introduction") || titleLower.Contains("overview"))
            return "Introduction";
        if (titleLower.Contains("conclusion") || titleLower.Contains("summary"))
            return "Conclusion";
        if (titleLower.Contains("table") || titleLower.Contains("figure"))
            return "Table/Figure";
        if (Regex.IsMatch(title, @"^\s*\d+\."))
            return "Numbered Section";
        if (title.ToUpper() == title)
            return "Major Header";
            
        return "Content Section";
    }

    private List<DocumentSection> MergeRelatedSections(List<DocumentSection> sections)
    {
        var mergedSections = new List<DocumentSection>();
        
        foreach (var section in sections)
        {
            // If content is too short, try to merge with previous section
            if (section.Content.Length < 50 && mergedSections.Any())
            {
                var lastSection = mergedSections.Last();
                if (lastSection.PageNumber == section.PageNumber)
                {
                    lastSection.Content += "\n" + section.Content;
                    continue;
                }
            }
            
            mergedSections.Add(section);
        }
        
        return mergedSections;
    }

    private async Task<List<SectionComparison>> CompareSectionsAsync(List<DocumentSection> sectionsA, List<DocumentSection> sectionsB)
    {
        var comparisons = new List<SectionComparison>();
        
        // Create a mapping for better section matching
        var processedB = new HashSet<string>();
        
        foreach (var sectionA in sectionsA)
        {
            var bestMatch = FindBestMatch(sectionA, sectionsB, processedB);
            
            if (bestMatch != null)
            {
                processedB.Add(bestMatch.SectionId);
                
                var comparison = await CreateSectionComparison(sectionA, bestMatch, SectionChangeType.Modified);
                comparisons.Add(comparison);
            }
            else
            {
                // Section was deleted
                var comparison = await CreateSectionComparison(sectionA, null, SectionChangeType.Deleted);
                comparisons.Add(comparison);
            }
        }
        
        // Handle sections that were added in document B
        foreach (var sectionB in sectionsB.Where(s => !processedB.Contains(s.SectionId)))
        {
            var comparison = await CreateSectionComparison(null, sectionB, SectionChangeType.Added);
            comparisons.Add(comparison);
        }
        
        return comparisons.OrderBy(c => c.PageNumberA ?? c.PageNumberB ?? 0).ToList();
    }

    private DocumentSection? FindBestMatch(DocumentSection sectionA, List<DocumentSection> sectionsB, HashSet<string> processedB)
    {
        DocumentSection? bestMatch = null;
        double bestSimilarity = 0.3; // Minimum threshold
        
        foreach (var sectionB in sectionsB.Where(s => !processedB.Contains(s.SectionId)))
        {
            var similarity = CalculateTextSimilarity(sectionA.Title + " " + sectionA.Content, 
                                                   sectionB.Title + " " + sectionB.Content);
            
            if (similarity > bestSimilarity)
            {
                bestSimilarity = similarity;
                bestMatch = sectionB;
            }
        }
        
        return bestMatch;
    }

    private double CalculateTextSimilarity(string text1, string text2)
    {
        // Simple Jaccard similarity with word n-grams
        var words1 = text1.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var words2 = text2.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        
        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();
        
        return union == 0 ? 0 : (double)intersection / union;
    }

    private async Task<SectionComparison> CreateSectionComparison(DocumentSection? sectionA, DocumentSection? sectionB, SectionChangeType changeType)
    {
        var comparison = new SectionComparison
        {
            ComparisonId = Guid.NewGuid().ToString(),
            SectionA = sectionA,
            SectionB = sectionB,
            ChangeType = changeType,
            PageNumberA = sectionA?.PageNumber,
            PageNumberB = sectionB?.PageNumber
        };
        
        if (sectionA != null && sectionB != null)
        {
            // Calculate detailed similarity
            comparison.SimilarityScore = CalculateTextSimilarity(sectionA.Content, sectionB.Content);
            
            // Determine if actually unchanged
            if (comparison.SimilarityScore > 0.95)
            {
                comparison.ChangeType = SectionChangeType.Unchanged;
            }
        }
        
        // Generate AI summary for this comparison
        comparison.AISummary = await GenerateSectionAISummary(comparison);
        
        return comparison;
    }

    private async Task<string> GenerateSectionAISummary(SectionComparison comparison)
    {
        try
        {
            var prompt = comparison.ChangeType switch
            {
                SectionChangeType.Added => $"Summarize this new section that was added:\n{comparison.SectionB?.Content}",
                SectionChangeType.Deleted => $"Summarize this section that was removed:\n{comparison.SectionA?.Content}",
                SectionChangeType.Modified => $"Summarize the changes between these sections:\nOriginal:\n{comparison.SectionA?.Content}\n\nNew:\n{comparison.SectionB?.Content}",
                _ => $"This section remained unchanged: {comparison.SectionA?.Title}"
            };
            
            return await _openAiService.GenerateSimpleSummaryAsync(prompt);
        }
        catch
        {
            return comparison.ChangeType switch
            {
                SectionChangeType.Added => "New section added",
                SectionChangeType.Deleted => "Section removed",
                SectionChangeType.Modified => "Section modified",
                _ => "Section unchanged"
            };
        }
    }

    private double CalculateOverallSimilarity(List<SectionComparison> comparisons)
    {
        if (!comparisons.Any()) return 0;
        
        var totalSections = comparisons.Count;
        var unchangedSections = comparisons.Count(c => c.ChangeType == SectionChangeType.Unchanged);
        var modifiedSections = comparisons.Where(c => c.ChangeType == SectionChangeType.Modified);
        
        var baseScore = (double)unchangedSections / totalSections;
        var modifiedScore = modifiedSections.Average(c => c.SimilarityScore ?? 0);
        
        return (baseScore + modifiedScore * modifiedSections.Count() / totalSections) / 
               (1 + modifiedSections.Count() / (double)totalSections);
    }

    private async Task<AISectionInsights> GenerateAISectionInsightsAsync(List<SectionComparison> comparisons)
    {
        try
        {
            var addedCount = comparisons.Count(c => c.ChangeType == SectionChangeType.Added);
            var deletedCount = comparisons.Count(c => c.ChangeType == SectionChangeType.Deleted);
            var modifiedCount = comparisons.Count(c => c.ChangeType == SectionChangeType.Modified);
            
            var overallSummary = await _openAiService.GenerateSimpleSummaryAsync(
                $"Analyze this document comparison: {addedCount} sections added, {deletedCount} sections deleted, " +
                $"{modifiedCount} sections modified. Provide a brief executive summary.");
            
            return new AISectionInsights
            {
                OverallSummary = overallSummary,
                KeyChanges = comparisons.Where(c => c.ChangeType != SectionChangeType.Unchanged).Take(5).ToList(),
                ChangeStatistics = new ChangeStatistics
                {
                    AddedSections = addedCount,
                    DeletedSections = deletedCount,
                    ModifiedSections = modifiedCount,
                    UnchangedSections = comparisons.Count(c => c.ChangeType == SectionChangeType.Unchanged)
                }
            };
        }
        catch
        {
            return new AISectionInsights
            {
                OverallSummary = "AI analysis temporarily unavailable",
                KeyChanges = comparisons.Where(c => c.ChangeType != SectionChangeType.Unchanged).Take(5).ToList(),
                ChangeStatistics = new ChangeStatistics
                {
                    AddedSections = comparisons.Count(c => c.ChangeType == SectionChangeType.Added),
                    DeletedSections = comparisons.Count(c => c.ChangeType == SectionChangeType.Deleted),
                    ModifiedSections = comparisons.Count(c => c.ChangeType == SectionChangeType.Modified),
                    UnchangedSections = comparisons.Count(c => c.ChangeType == SectionChangeType.Unchanged)
                }
            };
        }
    }

    private static string FixLigatureIssues(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Common ligature replacements
        var ligatureReplacements = new Dictionary<string, string>
        {
            // Unicode ligatures that might appear as empty boxes
            { "\uFB01", "fi" },  // ﬁ ligature
            { "\uFB02", "fl" },  // ﬂ ligature
            { "\uFB03", "ffi" }, // ﬃ ligature
            { "\uFB04", "ffl" }, // ﬄ ligature
            { "\uFB00", "ff" },  // ﬀ ligature
            { "\uFB05", "ft" },  // ﬅ ligature
            { "\uFB06", "st" },  // ﬆ ligature
            
            // Handle empty boxes or replacement characters
            { "\uFFFD", "" },   // Unicode replacement character
            
            // Common OCR/extraction issues
            { "Ɵ", "ti" },      // Sometimes ti appears as this character
            { "ɵ", "ti" },      // Alternative ti replacement
            { "ʄ", "ft" },      // Sometimes ft appears as this character
        };

        foreach (var replacement in ligatureReplacements)
        {
            text = text.Replace(replacement.Key, replacement.Value);
        }

        return text;
    }
}