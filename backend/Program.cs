using DocumentComparer.Models;
using DocumentComparer.Services;
using DocumentComparer.Utils;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

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
builder.Services.AddSingleton<EnhancedPdfService>();
builder.Services.AddSingleton<IDiffService, DiffService>();
builder.Services.AddSingleton<ISeverityClassifier, LocalSeverityClassifier>();
builder.Services.AddSingleton<IOpenAiService, OpenAiService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISectionComparisonService, SectionComparisonService>();

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

app.MapPost("/extract-sections", async (IFormFileCollection files, EnhancedPdfService enhancedPdfService) =>
{
    if (files.Count != 2)
        return Results.BadRequest("Please upload exactly two PDF files.");

    var file1 = files[0];
    var file2 = files[1];

    if (!FileHelpers.IsPdf(file1) || !FileHelpers.IsPdf(file2))
        return Results.BadRequest("Both files must be PDFs.");

    try
    {
        using var stream1 = file1.OpenReadStream();
        using var stream2 = file2.OpenReadStream();

        var documentA = await enhancedPdfService.ExtractWithCoordinatesAsync(stream1, file1.FileName);
        var documentB = await enhancedPdfService.ExtractWithCoordinatesAsync(stream2, file2.FileName);

        return Results.Ok(new
        {
            DocumentA = documentA,
            DocumentB = documentB
        });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error extracting PDF sections");
        return Results.Problem("Error processing PDF files.");
    }
});

app.MapPost("/compare-sections", async (HttpRequest request, ISectionComparisonService sectionComparisonService) =>
{
    try
    {
        var form = await request.ReadFormAsync();
        var file1 = form.Files["file1"];
        var file2 = form.Files["file2"];

        if (file1 == null || file2 == null)
        {
            return Results.BadRequest("Both file1 and file2 are required.");
        }

        if (!FileHelpers.IsPdf(file1) || !FileHelpers.IsPdf(file2))
        {
            return Results.BadRequest("Both files must be PDF documents.");
        }

        var result = await sectionComparisonService.CompareDocumentSectionsAsync(file1, file2);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error comparing document sections");
        return Results.Problem("Error processing document comparison.");
    }
});

app.Run();
