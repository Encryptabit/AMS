# Codebase Structure

**Analysis Date:** 2026-02-06

## Directory Layout

```
AMS/
├── host/                              # Main host projects (solution root)
│   ├── Ams.sln                        # Solution file (4 projects)
│   ├── Directory.Build.props          # Shared build properties
│   ├── Ams.Cli/                       # CLI Host (REPL)
│   │   ├── Commands/                  # System.CommandLine verb handlers
│   │   ├── Repl/                      # REPL session state
│   │   ├── Workspace/                 # CliWorkspace (IWorkspace impl)
│   │   ├── Services/                  # CLI-specific services (DSP config)
│   │   ├── Utilities/                 # Input resolvers, policy helpers
│   │   └── Models/                    # CLI-specific models
│   ├── Ams.Core/                      # Core Library (shared business logic)
│   │   ├── Application/               # Orchestration layer
│   │   │   ├── Commands/              # Use-case commands
│   │   │   ├── Pipeline/              # Pipeline service & stages
│   │   │   ├── Mfa/                   # MFA workflow & pronunciation
│   │   │   ├── Processes/             # Process supervisors
│   │   │   └── Validation/            # Validation services
│   │   ├── Services/                  # Domain services
│   │   │   ├── Alignment/             # Anchor, transcript, hydration
│   │   │   ├── Interfaces/            # Service contracts
│   │   │   ├── Integrations/FFmpeg/   # FFmpeg wrapper
│   │   │   └── Documents/             # Document service
│   │   ├── Runtime/                   # Resource context management
│   │   │   ├── Workspace/             # IWorkspace interface
│   │   │   ├── Book/                  # BookContext, BookManager
│   │   │   ├── Chapter/               # ChapterContext, ChapterManager
│   │   │   ├── Audio/                 # AudioBufferManager
│   │   │   ├── Artifacts/             # IArtifactResolver
│   │   │   └── Common/                # DocumentSlot<T>
│   │   ├── Processors/                # Domain algorithms
│   │   │   ├── Alignment/Anchors/     # Anchor discovery pipeline
│   │   │   ├── Alignment/Mfa/         # TextGrid parser, timing merger
│   │   │   ├── Alignment/Tx/          # Transcript aligner, window builder
│   │   │   └── Diffing/               # Text diff analysis
│   │   ├── Artifacts/                 # DTO models (ASR, alignment, audio)
│   │   ├── Asr/                       # ASR client, engine abstraction
│   │   ├── Audio/                     # Audio analysis, feature extraction
│   │   ├── Prosody/                   # Pause dynamics, timeline
│   │   ├── Common/                    # Logging, text normalization, metrics
│   │   └── Pipeline/                  # Sentence refinement
│   ├── Ams.Workstation.Server/        # Blazor Web UI Host
│   │   ├── Components/                # Razor components
│   │   │   ├── Pages/                 # Page components (Home, BookOverview, Proof)
│   │   │   ├── Shared/                # Shared components (PatternCard)
│   │   │   └── Layout/                # MainLayout, NavMenu
│   │   ├── Controllers/               # REST APIs (Audio, Proof)
│   │   ├── Services/                  # Web-specific services
│   │   ├── Models/                    # View models
│   │   └── wwwroot/                   # Static assets
│   ├── Ams.Tests/                     # xUnit Test Project
│   │   ├── Services/                  # Service-layer tests
│   │   ├── Audio/                     # Audio processing tests
│   │   ├── Common/                    # Utility tests
│   │   └── Prosody/                   # Prosody tests
│   └── Ams.Dsp.Native/               # Native DSP bindings
├── services/                          # External Python services
│   ├── asr-nemo/                      # NeMo ASR FastAPI service
│   └── aeneas/                        # Aeneas alignment service
├── poc/                               # Proof-of-concept experiments
├── analysis/                          # Analysis outputs/reports
├── dsp/                               # DSP config files
├── docs/                              # Documentation
├── tools/                             # Build/utility scripts
├── CLAUDE.md                          # Project instructions
└── README.md                          # Project readme
```

