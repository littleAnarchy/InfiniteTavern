#!/usr/bin/env pwsh

Write-Host "🎮 Infinite Tavern - Setup Script" -ForegroundColor Cyan
Write-Host ""

# Check for .NET 8
Write-Host "Checking for .NET 8 SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ .NET SDK version: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "✗ .NET 8 SDK not found. Please install from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Check for Docker
Write-Host "Checking for Docker..." -ForegroundColor Yellow
$dockerVersion = docker --version 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Docker found: $dockerVersion" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "Starting PostgreSQL container..." -ForegroundColor Yellow
    docker-compose up -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ PostgreSQL container started" -ForegroundColor Green
        Write-Host "Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }
} else {
    Write-Host "! Docker not found. You'll need to setup PostgreSQL manually." -ForegroundColor Yellow
}

# Restore dependencies
Write-Host ""
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ NuGet packages restored" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Build solution
Write-Host ""
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --configuration Debug
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build successful" -ForegroundColor Green
} else {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}

# Check for API key
Write-Host ""
Write-Host "Checking configuration..." -ForegroundColor Yellow
$appsettings = Get-Content "src/InfiniteTavern.API/appsettings.json" | ConvertFrom-Json
if ($appsettings.Anthropic.ApiKey -eq "your-anthropic-api-key-here") {
    Write-Host "! IMPORTANT: Please update the Anthropic API key in:" -ForegroundColor Yellow
    Write-Host "  src/InfiniteTavern.API/appsettings.json" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎉 Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Update the Anthropic API key in appsettings.json" -ForegroundColor White
Write-Host "2. Run database migrations:" -ForegroundColor White
Write-Host "   cd src/InfiniteTavern.API" -ForegroundColor Gray
Write-Host "   dotnet ef database update" -ForegroundColor Gray
Write-Host "3. Start the API:" -ForegroundColor White
Write-Host "   dotnet run --project src/InfiniteTavern.API" -ForegroundColor Gray
Write-Host ""
