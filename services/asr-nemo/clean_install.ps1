# Clean Installation Script - Removes existing venv and reinstalls everything fresh

param(
    [string]$PythonPath = "C:\Users\Jacar\AppData\Local\Programs\Python\Python312\python.exe",
    [string]$VenvDir = "venv312"
)

Write-Host "ASR-NeMo Clean Installation Script" -ForegroundColor Magenta
Write-Host "====================================" -ForegroundColor Magenta
Write-Host ""

# Remove existing virtual environment if it exists
if (Test-Path $VenvDir) {
    Write-Host "Removing existing virtual environment..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $VenvDir
    Write-Host "Existing virtual environment removed." -ForegroundColor Green
}

# Check if Python 3.12 exists
if (-not (Test-Path $PythonPath)) {
    Write-Host "Error: Python 3.12 not found at $PythonPath" -ForegroundColor Red
    Write-Host "Please install Python 3.12 or update the -PythonPath parameter." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Get Python version
$pythonVersion = & $PythonPath --version
Write-Host "Using Python: $pythonVersion" -ForegroundColor Green

# Create fresh virtual environment
Write-Host "Creating fresh Python 3.12 virtual environment..." -ForegroundColor Cyan
& $PythonPath -m venv $VenvDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to create virtual environment" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

$venvPython = Join-Path $VenvDir "Scripts\python.exe"
$venvPip = Join-Path $VenvDir "Scripts\pip.exe"

Write-Host "Virtual environment created successfully." -ForegroundColor Green
Write-Host "Now run: .\start_service.ps1 to install packages and start the service" -ForegroundColor Cyan