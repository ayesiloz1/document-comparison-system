## Document Comparison System — Step-by-Step Project Walkthrough

This guide explains what the project does and how each part works, end to end. It covers both backend and frontend flows, the data exchanged between them, and the algorithms/heuristics used under the hood.

---

## What this project does

This is a full-stack app that compares two PDF documents and explains the differences:

- Extracts text from each PDF, page by page
- Computes page-aware diffs (added/removed/modified)
- Classifies change severity using lightweight heuristics
- Optionally adds Azure OpenAI–powered summaries and insights
- Also supports section-by-section comparison with concise AI one‑liners
- Exports a professional PDF report summarizing results

Technologies:
- Backend: .NET 9 minimal API, iText 7 (PDF), DiffPlex (diff), QuestPDF (report), Azure OpenAI SDK (optional)
- Frontend: React + TypeScript, Framer Motion

Key endpoints:
- POST /compare — file-to-file diff, returns ComparisonResult
- POST /compare-sections — section-aware comparison + AI one-liners
- POST /export — returns a polished PDF report from ComparisonResult
- POST /extract-sections — extracts structured sections
- GET /health — health check

---

## Architecture overview

- Frontend
  - `src/App.tsx`: Orchestrates UX (chat-like guidance + section view + export)
  - `src/utils/api.ts`: Calls backend endpoints
  - `src/utils/types.ts`: TypeScript models mirroring backend
  - Components (e.g., `SectionChunkView`, `SummaryPanel`, `SideBySideDiff`) visualize results and AI insights

- Backend
  - `Program.cs`: Wires services, CORS, JSON enum serialization, routes
  - Services:
    - `PdfService` / `EnhancedPdfService`: Extract per-page text and section candidates
    - `DiffService`: Page-aware line diffs using DiffPlex
    - `LocalSeverityClassifier`: Heuristic severity classification
    - `OpenAiService`: Summaries and insights via Azure OpenAI (optional, graceful fallbacks)
    - `ReportService`: Professional PDF export with QuestPDF
    - `SectionComparisonService`: Section extraction, matching, summaries
  - Models: Data contracts sent over the wire
  - Utils: Text normalization, file helpers, similarity estimators

---

## End-to-end flow (simple diff mode)

1) Upload
- The React app collects two PDFs.

2) Request
- Frontend sends a multipart/form-data `POST /compare` with `pdf1` and `pdf2`.

3) PDF extraction
- `PdfService.ExtractAsync` (iText 7):
  - Reads each page, extracts text, cleans artifacts
  - Returns `PdfDocument { FullText, Pages: string[] }`
  - Includes a warning when very little text is extracted (possible scanned PDF)

4) Diff generation
- `DiffService.ComputePageAwareDiff(docA, docB)`:
  - For each page, run DiffPlex `InlineDiffBuilder`
  - Produce `DiffSegment` with `Type` (Inserted/Deleted/Modified/Unchanged), `Text`, and `PageNumberA/B`

5) Local severity classification
- `LocalSeverityClassifier.Classify`: assigns `Severity` based on heuristics:
  - Longer text -> higher severity
  - Presence of numbers and legal-ish keywords (shall, must, liability...) increases severity

6) Optional Azure OpenAI enrichment
- If `AzureOpenAI` is configured:
  - `OpenAiService.SummarizeChangesAsync` returns a semantic summary + similarity heuristic
  - `OpenAiService.GenerateInsightsAsync` returns `AIInsight { summary, keyChanges[], recommendations[], impact }`
- If not configured or it fails: backend returns best-effort results with local similarity fallback (`LocalSimilarityEstimator`)

7) Response
- Backend returns `ComparisonResult`:
  - `summary: string`
  - `similarityScore: number (0..1)`
  - `diffSegments: DiffSegment[]` (with severity and page mapping)
  - `aiInsights?: AIInsight`

8) Display
- Frontend renders the summary, metrics, and optionally a side-by-side diff view.

9) Export
- Frontend `POST /export` with `ComparisonResult`.
- `ReportService.GeneratePdfReport` produces a professional PDF (Executive Summary, Key Changes, Impact, Recommendations).

---

## End-to-end flow (section-aware mode)

This is the default path in `App.tsx` for fast, structured results with short AI summaries per section.

1) Upload
- User provides two PDFs.

2) Request
- Frontend sends `POST /compare-sections` with `file1`, `file2`.

3) Section extraction
- `SectionComparisonService.ExtractDocumentSectionsAsync` (iText 7 + `TextNormalizer`):
  - Scans lines for likely headers (numbered headings, ALL CAPS, short title case)
  - Builds `DocumentSection { title, sectionType, pageNumber, lineNumber, content }`
  - Merges tiny fragments on the same page when appropriate

