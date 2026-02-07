# Technology Stack

**Analysis Date:** 2026-02-06

## Languages

**Primary:**
- C# (.NET 9.0 / .NET 10.0) - All application code (`host/Ams.Core/Ams.Core.csproj`, `host/Ams.Cli/Ams.Cli.csproj`, `host/Ams.Workstation.Server/Ams.Workstation.Server.csproj`)

**Secondary:**
- Python 3.9+ - External services (ASR Nemo, Aeneas) (`services/asr-nemo/`, `services/aeneas/`)
- JavaScript - Frontend audio visualization via WaveSurfer.js CDN (`host/Ams.Workstation.Server/Components/App.razor`)

## Runtime

**Environment:**
- .NET 9.0 - Core library, CLI, Tests, DSP Native (`host/Ams.Core/global.json`)
- .NET 10.0 - Blazor Workstation Server (`host/Ams.Workstation.Server/Ams.Workstation.Server.csproj`)
- CUDA 12.1 (optional) - GPU-accelerated ASR via Whisper.NET

**Package Manager:**
- NuGet (implicit via SDK-style .csproj)
- Shared build props: `host/Directory.Build.props`
- No explicit lock files detected

## Frameworks

**Core:**
- ASP.NET Core / Blazor Server - Web UI host (`host/Ams.Workstation.Server/`)
- Microsoft.Extensions.Hosting 10.0.0 - CLI host (`host/Ams.Cli/Ams.Cli.csproj`)
- System.CommandLine 2.0.0-beta4 - CLI argument parsing (`host/Ams.Cli/Ams.Cli.csproj`)

**Testing:**
- xUnit 2.9.3 - Unit and integration tests (`host/Ams.Tests/Ams.Tests.csproj`)
- Coverlet 6.0.4 - Code coverage (`host/Ams.Tests/Ams.Tests.csproj`)

**Build/Dev:**
- .NET SDK 9.0 - Build toolchain
- Directory.Build.props - Shared project properties (`host/Directory.Build.props`)

## Key Dependencies

**Critical (Audio Processing):**
- Whisper.NET 1.8.1 - In-process ASR transcription (`host/Ams.Core/Ams.Core.csproj`)
- Whisper.NET.Runtime.Cuda 1.8.1 - GPU acceleration (`host/Ams.Core/Ams.Core.csproj`)
- FFmpeg.AutoGen 7.1.1 - Audio encoding/decoding bindings (`host/Ams.Core/Ams.Core.csproj`)
- Bundled FFmpeg binaries (avcodec-61.dll, avformat-61.dll, ffmpeg.exe, ffprobe.exe)

**Critical (Document Processing):**
- DocX 5.0.0 - Word document parsing (`host/Ams.Core/Ams.Core.csproj`)
- PDFiumCore 134.0.6982 - PDF text extraction (`host/Ams.Core/Ams.Core.csproj`)
- Tesseract 5.2.0 - OCR (`host/Ams.Core/Ams.Core.csproj`)

**Critical (Alignment):**
- DiffMatchPatch 4.0.0 - Text diffing for alignment (`host/Ams.Core/Ams.Core.csproj`)

**Infrastructure:**
- Serilog 4.3.0 + Console/File sinks - Structured logging (`host/Ams.Core/Ams.Core.csproj`)
- Spectre.Console 0.53.0 - Rich CLI output (`host/Ams.Cli/Ams.Cli.csproj`)
- Bit.BlazorUI 10.3.0 - Blazor UI components (`host/Ams.Workstation.Server/Ams.Workstation.Server.csproj`)
- Toolbelt.Blazor.HotKeys2 6.1.0 - Keyboard shortcuts (`host/Ams.Workstation.Server/Ams.Workstation.Server.csproj`)

## Configuration

**Environment:**
- Environment variables (AMS_* prefix) for runtime configuration
- `AMS_ASR_ENGINE` - ASR engine selection (whisper/nemo)
- `AMS_WHISPER_MODEL_PATH` - Whisper model file path
- `AMS_ASR_HTTP_TIMEOUT_SECONDS` - Nemo HTTP timeout
- `AMS_LOG_LEVEL` - Minimum log level (Debug/Information/Warning/Error)
- No .env files; all config via environment variables

**Build:**
- `host/Directory.Build.props` - Shared build properties
- Individual `.csproj` files per project
- `host/Ams.Workstation.Server/appsettings.json` - Blazor logging config

## Platform Requirements

**Development:**
- Windows (PowerShell integration for MFA process management)
- NVIDIA CUDA 12.1 (optional, for GPU-accelerated ASR)
- FFmpeg binaries bundled with project
- External tools: Plugalyzer.exe (audio analysis)

**Production:**
- Windows desktop workstation (single-user)
- .NET 9.0 / .NET 10.0 runtime
- Optional: Conda environment for Montreal Forced Aligner

---

*Stack analysis: 2026-02-06*
*Update after major dependency changes*
