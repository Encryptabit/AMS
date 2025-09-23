
# ASR‑NeMo Service Startup Script (PowerShell)
# - Tunables live at the top
# - Installs only what's missing (idempotent)
# - Uses CUDA 12.8 PyTorch wheels by default
# - Sets env vars for THIS process only (won't touch your global machine state)

param(
  # --- Launch / venv ---
  [string]$SeedPython    = "venv312\Scripts\python.exe", # If venv missing, this interpreter will create it
  [string]$VenvDir       = "venv312",
  [string]$ListenHost    = "0.0.0.0",
  [int]   $Port          = 8000,
  [switch]$ForceReinstall = $false,

  # --- PyTorch wheel channel (change to cu121/cu118/etc if desired) ---
  [string]$TorchIndexUrl = "https://download.pytorch.org/whl/cu128"
)

# =========================
# Tunables (service env vars)
# =========================
# NOTE: these are applied ONLY to this PowerShell process and the spawned uvicorn
#       (they do NOT persist or modify system/user env).
$ENV_VARS = [ordered]@{
  # Runtime niceties & memory behavior
  "PYTHONUTF8"                = "1"                # force UTF-8 mode for stdout/stderr and file writes
  "PYTORCH_CUDA_ALLOC_CONF"   = "expandable_segments:True"  # helps reduce allocator fragmentation on long runs

  # Hugging Face auth (optional): leave blank to inherit from your shell
  # "HUGGINGFACE_TOKEN"       = ""

  # ASR step (chunked Parakeet RNNT, timestamps OFF)
  "ASR_MODEL"                 = "nvidia/parakeet-tdt-0.6b-v3"
  "ASR_BEAM_SIZE"             = "12"
  "ASR_GPU_BATCH_SIZE"        = "1"
  "ASR_FUSED_BATCH_SIZE"      = "8"
  "ASR_GPU_FALLBACK_BATCH_SIZE" = "1"
  "ASR_PRECHUNK"              = "1"      # keep chunking for throughput
  "ASR_MIN_CHUNK_SEC"         = "160"
  "ASR_MAX_CHUNK_SEC"         = "3000"
  "ASR_SILENCE_DB"            = "35"     # higher => fewer segments
  

  # NFA step (single pass forced alignment over FULL audio + concatenated transcript)
  "NFA_MODEL"                 = "stt_en_fastconformer_hybrid_large_pc"
  "NFA_BATCH"                 = "1"
  "NFA_USE_STREAMING"         = "1"
  "NFA_CHUNK_LEN"             = "1.6"
  "NFA_TOTAL_BUFFER"          = "4.0"
  "NFA_CHUNK_BATCH"           = "32"

  # If you keep a vendor copy of align.py (fastest/most stable path)
  # Point NEMO_DIR at folder containing tools/nemo_forced_aligner/align.py
  "NEMO_DIR"                  = "C:\Projects\AMS\services\asr-nemo\vendor_nfa"
}

function PyExec {
  param([Parameter(Mandatory=$true, ValueFromPipeline=$true)][string]$Code)
  $Code | & $VenvPython -   # send here-string to python stdin
}


# =========================
# Script starts
# =========================
$ErrorActionPreference = "Stop"

Write-Host "ASR-NeMo Service Startup" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan

# Work from repo root (where this script lives)
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptRoot

# venv paths
$VenvPython = Join-Path $VenvDir "Scripts\python.exe"
$VenvPip    = Join-Path $VenvDir "Scripts\pip.exe"

# Create venv if missing
if (-not (Test-Path $VenvPython)) {
  if (-not (Test-Path $SeedPython)) {
    Write-Host "Seed Python not found: $SeedPython" -ForegroundColor Red
    Write-Host "Adjust -SeedPython or create the venv manually." -ForegroundColor Red
    exit 1
  }
  Write-Host "Creating virtual environment: $VenvDir" -ForegroundColor Yellow
  & $SeedPython -m venv $VenvDir
} else {
  Write-Host "Using existing virtual environment: $VenvDir" -ForegroundColor Green
}

# Show interpreter version
$venvPyVer = & $VenvPython --version
Write-Host "Venv Python: $venvPyVer ($VenvPython)" -ForegroundColor Green