4) Matching and comparison
- For each A-section, find best match in B by a Jaccard-like token similarity (title+content):
  - Labeled as Added, Deleted, Modified, or Unchanged
  - For Modified, compute a similarity score
  - Generate a concise `aiSummary` (via `OpenAiService.GenerateFastSummaryAsync`) or fallback text
  - Concurrency-limited to protect the AI service

5) Overall insights
- Compute overall similarity across sections
- Aggregate Added/Deleted/Modified/Unchanged counts
- Generate an `OverallSummary` (fast one-liner) for executives
- Return `SectionComparisonResult` with sections, comparisons, similarity, and `AISectionInsights`

6) Display
- App shows a card with AI overview and actions:
  - “Section-by-Section View” (driven by `SectionChunkView`)
  - “Export Report” (maps to `ComparisonResult` shape for `/export`)

---

## Backend routes and contracts

### POST /compare
- Input: multipart form (`pdf1`, `pdf2`)
- Output: `ComparisonResult`

### POST /compare-sections
- Input: multipart form (`file1`, `file2`)
- Output: `SectionComparisonResult`

### POST /extract-sections
- Input: 2 PDF files
- Output: raw structured extraction of sections

### POST /export
- Input: `ComparisonResult`
- Output: `application/pdf`

### GET /health
- Output: `{ status: "ok", now: Date }`

---

## Data models (wire format overview)

- `ComparisonResult`
  - `summary: string`
  - `similarityScore: number`
  - `diffSegments: DiffSegment[]`
  - `aiInsights?: AIInsight`

- `DiffSegment`
  - `type: "Unchanged" | "Inserted" | "Deleted" | "Modified"`
  - `text: string`
  - `severity: "Minor" | "Moderate" | "Major"`
  - `pageNumberA?: number`
  - `pageNumberB?: number`

- `SectionComparisonResult`
  - `documentAName: string`
  - `documentBName: string`
  - `documentASections: DocumentSection[]`
  - `documentBSections: DocumentSection[]`
  - `sectionComparisons: SectionComparison[]`
  - `overallSimilarity: number`
  - `aiSectionInsights: AISectionInsights`
  - `comparisonTimestamp: string`

- `SectionComparison`
  - `comparisonId: string`
  - `sectionA?: DocumentSection`
  - `sectionB?: DocumentSection`
  - `changeType: "Unchanged" | "Added" | "Deleted" | "Modified"`
  - `similarityScore?: number`
  - `pageNumberA?: number`
  - `pageNumberB?: number`
  - `aiSummary: string`
  - `severity: "Low" | "Medium" | "High" | "Critical"`

- `AIInsight`
  - `summary: string`
  - `keyChanges: string[]`
  - `recommendations: string[]`
  - `impact: string`

- `AISectionInsights`
  - `overallSummary: string`
  - `keyChanges: SectionComparison[]`
  - `changeStatistics: { addedSections, deletedSections, modifiedSections, unchangedSections, totalSections, changePercentage }`

---

## Algorithms and heuristics

- PDF text extraction (iText 7)
  - Per-page text; cleaned of PDF artifacts
  - `TextNormalizer` fixes ligatures (fi/fl…), broken “ti” patterns (e.g., “specifica on” → “specification”), and OCR quirks

- Page-aware diff (DiffPlex)
  - Inline, per-page comparison; `DiffSegment` tracks page mapping

- Local severity classification
  - Length, numbers, and legal keywords influence severity (Minor/Moderate/Major)

- Similarity estimates
  - Local fallback: token-based Jaccard similarity (`LocalSimilarityEstimator`)
  - AI-based similarity: heuristic mapping when AI deems documents similar

- Section detection & matching
  - Heuristics find headers by regex & casing patterns
  - Matching via token-based Jaccard similarity (title + content)
  - Added/Deleted/Modified/Unchanged based on best-match thresholds
  - AI one-liners per section (fast mode), with concurrency limits

---

## Report generation (QuestPDF)

- Layout includes:
  - Executive Summary
  - Key Changes Analysis (counts, severity/type indicators)
  - Impact Assessment (risk level by similarity and change severity)
  - Recommendations & Next Steps
  - Footer with timestamp

- Resilience
  - If generation fails, a minimal text-based PDF report is produced as fallback.

---

## Configuration notes

- Backend config from `appsettings.json` and `.env` (via `DotNetEnv.Env.Load()`).
- CORS allows `http://localhost:3000` by default.
- Request size limit increased (1 GB) for local dev.
- Azure OpenAI keys:
  - `OpenAiService` expects `AzureOpenAI:Endpoint`, `AzureOpenAI:ApiKey`, and `AzureOpenAI:DeploymentName`.
  - There is also an `OpenAiOptions` record with `DeploymentOrModelName`. Keep key names consistent if you change one side.

---

## Error handling and edge cases

- Validations: both files required and must be PDFs (for section routes)
- AI failures don’t break flow; backend returns best-effort results
- Low extracted text yields a warning (may be scanned PDF)
- AI calls are throttled in section mode to avoid overloading the model

