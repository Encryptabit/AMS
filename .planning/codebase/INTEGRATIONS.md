# External Integrations

**Analysis Date:** 2026-02-06

## APIs & External Services

**Automatic Speech Recognition (ASR):**
- Whisper.NET (Primary/Local) - In-process transcription using OpenAI Whisper models
  - SDK/Client: Whisper.NET 1.8.1 (`host/Ams.Core/Ams.Core.csproj`)
  - Auth: None (local execution)
  - Configuration: `AMS_ASR_ENGINE=whisper`, `AMS_WHISPER_MODEL_PATH` env vars
  - GPU: CUDA support via Whisper.NET.Runtime.Cuda

- Nemo ASR (Alternate/Remote) - Python FastAPI service for speech recognition
  - Client: HTTP via `host/Ams.Core/Asr/AsrClient.cs`
  - Default endpoint: `http://localhost:8000`
  - Health check: `GET /health`
  - Transcription: `POST /asr` with JSON payload
  - HTTP timeout: configurable via `AMS_ASR_HTTP_TIMEOUT_SECONDS` (default 15 min)
  - Auto-start: `host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`
  - Service code: `services/asr-nemo/app.py` (FastAPI + NeMo toolkit)

**Forced Alignment:**
- Montreal Forced Aligner (MFA) - Speech-text alignment via conda package
  - Models: `english_mfa` (dictionary + acoustic), `english_us_mfa` (G2P)
  - Supervisor: `host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`
  - Workflow: `host/Ams.Core/Application/Mfa/MfaWorkflow.cs`
  - Timing merger: `host/Ams.Core/Alignment/Mfa/MfaTimingMerger.cs`
  - Runs in persistent PowerShell session with conda environment

**ML Model Hub:**
- Hugging Face Hub - Model downloads for NeMo ASR service
  - Package: huggingface-hub>=0.19.0 (`services/asr-nemo/requirements.txt`)
  - Used by NeMo toolkit for model access

## Data Storage

**Databases:**
- None (file-based storage only)

**File Storage:**
- Local filesystem - All artifacts stored as files in chapter work directories
  - Audio: `.wav` files (source and treated)
  - ASR output: `{chapter}.asr.json`
  - Alignment artifacts: `{chapter}.align.anchors.json`, `{chapter}.align.tx.json`, `{chapter}.align.hydrate.json`
  - MFA output: `{chapter}.TextGrid`
  - Book index: JSON files
  - Document formats: `.docx`, `.txt`, `.md`, `.rtf`, `.pdf`

**Caching:**
- In-memory chapter context caching via `ChapterManager` (`host/Ams.Core/Runtime/Chapter/ChapterManager.cs`)
- Book index cache on disk via `BookCache` (`host/Ams.Core/Runtime/Book/BookCache.cs`)

## Authentication & Identity

**Auth Provider:**
- Not applicable (single-user desktop workstation)
- No auth providers, OAuth, or identity services configured

## Monitoring & Observability

**Logging:**
- Serilog structured logging - `host/Ams.Core/Common/Log.cs`
  - Console sink with color formatting
  - Rolling file sink: `%LOCALAPPDATA%/AMS/logs/ams-log.txt` (10MB max, 5 retained)
  - Log level: configurable via `AMS_LOG_LEVEL` env var
  - Per-class loggers: `Log.For<ClassName>()`

**Health Checks:**
- ASR service: `/health` endpoint check in `host/Ams.Core/Asr/AsrClient.cs`

**Error Tracking:**
- Not detected (errors logged to Serilog sinks only)

**Analytics:**
- Not detected

## CI/CD & Deployment

**Hosting:**
- Local desktop workstation deployment
- No cloud hosting or containerization detected

**CI Pipeline:**
- Not detected (no GitHub Actions, Azure Pipelines, or similar)

## Environment Configuration

**Development:**
- Required env vars: `AMS_ASR_ENGINE`, `AMS_WHISPER_MODEL_PATH` (if using Whisper)
- Optional env vars: `AMS_ASR_HTTP_TIMEOUT_SECONDS`, `AMS_ASR_DISABLE_AUTOSTART`, `AMS_ASR_START_SCRIPT`, `AMS_ASR_POWERSHELL`, `AMS_ASR_PYTHON`, `AMS_LOG_LEVEL`
- External tools: FFmpeg binaries (bundled), Plugalyzer.exe (bundled), MFA (conda)
- Nemo ASR service: `services/asr-nemo/` (Python FastAPI, separate virtual env)
- Aeneas service: `services/aeneas/` (Python, separate virtual env)

**Production:**
- Same as development (desktop application)

## Webhooks & Callbacks

**Incoming:**
- None

**Outgoing:**
- None

## External JavaScript Dependencies (CDN)

- WaveSurfer.js v7 - Audio waveform visualization (`https://unpkg.com/wavesurfer.js@7/dist/wavesurfer.min.js`)
- WaveSurfer Regions plugin v7 - Region selection (`https://unpkg.com/wavesurfer.js@7/dist/plugins/regions.min.js`)

## Bundled External Tools

- Plugalyzer.exe - Audio analysis tool (`host/Ams.Core/Ams.Core.csproj`)
- FFmpeg executables - ffmpeg.exe, ffprobe.exe, ffplay.exe (`host/Ams.Core/Ams.Core.csproj`)

---

*Integration audit: 2026-02-06*
*Update when adding/removing external services*
