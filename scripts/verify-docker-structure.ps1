# PowerShell script to verify Docker structure
Write-Host "Verifying Docker structure..." -ForegroundColor Yellow

$projectRoot = Split-Path -Parent $PSScriptRoot
$dockerConfigDir = Join-Path $projectRoot "configuration\docker"

Write-Host "Project root: $projectRoot" -ForegroundColor Yellow
Write-Host "Docker config directory: $dockerConfigDir" -ForegroundColor Yellow

# Check runner environments
Write-Host "`nChecking runner environments..." -ForegroundColor Green

$runnerLanguages = @("csharp", "javascript", "python", "typescript")
foreach ($language in $runnerLanguages) {
    $runnerPath = Join-Path $dockerConfigDir "environments\runners\$language"
    $dockerfilePath = Join-Path $runnerPath "Dockerfile"
    
    if (Test-Path $dockerfilePath) {
        Write-Host "✓ Found runner Dockerfile for $language" -ForegroundColor Green
    } else {
        Write-Host "✗ Missing runner Dockerfile for $language" -ForegroundColor Red
    }
}

# Check tester environments
Write-Host "`nChecking tester environments..." -ForegroundColor Green

$testerLanguages = @("csharp", "python", "typescript")
foreach ($language in $testerLanguages) {
    $testerPath = Join-Path $dockerConfigDir "environments\testers\$language"
    $dockerfilePath = Join-Path $testerPath "Dockerfile"
    
    if (Test-Path $dockerfilePath) {
        Write-Host "✓ Found tester Dockerfile for $language" -ForegroundColor Green
    } else {
        Write-Host "✗ Missing tester Dockerfile for $language" -ForegroundColor Red
    }
}

Write-Host "`nStructure verification completed!" -ForegroundColor Yellow