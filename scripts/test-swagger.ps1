# Test Swagger Availability

Write-Host "🔍 Testing Volcanion Auth API..." -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000"
$maxAttempts = 10
$attempt = 0

Write-Host "⏳ Waiting for API to start..." -ForegroundColor Yellow

while ($attempt -lt $maxAttempts) {
    try {
        $healthResponse = Invoke-WebRequest -Uri "$baseUrl/health/live" -UseBasicParsing -TimeoutSec 2
        if ($healthResponse.StatusCode -eq 200) {
            Write-Host "✅ API is running!" -ForegroundColor Green
            break
        }
    } catch {
        $attempt++
        Write-Host "  Attempt $attempt/$maxAttempts - waiting..." -ForegroundColor Gray
        Start-Sleep -Seconds 1
    }
}

if ($attempt -eq $maxAttempts) {
    Write-Host "❌ API failed to start. Please check if it's running:" -ForegroundColor Red
    Write-Host "   cd src\VolcanionAuth.API" -ForegroundColor Yellow
    Write-Host "   dotnet run" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Testing Swagger UI..." -ForegroundColor Cyan

try {
    $swaggerResponse = Invoke-WebRequest -Uri "$baseUrl/swagger/index.html" -UseBasicParsing -TimeoutSec 5
    if ($swaggerResponse.StatusCode -eq 200) {
        Write-Host "✅ Swagger UI is accessible!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Available URLs:" -ForegroundColor Cyan
        Write-Host "   Swagger UI:  $baseUrl/swagger" -ForegroundColor White
        Write-Host "   Health:      $baseUrl/health" -ForegroundColor White
        Write-Host "   Metrics:     $baseUrl/metrics" -ForegroundColor White
        Write-Host ""
        
        # Open browser
        Write-Host "🌐 Opening Swagger UI in browser..." -ForegroundColor Green
        Start-Process "$baseUrl/swagger"
    }
} catch {
    Write-Host "❌ Swagger UI is not accessible!" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Checking Swagger JSON..." -ForegroundColor Yellow
    
    try {
        $swaggerJsonResponse = Invoke-WebRequest -Uri "$baseUrl/swagger/v1/swagger.json" -UseBasicParsing -TimeoutSec 5
        Write-Host "✅ Swagger JSON is generated successfully" -ForegroundColor Green
    } catch {
        Write-Host "❌ Swagger JSON generation failed!" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "💡 Common fixes:" -ForegroundColor Yellow
        Write-Host "   1. Check for duplicate DTO names" -ForegroundColor Gray
        Write-Host "   2. Ensure all action methods have unique routes" -ForegroundColor Gray
        Write-Host "   3. Check ProducesResponseType attributes" -ForegroundColor Gray
        Write-Host "   4. Review Swagger configuration in Program.cs" -ForegroundColor Gray
    }
}

Write-Host ""
