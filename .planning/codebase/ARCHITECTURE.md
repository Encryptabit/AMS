# Architecture

**Analysis Date:** 2026-02-06

## Pattern Overview

**Overall:** Clean Architecture with Command-Based Orchestration, Multi-Host Design

**Key Characteristics:**
- Multi-host: CLI REPL and Blazor Web UI share the same Core library
- Command pattern: use-case commands encapsulate business operations
- Workspace abstraction: `IWorkspace` enables host-agnostic chapter operations
- Process supervision: external tools (Nemo ASR, MFA) managed as supervised processes
- File-based artifact storage with lazy-load `DocumentSlot<T>` pattern

## Layers

**Presentation/Host Layer (CLI & Web):**
- Purpose: Expose commands to users; handle I/O and routing
- Contains: CLI command verbs, REPL state, Blazor pages/controllers, workspace implementations
- CLI: `host/Ams.Cli/Program.cs`, `host/Ams.Cli/Commands/*.cs`, `host/Ams.Cli/Workspace/CliWorkspace.cs`
- Web: `host/Ams.Workstation.Server/Program.cs`, `host/Ams.Workstation.Server/Components/Pages/`, `host/Ams.Workstation.Server/Controllers/`
- Depends on: Application layer (commands, services)
- Used by: End users

**Application/Orchestration Layer:**
- Purpose: Coordinate multi-stage pipelines; bridge commands with domain services
- Contains: Use-case commands, pipeline orchestration, process supervisors, MFA workflow
- Commands: `host/Ams.Core/Application/Commands/*.cs` (GenerateTranscript, ComputeAnchors, BuildTranscriptIndex, HydrateTranscript, RunMfa, MergeTimings)
- Pipeline: `host/Ams.Core/Application/Pipeline/PipelineService.cs`
- Processes: `host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`, `host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`
- MFA: `host/Ams.Core/Application/Mfa/MfaWorkflow.cs`, `host/Ams.Core/Application/Mfa/MfaService.cs`
- Depends on: Service/Domain layer, Runtime contexts
- Used by: Host layer

**Service/Domain Layer:**
- Purpose: Reusable, stateless alignment, ASR, and I/O services
- Contains: Service interfaces and implementations, alignment algorithms
- Services: `host/Ams.Core/Services/AsrService.cs`, `host/Ams.Core/Services/Alignment/*.cs`
- Interfaces: `host/Ams.Core/Services/Interfaces/IAsrService.cs`, `host/Ams.Core/Services/Alignment/IAnchorComputeService.cs`
- Integrations: `host/Ams.Core/Services/Integrations/FFmpeg/`
- Depends on: Runtime contexts, common utilities
- Used by: Application commands

**Runtime/Context Layer:**
- Purpose: Manage long-lived resource contexts (books, chapters, audio); handle artifact resolution
- Contains: Workspace interface, Book/Chapter contexts, artifact resolvers, document slots
- Workspace: `host/Ams.Core/Runtime/Workspace/IWorkspace.cs`
- Book: `host/Ams.Core/Runtime/Book/BookContext.cs`, `host/Ams.Core/Runtime/Book/BookManager.cs`
- Chapter: `host/Ams.Core/Runtime/Chapter/ChapterContext.cs`, `host/Ams.Core/Runtime/Chapter/ChapterManager.cs`
- Artifacts: `host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`, `host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`
- Depends on: Common utilities, Artifact models
- Used by: Service and Application layers

**Processor Layer:**
- Purpose: Core domain algorithms (anchor discovery, transcript alignment, MFA merge, TextGrid parsing)
- Contains: Pure algorithmic processors
- Anchors: `host/Ams.Core/Processors/Alignment/Anchors/AnchorPipeline.cs`, `host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`
- Transcript: `host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`, `host/Ams.Core/Processors/Alignment/Tx/WindowBuilder.cs`
- MFA: `host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs`, `host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`
- Depends on: Common utilities, Artifact models
- Used by: Service layer

**Common/Utility Layer:**
- Purpose: Cross-cutting concerns (logging, text normalization, metrics)
- Contains: `host/Ams.Core/Common/Log.cs`, `host/Ams.Core/Common/TextNormalizer.cs`, `host/Ams.Core/Common/LevenshteinMetrics.cs`
- Depends on: Nothing (foundational)
- Used by: All layers

## Data Flow

**Pipeline Execution (Full: ASR -> Alignment -> MFA):**

1. User runs `pipeline run` (CLI) or triggers from web UI
2. `PipelineService.RunChapterAsync` orchestrates stages sequentially:
   - **BookIndex**: Validates/builds book index (`EnsureBookIndexAsync`)
   - **Asr**: `GenerateTranscriptCommand` -> `IAsrService.TranscribeAsync` -> Nemo or Whisper.NET
   - **Anchors**: `ComputeAnchorsCommand` -> `AnchorPipeline` -> finds reliable sync points
   - **Transcript**: `BuildTranscriptIndexCommand` -> `TranscriptIndexService` -> aligns book text to ASR tokens
   - **Hydrate**: `HydrateTranscriptCommand` -> `TranscriptHydrationService` -> creates fully-timed transcript
   - **Mfa**: `RunMfaCommand` -> `MfaWorkflow` -> corpus validation, G2P, forced alignment -> TextGrid
   - **Complete**: `MergeTimingsCommand` -> `MfaTimingMerger` -> merges TextGrid timings into JSON