---

## How the UI presents results

- Chat-first guidance: friendly prompts through upload and analysis
- Section-by-section view with per-section AI one-liners
- Optional side-by-side diff components are available
- One-click export to a polished PDF report

---

## Quick file map (selected)

- Backend
  - `Program.cs`: endpoints and service registration
  - `Services/PdfService.cs`: per-page text extraction
  - `Services/EnhancedPdfService.cs`: sections & basic coordinates
  - `Services/DiffService.cs`: DiffPlex page-aware diffs
  - `Services/LocalSeverityClassifier.cs`: heuristic severity
  - `Services/OpenAiService.cs`: AI summaries and insights
  - `Services/SectionComparisonService.cs`: section matching & summaries
  - `Services/ReportService.cs`: QuestPDF export
  - `Utils/TextNormalizer.cs`: fix ligatures/OCR quirks
  - `Utils/LocalSimilarityEstimator.cs`: token Jaccard fallback

- Frontend
  - `src/App.tsx`: main UX, section flow, export
  - `src/utils/api.ts`: calls to `/compare`, `/compare-sections`, `/export`
  - `src/utils/types.ts`: TS models
  - Components: `SectionChunkView`, `SummaryPanel`, `SideBySideDiff`, etc.

---

## TL;DR

Upload two PDFs → extract text → compute page-aware diffs or section-by-section changes → classify severity → optionally enrich with Azure OpenAI summaries → visualize results → export a polished PDF report.

---

## Possible future improvements

- Document ingestion and extraction
  - Add OCR for scanned PDFs (e.g., Tesseract via .NET wrappers or Azure Computer Vision) and auto-detect when OCR is needed.
  - Improve layout-aware extraction (coordinates, paragraphs, headers/footers) using iText custom text render listeners; cluster lines into paragraphs; detect lists and tables.
  - Handle hyphenation and line wraps more robustly; normalize whitespace, quotes, and unicode consistently.
  - Table/figure detection and basic structure reconstruction to compare tabular content and captions.

- Diff quality and semantics
  - Offer multiple diff strategies (character/word/sentence/paragraph-level; Patience/Myers; semantic diff for code/policies).
  - Collapse trivial whitespace/formatting changes; optionally highlight moved blocks.
  - Smarter page alignment (detect page insertions/deletions and re-sync) instead of strict index pairing.

- Section-aware comparison
  - Train better header detection (ML or rules tuned by domain); detect TOC and use it to guide section boundaries.
  - Weighted matching by title similarity, heading level, and content embeddings; reduce false matches.
  - Include inline per-section diffs with highlights in the UI and export.

- AI features
  - Support pluggable AI providers (Azure OpenAI, OpenAI, local models) behind a clean abstraction; add cost controls and caching.
  - RAG-style enrichment with domain glossary/policies to improve term consistency and reduce hallucinations.
  - Structured outputs (JSON schemas) with validation, and function-calling for precise, typed insights.
  - Batch prompts for per-section summaries to reduce latency and cost; retry/backoff strategy with circuit breaker.

- Performance and scale
  - Stream uploads and process pages incrementally to reduce peak memory; parallelize page extraction with controlled concurrency.
  - Cache results keyed by content hash (avoid recompute for the same files); enable resumable processing for large docs.
  - Background jobs/queue (Azure Queue/Service Bus) for long-running comparisons; webhooks or polling for status.

- Security and compliance
  - Add authN/authZ (e.g., Azure AD) and role-based access; per-tenant data isolation.
  - Virus scanning on upload; content-size/type enforcement; rate limiting and request validation.
  - PII redaction options; encryption at rest for temporary storage; strict log scrubbing (no raw content leak).

- Frontend UX
  - Built-in PDF viewer with synchronized page navigation and text highlight overlays.
  - Filters (type/severity), search within diffs/sections, deep linking to a specific page/section.
  - Progressive loading indicators, better error states, and accessibility (WCAG) improvements; i18n/l10n support.

- Reporting
  - Include page thumbnails, change heatmaps, and per-section subreports.
  - Export to multiple formats (PDF, DOCX, HTML); append raw diffs as an appendix.
  - Watermarking, branding themes, and custom templates per organization.

- Observability and quality
  - Add structured logging, metrics, and tracing (OpenTelemetry); dashboards for throughput, latency, OCR rates, AI usage.
  - Expand test coverage with goldens for extraction/diff; property-based tests for text normalization.
  - Synthetic datasets and benchmarks for large documents; load testing.

- DevEx and deployment
  - Containerize (Docker) backend and frontend; compose; add CI/CD (GitHub Actions/Azure DevOps) and infrastructure-as-code.
  - Harden configuration: unify OpenAI key names (`DeploymentName` vs `DeploymentOrModelName`), strongly-typed options, and environment-specific overrides.
  - Feature flags for experimental diff/AI modes; safe rollouts.
