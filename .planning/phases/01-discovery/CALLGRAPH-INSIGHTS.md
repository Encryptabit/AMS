# Call Graph Insights

Architectural observations extracted from reviewing existing call graphs.

**Analysis method:** Reviewed 15+ key call graphs, traced call chains, identified patterns.

---

## Key Entry Points

Primary entry points into the codebase, ranked by importance:

### 1. PipelineService.RunChapterAsync
**Location:** `Ams.Core/Services/PipelineService.cs`
**Role:** Main orchestrator for the full ASR → alignment → MFA → merge pipeline.

**Key observations:**
- Calls 30+ methods across 6 command classes
- Coordinates all major commands: GenerateTranscript, ComputeAnchors, BuildTranscriptIndex, HydrateTranscript, RunMfa, MergeTimings
- Manages semaphore-based concurrency control
- Handles book index caching
- Has most "Calls →" relationships of any method in codebase

### 2. MfaWorkflow.RunChapterAsync
**Location:** `Ams.Core/Application/Mfa/MfaWorkflow.cs`
**Role:** Orchestrates Montreal Forced Aligner workflow (validate → G2P → align).

**Key observations:**
- Called by RunMfaCommand.ExecuteAsync
- Manages MFA staging (lab files, audio copying)
- Handles OOV word discovery and G2P pronunciation generation
- Coordinates with MfaProcessSupervisor for conda environment

### 3. AlignmentService.BuildTranscriptIndexAsync
**Location:** `Ams.Core/Services/Alignment/AlignmentService.cs`
**Role:** Core alignment logic - builds transcript index from book + ASR.

**Key observations:**
- Most complex algorithm in alignment subsystem
- Calls AnchorPipeline.ComputeAnchors, TranscriptAligner.AlignWindows
- Builds word/sentence/paragraph rollups
- Uses phoneme matching for homophone resolution

### 4. CliWorkspace.OpenChapter
**Location:** `Ams.Cli/Workspace/CliWorkspace.cs`
**Role:** Entry point for CLI chapter operations.

**Key observations:**
- Called by all CLI commands that work with chapters
- Creates ChapterContextHandle for chapter lifecycle management
- Coordinates with ChapterManager for caching

### 5. AudioProcessor.Decode / AudioProcessor.Resample
**Location:** `Ams.Core/Processors/AudioProcessor.cs`
**Role:** Audio I/O and transformation entry points.

**Key observations:**
- Decode: Primary audio loading path (wraps FfDecoder)
- Resample: Used by ASR preparation (16kHz for Whisper)
- Multiple callers from tests and services

---

## High-Connectivity Files

Files with many incoming or outgoing dependencies:

### Most Callers (heavily depended upon)

| File | Callers | Role |
|------|---------|------|
| ChapterContext.Save | 6+ | Persistence coordination |
| BookContext.Save | 3+ | Book-level persistence |
| FfDecoder.Decode | 5+ | Primary audio decode path |
| AudioProcessor.Decode | 5+ | Audio loading facade |
| TextNormalizer methods | 10+ | Text normalization utilities |
| LevenshteinMetrics.Distance | 5+ | Edit distance for alignment |

### Most Callees (high fan-out)

| File | Callees | Role |
|------|---------|------|
| PipelineService.RunChapterAsync | 30+ | Main orchestrator |
| AlignmentService.BuildTranscriptIndexAsync | 15+ | Alignment coordination |
| BookIndexer.Process | 15+ | Book indexing logic |
| TranscriptAligner.Rollup | 10+ | Sentence/paragraph rollup |
| MfaWorkflow.RunChapterAsync | 10+ | MFA orchestration |

---

## Potential Dead Code Candidates

Files/methods with no external callers (may be orphaned or entry points):

### Likely Dead Code

| File | Method | Notes |
|------|--------|-------|
| DspDemoRunner.cs | RunDemo | No `Called-by ←` entries from production code - likely test/demo only |
| FeatureExtraction.cs | Detect | No external callers visible - breath detection may be disabled |
| FfResampler.cs | (unknown) | Missing from call graphs entirely - may be unused |

### Entry Points (Not Dead)

These have no callers in call graphs but are legitimate entry points:
- All CLI command Execute methods (called by Spectre.Console)
- Program.cs Main methods
- Test methods (called by test framework)

### Needs Investigation

