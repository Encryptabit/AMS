# Module Dependencies - Raw Reference Data

Generated: 2025-12-28

## Overview

This document extracts all project and package references from each `.csproj` file in the AMS codebase.

---

## Core Projects

### Ams.Core (`host/Ams.Core/Ams.Core.csproj`)

**Type:** Class Library
**Target Framework:** net9.0

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| ProjectReference | Ams.Dsp.Native | - | Native DSP operations |
| PackageReference | DiffMatchPatch | 4.0.0 | Text diffing |
| PackageReference | DocX | 5.0.0 | Word document parsing |
| PackageReference | FFmpeg.AutoGen | 7.1.1 | FFmpeg P/Invoke bindings |
| PackageReference | Microsoft.Extensions.Logging | 10.0.0 | Logging abstractions |
| PackageReference | Microsoft.Extensions.Logging.Console | 10.0.0 | Console logger |
| PackageReference | PDFiumCore | 134.0.6982 | PDF rendering |
| PackageReference | Serilog | 4.3.0 | Structured logging |
| PackageReference | Serilog.Extensions.Logging | 8.0.0 | Serilog/MEL integration |
| PackageReference | Serilog.Sinks.Console | 6.1.1 | Console sink |
| PackageReference | Serilog.Sinks.File | 7.0.0 | File sink |
| PackageReference | System.Text.Json | 10.0.0 | JSON serialization |
| PackageReference | Tesseract | 5.2.0 | OCR |
| PackageReference | Whisper.net | 1.8.1 | Whisper ASR |
| PackageReference | Whisper.net.Runtime.Cuda | 1.8.1 | CUDA acceleration |

**Native Assets:**
- FFmpeg binaries (avcodec-61.dll, avformat-61.dll, etc.)
- Plugalyzer.exe (VST plugin analyzer)
- Whisper CUDA runtime

---

### Ams.Dsp.Native (`host/Ams.Dsp.Native/Ams.Dsp.Native.csproj`)

**Type:** Class Library
**Target Framework:** net9.0
**Special:** AllowUnsafeBlocks=true

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| (none) | - | - | No dependencies - pure native code |

**Notes:** This is a leaf node in the dependency graph with no external dependencies. Contains low-level DSP operations.

---

## CLI & Host Projects

### Ams.Cli (`host/Ams.Cli/Ams.Cli.csproj`)

**Type:** Executable
**Target Framework:** net9.0

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| ProjectReference | Ams.Core | - | Core library |
| PackageReference | Microsoft.Extensions.Hosting | 10.0.0 | Generic host |
| PackageReference | Microsoft.Extensions.Logging | 10.0.0 | Logging abstractions |
| PackageReference | Microsoft.Extensions.Logging.Console | 10.0.0 | Console logger |
| PackageReference | Serilog.Extensions.Logging | 9.0.2 | Serilog/MEL integration |
| PackageReference | Serilog.Sinks.Console | 6.1.1 | Console sink |
| PackageReference | Serilog.Sinks.File | 7.0.0 | File sink |
| PackageReference | System.CommandLine | 2.0.0-beta4.22272.1 | CLI parsing (beta) |
| PackageReference | Spectre.Console | 0.53.0 | Rich console output |
| PackageReference | Whisper.net.Runtime.Cuda | 1.8.1 | CUDA acceleration |

**Notes:** Primary CLI host. Depends only on Ams.Core. Uses System.CommandLine (still in beta).

---

### Ams.Tests (`host/Ams.Tests/Ams.Tests.csproj`)

**Type:** Test Project
**Target Framework:** net9.0

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| ProjectReference | Ams.Core | - | Core library |
| PackageReference | coverlet.collector | 6.0.4 | Code coverage |
| PackageReference | Microsoft.NET.Test.Sdk | 18.0.1 | Test SDK |
| PackageReference | xunit | 2.9.3 | Test framework |
| PackageReference | xunit.runner.visualstudio | 3.1.5 | VS test runner |

**Notes:** Tests only reference Ams.Core, not CLI or other hosts.

---

## UI Projects

### Ams.UI.Avalonia (`host/Ams.UI.Avalonia/Ams.UI.Avalonia.csproj`)

**Type:** Windows Executable
**Target Framework:** net9.0

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| ProjectReference | Ams.Core | - | Core library |
| PackageReference | Avalonia | 11.3.8 | UI framework |
| PackageReference | Avalonia.Desktop | 11.3.8 | Desktop platform |
| PackageReference | Avalonia.Themes.Fluent | 11.3.8 | Fluent theme |
| PackageReference | Avalonia.Fonts.Inter | 11.3.8 | Inter font |
| PackageReference | Avalonia.Diagnostics | 11.3.8 | Debug only |

**Notes:** Desktop UI using Avalonia. Depends only on Ams.Core.

---

## Web Projects

### Ams.Web.Shared (`host/Ams.Web.Shared/Ams.Web.Shared.csproj`)

**Type:** Class Library
**Target Framework:** net9.0

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| (none) | - | - | No dependencies |

