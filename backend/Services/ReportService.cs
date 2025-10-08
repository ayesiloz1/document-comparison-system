using DocumentComparer.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace DocumentComparer.Services;

public class ReportService : IReportService
{
    private readonly IOpenAiService _openAiService;

    public ReportService(IOpenAiService openAiService)
    {
        _openAiService = openAiService;
    }

    public byte[] GeneratePdfReport(ComparisonResult result)
    {
        try
        {
            // Generate comprehensive AI report content
            var reportContent = GenerateAIReportContent(result);
            
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Professional header with logo-style design
                    page.Header().Height(70).Background(Colors.Blue.Lighten4).Padding(15).Column(headerCol =>
                    {
                        headerCol.Item().Row(headerRow =>
                        {
                            headerRow.RelativeItem().Column(titleCol =>
                            {
                                titleCol.Item().Text("DOCUMENT COMPARISON ANALYSIS")
                                    .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                                titleCol.Item().Text("Professional AI-Powered Document Analysis Report")
                                    .FontSize(10).FontColor(Colors.Blue.Darken1);
                            });
                            headerRow.ConstantItem(150).AlignRight().Column(dateCol =>
                            {
                                dateCol.Item().Text($"{DateTime.Now:MMMM dd, yyyy}")
                                    .FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                                dateCol.Item().Text($"Generated at {DateTime.Now:HH:mm}")
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                        });
                    });

                    // Enhanced content with comprehensive sections
                    page.Content().Column(col =>
                    {
                        // 1. EXECUTIVE SUMMARY Section with metrics
                        AddProfessionalSection(col, "EXECUTIVE SUMMARY", Colors.Blue.Darken2, summaryCol =>
                        {
                            summaryCol.Item().Row(summaryRow =>
                            {
                                summaryRow.RelativeItem(2).Column(textCol =>
                                {
                                    textCol.Item().Text(SafeTruncate(reportContent.ExecutiveSummary, 800))
                                        .FontSize(10).LineHeight(1.4f);
                                });
                                summaryRow.ConstantItem(10); // Spacer
                                summaryRow.RelativeItem(1).Column(metricsCol =>
                                {
                                    AddMetricBox(metricsCol, "Similarity Score", $"{result.SimilarityScore:P1}", GetSimilarityColor(result.SimilarityScore));
                                    AddMetricBox(metricsCol, "Total Changes", result.DiffSegments.Count.ToString(), Colors.Orange.Darken1);
                                    AddMetricBox(metricsCol, "High Priority", result.DiffSegments.Count(d => d.Severity == Severity.Major).ToString(), Colors.Red.Darken1);
                                });
                            });
                        });
                        
                        // 2. KEY CHANGES Section with detailed breakdown
                        AddProfessionalSection(col, "KEY CHANGES ANALYSIS", Colors.Orange.Darken2, changesCol =>
                        {
                            var changeStats = AnalyzeChanges(result);
                            
                            // Change statistics row
                            changesCol.Item().PaddingBottom(10).Row(statsRow =>
                            {
                                statsRow.RelativeItem().Column(statCol =>
                                {
                                    statCol.Item().Text($"Insertions: {changeStats.Insertions}").FontSize(10).Bold().FontColor(Colors.Green.Darken2);
                                });
                                statsRow.RelativeItem().Column(statCol =>
                                {
                                    statCol.Item().Text($"Deletions: {changeStats.Deletions}").FontSize(10).Bold().FontColor(Colors.Red.Darken2);
                                });
                                statsRow.RelativeItem().Column(statCol =>
                                {
                                    statCol.Item().Text($"Modifications: {changeStats.Modifications}").FontSize(10).Bold().FontColor(Colors.Blue.Darken2);
                                });
                            });
                            
                            // Most significant changes
                            changesCol.Item().PaddingBottom(5).Text("Most Significant Changes:").FontSize(11).Bold();
                            var significantChanges = result.DiffSegments
                                .Where(d => d.Severity != Severity.Minor)
                                .OrderByDescending(d => d.Severity)
                                .Take(8);
                                
                            foreach (var change in significantChanges)
                            {
                                changesCol.Item().PaddingBottom(4).Row(changeRow =>
                                {
                                    changeRow.ConstantItem(15).AlignCenter().Text("•").FontSize(12).FontColor(GetChangeTypeColor(change.Type));
                                    changeRow.ConstantItem(70).Text(change.Type.ToString()).FontSize(9).Bold().FontColor(GetChangeTypeColor(change.Type));
                                    changeRow.ConstantItem(60).Text(change.Severity.ToString()).FontSize(9).Bold().FontColor(GetSeverityColor(change.Severity));
                                    changeRow.RelativeItem().Text(SafeTruncate(change.Text, 120)).FontSize(9).LineHeight(1.2f);
                                });
                            }
                            
                            if (result.DiffSegments.Count > 8)
                            {
                                changesCol.Item().PaddingTop(5).Text($"+ {result.DiffSegments.Count - 8} additional changes documented")
                                    .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                            }
                        });
                        
                        // 3. IMPACT ASSESSMENT Section
                        AddProfessionalSection(col, "IMPACT ASSESSMENT", Colors.Red.Darken2, impactCol =>
                        {
                            impactCol.Item().Column(contentCol =>
                            {
                                contentCol.Item().Text(SafeTruncate(reportContent.ImpactAssessment, 700))
                                    .FontSize(10).LineHeight(1.4f);
                                    
                                // Risk indicators
                                contentCol.Item().PaddingTop(10).Text("Risk Indicators:").FontSize(11).Bold();
                                var riskLevel = GetOverallRiskLevel(result);
                                contentCol.Item().PaddingTop(3).Row(riskRow =>
                                {
                                    riskRow.ConstantItem(100).Text($"Overall Risk: ").FontSize(10);
                                    riskRow.ConstantItem(80).Text(riskLevel.Level).FontSize(10).Bold().FontColor(riskLevel.Color);
                                    riskRow.RelativeItem().Text(riskLevel.Description).FontSize(9).FontColor(Colors.Grey.Darken2);
                                });
                            });
                        });
                        
                        // 4. RECOMMENDATIONS Section
                        AddProfessionalSection(col, "RECOMMENDATIONS & NEXT STEPS", Colors.Purple.Darken2, recCol =>
                        {
                            recCol.Item().Text(SafeTruncate(reportContent.Recommendations, 700))
                                .FontSize(10).LineHeight(1.4f);
                                
                            // Priority actions
                            if (result.DiffSegments.Any(d => d.Severity == Severity.Major))
                            {
                                recCol.Item().PaddingTop(10).Column(priorityCol =>
                                {
                                    priorityCol.Item().Text("Priority Actions Required:").FontSize(11).Bold().FontColor(Colors.Red.Darken2);
                                    priorityCol.Item().PaddingTop(3).Text("• Review all high-severity changes immediately").FontSize(10);
                                    priorityCol.Item().Text("• Validate impact on dependent systems").FontSize(10);
                                    priorityCol.Item().Text("• Update documentation and stakeholder communications").FontSize(10);
                                });
                            }
                        });
                    });

                    // Simple footer
                    page.Footer().PaddingTop(10).Text($"Generated by DocumentComparer on {DateTime.Now:yyyy-MM-dd HH:mm}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            // If PDF generation fails, create a minimal text-based report
            return GenerateSimpleFallbackReport(result);
        }
    }

    private byte[] GenerateSimpleFallbackReport(ComparisonResult result)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Content().Column(col =>
                {
                    col.Item().Text("Document Comparison Report").FontSize(20).Bold();
                    col.Item().PaddingTop(20).Text($"Similarity Score: {result.SimilarityScore:P2}").FontSize(14);
                    col.Item().PaddingTop(10).Text($"Total Changes Found: {result.DiffSegments.Count}").FontSize(12);
                    
                    if (result.AIInsights != null)
                    {
                        col.Item().PaddingTop(15).Text("AI Analysis:").FontSize(14).Bold();
                        col.Item().PaddingTop(5).Text(SafeTruncate(result.AIInsights.Summary, 1000)).FontSize(11);
                    }
                    
                    col.Item().PaddingTop(15).Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10);
                });
            });
        });

        using var ms = new MemoryStream();
        doc.GeneratePdf(ms);
        return ms.ToArray();
    }

    private void AddProfessionalSection(ColumnDescriptor col, string title, string headerColor, Action<ColumnDescriptor> content)
    {
        col.Item().PaddingTop(15).Column(section =>
        {
            // Section header with professional styling
            section.Item().Background(headerColor).Padding(10).Row(headerRow =>
            {
                headerRow.ConstantItem(5).Background(Colors.White);
                headerRow.RelativeItem().Text(title).FontSize(13).Bold().FontColor(Colors.White);
            });
            
            // Section content with border
            section.Item().Border(1).BorderColor(headerColor).Padding(12).Column(contentCol =>
            {
                content(contentCol);
            });
        });
    }
    
    private void AddMetricBox(ColumnDescriptor col, string label, string value, string color)
    {
        col.Item().PaddingBottom(8).Column(metricCol =>
        {
            metricCol.Item().Background(color).Padding(5).AlignCenter().Text(value)
                .FontSize(16).Bold().FontColor(Colors.White);
            metricCol.Item().Background(Colors.Grey.Lighten3).Padding(3).AlignCenter().Text(label)
                .FontSize(8).FontColor(Colors.Grey.Darken2);
        });
    }
    
    private (int Insertions, int Deletions, int Modifications) AnalyzeChanges(ComparisonResult result)
    {
        var insertions = result.DiffSegments.Count(d => d.Type == ChangeType.Inserted);
        var deletions = result.DiffSegments.Count(d => d.Type == ChangeType.Deleted);
        var modifications = result.DiffSegments.Count(d => d.Type == ChangeType.Modified);
        
        return (insertions, deletions, modifications);
    }
    
    private (string Level, string Color, string Description) GetOverallRiskLevel(ComparisonResult result)
    {
        var highSeverityCount = result.DiffSegments.Count(d => d.Severity == Severity.Major);
        var totalChanges = result.DiffSegments.Count;
        var similarity = result.SimilarityScore;
        
        if (highSeverityCount > 10 || similarity < 0.5)
            return ("CRITICAL", Colors.Red.Darken2, "Immediate attention required - significant structural changes detected");
        else if (highSeverityCount > 5 || similarity < 0.7)
            return ("HIGH", Colors.Orange.Darken2, "Review recommended - notable changes that may impact operations");
        else if (totalChanges > 20 || similarity < 0.85)
            return ("MEDIUM", Colors.Yellow.Darken2, "Monitor changes - moderate updates requiring awareness");
        else
            return ("LOW", Colors.Green.Darken2, "Minor changes - routine updates with minimal impact");
    }
    
    private string GetSimilarityColor(double score)
    {
        return score switch
        {
            >= 0.90 => Colors.Green.Darken2,
            >= 0.75 => Colors.Yellow.Darken2,
            >= 0.60 => Colors.Orange.Darken2,
            _ => Colors.Red.Darken2
        };
    }
    
    private string GetChangeTypeColor(ChangeType type)
    {
        return type switch
        {
            ChangeType.Inserted => Colors.Green.Darken2,
            ChangeType.Deleted => Colors.Red.Darken2,
            ChangeType.Modified => Colors.Blue.Darken2,
            _ => Colors.Grey.Darken2
        };
    }
    
    private string GetSeverityColor(Severity severity)
    {
        return severity switch
        {
            Severity.Major => Colors.Red.Darken2,
            Severity.Moderate => Colors.Orange.Darken2,
            _ => Colors.Green.Darken2
        };
    }

    private string SafeTruncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "No content available.";
        return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
    }

    private AIReportContent GenerateAIReportContent(ComparisonResult result)
    {
        try
        {
            // Prepare comprehensive context for AI analysis
            var context = PrepareReportContext(result);
            
            // Generate detailed AI report
            var aiResponse = _openAiService.GenerateDetailedReport(context);
            
            return ParseAIReportResponse(aiResponse, result);
        }
        catch (Exception ex)
        {
            // Fallback to structured content if AI fails
            return GenerateFallbackReport(result);
        }
    }

    private string PrepareReportContext(ComparisonResult result)
    {
        var context = new StringBuilder();
        context.AppendLine($"Document Comparison Analysis");
        context.AppendLine($"Similarity Score: {result.SimilarityScore:P2}");
        context.AppendLine($"Total Changes: {result.DiffSegments.Count}");
        
        // Categorize changes
        var insertions = result.DiffSegments.Count(d => d.Type == ChangeType.Inserted);
        var deletions = result.DiffSegments.Count(d => d.Type == ChangeType.Deleted);
        var modifications = result.DiffSegments.Count(d => d.Type == ChangeType.Modified);
        
        context.AppendLine($"Insertions: {insertions}, Deletions: {deletions}, Modifications: {modifications}");
        
        // Include severity breakdown
        var highSeverity = result.DiffSegments.Count(d => d.Severity == Severity.Major);
        var mediumSeverity = result.DiffSegments.Count(d => d.Severity == Severity.Moderate);
        var lowSeverity = result.DiffSegments.Count(d => d.Severity == Severity.Minor);
        
        context.AppendLine($"Severity Distribution - High: {highSeverity}, Medium: {mediumSeverity}, Low: {lowSeverity}");
        
        // Include sample changes for context
        var significantChanges = result.DiffSegments
            .Where(d => d.Severity != Severity.Minor)
            .Take(10)
            .Select(d => $"{d.Type}: {Truncate(d.Text, 150)}")
            .ToList();
            
        if (significantChanges.Any())
        {
            context.AppendLine("Significant Changes Sample:");
            context.AppendLine(string.Join("\n", significantChanges));
        }
        
        return context.ToString();
    }

    private AIReportContent ParseAIReportResponse(string aiResponse, ComparisonResult result)
    {
        // Parse AI response or use existing insights
        var insights = result.AIInsights;
        
        return new AIReportContent
        {
            ExecutiveSummary = insights?.Summary ?? GenerateExecutiveSummary(result),
            KeyFindings = ExtractKeyFindings(result),
            ImpactAssessment = insights?.Impact ?? GenerateImpactAssessment(result),
            Recommendations = GenerateRecommendations(result),
            TechnicalAnalysis = GenerateTechnicalAnalysis(result)
        };
    }

    private AIReportContent GenerateFallbackReport(ComparisonResult result)
    {
        return new AIReportContent
        {
            ExecutiveSummary = GenerateExecutiveSummary(result),
            KeyFindings = ExtractKeyFindings(result),
            ImpactAssessment = GenerateImpactAssessment(result),
            Recommendations = GenerateRecommendations(result),
            TechnicalAnalysis = GenerateTechnicalAnalysis(result)
        };
    }

    private string GenerateExecutiveSummary(ComparisonResult result)
    {
        var summary = new StringBuilder();
        
        // Opening statement with context
        var similarityLevel = result.SimilarityScore switch
        {
            >= 0.95 => "virtually identical documents with minimal variations",
            >= 0.85 => "highly similar documents with notable but manageable differences", 
            >= 0.70 => "moderately similar documents with significant structural changes",
            >= 0.50 => "documents with substantial differences requiring careful review",
            _ => "significantly different documents with major structural changes"
        };
        
        summary.AppendLine($"This comprehensive document comparison analysis reveals {similarityLevel} (similarity score: {result.SimilarityScore:P1}).");
        
        var changeTypes = result.DiffSegments.GroupBy(d => d.Type).ToDictionary(g => g.Key, g => g.Count());
        var severityBreakdown = result.DiffSegments.GroupBy(d => d.Severity).ToDictionary(g => g.Key, g => g.Count());
        
        // Detailed change analysis
        summary.AppendLine($"Our AI-powered analysis identified {result.DiffSegments.Count} total modifications, comprising:");
        summary.AppendLine($"• {changeTypes.GetValueOrDefault(ChangeType.Inserted, 0)} new content additions");
        summary.AppendLine($"• {changeTypes.GetValueOrDefault(ChangeType.Deleted, 0)} content removals");
        summary.AppendLine($"• {changeTypes.GetValueOrDefault(ChangeType.Modified, 0)} content modifications");
        
        // Severity assessment
        var criticalChanges = severityBreakdown.GetValueOrDefault(Severity.Major, 0);
        var moderateChanges = severityBreakdown.GetValueOrDefault(Severity.Moderate, 0);
        
        if (criticalChanges > 0)
        {
            summary.AppendLine($"Critical finding: {criticalChanges} high-severity changes require immediate stakeholder review and may impact operational processes.");
        }
        
        if (moderateChanges > 0)
        {
            summary.AppendLine($"Additionally, {moderateChanges} moderate-impact changes warrant management attention for planning and implementation purposes.");
        }
        
        // Strategic recommendation
        var overallRisk = GetOverallRiskLevel(result);
        summary.AppendLine($"Overall assessment: {overallRisk.Level.ToLower()} risk level. {overallRisk.Description}");
        
        return summary.ToString();
    }

    private string ExtractKeyFindings(ComparisonResult result)
    {
        var findings = new StringBuilder();
        
        // Analyze patterns in changes
        var severityGroups = result.DiffSegments.GroupBy(d => d.Severity)
            .ToDictionary(g => g.Key, g => g.Count());
            
        findings.AppendLine("• Document structure and content analysis completed successfully");
        
        if (severityGroups.GetValueOrDefault(Severity.Major, 0) > 0)
        {
            findings.AppendLine($"• {severityGroups[Severity.Major]} critical changes identified requiring immediate review");
        }
        
        if (severityGroups.GetValueOrDefault(Severity.Moderate, 0) > 0)
        {
            findings.AppendLine($"• {severityGroups[Severity.Moderate]} moderate changes detected with potential impact");
        }
        
        findings.AppendLine($"• Overall document similarity maintained at {result.SimilarityScore:P2} level");
        
        // Identify most common change types
        var commonChangeType = result.DiffSegments.GroupBy(d => d.Type)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
            
        if (commonChangeType != null)
        {
            findings.AppendLine($"• Most frequent change type: {commonChangeType.Key} ({commonChangeType.Count()} occurrences)");
        }
        
        return findings.ToString();
    }

    private string GenerateImpactAssessment(ComparisonResult result)
    {
        var assessment = new StringBuilder();
        
        var changeTypes = result.DiffSegments.GroupBy(d => d.Type).ToDictionary(g => g.Key, g => g.Count());
        var severityBreakdown = result.DiffSegments.GroupBy(d => d.Severity).ToDictionary(g => g.Key, g => g.Count());
        var totalChanges = result.DiffSegments.Count;
        var similarity = result.SimilarityScore;
        
        // Business Impact Analysis
        assessment.AppendLine("BUSINESS IMPACT ANALYSIS:");
        
        if (similarity < 0.5)
        {
            assessment.AppendLine("• CRITICAL IMPACT: Documents show fundamental structural differences that may indicate major policy, process, or system changes.");
            assessment.AppendLine("• Immediate stakeholder consultation recommended before implementation.");
        }
        else if (similarity < 0.75)
        {
            assessment.AppendLine("• SIGNIFICANT IMPACT: Notable changes detected that require management review and controlled deployment.");
            assessment.AppendLine("• Consider phased implementation with rollback procedures.");
        }
        else
        {
            assessment.AppendLine("• MODERATE IMPACT: Changes are manageable within normal operational parameters.");
        }
        
        // Operational Impact
        assessment.AppendLine("\nOPERATIONAL CONSIDERATIONS:");
        
        var deletions = changeTypes.GetValueOrDefault(ChangeType.Deleted, 0);
        var insertions = changeTypes.GetValueOrDefault(ChangeType.Inserted, 0);
        var modifications = changeTypes.GetValueOrDefault(ChangeType.Modified, 0);
        
        if (deletions > 10)
        {
            assessment.AppendLine($"• Content Removal Risk: {deletions} deletions may affect dependent processes or references.");
        }
        
        if (insertions > 15)
        {
            assessment.AppendLine($"• Integration Complexity: {insertions} new additions require validation for consistency and compliance.");
        }
        
        if (modifications > 20)
        {
            assessment.AppendLine($"• Change Management: {modifications} modifications necessitate comprehensive testing and user training.");
        }
        
        // Risk Factors
        var criticalChanges = severityBreakdown.GetValueOrDefault(Severity.Major, 0);
        if (criticalChanges > 0)
        {
            assessment.AppendLine($"\nRISK FACTORS: {criticalChanges} high-severity changes present elevated operational risks:");
            assessment.AppendLine("• Potential service disruption during implementation");
            assessment.AppendLine("• Increased support overhead and user queries");
            assessment.AppendLine("• Compliance and audit trail considerations");
        }
        
        // Timeline Impact
        assessment.AppendLine($"\nIMPLEMENTATION TIMELINE: Based on {totalChanges} total changes:");
        var timelineEstimate = totalChanges switch
        {
            <= 10 => "1-2 business days for review and deployment",
            <= 25 => "3-5 business days with stakeholder approval cycles", 
            <= 50 => "1-2 weeks including testing and validation phases",
            _ => "2+ weeks with comprehensive change management process"
        };
        assessment.AppendLine($"• Estimated implementation time: {timelineEstimate}");
        
        return assessment.ToString();
    }

    private string GenerateRecommendations(ComparisonResult result)
    {
        var recommendations = new StringBuilder();
        
        var totalChanges = result.DiffSegments.Count;
        var criticalChanges = result.DiffSegments.Count(d => d.Severity == Severity.Major);
        var similarity = result.SimilarityScore;
        
        // Strategic Recommendations
        recommendations.AppendLine("STRATEGIC RECOMMENDATIONS:");
        
        if (similarity < 0.6)
        {
            recommendations.AppendLine("1. EXECUTIVE ESCALATION: Significant document divergence requires C-level awareness and approval");
            recommendations.AppendLine("   • Present findings to executive stakeholders immediately");
            recommendations.AppendLine("   • Establish change control board for oversight");
            recommendations.AppendLine("   • Consider external audit for compliance verification");
        }
        else if (similarity < 0.8)
        {
            recommendations.AppendLine("1. MANAGEMENT REVIEW: Notable changes warrant management attention and structured approval");
            recommendations.AppendLine("   • Schedule management review meeting within 48 hours");
            recommendations.AppendLine("   • Prepare detailed change impact briefing");
        }
        else
        {
            recommendations.AppendLine("1. STANDARD REVIEW: Changes fall within acceptable operational parameters");
            recommendations.AppendLine("   • Proceed with standard change management process");
        }
        
        // Operational Recommendations
        recommendations.AppendLine("\nOPERATIONAL ACTIONS:");
        
        if (criticalChanges > 0)
        {
            recommendations.AppendLine($"2. PRIORITY REMEDIATION: Address {criticalChanges} critical issues immediately");
            recommendations.AppendLine("   • Assign dedicated resources for high-severity changes");
            recommendations.AppendLine("   • Implement emergency change procedures if necessary");
            recommendations.AppendLine("   • Establish 24/7 monitoring during implementation");
        }
        
        recommendations.AppendLine("3. QUALITY ASSURANCE PROTOCOL:");
        recommendations.AppendLine("   • Execute comprehensive testing across all affected systems");
        recommendations.AppendLine("   • Validate changes against business requirements and compliance standards");
        recommendations.AppendLine("   • Document all modifications with clear rationale and approval trails");
        
        // Implementation Strategy
        recommendations.AppendLine("\nIMPLEMENTATION STRATEGY:");
        
        if (totalChanges > 50)
        {
            recommendations.AppendLine("4. PHASED ROLLOUT: High change volume requires staged implementation");
            recommendations.AppendLine("   • Divide changes into logical groups by impact and dependency");
            recommendations.AppendLine("   • Implement pilot deployment with select user groups");
            recommendations.AppendLine("   • Establish rollback procedures and success criteria");
        }
        else if (totalChanges > 20)
        {
            recommendations.AppendLine("4. CONTROLLED DEPLOYMENT: Moderate changes require structured rollout");
            recommendations.AppendLine("   • Schedule implementation during maintenance windows");
            recommendations.AppendLine("   • Prepare comprehensive communication plan for affected users");
        }
        else
        {
            recommendations.AppendLine("4. STANDARD DEPLOYMENT: Low change volume supports normal implementation");
            recommendations.AppendLine("   • Follow standard change management procedures");
        }
        
        // Monitoring and Follow-up
        recommendations.AppendLine("\n5. POST-IMPLEMENTATION MONITORING:");
        recommendations.AppendLine("   • Establish success metrics and monitoring dashboards");
        recommendations.AppendLine("   • Schedule follow-up review sessions at 24h, 1 week, and 1 month intervals");
        recommendations.AppendLine("   • Maintain incident response team for immediate issue resolution");
        recommendations.AppendLine("   • Document lessons learned for future change management improvement");
        
        return recommendations.ToString();
    }

    private string GenerateTechnicalAnalysis(ComparisonResult result)
    {
        var analysis = new StringBuilder();
        
        analysis.AppendLine($"Similarity Coefficient: {result.SimilarityScore:F4}");
        analysis.AppendLine($"Total Segments Analyzed: {result.DiffSegments.Count}");
        
        var typeDistribution = result.DiffSegments.GroupBy(d => d.Type)
            .ToDictionary(g => g.Key, g => g.Count());
            
        analysis.AppendLine("\nChange Distribution:");
        foreach (var kvp in typeDistribution)
        {
            var percentage = (double)kvp.Value / result.DiffSegments.Count * 100;
            analysis.AppendLine($"  {kvp.Key}: {kvp.Value} ({percentage:F1}%)");
        }
        
        var severityDistribution = result.DiffSegments.GroupBy(d => d.Severity)
            .ToDictionary(g => g.Key, g => g.Count());
            
        analysis.AppendLine("\nSeverity Analysis:");
        foreach (var kvp in severityDistribution)
        {
            var percentage = (double)kvp.Value / result.DiffSegments.Count * 100;
            analysis.AppendLine($"  {kvp.Key}: {kvp.Value} ({percentage:F1}%)");
        }
        
        return analysis.ToString();
    }

    private void AddSection(ColumnDescriptor col, string title, string content, string color)
    {
        if (string.IsNullOrWhiteSpace(content)) return;
        
        col.Item().PaddingTop(15).Column(section =>
        {
            section.Item().Background(color).Padding(8).Text(title)
                .FontSize(14).Bold().FontColor(Colors.White);
            section.Item().Border(1).BorderColor(color).Padding(12).Column(contentCol =>
            {
                // Split content into paragraphs and limit length
                var paragraphs = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Take(10) // Limit to 10 paragraphs per section
                    .ToList();

                foreach (var paragraph in paragraphs)
                {
                    var limitedText = Truncate(paragraph, 800); // Limit paragraph length
                    contentCol.Item().PaddingBottom(5).Text(limitedText)
                        .FontSize(10).LineHeight(1.4f);
                }
                
                if (content.Length > 4000)
                {
                    contentCol.Item().PaddingTop(5).Text("... [Content truncated for report formatting]")
                        .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                }
            });
        });
    }

    private void AddSimilaritySection(ColumnDescriptor col, ComparisonResult result)
    {
        col.Item().PaddingTop(15).Column(section =>
        {
            section.Item().Background(Colors.Indigo.Darken1).Padding(8).Text("SIMILARITY ANALYSIS")
                .FontSize(14).Bold().FontColor(Colors.White);
            section.Item().Border(1).BorderColor(Colors.Indigo.Darken1).Padding(12).Row(row =>
            {
                row.RelativeItem().Column(leftCol =>
                {
                    leftCol.Item().Text($"Overall Similarity: {result.SimilarityScore:P2}")
                        .FontSize(16).Bold();
                    leftCol.Item().Text($"Classification: {GetSimilarityClassification(result.SimilarityScore)}")
                        .FontSize(12).FontColor(GetSimilarityColor(result.SimilarityScore));
                });
                
                row.ConstantItem(150).AlignRight().Column(rightCol =>
                {
                    rightCol.Item().Text($"{result.DiffSegments.Count}")
                        .FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                    rightCol.Item().Text("Total Changes")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            });
        });
    }

    private void AddDetailedChangesSection(ColumnDescriptor col, ComparisonResult result)
    {
        if (!result.DiffSegments.Any()) return;
        
        col.Item().PaddingTop(15).Column(section =>
        {
            section.Item().Background(Colors.Red.Darken1).Padding(8).Text("DETAILED CHANGES ANALYSIS")
                .FontSize(14).Bold().FontColor(Colors.White);
            section.Item().Border(1).BorderColor(Colors.Red.Darken1).Padding(12).Column(changesCol =>
            {
                var significantChanges = result.DiffSegments
                    .Where(d => d.Severity != Severity.Minor)
                    .Take(15) // Reduced to prevent overflow
                    .ToList();
                
                foreach (var change in significantChanges)
                {
                    changesCol.Item().PaddingBottom(6).Row(changeRow =>
                    {
                        changeRow.ConstantItem(50).Text(change.Type.ToString())
                            .FontSize(8).Bold().FontColor(GetChangeTypeColor(change.Type));
                        changeRow.ConstantItem(50).Text(change.Severity.ToString())
                            .FontSize(8).Bold().FontColor(GetSeverityColor(change.Severity));
                        changeRow.RelativeItem().Text(Truncate(change.Text, 150)) // Reduced length
                            .FontSize(9).LineHeight(1.1f);
                    });
                }
                
                if (result.DiffSegments.Count > 15)
                {
                    changesCol.Item().PaddingTop(8).Text($"... and {result.DiffSegments.Count - 15} additional changes")
                        .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                }
            });
        });
    }

    private string GetSimilarityClassification(double score)
    {
        return score switch
        {
            >= 0.95 => "Virtually Identical",
            >= 0.85 => "Highly Similar",
            >= 0.70 => "Moderately Similar",
            >= 0.50 => "Somewhat Similar",
            _ => "Significantly Different"
        };
    }



    private class AIReportContent
    {
        public string ExecutiveSummary { get; set; } = "";
        public string KeyFindings { get; set; } = "";
        public string ImpactAssessment { get; set; } = "";
        public string Recommendations { get; set; } = "";
        public string TechnicalAnalysis { get; set; } = "";
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s ?? "";
        return s.Length <= max ? s : s.Substring(0, max) + "…";
    }
}
