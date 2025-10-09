using DocumentComparer.Models;
using DocumentComparer.Services;
using DocumentComparer.Utils;
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
            pageText = TextNormalizer.NormalizeLigatures(pageText);

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
                    Title = TextNormalizer.NormalizeSectionTitle(line),
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
        var comparisonTasks = new List<Task<SectionComparison>>();
        
        // Create a mapping for better section matching
        var processedB = new HashSet<string>();
        
        foreach (var sectionA in sectionsA)
        {
            var bestMatch = FindBestMatch(sectionA, sectionsB, processedB);
            
            if (bestMatch != null)
            {
                processedB.Add(bestMatch.SectionId);
                comparisonTasks.Add(CreateSectionComparisonAsync(sectionA, bestMatch, SectionChangeType.Modified));
            }
            else
            {
                // Section was deleted
                comparisonTasks.Add(CreateSectionComparisonAsync(sectionA, null, SectionChangeType.Deleted));
            }
        }
        
        // Handle sections that were added in document B
        foreach (var sectionB in sectionsB.Where(s => !processedB.Contains(s.SectionId)))
        {
            comparisonTasks.Add(CreateSectionComparisonAsync(null, sectionB, SectionChangeType.Added));
        }
        
        // Execute all AI analysis in parallel with concurrency limit
        var comparisons = await ExecuteWithConcurrencyLimit(comparisonTasks, maxConcurrency: 5);
        
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

    private async Task<List<T>> ExecuteWithConcurrencyLimit<T>(List<Task<T>> tasks, int maxConcurrency)
    {
        var results = new List<T>();
        var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        
        var throttledTasks = tasks.Select(async task =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await task;
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        return (await Task.WhenAll(throttledTasks)).ToList();
    }

    private async Task<SectionComparison> CreateSectionComparisonAsync(DocumentSection? sectionA, DocumentSection? sectionB, SectionChangeType changeType)
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
        
        // Generate AI summary for this comparison (use fast mode for efficiency)
        comparison.AISummary = await GenerateFastSectionAISummary(comparison);
        
        return comparison;
    }

    private async Task<string> GenerateSectionAISummary(SectionComparison comparison)
    {
        try
        {
            var prompt = comparison.ChangeType switch
            {
                SectionChangeType.Added => $@"Analyze this newly added section in detail. Explain what it covers, its purpose, key requirements or features it introduces, and how it enhances the document's scope:

SECTION TITLE: {comparison.SectionB?.Title}
SECTION CONTENT:
{comparison.SectionB?.Content}

Provide a comprehensive analysis of this addition, including its business significance and what capabilities or requirements it introduces.",

                SectionChangeType.Deleted => $@"Analyze this removed section in detail. Explain what functionality, requirements, or content was eliminated, why this might have been removed, and what impact this removal has on the overall document:

REMOVED SECTION TITLE: {comparison.SectionA?.Title}
REMOVED SECTION CONTENT:
{comparison.SectionA?.Content}

Provide a thorough analysis of what was lost and the potential implications of this removal.",

                SectionChangeType.Modified => $@"Perform a detailed comparative analysis of these modified sections. Identify specific changes, additions, enhancements, and any removed elements. Explain the business rationale behind these changes and their significance:

ORIGINAL VERSION:
Title: {comparison.SectionA?.Title}
Content: {comparison.SectionA?.Content}

UPDATED VERSION:
Title: {comparison.SectionB?.Title}
Content: {comparison.SectionB?.Content}

Provide a comprehensive analysis highlighting: 1) What specifically changed, 2) What was added or enhanced, 3) What was removed or simplified, 4) The likely business reasons for these changes, 5) The impact on requirements or functionality.",

                _ => $@"Analyze this unchanged section and explain its continued relevance and importance to the document:

SECTION: {comparison.SectionA?.Title}
CONTENT: {comparison.SectionA?.Content}