# Helper: install only when missing
function Ensure-Pkg {
  param(
    [Parameter(Mandatory=$true)][string]$Name,
    [string[]]$InstallArgs = @()
  )
  & $VenvPip show $Name *> $null
  if ($LASTEXITCODE -ne 0 -or $ForceReinstall) {
    Write-Host "Installing $Name $($InstallArgs -join ' ')" -ForegroundColor Cyan
    & $VenvPip install $Name @InstallArgs
  } else {
    Write-Host "$Name already present" -ForegroundColor DarkGray
  }
}

Write-Host "Upgrading pip/setuptools/wheel..." -ForegroundColor Magenta
& $VenvPip install --upgrade pip setuptools wheel *> $null

# Core deps (idempotent)
Ensure-Pkg -Name "fastapi>=0.104.0"
Ensure-Pkg -Name "uvicorn[standard]>=0.24.0"
Ensure-Pkg -Name "omegaconf"
Ensure-Pkg -Name "hydra-core>=1.3.0"
Ensure-Pkg -Name "soundfile>=0.12.0"
Ensure-Pkg -Name "librosa>=0.10.0"
Ensure-Pkg -Name "huggingface-hub>=0.19.0"
Ensure-Pkg -Name "transformers>=4.35.0"
Ensure-Pkg -Name "pytorch-lightning>=2.0.0,<3.0.0"

# CUDA-enabled PyTorch (uses selected index)
# If you already installed torch with CU12.8 in this venv, this is a no-op.
& $VenvPip show torch *> $null
if ($LASTEXITCODE -ne 0 -or $ForceReinstall) {
  Write-Host "Installing CUDA PyTorch wheels from $TorchIndexUrl" -ForegroundColor Cyan
  & $VenvPip install --index-url $TorchIndexUrl torch torchvision torchaudio
} else {
  Write-Host "torch already present" -ForegroundColor DarkGray
}

# CUDA Python bindings (enables NeMo's CUDA-graphs decoder paths)
Ensure-Pkg -Name "cuda-python>=12.3"

# NeMo (prefer using your cloned repo for NFA, but ensure wheel exists for ASR models)
& $VenvPip show nemo-toolkit *> $null
if ($LASTEXITCODE -ne 0 -or $ForceReinstall) {
  Write-Host "Installing NeMo toolkit (binary wheel)..." -ForegroundColor Cyan
  & $VenvPip install "nemo-toolkit[asr]>=2.6.0rc0"
} else {
  Write-Host "nemo-toolkit already present" -ForegroundColor DarkGray
}

# Quick, non-invasive preflight (does not change your machine)
Write-Host "Preflight (non-invasive): checking torch & CUDA..." -ForegroundColor Yellow
PyExec @'
import torch, sys
print("torch:", torch.__version__)
print("CUDA available:", torch.cuda.is_available())
if torch.cuda.is_available():
    print("GPU:", torch.cuda.get_device_name(0))
'@

# Apply per-process env vars (safe for dynamic names)
foreach ($kv in $ENV_VARS.GetEnumerator()) {
  $name = $kv.Key
  $val  = $kv.Value

  if ($null -ne $val -and $val -ne "") {
    # Use Env: provider for process-scoped variables
    Set-Item -Path ("Env:{0}" -f $name) -Value $val    # e.g. Env:PYTHONUTF8
  }
  # Pretty print (show <inherit> when we didn't set a value here)
  $display = if ([string]::IsNullOrEmpty($val)) { '<inherit>' } else { $val }
  Write-Host ("  {0}={1}" -f $name, $display) -ForegroundColor DarkGray
}

# Launch service
Write-Host ""
Write-Host "Starting ASR service..." -ForegroundColor Cyan
Write-Host "URL:        http://$($ListenHost):$Port" -ForegroundColor Green
Write-Host "Health:     http://$($ListenHost):$Port/health" -ForegroundColor Green
Write-Host "Press Ctrl+C to stop." -ForegroundColor Yellow
Write-Host ""

# Run uvicorn via the venv interpreter
& $VenvPython -m uvicorn app:app --host $ListenHost --port $Port --log-level info

Write-Host "Service stopped." -ForegroundColor Yellow