**Notes:** Shared contracts/DTOs for web projects. Leaf node.

---

### Ams.Web.Api (`host/Ams.Web.Api/Ams.Web.Api.csproj`)

**Type:** Web API (AOT-enabled)
**Target Framework:** net9.0

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| ProjectReference | Ams.Core | - | Core library |
| ProjectReference | Ams.Web.Shared | - | Shared DTOs |
| PackageReference | Riok.Mapperly | 4.3.0 | Object mapping |

**Notes:** Minimal API with AOT compilation. Maps between Core models and Web DTOs.

---

### Ams.Web.Client (`host/Ams.Web/Ams.Web.Client/Ams.Web.Client.csproj`)

**Type:** Blazor WebAssembly
**Target Framework:** net9.0

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| ProjectReference | Ams.Web.Shared | - | Shared DTOs |
| PackageReference | Microsoft.AspNetCore.Components.WebAssembly | 9.0.11 | WASM host |
| PackageReference | Microsoft.AspNetCore.WebUtilities | 9.0.11 | Query helpers |
| PackageReference | Bit.BlazorUI | 10.1.1 | UI components |
| PackageReference | Bit.BlazorUI.Assets | 10.1.1 | Assets |
| PackageReference | Bit.BlazorUI.Extras | 10.1.1 | Extra components |
| PackageReference | Bit.BlazorUI.Icons | 10.1.1 | Icons |
| PackageReference | Bit.CodeAnalyzers | 10.1.1 | Analyzers |
| PackageReference | Bit.SourceGenerators | 10.1.1 | Source gen |

**Notes:** Client-side Blazor app using Bit component library. Does NOT reference Ams.Core.

---

### Ams.Web (`host/Ams.Web/Ams.Web/Ams.Web.csproj`)

**Type:** Blazor Server Host
**Target Framework:** net9.0

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| ProjectReference | Ams.Web.Client | - | Blazor client |
| PackageReference | Microsoft.AspNetCore.Components.WebAssembly.Server | 9.0.11 | WASM server |
| PackageReference | Bit.BlazorUI | 10.1.1 | UI components |
| PackageReference | Bit.CodeAnalyzers | 10.1.1 | Analyzers |
| PackageReference | Bit.SourceGenerators | 10.1.1 | Source gen |

**Notes:** Blazor server-side host. References only the client project.

---

## Utility/Analysis Projects

### InspectDocX (`out/InspectDocX/InspectDocX.csproj`)

**Type:** Executable
**Target Framework:** net9.0

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| PackageReference | DocX | 4.0.25105.5786 | Word document parsing |

**Notes:** Standalone utility for inspecting Word documents. No AMS dependencies.

---

### OverlayTest (`analysis/OverlayTest/OverlayTest.csproj`)

**Type:** Executable
**Target Framework:** net9.0

| Reference Type | Reference Name | Version | Notes |
|----------------|----------------|---------|-------|
| ProjectReference | Ams.Core | - | Core library |

**Notes:** Analysis/test tool for audio overlay visualization. Copies FFmpeg from Ams.Core.

---

## Summary Statistics

### Project References Summary

| Project | References | Dependents |
|---------|------------|------------|
| Ams.Dsp.Native | 0 | 1 (Ams.Core) |
| Ams.Core | 1 | 6 (Cli, Tests, UI.Avalonia, Web.Api, OverlayTest) |
| Ams.Web.Shared | 0 | 2 (Web.Api, Web.Client) |
| Ams.Web.Client | 1 | 1 (Ams.Web) |
| Ams.Cli | 1 | 0 |
| Ams.Tests | 1 | 0 |
| Ams.UI.Avalonia | 1 | 0 |
| Ams.Web.Api | 2 | 0 |
| Ams.Web | 1 | 0 |
| InspectDocX | 0 | 0 |
| OverlayTest | 1 | 0 |

### Package Categories

| Category | Packages |
|----------|----------|
| Logging | Serilog.*, Microsoft.Extensions.Logging.* |
| Audio/DSP | Whisper.net, FFmpeg.AutoGen |
| Document Processing | DocX, PDFiumCore, Tesseract |
| CLI | System.CommandLine, Spectre.Console |
| Web | ASP.NET Core, Bit.BlazorUI |
| UI | Avalonia |
| Testing | xunit, coverlet |
| Utilities | DiffMatchPatch, Riok.Mapperly |

### Observations

1. **No circular dependencies detected** - The graph is acyclic
2. **Ams.Core is the central hub** - Most projects depend on it
3. **Ams.Dsp.Native is isolated** - Only Ams.Core uses it
4. **Web tier is separated** - Web.Client does not reference Core (good isolation)
5. **Duplicate packages** - Some version mismatches in Serilog packages between Core and CLI
6. **Beta dependency** - System.CommandLine is still in beta (2.0.0-beta4)
7. **Heavy external dependencies in Core** - 14+ packages including audio, PDF, OCR, document processing
