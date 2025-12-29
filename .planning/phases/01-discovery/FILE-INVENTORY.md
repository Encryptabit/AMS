# AMS File Inventory

Complete inventory of all C# source files with purpose annotations.

**Categories:**
- **Command** - CLI command handlers or use-case commands
- **Service** - Business logic services
- **Model** - Data models, DTOs, artifacts
- **Interface** - Interface definitions
- **Utility** - Helper utilities
- **Test** - Test files
- **Config** - Configuration and setup
- **Processor** - Data processing logic
- **Runtime** - Runtime context and management
- **Integration** - External system integrations

---

## Ams.Cli (22 files)

### Commands/

| File | Category | Purpose |
|------|----------|---------|
| AlignCommand.cs | Command | CLI handler for alignment operations (anchors, transcript index, hydrate) |
| AsrCommand.cs | Command | CLI handler for ASR operations (run, status) |
| BookCommand.cs | Command | CLI handler for book operations |
| BuildIndexCommand.cs | Command | CLI handler for building book index |
| DspCommand.cs | Command | CLI handler for DSP operations |
| PipelineCommand.cs | Command | CLI handler for full pipeline execution |
| RefineSentencesCommand.cs | Command | CLI handler for sentence refinement |
| TextCommand.cs | Command | CLI handler for text operations |
| ValidateCommand.cs | Command | CLI handler for validation report generation |
| ValidateTimingSession.cs | Command | Interactive timing validation session handler |

### Models/

| File | Category | Purpose |
|------|----------|---------|
| DspConfigModels.cs | Model | Configuration models for DSP processing |
| FilterChainConfig.cs | Model | Filter chain configuration for audio processing |
| TreatmentModels.cs | Model | Audio treatment configuration models |

### Repl/

| File | Category | Purpose |
|------|----------|---------|
| ReplContext.cs | Runtime | REPL session state and context management |

### Services/

| File | Category | Purpose |
|------|----------|---------|
| DspConfigService.cs | Service | DSP configuration loading and management |
| DspSessionState.cs | Service | DSP session state tracking |
| PlugalyzerService.cs | Service | Integration with external Plugalyzer tool |

### Utilities/

| File | Category | Purpose |
|------|----------|---------|
| CommandInputResolver.cs | Utility | Resolves command inputs from arguments/context |
| PausePolicyResolver.cs | Utility | Resolves pause policy configuration |

### Workspace/

| File | Category | Purpose |
|------|----------|---------|
| CliWorkspace.cs | Runtime | CLI-specific workspace implementation |

### Root/

| File | Category | Purpose |
|------|----------|---------|
| GlobalUsings.cs | Config | Global using directives |
| Program.cs | Config | Application entry point, DI setup, REPL loop |

---

## Ams.Core (96 files)

### Application/Commands/

| File | Category | Purpose |
|------|----------|---------|
| BuildTranscriptIndexCommand.cs | Command | Builds transcript index from book and ASR output |
| ComputeAnchorsCommand.cs | Command | Computes anchor points between book and ASR |
| GenerateTranscriptCommand.cs | Command | Generates ASR transcript from audio |
| HydrateTranscriptCommand.cs | Command | Hydrates transcript with book text and timings |
| MergeTimingsCommand.cs | Command | Merges MFA timings into hydrated transcript |
| RunMfaCommand.cs | Command | Runs Montreal Forced Aligner |

### Application/Mfa/

| File | Category | Purpose |
|------|----------|---------|
| MfaDetachedProcessRunner.cs | Integration | Runs MFA in detached process |
| MfaPronunciationProvider.cs | Integration | Provides pronunciations for MFA dictionary |
| MfaService.cs | Service | High-level MFA service coordination |
| MfaWorkflow.cs | Service | MFA workflow orchestration (validate, G2P, align) |

### Application/Pipeline/

| File | Category | Purpose |
|------|----------|---------|
| PipelineChapterResult.cs | Model | Result model for pipeline chapter execution |
| PipelineConcurrencyControl.cs | Service | Pipeline concurrency management |
| PipelineRunOptions.cs | Model | Options for pipeline execution |
| PipelineStage.cs | Model | Enum/model for pipeline stages |

### Application/Processes/

| File | Category | Purpose |
|------|----------|---------|
| AsrProcessSupervisor.cs | Service | Manages external ASR process lifecycle |
| MfaProcessSupervisor.cs | Service | Manages MFA conda environment and process |

### Artifacts/

