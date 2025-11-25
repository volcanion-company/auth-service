# Database Setup Script for Volcanion Auth Service
# This script handles database creation, migration, and seeding

param(
    [Parameter(Mandatory=$false)]
    [switch]$SkipMigration,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipSeeding,
    
    [Parameter(Mandatory=$false)]
    [switch]$DropDatabase,
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Development"
)

# Configuration
$ErrorActionPreference = "Stop"
$ProjectPath = "src\VolcanionAuth.API\VolcanionAuth.API.csproj"
$ScriptRoot = $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Volcanion Auth Database Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET CLI is available
Write-Host "Checking .NET CLI..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET CLI version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET CLI not found. Please install .NET SDK." -ForegroundColor Red
    exit 1
}

# Check if EF Core tools are installed
Write-Host "Checking EF Core tools..." -ForegroundColor Yellow
$efToolsInstalled = dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efToolsInstalled) {
    Write-Host "! EF Core tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "✓ EF Core tools installed successfully" -ForegroundColor Green
} else {
    Write-Host "✓ EF Core tools are already installed" -ForegroundColor Green
}

# Navigate to project directory
$projectDir = Join-Path $ScriptRoot $ProjectPath | Split-Path
Push-Location $projectDir

try {
    # Drop database if requested
    if ($DropDatabase) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Yellow
        Write-Host "Dropping existing database..." -ForegroundColor Yellow
        Write-Host "========================================" -ForegroundColor Yellow
        
        try {
            dotnet ef database drop --force --environment $Environment
            Write-Host "✓ Database dropped successfully" -ForegroundColor Green
        } catch {
            Write-Host "! Database does not exist or could not be dropped" -ForegroundColor Yellow
        }
    }

    # Run migrations
    if (-not $SkipMigration) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Yellow
        Write-Host "Running database migrations..." -ForegroundColor Yellow
        Write-Host "========================================" -ForegroundColor Yellow
        
        # Update database
        Write-Host "Applying migrations to database..." -ForegroundColor Yellow
        dotnet ef database update --environment $Environment
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Database migrations applied successfully" -ForegroundColor Green
        } else {
            Write-Host "✗ Failed to apply database migrations" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "⊘ Skipping database migrations" -ForegroundColor Yellow
    }

    # Run seeding
    if (-not $SkipSeeding) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Yellow
        Write-Host "Seeding database with sample data..." -ForegroundColor Yellow
        Write-Host "========================================" -ForegroundColor Yellow
        
        # Build the project first
        Write-Host "Building project..." -ForegroundColor Yellow
        dotnet build --configuration Release
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Build failed" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "✓ Build completed successfully" -ForegroundColor Green
        
        # Run the application with seed command
        Write-Host "Executing database seeder..." -ForegroundColor Yellow
        
        # Run the application with seed argument
        dotnet run --environment $Environment --no-build --configuration Release -- seed
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Database seeded successfully" -ForegroundColor Green
        } else {
            Write-Host "! Seeding completed. Check output above for any issues." -ForegroundColor Yellow
        }
    } else {
        Write-Host "⊘ Skipping database seeding" -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Database Setup Completed!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Sample Users Created:" -ForegroundColor Yellow
    Write-Host "  Admin:     admin@volcanion.com     / Admin@123456" -ForegroundColor White
    Write-Host "  Manager:   manager@volcanion.com   / Manager@123456" -ForegroundColor White
    Write-Host "  User 1:    user1@volcanion.com     / User@123456" -ForegroundColor White
    Write-Host "  User 2:    user2@volcanion.com     / User@123456" -ForegroundColor White
    Write-Host "  Guest:     guest@volcanion.com     / Guest@123456" -ForegroundColor White
    Write-Host "  Developer: developer@volcanion.com / Dev@123456" -ForegroundColor White
    Write-Host "  Support:   support@volcanion.com   / Support@123456" -ForegroundColor White
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "✗ An error occurred: $_" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
} finally {
    # Return to original directory
    Pop-Location
}

Write-Host "Usage Examples:" -ForegroundColor Cyan
Write-Host "  .\setup.ps1                          # Full setup (migrate + seed)" -ForegroundColor Gray
Write-Host "  .\setup.ps1 -DropDatabase           # Drop and recreate database" -ForegroundColor Gray
Write-Host "  .\setup.ps1 -SkipMigration          # Only seed data" -ForegroundColor Gray
Write-Host "  .\setup.ps1 -SkipSeeding            # Only run migrations" -ForegroundColor Gray
Write-Host "  .\setup.ps1 -Environment Production # Use production settings" -ForegroundColor Gray
Write-Host ""
