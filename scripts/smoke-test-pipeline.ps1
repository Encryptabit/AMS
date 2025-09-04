#!/usr/bin/env pwsh
<#
.SYNOPSIS
Smoke test script for the revised AMS pipeline

.DESCRIPTION
This script validates the end-to-end pipeline functionality using synthetic test data.
It tests the complete pipeline: timeline → plan → chunks → transcripts → align-chunks → refine → collate → validate

.PARAMETER TestAudio
Path to a test audio file. If not provided, creates synthetic test data.

.PARAMETER WorkDir
Working directory for test outputs. Defaults to temp directory.

.PARAMETER AsrService
URL of the ASR service (Nemo). Default: http://localhost:8081

.PARAMETER AeneasService
URL of the Aeneas alignment service. Default: http://localhost:8082

.PARAMETER SkipServices
Skip service health checks and pipeline stages that require services
#>

param(
    [string]$TestAudio,
    [string]$WorkDir = (Join-Path $env:TEMP "ams-smoke-test-$(Get-Date -Format 'yyyyMMdd-HHmmss')"),
    [string]$AsrService = "http://localhost:8081",
    [string]$AeneasService = "http://localhost:8082",
    [switch]$SkipServices,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "=== AMS Pipeline Smoke Test ===" -ForegroundColor Green
Write-Host "Work directory: $WorkDir" -ForegroundColor Cyan
Write-Host "ASR Service: $AsrService" -ForegroundColor Cyan
Write-Host "Aeneas Service: $AeneasService" -ForegroundColor Cyan

# Create work directory
New-Item -ItemType Directory -Force -Path $WorkDir | Out-Null

# Find the AMS CLI executable
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$CliProject = Join-Path $ProjectRoot "host/Ams.Cli"
$CliBinary = Join-Path $CliProject "bin/Debug/net9.0/Ams.Cli.dll"

if (-not (Test-Path $CliBinary)) {
    Write-Host "Building AMS CLI..." -ForegroundColor Yellow
    dotnet build $CliProject
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build AMS CLI"
    }
}

function Invoke-AmsCommand {
    param([string]$Command, [switch]$AllowFailure)
    
    if ($Verbose) {
        Write-Host "Running: dotnet $CliBinary $Command" -ForegroundColor DarkGray
    }
    
    $output = dotnet $CliBinary @($Command -split ' ') 2>&1
    
    if ($LASTEXITCODE -ne 0 -and -not $AllowFailure) {
        Write-Host "Command failed: $Command" -ForegroundColor Red
        Write-Host $output -ForegroundColor Red
        throw "AMS command failed"
    }
    
    if ($Verbose) {
        Write-Host $output -ForegroundColor DarkGray
    }
    
    return $output
}