| File | Category | Purpose |
|------|----------|---------|
| AudioBuffer.cs | Model | Audio sample buffer container |
| AudioBufferMetadata.cs | Model | Metadata for audio buffers |
| FragmentTiming.cs | Model | Timing data for audio fragments |
| SentenceTiming.cs | Model | Timing data for sentences |
| TimingOverrides.cs | Model | Manual timing override storage |
| TimingRange.cs | Model | Time range representation |
| TranscriptModels.cs | Model | Transcript data models |

### Artifacts/Alignment/

| File | Category | Purpose |
|------|----------|---------|
| AnchorDocument.cs | Model | Anchor points document |
| MfaChapterContext.cs | Model | MFA-specific chapter context |
| MfaCommandResult.cs | Model | MFA command execution result |

### Artifacts/Alignment/Mfa/

| File | Category | Purpose |
|------|----------|---------|
| TextGridDocument.cs | Model | Praat TextGrid document representation |

### Artifacts/Hydrate/

| File | Category | Purpose |
|------|----------|---------|
| HydratedTranscript.cs | Model | Fully hydrated transcript with all timings |

### Artifacts/Validation/

| File | Category | Purpose |
|------|----------|---------|
| ValidationReportModels.cs | Model | Validation report data models |

### Asr/

| File | Category | Purpose |
|------|----------|---------|
| AsrClient.cs | Integration | HTTP client for external ASR service |
| AsrEngine.cs | Config | ASR engine configuration and selection |
| AsrModels.cs | Model | ASR request/response models |
| AsrTranscriptBuilder.cs | Service | Builds transcript from ASR output |

### Audio/

| File | Category | Purpose |
|------|----------|---------|
| AudioIntegrityVerifier.cs | Utility | Verifies audio file integrity |
| DspDemoRunner.cs | Utility | Demo runner for DSP operations |
| FeatureExtraction.cs | Processor | Audio feature extraction |
| SentenceTimelineBuilder.cs | Service | Builds sentence timeline from timings |

### Common/

| File | Category | Purpose |
|------|----------|---------|
| LevenshteinMetrics.cs | Utility | Levenshtein distance calculation |
| Log.cs | Utility | Logging configuration and helpers |
| TextNormalizer.cs | Utility | Text normalization for comparison |

### Pipeline/

| File | Category | Purpose |
|------|----------|---------|
| ManifestV2.cs | Model | Pipeline manifest format V2 |
| SentenceRefinementService.cs | Service | Sentence boundary refinement |

### Processors/

| File | Category | Purpose |
|------|----------|---------|
| AsrProcessor.cs | Processor | ASR processing logic |
| AudioProcessor.cs | Processor | Audio file processing (decode, encode, filters) |
| AudioProcessor.Analysis.cs | Processor | Audio analysis extensions |

### Processors/Alignment/Anchors/

| File | Category | Purpose |
|------|----------|---------|
| AnchorDiscovery.cs | Processor | Discovers anchor points in text |
| AnchorPipeline.cs | Processor | Anchor discovery pipeline |
| AnchorPreprocessor.cs | Processor | Preprocesses text for anchor detection |
| AnchorTokenizer.cs | Processor | Tokenizes text for anchor matching |
| SectionLocator.cs | Processor | Locates sections in book text |
| StopwordSets.cs | Config | Stopword sets for anchor filtering |

### Processors/Alignment/Mfa/

| File | Category | Purpose |
|------|----------|---------|
| MfaTimingMerger.cs | Processor | Merges MFA TextGrid timings |
| TextGridParser.cs | Processor | Parses Praat TextGrid files |

### Processors/Alignment/Tx/

| File | Category | Purpose |
|------|----------|---------|
| TranscriptAligner.cs | Processor | Aligns transcript to book text |
| WindowBuilder.cs | Processor | Builds alignment windows |

### Processors/Diffing/

| File | Category | Purpose |
|------|----------|---------|
| TextDiffAnalyzer.cs | Processor | Analyzes text differences for WER/CER |

### Processors/DocumentProcessor/

| File | Category | Purpose |
|------|----------|---------|
| DocumentProcessor.Cache.cs | Processor | Document processing with caching |
| DocumentProcessor.Indexing.cs | Processor | Document indexing operations |
| DocumentProcessor.Phonemes.cs | Processor | Phoneme extraction from documents |

### Processors/Validation/

| File | Category | Purpose |
|------|----------|---------|
| ValidationReportBuilder.cs | Processor | Builds validation reports |

### Prosody/