3. Returns `PipelineChapterResult` with stage timings, errors, artifacts
4. Batch mode: parallel tasks per chapter with `SemaphoreSlim` concurrency control

**CLI Command Execution:**

1. `Program.cs` -> System.CommandLine parses args -> dispatches to verb handler
2. Command handler resolves options, obtains `IWorkspace` from `ReplContext`
3. `workspace.OpenChapter(options)` -> `ChapterManager.CreateContext()` -> `ChapterContext`
4. Command calls service (e.g., `GenerateTranscriptCommand.ExecuteAsync(chapter, options, token)`)
5. Service produces artifacts -> stored via `DocumentSlot<T>` -> `chapter.Save()`
6. Return to REPL or next command

**State Management:**
- CLI REPL state: persisted in `%APPDATA%/AMS/repl-state.json` (`host/Ams.Cli/Repl/ReplContext.cs`)
- Web workspace state: persisted in `%LOCALAPPDATA%/AMS/workstation-state.json` (`host/Ams.Workstation.Server/Services/BlazorWorkspace.cs`)
- Chapter contexts: ephemeral per-command, cached in `ChapterManager`
- Artifacts: file-backed via `DocumentSlot<T>` (lazy-loaded, JSON/binary)

## Key Abstractions

**Command:**
- Purpose: Single-method use-case classes
- Examples: `GenerateTranscriptCommand`, `ComputeAnchorsCommand`, `HydrateTranscriptCommand`
- Pattern: `ExecuteAsync(ChapterContext, Options, CancellationToken)`
- Location: `host/Ams.Core/Application/Commands/*.cs`

**Service:**
- Purpose: Stateless, reusable business logic
- Examples: `AsrService`, `AnchorComputeService`, `TranscriptIndexService`, `TranscriptHydrationService`
- Pattern: Interface + implementation, registered as singletons via DI
- Location: `host/Ams.Core/Services/*.cs`

**Workspace:**
- Purpose: Host-agnostic chapter access and resource lifecycle
- Examples: `CliWorkspace`, `BlazorWorkspace`
- Pattern: `IWorkspace` interface with `RootPath`, `Book`, `OpenChapter(options)`
- Location: `host/Ams.Core/Runtime/Workspace/IWorkspace.cs`

**Context:**
- Purpose: Long-lived resource holders with cached state
- Examples: `BookContext`, `ChapterContext`, `AudioBufferContext`
- Pattern: Disposable context with `Documents` slots and `Save()` persistence
- Location: `host/Ams.Core/Runtime/{Book,Chapter,Audio}/*.cs`

**DocumentSlot:**
- Purpose: Lazy-load wrapper for JSON/binary artifacts
- Pattern: Deserialize on first access, dirty-track for save
- Location: `host/Ams.Core/Runtime/Common/DocumentSlot.cs`

**Process Supervisor:**
- Purpose: External process lifecycle management
- Examples: `AsrProcessSupervisor`, `MfaProcessSupervisor`
- Pattern: Background warmup, health checks, coordinated shutdown
- Location: `host/Ams.Core/Application/Processes/*.cs`

## Entry Points

**CLI Entry:**
- Location: `host/Ams.Cli/Program.cs`
- Triggers: User runs `dotnet Ams.Cli.dll <command>`
- Responsibilities: DI bootstrap, System.CommandLine registration, REPL orchestrator

**Web Entry:**
- Location: `host/Ams.Workstation.Server/Program.cs`
- Triggers: ASP.NET Core / Blazor Server startup
- Responsibilities: DI setup (singleton workspace, transient services), middleware config

**Test Entry:**
- Location: `host/Ams.Tests/*.cs`
- Triggers: `dotnet test`
- Responsibilities: xUnit test discovery and execution

## Error Handling

**Strategy:** Validate inputs early, throw descriptive exceptions, catch at command/controller boundaries

**Patterns:**
- `ArgumentNullException.ThrowIfNull()` for null validation
- `InvalidOperationException` for missing artifacts or invalid state (e.g., "BookIndex is not loaded")
- Try-catch at CLI command level with `Log.Error()` and `context.ExitCode = 1`
- Pipeline catches per-stage exceptions, returns `PipelineChapterResult` with error details
- `TryDelete()` pattern for safe file cleanup in finally blocks

## Cross-Cutting Concerns

**Logging:**
- Serilog via static `Log` facade (`host/Ams.Core/Common/Log.cs`)
- Per-class loggers: `Log.For<ClassName>()`
- Levels: Trace, Debug, Info, Warn, Error, Critical
- Sinks: Console (colored) + Rolling file

**Dependency Injection:**
- Microsoft.Extensions.DependencyInjection + IHostBuilder
- All services registered as singletons (stateless)
- Commands registered as singletons

**Async/Await:**
- Pervasive throughout; all I/O-bound operations are async
- `ConfigureAwait(false)` in library code
- `CancellationToken` threading on all long-running operations
- `SemaphoreSlim` for controlled parallelism in batch operations

**Validation:**
- Input validation at command boundaries
- `ArgumentNullException.ThrowIfNull()` throughout
- Null-coalescing for optional parameters: `options ?? Default`

---

*Architecture analysis: 2026-02-06*
*Update when major patterns change*
