<#
.SYNOPSIS
  Builds, packs, and pushes all HealthData.Interop NuGet packages to nuget.org in one command.

.DESCRIPTION
  This script automates the full release pipeline:
    1. dotnet restore
    2. dotnet build (Release)
    3. dotnet test
    4. dotnet pack (produces .nupkg + .snupkg for all 4 packages)
    5. dotnet nuget push (all .nupkg + .snupkg files to nuget.org)

  Prerequisites:
    - NUGET_API_KEY environment variable set to your nuget.org API key
    - .NET 10 SDK installed (per global.json)

.PARAMETER SkipTests
  Skip the dotnet test step (faster, but less safe).

.PARAMETER SkipPush
  Build and pack only; do not push to nuget.org. Useful for local inspection.

.PARAMETER Version
  Override the version number for all packages (e.g., -Version 1.4.1).
  By default, the version in each .csproj is used.

.EXAMPLE
  .\publish-packages.ps1
  Full pipeline: build, test, pack, push all 4 packages to nuget.org.

.EXAMPLE
  .\publish-packages.ps1 -SkipPush
  Build and pack only; inspect the .nupkg files in dist/ before pushing manually.

.EXAMPLE
  .\publish-packages.ps1 -Version 1.4.1
  Bump all packages to 1.4.1 and publish.
#>

[CmdletBinding()]
param(
    [switch]$SkipTests,
    [switch]$SkipPush,
    [string]$Version
)

$ErrorActionPreference = "Stop"
$repoRoot = $PSScriptRoot
$distDir  = Join-Path $repoRoot "dist"

# --- 1. Restore ---
Write-Host "`n==> dotnet restore" -ForegroundColor Cyan
dotnet restore (Join-Path $repoRoot "HealthData.Interop.slnx")
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }

# --- 2. Build ---
Write-Host "`n==> dotnet build (Release)" -ForegroundColor Cyan
dotnet build (Join-Path $repoRoot "HealthData.Interop.slnx") --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

# --- 3. Test ---
if (-not $SkipTests) {
    Write-Host "`n==> dotnet test" -ForegroundColor Cyan
    dotnet test (Join-Path $repoRoot "src\tests\HealthData.Interop.Tests.csproj") --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) { throw "dotnet test failed" }
} else {
    Write-Host "`n==> Skipping tests (-SkipTests)" -ForegroundColor Yellow
}

# --- 4. Pack ---
Write-Host "`n==> dotnet pack (Release)" -ForegroundColor Cyan

$packArgs = @(
    "--configuration", "Release",
    "--no-build",
    "-o", $distDir
)

# Version override
if ($Version) {
    $packArgs += @("--version-suffix", $Version)
}

# Pack each library project (demos and tests are not packable)
$packableProjects = @(
    "src\Abstractions\HealthData.Interop.Abstractions\HealthData.Interop.Abstractions.csproj"
    "src\Logging.Extensions\HealthData.Interop.Logging.Extensions\HealthData.Interop.Logging.Extensions.csproj"
    "src\Logging.Serilog\HealthData.Interop.Logging.Serilog\HealthData.Interop.Logging.Serilog.csproj"
    "src\HealthData.Interop.Fhir\HealthDataInteropSharedLibrary.csproj"
)

foreach ($proj in $packableProjects) {
    $projPath = Join-Path $repoRoot $proj
    Write-Host "  Packing: $proj" -ForegroundColor DarkGray
    dotnet pack $projPath @packArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed for $proj" }
}

# --- 5. Push ---
if (-not $SkipPush) {
    # Verify NUGET_API_KEY
    if (-not $env:NUGET_API_KEY) {
        throw "NUGET_API_KEY environment variable is not set. Get your API key from https://www.nuget.org/account/api and set it before running this script."
    }

    Write-Host "`n==> dotnet nuget push (nuget.org)" -ForegroundColor Cyan

    $nupkgs = Get-ChildItem $distDir -Filter "*.nupkg" | Sort-Object Name
    $snupkgs = Get-ChildItem $distDir -Filter "*.snupkg" | Sort-Object Name

    Write-Host "  Found $($nupkgs.Count) .nupkg and $($snupkgs.Count) .snupkg files" -ForegroundColor DarkGray

    foreach ($pkg in $nupkgs + $snupkgs) {
        Write-Host "  Pushing: $($pkg.Name)" -ForegroundColor DarkGray
        dotnet nuget push $pkg.FullName --source nuget.org --api-key $env:NUGET_API_KEY --skip-duplicate
        if ($LASTEXITCODE -ne 0) { throw "dotnet nuget push failed for $($pkg.Name)" }
    }

    Write-Host "`n==> All packages pushed successfully!" -ForegroundColor Green
} else {
    Write-Host "`n==> Skipping push (-SkipPush)" -ForegroundColor Yellow
    Write-Host "    Packages are in: $distDir" -ForegroundColor DarkGray
    Write-Host "    To push manually:  dotnet nuget push <file>.nupkg --source nuget.org" -ForegroundColor DarkGray
}

# --- Summary ---
Write-Host "`n==> Dist folder contents:" -ForegroundColor Cyan
Get-ChildItem $distDir -Filter "*.nupkg" | ForEach-Object { Write-Host "    $($_.Name)  ($([math]::Round($_.Length / 1MB, 2)) MB)" }
Get-ChildItem $distDir -Filter "*.snupkg" | ForEach-Object { Write-Host "    $($_.Name)  ($([math]::Round($_.Length / 1MB, 2)) MB)" }

Write-Host "`nDone." -ForegroundColor Green