| File | Category | Purpose |
|------|----------|---------|
| PauseAdjustmentsDocument.cs | Model | Pause adjustment storage |
| PauseAnalysisReport.cs | Model | Pause analysis report model |
| PauseCompressionMath.cs | Utility | Pause compression calculations |
| PauseDynamicsService.cs | Service | Dynamic pause adjustment service |
| PauseMapBuilder.cs | Service | Builds pause maps from text |
| PauseMapModels.cs | Model | Pause map data models |
| PauseModels.cs | Model | General pause data models |
| PausePolicyStorage.cs | Service | Pause policy persistence |
| PauseTimelineApplier.cs | Service | Applies pause adjustments to timeline |

### Runtime/Artifacts/

| File | Category | Purpose |
|------|----------|---------|
| FileArtifactResolver.cs | Runtime | Resolves file-based artifacts |
| IArtifactResolver.cs | Interface | Artifact resolution interface |

### Runtime/Audio/

| File | Category | Purpose |
|------|----------|---------|
| AudioBufferContext.cs | Runtime | Audio buffer runtime context |
| AudioBufferManager.cs | Runtime | Manages audio buffer lifecycle |

### Runtime/Book/

| File | Category | Purpose |
|------|----------|---------|
| BookCache.cs | Runtime | Caches parsed book data |
| BookContext.cs | Runtime | Book-level runtime context |
| BookDocuments.cs | Model | Book document collection |
| BookIndexer.cs | Processor | Indexes book text for search |
| BookManager.cs | Runtime | Manages book lifecycle |
| BookModels.cs | Model | Book data models |
| BookParser.cs | Processor | Parses book markdown files |
| BookPhonemePopulator.cs | Processor | Populates phoneme data for book |
| IBookServices.cs | Interface | Book service interfaces |
| IPronunciationProvider.cs | Interface | Pronunciation provider interface |
| PronunciationHelper.cs | Utility | Pronunciation helper utilities |

### Runtime/Chapter/

| File | Category | Purpose |
|------|----------|---------|
| ChapterContext.cs | Runtime | Chapter-level runtime context |
| ChapterContextHandle.cs | Runtime | Disposable chapter context handle |
| ChapterDocuments.cs | Model | Chapter document collection |
| ChapterManager.cs | Runtime | Manages chapter lifecycle and caching |

### Runtime/Common/

| File | Category | Purpose |
|------|----------|---------|
| DelegateDocumentSlotAdapter.cs | Runtime | Delegate-based document slot adapter |
| DocumentSlot.cs | Runtime | Document slot abstraction |
| DocumentSlotOptions.cs | Model | Document slot configuration |
| IDocumentSlotAdapter.cs | Interface | Document slot adapter interface |

### Runtime/Interfaces/

| File | Category | Purpose |
|------|----------|---------|
| IAudioBufferManager.cs | Interface | Audio buffer manager interface |
| IBookManager.cs | Interface | Book manager interface |
| IChapterManager.cs | Interface | Chapter manager interface |

### Runtime/Workspace/

| File | Category | Purpose |
|------|----------|---------|
| IWorkspace.cs | Interface | Workspace abstraction interface |
| WorkspaceChapterDiscovery.cs | Runtime | Discovers chapters in workspace |

### Services/

| File | Category | Purpose |
|------|----------|---------|
| AsrService.cs | Service | ASR service orchestration |
| AudioService.cs | Service | Audio processing service |
| PipelineService.cs | Service | Full pipeline orchestration |
| ValidationService.cs | Service | Validation report generation |

### Services/Alignment/

| File | Category | Purpose |
|------|----------|---------|
| AlignmentOptions.cs | Model | Alignment configuration options |
| AlignmentService.cs | Service | Alignment operations service |

### Services/Documents/

| File | Category | Purpose |
|------|----------|---------|
| DocumentService.cs | Service | Document processing service |

### Services/Integrations/ASR/WhisperNet/

| File | Category | Purpose |
|------|----------|---------|
| WnModel.cs | Integration | Whisper.NET model management |
| WnSession.cs | Integration | Whisper.NET session management |
| WnTranscriber.cs | Integration | Whisper.NET transcription |
| WnUtils.cs | Utility | Whisper.NET utility functions |

### Services/Integrations/FFmpeg/

| File | Category | Purpose |
|------|----------|---------|
| FfDecoder.cs | Integration | FFmpeg audio decoder |
| FfEncoder.cs | Integration | FFmpeg audio encoder |
| FfFilterGraph.cs | Integration | FFmpeg filter graph builder |
| FfFilterGraphRunner.cs | Integration | FFmpeg filter graph executor |
| FfLogCapture.cs | Integration | FFmpeg log capture |
| FfResampler.cs | Integration | FFmpeg audio resampler |
| FfSession.cs | Integration | FFmpeg session management |
| FfUtils.cs | Utility | FFmpeg utility functions |
| FilterSpecs.cs | Model | FFmpeg filter specifications |

