# Quick Start Script for Volcanion Auth

Write-Host "🚀 Volcanion Auth Service - Quick Start" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check Docker
try {
    docker --version | Out-Null
    Write-Host "✅ Docker is installed" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not installed. Please install Docker Desktop." -ForegroundColor Red
    exit 1
}

# Check Docker Compose
try {
    docker-compose --version | Out-Null
    Write-Host "✅ Docker Compose is installed" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker Compose is not installed." -ForegroundColor Red
    exit 1
}

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK is installed (version $dotnetVersion)" -ForegroundColor Green
} catch {
    Write-Host "⚠️  .NET SDK not found. Docker-only mode will be used." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Starting services..." -ForegroundColor Yellow

# Start Docker Compose
docker-compose up -d

Write-Host ""
Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check service health
$maxAttempts = 30
$attempt = 0
$healthy = $false

while ($attempt -lt $maxAttempts -and -not $healthy) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -eq 200) {
            $healthy = $true
        }
    } catch {
        $attempt++
        Write-Host "⏳ Waiting for API to be ready... ($attempt/$maxAttempts)" -ForegroundColor Yellow
        Start-Sleep -Seconds 2
    }
}

if ($healthy) {
    Write-Host ""
    Write-Host "✅ All services are running!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📡 Service URLs:" -ForegroundColor Cyan
    Write-Host "   API:        http://localhost:8080" -ForegroundColor White
    Write-Host "   Swagger:    http://localhost:8080/swagger" -ForegroundColor White
    Write-Host "   Health:     http://localhost:8080/health" -ForegroundColor White
    Write-Host "   Metrics:    http://localhost:8080/metrics" -ForegroundColor White
    Write-Host "   Prometheus: http://localhost:9090" -ForegroundColor White
    Write-Host "   Grafana:    http://localhost:3000 (admin/admin)" -ForegroundColor White
    Write-Host ""
    Write-Host "🗄️  Database Ports:" -ForegroundColor Cyan
    Write-Host "   PostgreSQL (Write): localhost:5432" -ForegroundColor White
    Write-Host "   PostgreSQL (Read1): localhost:5433" -ForegroundColor White
    Write-Host "   PostgreSQL (Read2): localhost:5434" -ForegroundColor White
    Write-Host "   Redis:              localhost:6379" -ForegroundColor White
    Write-Host ""
    Write-Host "📝 Quick Test Commands:" -ForegroundColor Cyan
    Write-Host ""
    
    # Test registration
    Write-Host "1️⃣  Register a test user:" -ForegroundColor Yellow
    Write-Host '   $body = @{email="test@example.com";password="Test@123456";firstName="John";lastName="Doe"} | ConvertTo-Json' -ForegroundColor Gray
    Write-Host '   Invoke-RestMethod -Uri "http://localhost:8080/api/v1/authentication/register" -Method Post -ContentType "application/json" -Body $body' -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "2️⃣  Login:" -ForegroundColor Yellow
    Write-Host '   $loginBody = @{email="test@example.com";password="Test@123456"} | ConvertTo-Json' -ForegroundColor Gray
    Write-Host '   $response = Invoke-RestMethod -Uri "http://localhost:8080/api/v1/authentication/login" -Method Post -ContentType "application/json" -Body $loginBody' -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "3️⃣  Check health:" -ForegroundColor Yellow
    Write-Host '   Invoke-RestMethod -Uri "http://localhost:8080/health"' -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "📚 Documentation:" -ForegroundColor Cyan
    Write-Host "   README.md              - Project overview" -ForegroundColor White
    Write-Host "   docs/ARCHITECTURE.md   - Architecture details" -ForegroundColor White
    Write-Host "   docs/API_EXAMPLES.md   - API usage examples" -ForegroundColor White
    Write-Host "   docs/MIGRATION_GUIDE.md - Database migrations" -ForegroundColor White
    Write-Host ""
    
    Write-Host "🛑 To stop all services:" -ForegroundColor Cyan
    Write-Host "   docker-compose down" -ForegroundColor White
    Write-Host ""
    
    Write-Host "✨ Opening Swagger UI in browser..." -ForegroundColor Green
    Start-Sleep -Seconds 2
    Start-Process "http://localhost:8080/swagger"
    
} else {
    Write-Host ""
    Write-Host "❌ Services failed to start within timeout period." -ForegroundColor Red
    Write-Host "   Check logs with: docker-compose logs" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
