# ASR-NeMo Service Startup Script (PowerShell)
param(
    [string]$PythonPath = "C:\Users\Jacar\AppData\Local\Programs\Python\Python312\python.exe",
    [string]$VenvDir = "venv312",
    [int]$Port = 8000,
    [switch]$ForceReinstall = $false
)

Write-Host "ASR-NeMo Service Startup Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Check if Python 3.12 exists
if (-not (Test-Path $PythonPath)) {
    Write-Host "Error: Python 3.12 not found at $PythonPath" -ForegroundColor Red
    Write-Host "Please install Python 3.12 or update the -PythonPath parameter." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Get Python version
$pythonVersion = & $PythonPath --version
Write-Host "Found Python: $pythonVersion" -ForegroundColor Green

# Create virtual environment if it doesn't exist
if (-not (Test-Path $VenvDir)) {
    Write-Host "Creating Python 3.12 virtual environment..." -ForegroundColor Yellow
    & $PythonPath -m venv $VenvDir
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Failed to create virtual environment" -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
    Write-Host "Virtual environment created successfully." -ForegroundColor Green
} else {
    Write-Host "Virtual environment already exists." -ForegroundColor Green
}

# Activate virtual environment
Write-Host "Activating virtual environment..." -ForegroundColor Yellow
$venvPython = Join-Path $VenvDir "Scripts\python.exe"
$venvPip = Join-Path $VenvDir "Scripts\pip.exe"

# Verify virtual environment Python version
$venvPythonVersion = & $venvPython --version 2>$null
if ($venvPythonVersion -match "3\.1[2-9]") {
    Write-Host "Successfully using virtual environment: $venvPythonVersion" -ForegroundColor Green
} else {
    Write-Host "Warning: Virtual environment may not be using Python 3.12+" -ForegroundColor Yellow
    Write-Host "Virtual environment Python: $venvPythonVersion" -ForegroundColor Yellow
}

# Package installation logic
if ($ForceReinstall) {
    Write-Host "Force reinstall requested - installing/updating packages..." -ForegroundColor Yellow
    $ShouldInstall = $true
} else {
    # Check if essential packages are already installed
    Write-Host "Checking if packages are already installed..." -ForegroundColor Cyan
    
    $packagesToCheck = @("fastapi", "uvicorn", "torch", "nemo-toolkit")
    $missingPackages = @()
    
    foreach ($package in $packagesToCheck) {
        $result = & $venvPip show $package 2>$null
        if ($LASTEXITCODE -ne 0) {
            $missingPackages += $package
        }
    }
    
    if ($missingPackages.Count -gt 0) {
        Write-Host "Missing packages detected: $($missingPackages -join ', ')" -ForegroundColor Yellow
        Write-Host "Installing missing packages..." -ForegroundColor Yellow
        $ShouldInstall = $true
    } else {
        Write-Host "All essential packages already installed. Skipping installation." -ForegroundColor Green
        Write-Host "Use -ForceReinstall to force package installation." -ForegroundColor Cyan
        $ShouldInstall = $false
    }
}

if ($ShouldInstall) {

# FIRST PRIORITY: Always upgrade pip before anything else
Write-Host "Stage 0: Upgrading pip..." -ForegroundColor Magenta
& $venvPip install --upgrade pip
Write-Host "pip upgrade completed." -ForegroundColor Green

# First: Install essential build tools
Write-Host "Stage 1: Installing build essentials..." -ForegroundColor Cyan
& $venvPip install --upgrade setuptools wheel

# Second: Install core packages that other packages depend on
Write-Host "Stage 2: Installing core dependencies..." -ForegroundColor Cyan
& $venvPip install "numpy>=1.24.0,<2.0.0" omegaconf "hydra-core>=1.3.0"

Write-Host "Stage 2b: Installing CUDA-enabled PyTorch..." -ForegroundColor Cyan
& $venvPip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121

# Third: Install web framework
Write-Host "Stage 3: Installing web framework..." -ForegroundColor Cyan
& $venvPip install "fastapi>=0.104.0" "uvicorn[standard]>=0.24.0"

# Fourth: Install audio processing
Write-Host "Stage 4: Installing audio processing..." -ForegroundColor Cyan
& $venvPip install "soundfile>=0.12.0" "librosa>=0.10.0"

# Fifth: Install Hugging Face packages
Write-Host "Stage 5: Installing Hugging Face packages..." -ForegroundColor Cyan
& $venvPip install "huggingface-hub>=0.19.0" "transformers>=4.35.0"

# Sixth: Install PyTorch Lightning (needed by NeMo)
Write-Host "Stage 6: Installing PyTorch Lightning..." -ForegroundColor Cyan
& $venvPip install "pytorch-lightning>=2.0.0,<3.0.0"

# Final: Install NeMo (most complex dependency)
Write-Host "Stage 7: Installing NeMo toolkit..." -ForegroundColor Cyan
& $venvPip install "nemo-toolkit[asr]>=1.20.0,<2.0.0" --no-deps --force-reinstall
& $venvPip install "nemo-toolkit[asr]>=1.20.0,<2.0.0"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Warning: NeMo installation may have failed. Trying alternative approach..." -ForegroundColor Yellow
    # Try without version constraints as fallback
    & $venvPip install nemo-toolkit[asr] --no-deps
    & $venvPip install nemo-toolkit[asr]
}

# End of installation block
}

# Start the service
Write-Host "" 
Write-Host "Starting ASR service..." -ForegroundColor Cyan
Write-Host "Service will be available at: http://localhost:$Port" -ForegroundColor Green
Write-Host "Health check: http://localhost:$Port/health" -ForegroundColor Green
Write-Host "Service logs: logs/asr_service.log" -ForegroundColor Cyan
Write-Host "NeMo debug logs: logs/nemo-output.log" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop the service" -ForegroundColor Yellow
Write-Host ""

# Ensure Python streams emit UTF-8 so logging can handle non-ASCII text on Windows consoles
Set-Item -Path Env:PYTHONIOENCODING -Value "utf-8"
Set-Item -Path Env:PYTHONUTF8 -Value "1"

try {
    & $venvPython -X utf8 app.py
} catch {
    Write-Host "Error starting service: $_" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# If we get here, the service stopped normally
Write-Host "Service stopped." -ForegroundColor Yellow
