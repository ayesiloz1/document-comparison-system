using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using DocumentComparer.Models;

namespace DocumentComparer.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly AzureOpenAIClient? _client;
        private readonly string? _deploymentName;
        private readonly bool _isConfigured;

        public OpenAiService(IConfiguration config)
        {
            var endpointStr = config["AzureOpenAI:Endpoint"];
            var key = config["AzureOpenAI:ApiKey"];
            _deploymentName = config["AzureOpenAI:DeploymentName"];

            // Check if Azure OpenAI is properly configured
            _isConfigured = !string.IsNullOrEmpty(endpointStr) && 
                           !string.IsNullOrEmpty(key) && 
                           !string.IsNullOrEmpty(_deploymentName);

            if (_isConfigured)
            {
                var endpoint = new Uri(endpointStr!);
                _client = new AzureOpenAIClient(
                    endpoint,
                    new ApiKeyCredential(key!)
                );
            }
        }

        public async Task<string> CompareDocumentsAsync(string textA, string textB)
        {
            if (!_isConfigured || _client == null)
            {
                throw new InvalidOperationException("Azure OpenAI is not configured");
            }

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a document comparison assistant. Compare the two documents and describe differences, similarities, and revisions."),
                new UserChatMessage($"Document A:\n{textA}\n\nDocument B:\n{textB}")
            };

            var chatClient = _client.GetChatClient(_deploymentName!);
            var response = await chatClient.CompleteChatAsync(messages);

            var result = response.Value.Content[0].Text;
            return result ?? "No response.";
        }

        public async Task<bool> AreDocumentsSimilarAsync(string textA, string textB)
        {
            if (!_isConfigured || _client == null)
            {
                throw new InvalidOperationException("Azure OpenAI is not configured");
            }

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a document similarity evaluator. Return only 'true' or 'false' based on whether these are essentially the same document."),
                new UserChatMessage($"Document A:\n{textA}\n\nDocument B:\n{textB}")
            };

            var chatClient = _client.GetChatClient(_deploymentName!);
            var response = await chatClient.CompleteChatAsync(messages);

            var result = response.Value.Content[0].Text?.Trim().ToLowerInvariant();
            return result?.Contains("true") ?? false;
        }

        public async Task<ComparisonSummary> SummarizeChangesAsync(string oldText, string newText, CancellationToken cancellationToken = default)
        {
            if (!_isConfigured || _client == null)
            {
                throw new InvalidOperationException("Azure OpenAI is not configured");
            }

            var comparison = await CompareDocumentsAsync(oldText, newText);
            var similar = await AreDocumentsSimilarAsync(oldText, newText);
            
            return new ComparisonSummary
            {
                SummaryText = comparison,
                SimilarityScore = similar ? 0.95 : 0.5 // Simple heuristic
            };
        }

        public async Task<List<(int index, string severity)>> ClassifySegmentsAsync(List<string> segmentTexts, CancellationToken cancellationToken = default)
        {
            if (!_isConfigured || _client == null)
            {
                throw new InvalidOperationException("Azure OpenAI is not configured");
            }

            var result = new List<(int index, string severity)>();
            
            for (int i = 0; i < segmentTexts.Count; i++)
            {
                var text = segmentTexts[i];
                var severity = text.Length > 400 ? "Major" : text.Length > 150 ? "Moderate" : "Minor";
                result.Add((i, severity));
            }
            
            return result;
        }

        public async Task<AIInsight> GenerateInsightsAsync(string oldText, string newText, List<DiffSegment> diffSegments, CancellationToken cancellationToken = default)
        {
            if (!_isConfigured || _client == null)
            {
                throw new InvalidOperationException("Azure OpenAI is not configured");
            }

            var changesContext = string.Join("\n", diffSegments.Select(d => 
                $"Change Type: {d.Type}, Text: {d.Text?.Substring(0, Math.Min(d.Text.Length, 100))}..., Severity: {d.Severity}"));

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(@"You are an expert document analyst. Analyze the changes between two documents and provide comprehensive insights. 
                Return your response in this JSON format:
                {
                    ""summary"": ""Brief overview of what changed"",
                    ""keyChanges"": [""Change 1"", ""Change 2"", ""Change 3""],
                    ""recommendations"": [""Recommendation 1"", ""Recommendation 2""],
                    ""impact"": ""Assessment of the overall impact of these changes""
                }"),
                new UserChatMessage($@"Analyze these document changes:

Original Document Length: {oldText.Length} characters
Updated Document Length: {newText.Length} characters
Number of Changes: {diffSegments.Count}

Key Changes:
{changesContext}

Provide insights about what changed, why it might have changed, and recommendations for review.")
            };

            try
            {
                var chatClient = _client.GetChatClient(_deploymentName!);
                var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
                var result = response.Value.Content[0].Text ?? "";

                // Try to parse JSON response
                try
                {
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(result);
                    var root = jsonDoc.RootElement;

                    return new AIInsight
                    {
                        Summary = root.TryGetProperty("summary", out var summary) ? summary.GetString() ?? "" : "Analysis completed",
                        KeyChanges = root.TryGetProperty("keyChanges", out var keyChanges) && keyChanges.ValueKind == System.Text.Json.JsonValueKind.Array
                            ? keyChanges.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList()
                            : new List<string> { "Multiple text modifications detected", "Content structure updated", "Formatting changes applied" },
                        Recommendations = root.TryGetProperty("recommendations", out var recommendations) && recommendations.ValueKind == System.Text.Json.JsonValueKind.Array
                            ? recommendations.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList()
                            : new List<string> { "Review all changes for accuracy", "Verify critical information is preserved" },
                        Impact = root.TryGetProperty("impact", out var impact) ? impact.GetString() ?? "" : "Medium impact changes detected"
                    };
                }
                catch
                {
                    // Fallback if JSON parsing fails
                    return new AIInsight
                    {
                        Summary = result.Length > 200 ? result.Substring(0, 200) + "..." : result,
                        KeyChanges = diffSegments.Take(5).Select(d => $"{d.Type}: {d.Text?.Substring(0, Math.Min(d.Text.Length, 50))}...").ToList(),
                        Recommendations = new List<string> { "Review changes carefully", "Verify content accuracy" },
                        Impact = $"Detected {diffSegments.Count} changes across the document"
                    };
                }
            }
            catch (Exception ex)
            {
                // Return fallback insights if AI fails
                return new AIInsight
                {
                    Summary = $"Automated analysis of {diffSegments.Count} changes between documents",
                    KeyChanges = diffSegments.Take(5).Select(d => $"{d.Type} detected").ToList(),
                    Recommendations = new List<string> { "Manual review recommended", "Verify important content" },
                    Impact = "AI analysis unavailable - manual review suggested"
                };
            }
        }
    }
}