| File | Concern |
|------|---------|
| WnModel.cs, WnSession.cs, WnTranscriber.cs | Whisper.NET integration - are these still used or replaced by Nemo ASR? |
| PauseAdjustmentsDocument.cs | Part of prosody system - is this active? |
| ScriptValidator.md | Call graph exists but no matching source file |

---

## Architectural Patterns Observed

### 1. Context Hierarchy Pattern
```
BookContext
  └── ChapterContext (via ChapterManager)
        └── ChapterContextHandle (disposable wrapper)
```
- BookContext holds book-level state (sections, index)
- ChapterContext holds chapter-level state (ASR, anchors, transcript)
- ChapterContextHandle provides RAII-style lifecycle management

### 2. Command Pattern (Application Layer)
```
PipelineService
  ├── GenerateTranscriptCommand.ExecuteAsync
  ├── ComputeAnchorsCommand.ExecuteAsync
  ├── BuildTranscriptIndexCommand.ExecuteAsync
  ├── HydrateTranscriptCommand.ExecuteAsync
  ├── RunMfaCommand.ExecuteAsync
  └── MergeTimingsCommand.ExecuteAsync
```
- Each command encapsulates one pipeline stage
- Commands are DI-injected into PipelineService
- All share `ExecuteAsync(ChapterContext, Options, CancellationToken)` signature

### 3. Processor/Service Split
- **Processors**: Stateless algorithms (AudioProcessor, TranscriptAligner, AnchorPipeline)
- **Services**: Stateful coordination (PipelineService, AlignmentService, AsrService)
- Pattern is consistent but boundary sometimes blurry

### 4. Integration Facades
```
AudioProcessor → FfDecoder, FfEncoder, FfFilterGraph
AsrProcessor → WhisperFactory, WhisperProcessor
MfaWorkflow → MfaProcessSupervisor (conda/MFA CLI)
```
- Core code calls high-level facades
- Facades wrap external libraries/tools

---

## Circular Dependencies

### Observed Circular Reference Patterns

1. **Save propagation loop:**
   - ChapterContext.Save → DocumentSlot.SaveChanges → (writes to disk)
   - BookContext.Save → Deallocate → ChapterContext.Save
   - No infinite loop, but tight coupling

2. **Self-referential methods:**
   - Several methods call themselves recursively (e.g., Rollup → Rollup overloads)
   - This is intentional method overloading, not a problem

### No Concerning Circular Dependencies Found

The call graphs don't reveal problematic circular module dependencies.

---

## Integration Boundaries

### External System Touchpoints

| Integration | Files | Nature |
|-------------|-------|--------|
| **FFmpeg** | FfDecoder, FfEncoder, FfFilterGraph, FfFilterGraphRunner, FfSession, FfLogCapture, FfUtils, FfResampler | Native P/Invoke via FFmpeg.AutoGen |
| **Whisper.NET** | WnModel, WnSession, WnTranscriber, WnUtils, AsrProcessor | .NET binding to whisper.cpp |
| **Nemo ASR** | AsrClient | HTTP client to external Python service |
| **MFA** | MfaProcessSupervisor, MfaDetachedProcessRunner, MfaWorkflow | Conda environment + CLI invocation |

### P/Invoke Surface (needs Plan 01-03 documentation)

| File | P/Invoke Surface |
|------|------------------|
| FfDecoder.cs | AVFrame*, SwrContext*, pointer arithmetic |
| FfEncoder.cs | AVCodecContext*, AVPacket*, memory management |
| FfFilterGraph.cs | AVFilterGraph*, filter chaining |
| AmsDsp.cs | Native DSP library calls |
| Native.cs | Native method declarations |

---

## Key Insights Summary

1. **PipelineService is the coordination hub** - Understanding its call graph is essential for understanding the system

2. **Alignment is the algorithmic core** - AlignmentService and TranscriptAligner contain the most complex algorithms

3. **Context hierarchy is the data backbone** - BookContext/ChapterContext manage all runtime state

4. **FFmpeg integration is the main P/Invoke surface** - Plan 01-03 should focus here

5. **Potential dead code exists** - DspDemoRunner, FeatureExtraction (breath detection), possibly Whisper.NET integrations

6. **Web stack has no call graphs** - Ams.Web.* projects are nascent and undocumented

7. **Coverage is strong (90%)** - Most critical files have call graphs; gaps are mostly config/models

---

*Generated: 2025-12-28*
