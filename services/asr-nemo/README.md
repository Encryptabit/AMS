# ASR-NeMo Service

Automatic Speech Recognition service using NVIDIA NeMo with integrated Python 3.12 virtual environment management.

## Quick Start

### Option 1: Batch Script (Recommended for Windows)
```bash
# Normal startup (skips installation if packages exist)
start_service.bat

# Force package reinstallation
start_service.bat --force-reinstall
start_service.bat -f
```

### Option 2: PowerShell Script
```powershell
# Normal startup (skips installation if packages exist)
.\start_service.ps1

# Force package reinstallation
.\start_service.ps1 -ForceReinstall

# Custom Python path and port
.\start_service.ps1 -PythonPath "C:\Python312\python.exe" -Port 8080 -ForceReinstall
```

### Option 3: Manual Startup
```bash
# If you want to run manually (will show Python version warning if not 3.10+)
python app.py
```

## Features

- **Automatic Virtual Environment**: Creates and manages a Python 3.12 virtual environment
- **Model Fallbacks**: Tries nvidia/parakeet-tdt-0.6b-v3 first, falls back to compatible models
- **Python Version Detection**: Warns if running on older Python versions
- **Dependency Management**: Automatically installs required packages
- **Health Monitoring**: Built-in health and memory status endpoints
- **Comprehensive Logging**: Logs to both console and rotating files (`logs/asr_service.log`)
- **Smart Package Installation**: Skips installation if packages exist, unless forced

## Supported Models (in priority order)

1. `nvidia/parakeet-tdt-0.6b-v3` - Modern multilingual model with timestamps (requires Python 3.10+)
2. `nvidia/stt_en_quartznet15x5` - Stable, widely compatible
3. `nvidia/stt_en_citrinet_256` - Good performance fallback

## API Endpoints

- `POST /asr` - Transcribe audio with timestamps
- `GET /health` - Service health and model status
- `GET /memory` - Memory usage information
- `POST /auth/hf` - Test Hugging Face authentication
- `POST /cleanup` - Free model memory

## Environment Variables

- `HUGGINGFACE_TOKEN` or `HF_TOKEN` - Required for model access
- `CUDA_VISIBLE_DEVICES` - Control GPU usage (optional)

## Requirements

- Python 3.12 (automatically managed by startup scripts)
- NVIDIA GPU (recommended) or CPU fallback
- Hugging Face account with model access

## Troubleshooting

### "use_bias" Error
This indicates you're running with an older Python/NeMo version. Use the startup scripts to ensure Python 3.12 is used.

### Model Loading Issues
Check your Hugging Face authentication with:
```bash
curl -X POST http://localhost:8000/auth/hf
```

### Memory Issues
Free up model memory:
```bash
curl -X POST http://localhost:8000/cleanup
```

## Logging

The service writes comprehensive logs with automatic rotation:

### Main Service Logs (`logs/asr_service.log`)
- **Log Rotation**: 10MB max per file, keeps 5 backup files
- **Console Output**: Shows INFO level and above
- **File Logging**: Captures DEBUG level and above
- **Request Tracking**: Logs all ASR requests with timing and results
- **Error Logging**: Full stack traces for debugging
- **Startup Information**: Python version, PyTorch, CUDA details

### NeMo Debug Logs (`logs/nemo-output.log`)
- **Raw NeMo Output**: Complete hypothesis data from transcription
- **Timestamp Analysis**: Detailed word/segment/char timestamp data
- **Confidence Debugging**: Shows actual confidence fields from NeMo
- **Log Rotation**: 50MB max per file, keeps 3 backup files
- **Perfect for debugging**: Timestamp logic and confidence issues

## File Structure

```
asr-nemo/
├── app.py                 # Main FastAPI service
├── requirements.txt       # Python dependencies
├── start_service.bat      # Windows batch startup script
├── start_service.ps1      # PowerShell startup script
├── clean_install.ps1      # Clean virtual environment script
├── venv312/              # Auto-created Python 3.12 virtual environment
├── logs/                 # Service logs directory
│   ├── asr_service.log   # Main service log file
│   ├── asr_service.log.* # Rotated service log files
│   ├── nemo-output.log   # Raw NeMo output debug log
│   └── nemo-output.log.* # Rotated NeMo debug log files
└── README.md             # This file
```