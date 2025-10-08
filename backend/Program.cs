using DocumentComparer.Models;
using DocumentComparer.Services;
using DocumentComparer.Utils;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;

// Configure QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure settings (appsettings.json and env)
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("AzureOpenAI"));

// Register services
builder.Services.AddSingleton<IPdfService, PdfService>();
builder.Services.AddSingleton<IDiffService, DiffService>();
builder.Services.AddSingleton<ISeverityClassifier, LocalSeverityClassifier>();
builder.Services.AddSingleton<IOpenAiService, OpenAiService>();
builder.Services.AddSingleton<IReportService, ReportService>();

// Increase limits for large files if running locally (adjust as needed for production)
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 1024L * 1024L * 1024L; // 1 GB
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowReactApp");

app.MapGet("/health", () => Results.Ok(new { status = "ok", now = DateTime.UtcNow }));

app.MapPost("/compare", async (HttpRequest request,
                               IPdfService pdfService,
                               IDiffService diffService,
                               IOpenAiService openAiService,
                               ISeverityClassifier severityClassifier,
                               IReportService reportService) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest(new { error = "Form content-type required with two PDF files (pdf1, pdf2)." });

    var form = await request.ReadFormAsync();
    var pdf1 = form.Files.GetFile("pdf1");
    var pdf2 = form.Files.GetFile("pdf2");

    if (pdf1 == null || pdf2 == null)
        return Results.BadRequest(new { error = "Please upload two files with names 'pdf1' and 'pdf2'." });

    // Extract per-page text (keeps page mapping for highlighting & navigation)
    var docA = await pdfService.ExtractAsync(pdf1.OpenReadStream(), pdf1.FileName);
    var docB = await pdfService.ExtractAsync(pdf2.OpenReadStream(), pdf2.FileName);

    // Decide chunking strategy: here simple per-page concatenation, but keep page numbers
    var textA = docA.FullText;
    var textB = docB.FullText;

    // Compute page-aware diff to maintain page number information
    var diffSegments = diffService.ComputePageAwareDiff(docA, docB);

    // Local severity classification (cheap)
    var classified = severityClassifier.Classify(diffSegments);

    // Ask OpenAI for a concise semantic summary and AI insights (if configured)
    var openAiOptions = app.Services.GetRequiredService<IOptions<OpenAiOptions>>().Value;
    ComparisonSummary? summary = null;
    AIInsight? aiInsights = null;
    
    if (!string.IsNullOrEmpty(openAiOptions.Endpoint) && !string.IsNullOrEmpty(openAiOptions.ApiKey))
    {
        try
        {
            summary = await openAiService.SummarizeChangesAsync(textA, textB);
            aiInsights = await openAiService.GenerateInsightsAsync(textA, textB, classified);
        }
        catch (Exception ex)
        {
            // Don't fail the whole request if OpenAI fails — return best-effort result
            app.Logger.LogWarning(ex, "OpenAI analysis failed; returning local diff only.");
        }
    }

    var result = new ComparisonResult
    {
        Summary = summary?.SummaryText ?? "No semantic summary available. Enable Azure OpenAI to get a GPT-based summary.",
        SimilarityScore = summary?.SimilarityScore ?? LocalSimilarityEstimator.EstimateSimilarity(diffSegments),
        DiffSegments = classified,
        AIInsights = aiInsights
    };

    return Results.Ok(result);
}).Accepts<IFormFile>("multipart/form-data");

app.MapPost("/export", async (ComparisonResult result, IReportService reportService) =>
{
    var bytes = reportService.GeneratePdfReport(result);
    return Results.File(bytes, "application/pdf", "comparison_report.pdf");
});

app.Run();