### Services/Interfaces/

| File | Category | Purpose |
|------|----------|---------|
| IAlignmentService.cs | Interface | Alignment service interface |
| IAsrService.cs | Interface | ASR service interface |
| IAudioService.cs | Interface | Audio service interface |
| IDocumentService.cs | Interface | Document service interface |

### Root/

| File | Category | Purpose |
|------|----------|---------|
| AssemblyInfo.cs | Config | Assembly metadata |
| GlobalUsings.cs | Config | Global using directives |

---

## Ams.Dsp.Native (2 files)

| File | Category | Purpose |
|------|----------|---------|
| AmsDsp.cs | Integration | DSP native method declarations |
| Native.cs | Integration | Native interop definitions |

---

## Ams.Tests (9 files)

| File | Category | Purpose |
|------|----------|---------|
| AnchorDiscoveryTests.cs | Test | Tests for anchor discovery logic |
| AudioProcessorFilterTests.cs | Test | Tests for audio filter processing |
| BookParsingTests.cs | Test | Tests for book parsing |
| GlobalUsings.cs | Config | Global using directives |
| TokenizerTests.cs | Test | Tests for tokenization |
| TxAlignTests.cs | Test | Tests for transcript alignment |
| WavIoTests.cs | Test | Tests for WAV I/O operations |
| Prosody/PauseApplierTests.cs | Test | Tests for pause application |
| Prosody/PauseDynamicsServiceTests.cs | Test | Tests for pause dynamics service |

---

## Ams.UI.Avalonia (3 files)

| File | Category | Purpose |
|------|----------|---------|
| App.axaml.cs | Config | Avalonia application setup |
| MainWindow.axaml.cs | Runtime | Main window code-behind |
| Program.cs | Config | Application entry point |

---

## Ams.Web.Api (6 files)

| File | Category | Purpose |
|------|----------|---------|
| Program.cs | Config | Minimal API setup and endpoints |
| WorkspaceState.cs | Runtime | Workspace state for web API |
| Json/ApiJsonSerializerContext.cs | Config | AOT-compatible JSON serializer context |
| Mappers/ValidationMapper.cs | Utility | Maps domain models to DTOs |
| Payloads/ChapterSummary.cs | Model | Chapter summary payload models |
| Services/ReviewedStateService.cs | Service | Tracks reviewed chapter state |

---

## Ams.Web.Shared (3 files)

| File | Category | Purpose |
|------|----------|---------|
| Class1.cs | Model | Placeholder/empty class |
| Validation/ValidationDtos.cs | Model | Validation-related DTOs |
| Workspace/WorkspaceDtos.cs | Model | Workspace-related DTOs |

---

## Ams.Web.Client (2 files)

| File | Category | Purpose |
|------|----------|---------|
| Program.cs | Config | Blazor WASM application entry |
| Services/ValidationApiClient.cs | Service | HTTP client for validation API |

---

## Ams.Web (1 file)

| File | Category | Purpose |
|------|----------|---------|
| Program.cs | Config | Blazor server host entry point |

---

## OverlayTest (1 file)

| File | Category | Purpose |
|------|----------|---------|
| Program.cs | Utility | Tests AudioProcessor.OverlayRoomtone |

---

## InspectDocX (1 file)

| File | Category | Purpose |
|------|----------|---------|
| Program.cs | Utility | Inspects DocX library API capabilities |

---

## Summary by Category

| Category | Count | Percentage |
|----------|-------|------------|
| Processor | 19 | 13.0% |
| Runtime | 18 | 12.3% |
| Model | 27 | 18.5% |
| Service | 19 | 13.0% |
| Integration | 16 | 11.0% |
| Command | 16 | 11.0% |
| Interface | 10 | 6.8% |
| Utility | 10 | 6.8% |
| Test | 9 | 6.2% |
| Config | 9 | 6.2% |
| **Total** | **146** | **100%** |

---

## Notes

1. **Core is the center of gravity**: 96 of 146 files (66%) are in Ams.Core
2. **Processor-heavy architecture**: Alignment, audio, and document processing dominate
3. **Runtime context pattern**: Book/Chapter/Workspace contexts manage lifecycle
4. **Integration surface**: FFmpeg and Whisper.NET integrations are significant
5. **Test coverage gap**: Only 9 test files for 137 source files (~6.5% ratio by file count)