## Directory Purposes

**`host/Ams.Cli/`:**
- Purpose: CLI REPL host; thin command wrappers delegating to Core
- Contains: System.CommandLine verb handlers, REPL state management, workspace implementation
- Key files: `Program.cs` (entry point), `Commands/PipelineCommand.cs`, `Workspace/CliWorkspace.cs`

**`host/Ams.Core/Application/`:**
- Purpose: Use-case orchestration; bridges CLI/Web with domain services
- Contains: Commands (6 use-case classes), pipeline coordination, process supervisors, MFA workflow
- Key files: `Commands/GenerateTranscriptCommand.cs`, `Pipeline/PipelineService.cs`, `Processes/AsrProcessSupervisor.cs`

**`host/Ams.Core/Services/`:**
- Purpose: Reusable, stateless business logic
- Contains: ASR service, alignment services (anchor compute, transcript index, hydration), FFmpeg integration
- Key files: `AsrService.cs`, `Alignment/AnchorComputeService.cs`, `Alignment/TranscriptHydrationService.cs`

**`host/Ams.Core/Runtime/`:**
- Purpose: Long-lived resource contexts and lifecycle management
- Contains: Workspace interface, Book/Chapter contexts, artifact resolvers, DocumentSlot lazy-loader
- Key files: `Workspace/IWorkspace.cs`, `Chapter/ChapterContext.cs`, `Book/BookContext.cs`

**`host/Ams.Core/Processors/`:**
- Purpose: Core domain algorithms (pure computation)
- Contains: Anchor discovery, transcript alignment, TextGrid parsing, text diffing
- Key files: `Alignment/Anchors/AnchorPipeline.cs`, `Alignment/Mfa/TextGridParser.cs`, `Alignment/Tx/TranscriptAligner.cs`

**`host/Ams.Core/Artifacts/`:**
- Purpose: Data transfer objects and document models
- Contains: ASR response models, alignment models, audio buffer, timing models
- Key files: `AudioBuffer.cs`, `TranscriptModels.cs`, `Alignment/AnchorDocument.cs`, `Hydrate/HydratedTranscript.cs`

**`host/Ams.Workstation.Server/`:**
- Purpose: Blazor 8 Server web UI with REST APIs
- Contains: Razor pages, REST controllers, web-specific services, view models
- Key files: `Program.cs`, `Services/BlazorWorkspace.cs`, `Controllers/AudioController.cs`

**`host/Ams.Tests/`:**
- Purpose: xUnit test suite
- Contains: Unit tests for algorithms, service tests, integration tests
- Key files: `AnchorDiscoveryTests.cs`, `BookParsingTests.cs`, `TxAlignTests.cs`

**`services/`:**
- Purpose: External Python services for ML/ASR
- Contains: NeMo ASR FastAPI service, Aeneas alignment service
- Key files: `asr-nemo/app.py`, `asr-nemo/requirements.txt`

## Key File Locations

**Entry Points:**
- `host/Ams.Cli/Program.cs` - CLI entry point, DI bootstrap, REPL orchestrator
- `host/Ams.Workstation.Server/Program.cs` - Blazor Server startup, DI config

**Configuration:**
- `host/Directory.Build.props` - Shared build properties
- `host/Ams.Workstation.Server/appsettings.json` - Blazor logging config
- `host/Ams.Core/Ams.Core.csproj` - Core dependency declarations
- Environment variables (AMS_* prefix) - Runtime configuration

**Core Logic:**
- `host/Ams.Core/Services/PipelineService.cs` - Pipeline orchestration
- `host/Ams.Core/Application/Commands/*.cs` - Use-case commands
- `host/Ams.Core/Processors/Alignment/Anchors/AnchorPipeline.cs` - Anchor algorithm
- `host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs` - MFA merge

