# AMS Project Status Report

## Overview

The AMS (Audio Management System) solution contains **11 .csproj projects** across three categories:
- **Host projects** (7): Core application components
- **Analysis tools** (2): Standalone utilities for development/testing
- **Web projects** (2): Nascent Blazor UI components

---

## Project Status Summary

| Project | Framework | Source Files | Entry Point | Status | Notes |
|---------|-----------|--------------|-------------|--------|-------|
| Ams.Cli | net9.0 | 22 | Program.cs | **Active** | Main CLI with REPL, DI-wired commands |
| Ams.Core | net9.0 | 96 | Library | **Active** | Core logic: ASR, alignment, MFA, runtime |
| Ams.Dsp.Native | net9.0 | 2 | Library | **Active** | Native DSP interop layer |
| Ams.Tests | net9.0 | 9 | xUnit | **Stale** | Tests exist but coverage unclear |
| Ams.UI.Avalonia | net9.0 | 3 | Program.cs | **Dormant** | Avalonia desktop app skeleton |
| Ams.Web.Api | net9.0 | 6 | Program.cs | **Nascent** | Minimal API for validation UI |
| Ams.Web.Shared | net9.0 | 3 | Library | **Nascent** | Shared DTOs for web projects |
| Ams.Web.Client | net9.0 | 2 | Program.cs | **Nascent** | Blazor WASM client |
| Ams.Web | net9.0 | 1 | Program.cs | **Nascent** | Blazor server host |
| OverlayTest | net9.0 | 1 | Program.cs | **Analysis** | Tests AudioProcessor.OverlayRoomtone |
| InspectDocX | net9.0 | 1 | Program.cs | **Analysis** | Inspects DocX library capabilities |

**Total Source Files (excluding obj/bin): ~146 C# files**

---

## Detailed Project Descriptions

### Active Projects

#### Ams.Cli (`host/Ams.Cli/`)
- **Framework**: .NET 9.0, Console application
- **Dependencies**: Ams.Core, System.CommandLine, Spectre.Console, Serilog, Whisper.net
- **Role**: Main CLI entry point with REPL mode
- **Entry Point**: `Program.cs` - bootstraps DI, registers commands, starts REPL if no args
- **Key Commands**: asr, align, pipeline, validate, book, text, dsp, refine-sentences
- **Status**: Actively developed, full pipeline coordination

#### Ams.Core (`host/Ams.Core/`)
- **Framework**: .NET 9.0, Class library
- **Dependencies**: Ams.Dsp.Native, FFmpeg.AutoGen, Whisper.net, DocX, Tesseract, PDFiumCore
- **Role**: Core business logic and services
- **Key Modules**:
  - `Application/Commands/` - Use-case command implementations
  - `Application/Mfa/` - Montreal Forced Aligner integration
  - `Application/Pipeline/` - Pipeline execution control
  - `Asr/` - ASR client and models
  - `Processors/` - Audio, alignment, document processors
  - `Runtime/` - Book, chapter, workspace management
  - `Services/` - High-level service orchestration
  - `Services/Integrations/FFmpeg/` - Native FFmpeg bindings
- **Status**: Core of the system, actively developed

#### Ams.Dsp.Native (`host/Ams.Dsp.Native/`)
- **Framework**: .NET 9.0, Class library
- **Dependencies**: None (pure native interop)
- **Role**: Native DSP function declarations
- **Files**: `AmsDsp.cs`, `Native.cs`
- **Status**: Active, minimal surface area

### Stale/Dormant Projects

#### Ams.Tests (`host/Ams.Tests/`)
- **Framework**: .NET 9.0, xUnit test project
- **Dependencies**: Ams.Core, xunit, coverlet
- **Role**: Unit tests
- **Test Files**:
  - `AnchorDiscoveryTests.cs`
  - `AudioProcessorFilterTests.cs`
  - `BookParsingTests.cs`
  - `TokenizerTests.cs`
  - `TxAlignTests.cs`
  - `WavIoTests.cs`
  - `Prosody/PauseApplierTests.cs`
  - `Prosody/PauseDynamicsServiceTests.cs`
