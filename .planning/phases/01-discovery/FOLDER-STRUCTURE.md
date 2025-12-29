# AMS Folder Structure

Tree-style folder hierarchy with purpose annotations.

---

```
AMS/
├── .planning/                           # Planning and documentation
│   ├── PROJECT.md                       # Project overview
│   ├── ROADMAP.md                       # Development roadmap
│   ├── STATE.md                         # Current state tracking
│   ├── codebase/                        # Codebase analysis docs
│   └── phases/                          # Phase-based planning
│       └── 01-discovery/                # Discovery phase artifacts
│
├── analysis/                            # Analysis/testing tools
│   └── OverlayTest/                     # AudioProcessor.OverlayRoomtone test
│       ├── OverlayTest.csproj
│       └── Program.cs
│
├── host/                                # Main application projects
│   │
│   ├── Ams.Cli/                         # CLI entry point [ACTIVE]
│   │   ├── Ams.Cli.csproj
│   │   ├── Program.cs                   # Entry point, DI, REPL loop
│   │   ├── GlobalUsings.cs
│   │   ├── Commands/                    # CLI command handlers
│   │   │   ├── AlignCommand.cs
│   │   │   ├── AsrCommand.cs
│   │   │   ├── BookCommand.cs
│   │   │   ├── BuildIndexCommand.cs
│   │   │   ├── DspCommand.cs
│   │   │   ├── PipelineCommand.cs
│   │   │   ├── RefineSentencesCommand.cs
│   │   │   ├── TextCommand.cs
│   │   │   ├── ValidateCommand.cs
│   │   │   └── ValidateTimingSession.cs
│   │   ├── Models/                      # CLI-specific models
│   │   │   ├── DspConfigModels.cs
│   │   │   ├── FilterChainConfig.cs
│   │   │   └── TreatmentModels.cs
│   │   ├── Repl/                        # REPL state management
│   │   │   └── ReplContext.cs
│   │   ├── Services/                    # CLI services
│   │   │   ├── DspConfigService.cs
│   │   │   ├── DspSessionState.cs
│   │   │   └── PlugalyzerService.cs
│   │   ├── Utilities/                   # CLI utilities
│   │   │   ├── CommandInputResolver.cs
│   │   │   └── PausePolicyResolver.cs
│   │   └── Workspace/                   # CLI workspace impl
│   │       └── CliWorkspace.cs
│   │
│   ├── Ams.Core/                        # Core library [ACTIVE]
│   │   ├── Ams.Core.csproj
│   │   ├── AssemblyInfo.cs
│   │   ├── GlobalUsings.cs
│   │   │
│   │   ├── Application/                 # Use-case layer
│   │   │   ├── Commands/                # Command implementations
│   │   │   │   ├── BuildTranscriptIndexCommand.cs
│   │   │   │   ├── ComputeAnchorsCommand.cs
│   │   │   │   ├── GenerateTranscriptCommand.cs
│   │   │   │   ├── HydrateTranscriptCommand.cs
│   │   │   │   ├── MergeTimingsCommand.cs
│   │   │   │   └── RunMfaCommand.cs
│   │   │   ├── Mfa/                     # MFA integration
│   │   │   │   ├── MfaDetachedProcessRunner.cs
│   │   │   │   ├── MfaPronunciationProvider.cs
│   │   │   │   ├── MfaService.cs
│   │   │   │   └── MfaWorkflow.cs
│   │   │   ├── Pipeline/                # Pipeline coordination
│   │   │   │   ├── PipelineChapterResult.cs
│   │   │   │   ├── PipelineConcurrencyControl.cs
│   │   │   │   ├── PipelineRunOptions.cs
│   │   │   │   └── PipelineStage.cs
│   │   │   └── Processes/               # External process mgmt
│   │   │       ├── AsrProcessSupervisor.cs
│   │   │       └── MfaProcessSupervisor.cs
│   │   │
│   │   ├── Artifacts/                   # Data artifacts/models
│   │   │   ├── AudioBuffer.cs
│   │   │   ├── AudioBufferMetadata.cs
│   │   │   ├── FragmentTiming.cs
│   │   │   ├── SentenceTiming.cs
│   │   │   ├── TimingOverrides.cs
│   │   │   ├── TimingRange.cs
│   │   │   ├── TranscriptModels.cs
│   │   │   ├── Alignment/               # Alignment artifacts
│   │   │   │   ├── AnchorDocument.cs
│   │   │   │   ├── MfaChapterContext.cs
│   │   │   │   ├── MfaCommandResult.cs
│   │   │   │   └── Mfa/
│   │   │   │       └── TextGridDocument.cs
│   │   │   ├── Hydrate/                 # Hydration artifacts
│   │   │   │   └── HydratedTranscript.cs
│   │   │   └── Validation/              # Validation artifacts
│   │   │       └── ValidationReportModels.cs
│   │   │
│   │   ├── Asr/                         # ASR subsystem
│   │   │   ├── AsrClient.cs
│   │   │   ├── AsrEngine.cs
│   │   │   ├── AsrModels.cs
│   │   │   └── AsrTranscriptBuilder.cs
│   │   │
│   │   ├── Audio/                       # Audio utilities
│   │   │   ├── AudioIntegrityVerifier.cs
│   │   │   ├── DspDemoRunner.cs
│   │   │   ├── FeatureExtraction.cs
│   │   │   └── SentenceTimelineBuilder.cs
│   │   │
│   │   ├── Common/                      # Shared utilities
│   │   │   ├── LevenshteinMetrics.cs
│   │   │   ├── Log.cs
│   │   │   └── TextNormalizer.cs
│   │   │
│   │   ├── Pipeline/                    # Pipeline models
│   │   │   ├── ManifestV2.cs
│   │   │   └── SentenceRefinementService.cs
│   │   │
│   │   ├── Processors/                  # Processing logic
│   │   │   ├── AsrProcessor.cs
│   │   │   ├── AudioProcessor.cs
│   │   │   ├── AudioProcessor.Analysis.cs
│   │   │   ├── Alignment/               # Alignment processors
│   │   │   │   ├── Anchors/             # Anchor detection
│   │   │   │   │   ├── AnchorDiscovery.cs
│   │   │   │   │   ├── AnchorPipeline.cs
│   │   │   │   │   ├── AnchorPreprocessor.cs
│   │   │   │   │   ├── AnchorTokenizer.cs
│   │   │   │   │   ├── SectionLocator.cs
│   │   │   │   │   └── StopwordSets.cs
│   │   │   │   ├── Mfa/                 # MFA processing
│   │   │   │   │   ├── MfaTimingMerger.cs
│   │   │   │   │   └── TextGridParser.cs
│   │   │   │   └── Tx/                  # Transcript alignment
│   │   │   │       ├── TranscriptAligner.cs
│   │   │   │       └── WindowBuilder.cs
│   │   │   ├── Diffing/                 # Text diff analysis
│   │   │   │   └── TextDiffAnalyzer.cs
│   │   │   ├── DocumentProcessor/       # Document processing
│   │   │   │   ├── DocumentProcessor.Cache.cs
│   │   │   │   ├── DocumentProcessor.Indexing.cs
│   │   │   │   └── DocumentProcessor.Phonemes.cs
│   │   │   └── Validation/              # Validation processing
│   │   │       └── ValidationReportBuilder.cs
│   │   │
│   │   ├── Prosody/                     # Prosody/pause system
│   │   │   ├── PauseAdjustmentsDocument.cs
│   │   │   ├── PauseAnalysisReport.cs
│   │   │   ├── PauseCompressionMath.cs
│   │   │   ├── PauseDynamicsService.cs
│   │   │   ├── PauseMapBuilder.cs
│   │   │   ├── PauseMapModels.cs
│   │   │   ├── PauseModels.cs
│   │   │   ├── PausePolicyStorage.cs
│   │   │   └── PauseTimelineApplier.cs
│   │   │
│   │   ├── Runtime/                     # Runtime context layer
│   │   │   ├── Artifacts/               # Artifact resolution
│   │   │   │   ├── FileArtifactResolver.cs
│   │   │   │   └── IArtifactResolver.cs
│   │   │   ├── Audio/                   # Audio runtime
│   │   │   │   ├── AudioBufferContext.cs
│   │   │   │   └── AudioBufferManager.cs
│   │   │   ├── Book/                    # Book context
│   │   │   │   ├── BookCache.cs
│   │   │   │   ├── BookContext.cs
│   │   │   │   ├── BookDocuments.cs
│   │   │   │   ├── BookIndexer.cs
│   │   │   │   ├── BookManager.cs
│   │   │   │   ├── BookModels.cs
│   │   │   │   ├── BookParser.cs
│   │   │   │   ├── BookPhonemePopulator.cs
│   │   │   │   ├── IBookServices.cs
│   │   │   │   ├── IPronunciationProvider.cs
│   │   │   │   └── PronunciationHelper.cs
│   │   │   ├── Chapter/                 # Chapter context
│   │   │   │   ├── ChapterContext.cs
│   │   │   │   ├── ChapterContextHandle.cs
│   │   │   │   ├── ChapterDocuments.cs
│   │   │   │   └── ChapterManager.cs
│   │   │   ├── Common/                  # Common runtime
│   │   │   │   ├── DelegateDocumentSlotAdapter.cs
│   │   │   │   ├── DocumentSlot.cs
│   │   │   │   ├── DocumentSlotOptions.cs
│   │   │   │   └── IDocumentSlotAdapter.cs
│   │   │   ├── Interfaces/              # Runtime interfaces
│   │   │   │   ├── IAudioBufferManager.cs
│   │   │   │   ├── IBookManager.cs
│   │   │   │   └── IChapterManager.cs
│   │   │   └── Workspace/               # Workspace abstraction
│   │   │       ├── IWorkspace.cs
│   │   │       └── WorkspaceChapterDiscovery.cs
│   │   │
│   │   ├── Services/                    # High-level services
│   │   │   ├── AsrService.cs
│   │   │   ├── AudioService.cs
│   │   │   ├── PipelineService.cs
│   │   │   ├── ValidationService.cs
│   │   │   ├── Alignment/               # Alignment service
│   │   │   │   ├── AlignmentOptions.cs
│   │   │   │   └── AlignmentService.cs
│   │   │   ├── Documents/               # Document service
│   │   │   │   └── DocumentService.cs
│   │   │   ├── Integrations/            # External integrations
│   │   │   │   ├── ASR/                 # ASR integrations
│   │   │   │   │   └── WhisperNet/      # Whisper.NET
│   │   │   │   │       ├── WnModel.cs
│   │   │   │   │       ├── WnSession.cs
│   │   │   │   │       ├── WnTranscriber.cs
│   │   │   │   │       └── WnUtils.cs
│   │   │   │   └── FFmpeg/              # FFmpeg integration
│   │   │   │       ├── FfDecoder.cs
│   │   │   │       ├── FfEncoder.cs
│   │   │   │       ├── FfFilterGraph.cs
│   │   │   │       ├── FfFilterGraphRunner.cs
│   │   │   │       ├── FfLogCapture.cs
│   │   │   │       ├── FfResampler.cs
│   │   │   │       ├── FfSession.cs
│   │   │   │       ├── FfUtils.cs
│   │   │   │       ├── FilterSpecs.cs
│   │   │   │       └── models/          # Whisper models (binary)
│   │   │   └── Interfaces/              # Service interfaces
│   │   │       ├── IAlignmentService.cs
│   │   │       ├── IAsrService.cs
│   │   │       ├── IAudioService.cs
│   │   │       └── IDocumentService.cs
│   │   │
│   │   └── ExtTools/                    # External tool binaries
│   │       ├── Plugalyzer.exe
│   │       └── ffmpeg/
│   │           └── binaries/
│   │
│   ├── Ams.Dsp.Native/                  # Native DSP layer [ACTIVE]
│   │   ├── Ams.Dsp.Native.csproj
│   │   ├── AmsDsp.cs
│   │   └── Native.cs
│   │
│   ├── Ams.Tests/                       # Unit tests [STALE]
│   │   ├── Ams.Tests.csproj
│   │   ├── GlobalUsings.cs
│   │   ├── AnchorDiscoveryTests.cs
│   │   ├── AudioProcessorFilterTests.cs
│   │   ├── BookParsingTests.cs
│   │   ├── TokenizerTests.cs
│   │   ├── TxAlignTests.cs
│   │   ├── WavIoTests.cs
│   │   └── Prosody/
│   │       ├── PauseApplierTests.cs
│   │       └── PauseDynamicsServiceTests.cs
│   │
│   ├── Ams.UI.Avalonia/                 # Desktop UI [DORMANT]
│   │   ├── Ams.UI.Avalonia.csproj
│   │   ├── App.axaml.cs
│   │   ├── MainWindow.axaml.cs
│   │   └── Program.cs
│   │
│   ├── Ams.Web.Api/                     # REST API [NASCENT]
│   │   ├── Ams.Web.Api.csproj
│   │   ├── Program.cs
│   │   ├── WorkspaceState.cs
│   │   ├── Json/
│   │   │   └── ApiJsonSerializerContext.cs
│   │   ├── Mappers/
│   │   │   └── ValidationMapper.cs
│   │   ├── Payloads/
│   │   │   └── ChapterSummary.cs
│   │   └── Services/
│   │       └── ReviewedStateService.cs
│   │
│   ├── Ams.Web.Shared/                  # Shared DTOs [NASCENT]
│   │   ├── Ams.Web.Shared.csproj
│   │   ├── Class1.cs
│   │   ├── Validation/
│   │   │   └── ValidationDtos.cs
│   │   └── Workspace/
│   │       └── WorkspaceDtos.cs
│   │
│   └── Ams.Web/                         # Web UI [NASCENT]
│       ├── Ams.Web.Client/              # Blazor WASM client
│       │   ├── Ams.Web.Client.csproj
│       │   ├── Program.cs
│       │   ├── Services/
│       │   │   └── ValidationApiClient.cs
│       │   └── wwwroot/
│       └── Ams.Web/                     # Blazor server host
│           ├── Ams.Web.csproj
│           └── Program.cs
│
├── out/                                 # Output/analysis tools
│   └── InspectDocX/                     # DocX API inspector
│       ├── InspectDocX.csproj
│       └── Program.cs
│
├── services/                            # External service configs
│   └── asr-nemo/                        # Nemo ASR service
│       ├── node_modules/                # Node dependencies
│       └── venv312/                     # Python venv
│
├── CLAUDE.md                            # AI assistant context
└── CheckEnv.cs                          # Environment check script
```