**Testing:**
- `host/Ams.Tests/*.cs` - All test files
- `host/Ams.Tests/Services/` - Service-layer tests

**Documentation:**
- `CLAUDE.md` - Project instructions and pipeline documentation
- `README.md` - Project readme

## Naming Conventions

**Files:**
- PascalCase.cs for all C# files (e.g., `AnchorDiscovery.cs`, `PipelineService.cs`)
- ClassName.Feature.cs for partial classes (e.g., `DocumentProcessor.Indexing.cs`, `AudioProcessor.Analysis.cs`)
- IInterfaceName.cs for interfaces (e.g., `IWorkspace.cs`, `IAsrService.cs`)
- *Tests.cs for test files (e.g., `AnchorDiscoveryTests.cs`)
- GlobalUsings.cs for project-wide using declarations

**Directories:**
- PascalCase for project names: `Ams.Cli`, `Ams.Core`, `Ams.Workstation.Server`
- PascalCase for feature domains: `Application`, `Services`, `Runtime`, `Processors`
- Plural for collections: `Commands`, `Services`, `Artifacts`, `Processes`

**Special Patterns:**
- `*Command.cs` for use-case commands
- `*Service.cs` for stateless services
- `*Context.cs` for resource holders
- `*Manager.cs` for lifecycle/collection managers
- `*Options.cs` for configuration DTOs
- `*Supervisor.cs` for external process managers

## Where to Add New Code

**New Pipeline Stage:**
1. Add enum value: `host/Ams.Core/Application/Pipeline/PipelineStage.cs`
2. Create command: `host/Ams.Core/Application/Commands/YourCommand.cs`
3. Register in DI: `host/Ams.Cli/Program.cs` and `host/Ams.Workstation.Server/Program.cs`
4. Add orchestration: `host/Ams.Core/Services/PipelineService.cs`
5. Add CLI verb: `host/Ams.Cli/Commands/YourCommand.cs`

**New Alignment Algorithm:**
1. Add processor: `host/Ams.Core/Processors/Alignment/{domain}/YourProcessor.cs`
2. Add service interface: `host/Ams.Core/Services/Alignment/IYourService.cs`
3. Add implementation: `host/Ams.Core/Services/Alignment/YourService.cs`
4. Register in DI: `Program.cs`

**New Blazor Page:**
1. Create page: `host/Ams.Workstation.Server/Components/Pages/YourPage.razor`
2. Create controller (if API needed): `host/Ams.Workstation.Server/Controllers/YourController.cs`
3. Create service (if logic needed): `host/Ams.Workstation.Server/Services/YourService.cs`

**New Test:**
1. Create test class: `host/Ams.Tests/YourTests.cs` (or in subdirectory mirroring source)
2. Use `[Fact]` or `[Theory]` attributes

**Utilities:**
- Shared: `host/Ams.Core/Common/`
- CLI-specific: `host/Ams.Cli/Utilities/`

## Special Directories

**`bin/`, `obj/`:**
- Purpose: Build artifacts and intermediate files
- Source: .NET SDK build output
- Committed: No (gitignored)

**`poc/`:**
- Purpose: Proof-of-concept experiments (VelloSharpPoc, SkiaSharpPoc, HybridVelloPoc)
- Source: Developer experiments, not production code
- Committed: Yes

**`wwwroot/`:**
- Purpose: Blazor static assets (HTML, CSS, JS)
- Location: `host/Ams.Workstation.Server/wwwroot/`
- Committed: Yes

**`ExtTools/`:**
- Purpose: Bundled external binaries (FFmpeg, Plugalyzer, Whisper models, Silero VAD, Tesseract)
- Location: `host/Ams.Core/ExtTools/`
- Committed: Varies (binaries may be gitignored)

---

*Structure analysis: 2026-02-06*
*Update when directory structure changes*
