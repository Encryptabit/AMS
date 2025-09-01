@echo off
setlocal EnableDelayedExpansion

:: Parse command line arguments
set "FORCE_REINSTALL=false"
:parse_args
if "%1"=="" goto :done_parsing
if "%1"=="--force-reinstall" set "FORCE_REINSTALL=true"
if "%1"=="-f" set "FORCE_REINSTALL=true"
shift
goto :parse_args
:done_parsing

echo ASR-NeMo Service Startup Script
echo ================================

:: Set Python 3.12 path
set PYTHON312="C:\Users\Jacar\AppData\Local\Programs\Python\Python312\python.exe"
set VENV_DIR=venv312

:: Check if Python 3.12 exists
if not exist %PYTHON312% (
    echo Error: Python 3.12 not found at %PYTHON312%
    echo Please install Python 3.12 or update the path in this script.
    pause
    exit /b 1
)

:: Create virtual environment if it doesn't exist
if not exist "%VENV_DIR%" (
    echo Creating Python 3.12 virtual environment...
    %PYTHON312% -m venv %VENV_DIR%
    if errorlevel 1 (
        echo Error: Failed to create virtual environment
        pause
        exit /b 1
    )
    echo Virtual environment created successfully.
) else (
    echo Virtual environment already exists.
)

:: Activate virtual environment
echo Activating virtual environment...
call "%VENV_DIR%\Scripts\activate.bat"

:: Check if we're in the virtual environment
python --version | findstr "3.12" > nul
if errorlevel 1 (
    echo Warning: Virtual environment activation may have failed
    echo Current Python version:
    python --version
) else (
    echo Successfully activated Python 3.12 virtual environment
)

:: Package installation logic
if "%FORCE_REINSTALL%"=="true" (
    echo Force reinstall requested - installing/updating packages in stages...
    goto :install_packages
)

echo Checking if packages are already installed...
pip show fastapi >nul 2>&1 && pip show uvicorn >nul 2>&1 && pip show torch >nul 2>&1 && pip show nemo-toolkit >nul 2>&1
if errorlevel 1 (
    echo Missing packages detected. Installing...
    goto :install_packages
) else (
    echo All essential packages already installed. Skipping installation.
    echo Use --force-reinstall to force package installation.
    goto :start_service
)

:install_packages
echo Installing/updating packages in stages...

echo Stage 0: Upgrading pip...
pip install --upgrade pip
echo pip upgrade completed.

echo Stage 1: Installing build essentials...
pip install --upgrade setuptools wheel

echo Stage 2: Installing core dependencies...
pip install "numpy>=1.24.0,<2.0.0" omegaconf "hydra-core>=1.3.0"

echo Stage 2b: Installing CUDA-enabled PyTorch...
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121

echo Stage 3: Installing web framework...
pip install "fastapi>=0.104.0" "uvicorn[standard]>=0.24.0"

echo Stage 4: Installing audio processing...
pip install "soundfile>=0.12.0" "librosa>=0.10.0"

echo Stage 5: Installing Hugging Face packages...
pip install "huggingface-hub>=0.19.0" "transformers>=4.35.0"

echo Stage 6: Installing PyTorch Lightning...
pip install "pytorch-lightning>=2.0.0,<3.0.0"

echo Stage 7: Installing NeMo toolkit...
pip install "nemo-toolkit[asr]>=1.20.0,<2.0.0"
if errorlevel 1 (
    echo Warning: NeMo installation failed, trying alternative approach...
    pip install nemo-toolkit[asr]
)

:start_service
:: Start the service
echo Starting ASR service...
echo Service will be available at: http://localhost:8000
echo Health check: http://localhost:8000/health
echo Service logs: logs\asr_service.log
echo NeMo debug logs: logs\nemo-output.log
echo Press Ctrl+C to stop the service
echo.

python app.py

:: Keep window open if there's an error
if errorlevel 1 (
    echo.
    echo Service exited with error. Press any key to close...
    pause > nul
)