---

## Key Folder Patterns

### Application Layer (`Ams.Core/Application/`)
Use-case commands that orchestrate business logic. Each command implements a single `ExecuteAsync` entry point.

### Processors (`Ams.Core/Processors/`)
Stateless processing logic organized by domain:
- **Alignment/**: Text-to-audio alignment (anchors, MFA, transcript)
- **Diffing/**: Text comparison and WER/CER calculation
- **DocumentProcessor/**: Book document parsing and indexing
- **Validation/**: Report generation

### Runtime (`Ams.Core/Runtime/`)
Context management for workspace, book, and chapter lifecycles:
- **Book/**: Book-level context and caching
- **Chapter/**: Chapter-level context with document slots
- **Workspace/**: Cross-cutting workspace abstraction

### Services (`Ams.Core/Services/`)
High-level service orchestration:
- **Integrations/**: External system wrappers (FFmpeg, Whisper.NET)
- Service classes coordinate between processors and runtime

### CLI Commands (`Ams.Cli/Commands/`)
Thin CLI command handlers that:
1. Parse command-line arguments
2. Resolve workspace/chapter context
3. Delegate to Application/Service layer
4. Format output

---

## Status Legend

- **[ACTIVE]** - Actively developed, builds, in use
- **[STALE]** - May build but needs review
- **[DORMANT]** - Skeleton only, not in use
- **[NASCENT]** - Early development, minimal implementation
