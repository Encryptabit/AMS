# Aeneas Service Startup Script (PowerShell)
param(
    [string]$AeneasPython = $null,  # Path to Python with Aeneas installed
    [int]$Port = 8082,
    [switch]$UseVenv = $false,      # Legacy option to use virtual environment
    [string]$VenvDir = "venv39",    # Only used if -UseVenv is specified
    [switch]$ForceReinstall = $false # Only used if -UseVenv is specified
)

Write-Host "Aeneas Service Startup Script (Subprocess Mode)" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

if ($UseVenv) {
    Write-Host "Using legacy virtual environment mode" -ForegroundColor Yellow
    # Legacy virtual environment logic
    $PythonPath = "C:\Users\Jacar\AppData\Local\Programs\Python\Python39\python.exe"
    
    # Check if Python 3.9 exists
    if (-not (Test-Path $PythonPath)) {
        Write-Host "Error: Python 3.9 not found at $PythonPath" -ForegroundColor Red
        Write-Host "Please install Python 3.9 or update the path." -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    # Get Python version
    $pythonVersion = & $PythonPath --version
    Write-Host "Found Python: $pythonVersion" -ForegroundColor Green
} else {
    Write-Host "Using subprocess mode (no virtual environment)" -ForegroundColor Green
    
    if ($AeneasPython) {
        Write-Host "Using specified Python: $AeneasPython" -ForegroundColor Cyan
        if (-not (Test-Path $AeneasPython)) {
            Write-Host "Error: Specified Python not found at $AeneasPython" -ForegroundColor Red
            Read-Host "Press Enter to exit"
            exit 1
        }
        $env:AENEAS_PYTHON = $AeneasPython
    } else {
        Write-Host "Auto-detecting Python with Aeneas installed..." -ForegroundColor Cyan
        $candidates = @("python", "python3", "py")
        $foundPython = $null
        
        foreach ($candidate in $candidates) {
            try {
                $result = & $candidate -c "import aeneas; print('OK')" 2>$null
                if ($LASTEXITCODE -eq 0 -and $result -eq "OK") {
                    $foundPython = $candidate
                    $version = & $candidate --version
                    Write-Host "Found Python with Aeneas: $candidate ($version)" -ForegroundColor Green
                    break
                }
            } catch {
                # Continue to next candidate
            }
        }
        
        if (-not $foundPython) {
            Write-Host "Error: No Python installation with Aeneas found" -ForegroundColor Red
            Write-Host "Please:" -ForegroundColor Yellow
            Write-Host "  1. Install Aeneas: pip install aeneas" -ForegroundColor Yellow
            Write-Host "  2. Or specify Python path: -AeneasPython 'C:\path\to\python.exe'" -ForegroundColor Yellow
            Write-Host "  3. Or use legacy mode: -UseVenv" -ForegroundColor Yellow
            Read-Host "Press Enter to exit"
            exit 1
        }
    }
}

if ($UseVenv) {
    # Create virtual environment if it doesn't exist
    if (-not (Test-Path $VenvDir)) {
        Write-Host "Creating Python 3.9 virtual environment..." -ForegroundColor Yellow
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
    if ($venvPythonVersion -match "3\.9") {
        Write-Host "Successfully using virtual environment: $venvPythonVersion" -ForegroundColor Green
    } else {
        Write-Host "Warning: Virtual environment may not be using Python 3.9" -ForegroundColor Yellow
        Write-Host "Virtual environment Python: $venvPythonVersion" -ForegroundColor Yellow
    }

    # Package installation logic
    if ($ForceReinstall) {
        Write-Host "Force reinstall requested - installing/updating packages..." -ForegroundColor Yellow
        $ShouldInstall = $true
    } else {
        # Check if essential packages are already installed
        Write-Host "Checking if packages are already installed..." -ForegroundColor Cyan
        
        $packagesToCheck = @("fastapi", "uvicorn", "pydantic", "aeneas")
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

    # Second: Install system dependencies for Aeneas
    Write-Host "Stage 2: Installing system dependencies..." -ForegroundColor Cyan
    & $venvPip install numpy scipy

    # Third: Install web framework dependencies
    Write-Host "Stage 3: Installing web framework..." -ForegroundColor Cyan
    & $venvPip install -r requirements.txt

    # Fourth: Install Aeneas
    Write-Host "Stage 4: Installing Aeneas..." -ForegroundColor Cyan
    & $venvPip install aeneas

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Warning: Aeneas installation may have failed. Trying alternative approach..." -ForegroundColor Yellow
        # Try installing dependencies manually
        & $venvPip install espeak espeak-data
        & $venvPip install aeneas --no-deps
    }

    # End of installation block
    }
    
    $servicePython = $venvPython
} else {
    Write-Host "Subprocess mode: Using system/specified Python with Aeneas" -ForegroundColor Green
    $servicePython = "python"  # Will use auto-detected or env var
}

# Start the service
Write-Host "" 
Write-Host "Starting Aeneas service..." -ForegroundColor Cyan
Write-Host "Service will be available at: http://localhost:$Port" -ForegroundColor Green
Write-Host "Health check: http://localhost:$Port/v1/health" -ForegroundColor Green
Write-Host "Service logs: logs/aeneas_service.log" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop the service" -ForegroundColor Yellow
Write-Host ""

try {
    & $servicePython -m app.main
} catch {
    Write-Host "Error starting service: $_" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# If we get here, the service stopped normally
Write-Host "Service stopped." -ForegroundColor Yellow