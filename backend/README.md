# DocumentComparer (Backend) - .NET 9

A modular, minimal .NET 9 backend for comparing two PDF documents. Designed to be **stateless** (no SQL, no Blob) and fast enough for prototyping and early production.

## Features

- Upload two PDFs via `/compare` (multipart/form-data: `pdf1`, `pdf2`)
- Per-page text extraction with PdfPig
- Text diff with DiffPlex
- Local severity classification (heuristic)
- Optional Azure OpenAI (GPT) semantic summary and classification
- Export PDF report via `/export`
- No database required

## Quick start

1. Install .NET 9 SDK.
2. Add your Azure OpenAI settings to `appsettings.json` or environment variables:
   - `AzureOpenAI:Endpoint`
   - `AzureOpenAI:ApiKey`
   - `AzureOpenAI:DeploymentOrModelName` (e.g. `gpt-4o-mini`)
3. Run:

```bash
dotnet restore
dotnet run