Explain why this section remained stable and its ongoing significance."
            };
            
            return await _openAiService.GenerateSimpleSummaryAsync(prompt);
        }
        catch
        {
            return comparison.ChangeType switch
            {
                SectionChangeType.Added => "New section added - detailed analysis unavailable",
                SectionChangeType.Deleted => "Section removed - detailed analysis unavailable", 
                SectionChangeType.Modified => "Section modified - detailed analysis unavailable",
                _ => "Section unchanged - detailed analysis unavailable"
            };
        }
    }

    private async Task<string> GenerateFastSectionAISummary(SectionComparison comparison)
    {
        try
        {
            var prompt = comparison.ChangeType switch
            {
                SectionChangeType.Added => $"Briefly summarize this new section: {comparison.SectionB?.Title ?? "Untitled"}\nContent: {TruncateContent(comparison.SectionB?.Content, 200)}",
                SectionChangeType.Deleted => $"Briefly summarize this removed section: {comparison.SectionA?.Title ?? "Untitled"}\nContent: {TruncateContent(comparison.SectionA?.Content, 200)}",
                SectionChangeType.Modified => $"Briefly describe changes in '{comparison.SectionA?.Title ?? "Section"}': Original had {comparison.SectionA?.Content?.Length ?? 0} chars, new has {comparison.SectionB?.Content?.Length ?? 0} chars. Key changes: {GetKeyChangesHint(comparison)}",
                _ => $"Section '{comparison.SectionA?.Title ?? "Untitled"}' remained unchanged."
            };
            
            return await _openAiService.GenerateFastSummaryAsync(prompt);
        }
        catch
        {
            return comparison.ChangeType switch
            {
                SectionChangeType.Added => $"New section added: {comparison.SectionB?.Title ?? "Untitled"}",
                SectionChangeType.Deleted => $"Section removed: {comparison.SectionA?.Title ?? "Untitled"}",
                SectionChangeType.Modified => $"Section modified: {comparison.SectionA?.Title ?? "Untitled"}",
                _ => $"Section unchanged: {comparison.SectionA?.Title ?? "Untitled"}"
            };
        }
    }

    private string TruncateContent(string? content, int maxLength)
    {
        if (string.IsNullOrEmpty(content)) return "No content";
        return content.Length <= maxLength ? content : content.Substring(0, maxLength) + "...";
    }

    private string GetKeyChangesHint(SectionComparison comparison)
    {
        var oldLength = comparison.SectionA?.Content?.Length ?? 0;
        var newLength = comparison.SectionB?.Content?.Length ?? 0;
        
        if (newLength > oldLength * 1.5) return "significant expansion";
        if (newLength < oldLength * 0.5) return "significant reduction";
        if (Math.Abs(newLength - oldLength) < 50) return "minor text changes";
        return "moderate changes";
    }

    private double CalculateOverallSimilarity(List<SectionComparison> comparisons)
    {
        if (!comparisons.Any()) return 0;
        
        var totalSections = comparisons.Count;
        var unchangedSections = comparisons.Count(c => c.ChangeType == SectionChangeType.Unchanged);
        var modifiedSections = comparisons.Where(c => c.ChangeType == SectionChangeType.Modified).ToList();
        
        var baseScore = (double)unchangedSections / totalSections;
        
        // Handle case where there are no modified sections
        var modifiedScore = modifiedSections.Any() 
            ? modifiedSections.Average(c => c.SimilarityScore ?? 0) 
            : 0;
        
        if (!modifiedSections.Any())
        {
            return baseScore; // If no modified sections, return just the base score
        }
        
        return (baseScore + modifiedScore * modifiedSections.Count / totalSections) / 
               (1 + modifiedSections.Count / (double)totalSections);
    }

    private async Task<AISectionInsights> GenerateAISectionInsightsAsync(List<SectionComparison> comparisons)
    {
        try
        {
            var addedCount = comparisons.Count(c => c.ChangeType == SectionChangeType.Added);
            var deletedCount = comparisons.Count(c => c.ChangeType == SectionChangeType.Deleted);
            var modifiedCount = comparisons.Count(c => c.ChangeType == SectionChangeType.Modified);
            
            var totalSections = comparisons.Count;
            var changedSections = addedCount + deletedCount + modifiedCount;
            var changePercentage = totalSections > 0 ? (double)changedSections / totalSections * 100 : 0;
            
            var overallSummary = await _openAiService.GenerateFastSummaryAsync(
                $@"Document comparison: {addedCount} added, {deletedCount} deleted, {modifiedCount} modified sections out of {totalSections} total ({changePercentage:F0}% changed). " +
                $"Assess if this is a {(changePercentage > 70 ? "major overhaul" : changePercentage > 30 ? "significant update" : "minor revision")} and provide brief business impact analysis.");
            
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


}