# Test the document comparison API
$uri = "http://localhost:5000/compare"

Write-Host "Testing document comparison API..."
Write-Host "Uploading SoftwareSpec_v1.pdf and SoftwareSpec_v2.pdf..."

try {
    # Create multipart form data manually
    $boundary = [System.Guid]::NewGuid().ToString()
    $LF = "`r`n"
    
    # Read PDF files (use full paths)
    $pdf1Path = Join-Path $PWD "SoftwareSpec_v1.pdf"
    $pdf2Path = Join-Path $PWD "SoftwareSpec_v2.pdf"
    
    if (-not (Test-Path $pdf1Path)) {
        throw "Could not find SoftwareSpec_v1.pdf in current directory: $PWD"
    }
    if (-not (Test-Path $pdf2Path)) {
        throw "Could not find SoftwareSpec_v2.pdf in current directory: $PWD"
    }
    
    $pdf1Content = [System.IO.File]::ReadAllBytes($pdf1Path)
    $pdf2Content = [System.IO.File]::ReadAllBytes($pdf2Path)
    
    # Build multipart form data
    $bodyLines = @(
        "--$boundary",
        "Content-Disposition: form-data; name=`"pdf1`"; filename=`"SoftwareSpec_v1.pdf`"",
        "Content-Type: application/pdf",
        "",
        [System.Text.Encoding]::GetEncoding("iso-8859-1").GetString($pdf1Content),
        "--$boundary",
        "Content-Disposition: form-data; name=`"pdf2`"; filename=`"SoftwareSpec_v2.pdf`"",
        "Content-Type: application/pdf", 
        "",
        [System.Text.Encoding]::GetEncoding("iso-8859-1").GetString($pdf2Content),
        "--$boundary--"
    )
    
    $body = $bodyLines -join $LF
    $bodyBytes = [System.Text.Encoding]::GetEncoding("iso-8859-1").GetBytes($body)
    
    $response = Invoke-RestMethod -Uri $uri -Method Post -Body $bodyBytes -ContentType "multipart/form-data; boundary=$boundary"
    
    Write-Host "`nComparison Results:" -ForegroundColor Green
    Write-Host "===================" -ForegroundColor Green
    Write-Host "Summary: $($response.Summary)"
    Write-Host "Similarity Score: $($response.SimilarityScore)"
    Write-Host "Number of Diff Segments: $($response.DiffSegments.Count)"
    
    Write-Host "`nFirst 5 Diff Segments:" -ForegroundColor Yellow
    for ($i = 0; $i -lt [Math]::Min(5, $response.DiffSegments.Count); $i++) {
        $segment = $response.DiffSegments[$i]
        Write-Host "[$($segment.Type)] Severity: $($segment.Severity) - $($segment.Text.Substring(0, [Math]::Min(100, $segment.Text.Length)))..."
    }
    
    Write-Host "`nAPI test completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error testing API: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the server is running on http://localhost:5000" -ForegroundColor Yellow
}