- **Status**: Tests exist but may not be current with code changes

#### Ams.UI.Avalonia (`host/Ams.UI.Avalonia/`)
- **Framework**: .NET 9.0, Avalonia WinExe
- **Dependencies**: Ams.Core, Avalonia 11.x
- **Role**: Desktop UI (planned)
- **Files**: `App.axaml.cs`, `MainWindow.axaml.cs`, `Program.cs`
- **Status**: Dormant, minimal skeleton only

### Nascent Web Projects

#### Ams.Web.Api (`host/Ams.Web.Api/`)
- **Framework**: .NET 9.0, Minimal API (AOT-ready)
- **Dependencies**: Ams.Core, Ams.Web.Shared, Riok.Mapperly
- **Role**: REST API for validation UI
- **Endpoints**: workspace, validation, audio streaming
- **Files**: `Program.cs`, `WorkspaceState.cs`, mappers, payloads, services
- **Status**: Nascent, basic endpoints working

#### Ams.Web.Shared (`host/Ams.Web.Shared/`)
- **Framework**: .NET 9.0, Class library
- **Role**: Shared DTOs between API and Client
- **Files**: `Class1.cs`, `ValidationDtos.cs`, `WorkspaceDtos.cs`
- **Status**: Nascent, minimal DTOs

#### Ams.Web.Client (`host/Ams.Web/Ams.Web.Client/`)
- **Framework**: .NET 9.0, Blazor WebAssembly
- **Dependencies**: Bit.BlazorUI, Ams.Web.Shared
- **Role**: Web client for validation viewing
- **Files**: `Program.cs`, `ValidationApiClient.cs`
- **Status**: Nascent, structure in place

#### Ams.Web (`host/Ams.Web/Ams.Web/`)
- **Framework**: .NET 9.0, ASP.NET Core Web
- **Dependencies**: Ams.Web.Client
- **Role**: Blazor server host
- **Files**: `Program.cs`
- **Status**: Nascent, minimal host setup

### Analysis Tools

#### OverlayTest (`analysis/OverlayTest/`)
- **Framework**: .NET 9.0, Console
- **Dependencies**: Ams.Core
- **Role**: Tests AudioProcessor.OverlayRoomtone functionality
- **Status**: Development utility, not production

#### InspectDocX (`out/InspectDocX/`)
- **Framework**: .NET 9.0, Console
- **Dependencies**: DocX 4.0.x
- **Role**: Inspects DocX library API
- **Status**: Development utility, not production

---

## File Count by Project

| Project | Source Files | Test Files | Total |
|---------|-------------|------------|-------|
| Ams.Core | 96 | - | 96 |
| Ams.Cli | 22 | - | 22 |
| Ams.Tests | - | 9 | 9 |
| Ams.Web.Api | 6 | - | 6 |
| Ams.Web.Shared | 3 | - | 3 |
| Ams.UI.Avalonia | 3 | - | 3 |
| Ams.Dsp.Native | 2 | - | 2 |
| Ams.Web.Client | 2 | - | 2 |
| Ams.Web | 1 | - | 1 |
| OverlayTest | 1 | - | 1 |
| InspectDocX | 1 | - | 1 |
| **Total** | **137** | **9** | **146** |

---

## Notes

1. **Ams.Core is the heart of the system** with ~96 source files spanning ASR, alignment, MFA, prosody, and runtime management.

2. **Web stack is nascent** - The Ams.Web.* projects have basic structure but minimal implementation. There's also an older `host/Ams.Web/src/` folder with .NET 10 preview builds (not included in main solution).

3. **UI.Avalonia is dormant** - Only a basic Avalonia skeleton exists.

4. **Tests are potentially stale** - The test project references Ams.Core but test coverage may not match current code.

5. **Analysis tools are ad-hoc** - OverlayTest and InspectDocX are standalone utilities for specific development tasks.

6. **Deviation from plan expectation**: The plan expected ~277 files but actual source file count is ~146 (excluding obj/bin generated files). The 277 may have included generated files or counted differently.