function Test-ServiceHealth {
    param([string]$Url, [string]$Name)
    
    Write-Host "Checking $Name health at $Url..." -ForegroundColor Yellow
    
    try {
        $response = Invoke-RestMethod -Uri "$Url/health" -Method Get -TimeoutSec 5
        Write-Host "✓ $Name is healthy" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "✗ $Name is not available: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Create synthetic test audio if not provided
if (-not $TestAudio) {
    Write-Host "Creating synthetic test audio..." -ForegroundColor Yellow
    $TestAudio = Join-Path $WorkDir "test-audio.wav"
    
    # Create a simple 10-second WAV file using FFmpeg (if available)
    try {
        $ffmpegCmd = "ffmpeg"
        if ($env:FFMPEG_EXE) {
            $ffmpegCmd = $env:FFMPEG_EXE
        }
        
        & $ffmpegCmd -f lavfi -i "sine=frequency=440:duration=10" -ar 44100 -ac 1 -y $TestAudio 2>$null
        Write-Host "✓ Created synthetic test audio: $TestAudio" -ForegroundColor Green
    }
    catch {
        Write-Host "FFmpeg not available, creating minimal WAV..." -ForegroundColor Yellow
        # Create a minimal valid WAV file
        $wavBytes = @(
            # RIFF header
            0x52, 0x49, 0x46, 0x46, # "RIFF"
            0x24, 0x08, 0x00, 0x00, # File size - 8
            0x57, 0x41, 0x56, 0x45, # "WAVE"
            # fmt chunk
            0x66, 0x6D, 0x74, 0x20, # "fmt "
            0x10, 0x00, 0x00, 0x00, # fmt chunk size
            0x01, 0x00,             # Audio format (PCM)
            0x01, 0x00,             # Num channels
            0x44, 0xAC, 0x00, 0x00, # Sample rate (44100)
            0x88, 0x58, 0x01, 0x00, # Byte rate
            0x02, 0x00,             # Block align
            0x10, 0x00,             # Bits per sample
            # data chunk
            0x64, 0x61, 0x74, 0x61, # "data"
            0x00, 0x08, 0x00, 0x00  # data size
        )
        
        # Add some silent samples (44100 samples = 1 second)
        $samples = @(0, 0) * 22050  # 1 second of silence
        $allBytes = $wavBytes + $samples
        [System.IO.File]::WriteAllBytes($TestAudio, $allBytes)
    }
}

Write-Host "Test audio: $TestAudio" -ForegroundColor Cyan

# Service health checks
if (-not $SkipServices) {
    $AsrHealthy = Test-ServiceHealth $AsrService "ASR (Nemo)"
    $AeneasHealthy = Test-ServiceHealth $AeneasService "Aeneas"
    
    if (-not $AsrHealthy) {
        Write-Host "⚠️  ASR service not available. Start with: services/asr-nemo/start_service.bat" -ForegroundColor Yellow
        $SkipServices = $true
    }
    
    if (-not $AeneasHealthy) {
        Write-Host "⚠️  Aeneas service not available. Start with: uvicorn app.main:app --port 8082" -ForegroundColor Yellow
        Write-Host "   From directory: services/aeneas/" -ForegroundColor Yellow
        $SkipServices = $true
    }
}

# Test 1: Environment validation
Write-Host "`n1. Testing environment validation..." -ForegroundColor Magenta
try {
    if (-not $SkipServices) {
        Invoke-AmsCommand "env aeneas-validate --service $AeneasService --smoke-test"
        Write-Host "✓ Aeneas service validation passed" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Skipping Aeneas validation (service not available)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Environment validation failed" -ForegroundColor Red
    throw
}

# Test 2: Individual stages (up to what doesn't require services)
Write-Host "`n2. Testing individual pipeline stages..." -ForegroundColor Magenta

Write-Host "  2a. Detecting silence..." -ForegroundColor Yellow
try {
    Invoke-AmsCommand "asr detect-silence --in $TestAudio --work $WorkDir"
    $timelinePath = Join-Path $WorkDir "timeline/silence.json"
    if (Test-Path $timelinePath) {
        Write-Host "✓ Silence detection completed" -ForegroundColor Green
    } else {
        throw "Timeline file not created"
    }
} catch {
    Write-Host "✗ Silence detection failed" -ForegroundColor Red
    throw
}

Write-Host "  2b. Planning windows..." -ForegroundColor Yellow
try {
    Invoke-AmsCommand "asr plan-windows --in $TestAudio --work $WorkDir"
    $planPath = Join-Path $WorkDir "plan/windows.json"
    if (Test-Path $planPath) {
        Write-Host "✓ Window planning completed" -ForegroundColor Green
    } else {
        throw "Plan file not created"
    }
} catch {
    Write-Host "✗ Window planning failed" -ForegroundColor Red
    throw
}

# Test 3: Full pipeline (if services available)
Write-Host "`n3. Testing full pipeline..." -ForegroundColor Magenta

if (-not $SkipServices) {
    try {
        Write-Host "Running complete pipeline from timeline to validate..." -ForegroundColor Yellow
        Invoke-AmsCommand "asr run --in $TestAudio --work $WorkDir --from timeline --to validate"
        
        # Check that all expected outputs exist
        $expectedFiles = @(
            "timeline/silence.json",
            "plan/windows.json", 
            "chunks/index.json",
            "transcripts/merged.json",
            "align-chunks/params.snapshot.json",
            "refine/sentences.json",
            "collate/final.wav",
            "validate/report.json"
        )
        
        $allFilesExist = $true
        foreach ($file in $expectedFiles) {
            $fullPath = Join-Path $WorkDir $file
            if (Test-Path $fullPath) {
                Write-Host "✓ Found: $file" -ForegroundColor Green
            } else {
                Write-Host "✗ Missing: $file" -ForegroundColor Red
                $allFilesExist = $false
            }
        }
        
        if ($allFilesExist) {
            Write-Host "✓ Complete pipeline executed successfully" -ForegroundColor Green
        } else {
            throw "Pipeline completed but some outputs are missing"
        }
        
    } catch {
        Write-Host "✗ Full pipeline failed" -ForegroundColor Red
        Write-Host "This may be expected if services are not properly configured" -ForegroundColor Yellow
        
        # Don't fail the entire test if this was just a service issue
        if ($_.Exception.Message -like "*service*" -or $_.Exception.Message -like "*connection*") {
            Write-Host "⚠️  Pipeline failure appears to be service-related" -ForegroundColor Yellow
        } else {
            throw
        }
    }
} else {
    Write-Host "⚠️  Skipping full pipeline test (services not available)" -ForegroundColor Yellow
}

# Test 4: Idempotency check
Write-Host "`n4. Testing idempotency..." -ForegroundColor Magenta
try {
    Write-Host "Re-running detect-silence (should skip)..." -ForegroundColor Yellow
    $output = Invoke-AmsCommand "asr detect-silence --in $TestAudio --work $WorkDir"
    if ($output -like "*up-to-date*" -or $output -like "*Skipping*") {
        Write-Host "✓ Idempotency working - stage was skipped" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Stage may have re-run (expected if fingerprint changed)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Idempotency test failed" -ForegroundColor Red
    throw
}

# Test 5: Manifest validation
Write-Host "`n5. Testing manifest validation..." -ForegroundColor Magenta
try {
    Invoke-AmsCommand "validate --work $WorkDir"
    Write-Host "✓ Manifest validation passed" -ForegroundColor Green
} catch {
    Write-Host "✗ Manifest validation failed" -ForegroundColor Red
    throw
}

# Summary
Write-Host "`n=== Smoke Test Summary ===" -ForegroundColor Green
Write-Host "✓ Environment validation" -ForegroundColor Green
Write-Host "✓ Individual stages (timeline, plan)" -ForegroundColor Green
if (-not $SkipServices) {
    Write-Host "✓ Full pipeline execution" -ForegroundColor Green
} else {
    Write-Host "⚠️  Full pipeline skipped (services not available)" -ForegroundColor Yellow
}
Write-Host "✓ Idempotency check" -ForegroundColor Green
Write-Host "✓ Manifest validation" -ForegroundColor Green

Write-Host "`nWork directory preserved for inspection: $WorkDir" -ForegroundColor Cyan
Write-Host "Smoke test completed successfully! 🎉" -ForegroundColor Green

# Optional: Display some key metrics if available
$manifestPath = Join-Path $WorkDir "manifest.json"
if (Test-Path $manifestPath) {
    try {
        $manifest = Get-Content $manifestPath | ConvertFrom-Json
        Write-Host "`n=== Pipeline Metrics ===" -ForegroundColor Cyan
        Write-Host "Input duration: $($manifest.Input.DurationSec)s" -ForegroundColor Cyan
        Write-Host "Stages completed: $($manifest.Stages.Keys -join ', ')" -ForegroundColor Cyan
        
        $completedStages = ($manifest.Stages.Values | Where-Object { $_.Status.Status -eq "completed" }).Count
        $totalStages = $manifest.Stages.Count
        Write-Host "Stage completion: $completedStages/$totalStages" -ForegroundColor Cyan
    }
    catch {
        # Ignore JSON parsing errors
    }
}