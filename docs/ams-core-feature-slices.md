# AMS Core Feature Slice Catalogue

Last catalogued: 2026-05-16

Source of truth for this pass:

- Current filesystem under `host/Ams.Core`.
- Refreshed Code2Obsidian structural database at `host/vault/.code2obsidian-state.db`.
- Historical planning/audit notes that previously lived under `.planning`, used only as prior evidence.

Reader: an internal engineer preparing an AMS Core audit, cleanup, or refactor.

Post-read action: assign any AMS Core file, code line, or cleanup finding to a stable feature slice before changing it.

## Scope

This document covers the physical contents of `host/Ams.Core` only. Host projects, tests, archived UI projects, generated build output, and the Obsidian vault are referenced only when they prove whether Core code is used.

Every current AMS Core file has one labelled home. The ledger assigns whole-file line ranges. If a future audit needs to split a large file by method or class, create a child slice under the owning feature slice instead of leaving it unlabeled.

## Evidence Notes

The refreshed Code2Obsidian run completed without enrichment:

| Signal | Value |
| --- | ---: |
| Current commit recorded by DB | `78f808b89ea7166f1d27df5298f0ddf2d3979ad0` |
| DB run timestamp | `2026-05-16T21:38:32.3355518Z` |
| Emitted notes | 4,642 |
| Type index entries | 748 |
| Method index entries | 3,894 |
| Call edges | 6,168 |

Important caveat: Code2Obsidian still has stale entries for two removed Core files:

- `Ams.Core/Asr/AsrClient.cs`
- `Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

Those files do not exist on disk. Their vault notes still exist, so the vault is useful for structure but not yet a perfect stale-source cleanup oracle.

Historical `.planning` findings are not automatically current. Several previously flagged dead-code items are already gone from the current tree, including the old WhisperNet placeholder files, `AudioService`, `IAudioService`, `ManifestV2`, `DspDemoRunner`, and `SentenceTimelineBuilder`.

## Slice Index

| Slice | Home | Responsibility | Current files | Current lines |
| --- | --- | --- | ---: | ---: |
| FS00 | Build and project surface | Project build rules, assembly metadata, global compilation context | 4 | 178 |
| FS01 | Runtime workspace and artifact lifecycle | Workspace abstraction, book/chapter/audio context lifecycles, artifact path resolution, lazy document slots | 20 | 2,613 |
| FS02 | Book ingestion, indexing, and pronunciation | Manuscript parsing, book indexing, cache, pronunciation lookup, proper noun prompting | 19 | 4,243 |
| FS03 | ASR transcription | ASR engine selection, Whisper.NET processing, ASR service contracts, transcript response models | 8 | 2,481 |
| FS04 | Audio DSP, QC, and FFmpeg integration | Audio buffers through treatment, splicing, QC, silence detection, FFmpeg wrappers, filter specs | 25 | 8,704 |
| FS05 | Alignment, timing, and artifact contracts | Anchor selection, transcript alignment, hydration, MFA timing merge models, timing DTOs | 34 | 4,880 |
| FS06 | MFA forced alignment | MFA corpus construction, G2P/pronunciation support, process invocation, TextGrid aggregation | 12 | 3,750 |
| FS07 | Use-case commands and pipeline entry points | Command wrappers, pipeline orchestration, run states, module IDs, progress/failure contracts | 20 | 3,217 |
| FS08 | Benchmark and determinism | Benchmark run/compare contracts, deterministic gate, metrics, manifests, artifact store | 12 | 5,937 |
| FS09 | Validation and reporting | Validation reports, script validation, hydrated/text diff scoring | 6 | 1,961 |
| FS10 | Prosody and pause dynamics | Pause maps, pause policies, dynamics/compression math, timeline application | 9 | 2,744 |
| FS11 | Common infrastructure | Logging, path resolution, text normalization, natural sorting, edit distance helpers | 7 | 1,196 |
| FS12 | Embedded resources and model assets | Embedded word-frequency resource and bundled FFmpeg/Tesseract/Silero assets | 4 | 122,939 |

Total current AMS Core coverage: 180 files, 164,843 lines.

## Slice Responsibilities

### FS00: Build and Project Surface

Owns the Core project boundary: target framework, package references, native/asset copy rules, assembly metadata, global usings, and SDK pinning.

Audit focus:

- Keep build-time asset copying explicit and reproducible.
- Keep platform-specific FFmpeg/CUDA behavior documented and isolated from runtime logic.
- Avoid adding behavioral code here.

### FS01: Runtime Workspace and Artifact Lifecycle

Owns runtime object lifetimes and host-agnostic access to books, chapters, audio buffers, and file-backed artifacts.

Primary concepts:

- `IWorkspace` gives hosts a stable way to open chapters.
- `BookManager`, `ChapterManager`, and `AudioBufferManager` control cached context lifetimes.
- `FileArtifactResolver` centralizes canonical artifact paths.
- `DocumentSlot<T>` handles lazy artifact load/save behavior.

Audit focus:

- Preserve host neutrality. CLI and Workstation should depend on this layer, not the other way around.
- Audit host-specific path resolution as a first-class FS01 concern. CLI currently resolves command inputs and REPL defaults in its own resolver/workspace layer; Workstation resolves workspace defaults, chapter stems, playback audio variants, hydrate metrics, Polish artifacts, and CRX/export files across its server services. The likely Core representation is not "hosts load files and pass loaded files into Core"; it is closer to "hosts provide workspace metadata, selected chapter identity, explicit user overrides, and host-specific roots, then Core resolves canonical artifact addresses and lazily loads file-backed documents when the use case needs them."
- Separate host policy from Core ownership. Hosts may decide which workspace root, active chapter, or override path the user selected; Core should own the reusable rules for canonical book index, chapter directory, transcript, hydrate, ASR, treated audio, TextGrid, pause policy, and related artifact locations.
- Watch for cache invalidation and lifetime bugs.
- Keep serialization slot behavior consistent across all artifact documents.

### FS02: Book Ingestion, Indexing, and Pronunciation

Owns manuscript parsing, index construction, cached book metadata, audio descriptors, pronunciation providers, and proper noun filtering.

Primary concepts:

- `BookParser` extracts source text from supported manuscript formats.
- `BookIndexer` builds the canonical book index.
- `BookCache` caches parse/index results.
- Pronunciation helpers and providers enrich book/index data for alignment and MFA.

Audit focus:

- Keep book-index schema compatibility stable.
- Keep parser/indexer responsibilities separate from chapter runtime state.
- Treat `DocumentProcessor` as a compatibility facade over the newer runtime book services.

### FS03: ASR Transcription

Owns speech-to-text engine selection and transcript generation.

Primary concepts:

- `AsrEngine` and `AmsAsrModel` resolve runtime ASR mode/model.
- `AsrProcessor` contains the active Whisper.NET implementation.
- `AsrService` bridges runtime chapters/audio buffers to ASR artifacts.
- `AsrAudioPreparer` normalizes buffers for ASR.

Audit focus:

- The old `AsrClient` and `AsrProcessSupervisor` vault entries are stale. Do not treat them as current code.
- Keep engine choice explicit; do not blur Whisper.NET, Nemo, and future ASR engines.
- `AsrProcessor` is large and hot-path heavy; future cleanup should preserve test coverage around splicing, prompt filtering, and token timings.

### FS04: Audio DSP, QC, and FFmpeg Integration

Owns lower-level audio processing and FFmpeg interop.

Primary concepts:

- `AudioBuffer` is shared as an artifact contract, but concrete DSP operations live here.
- `AudioProcessor` partials provide decode, encode, analysis, loudness, activity tracking, and silence detection.
- `Ff*` classes wrap FFmpeg.AutoGen primitives and filter graphs.
- `AudioTreatmentService`, `AudioSpliceService`, QC, silence chunking, and boundary selection prepare audio for workstation and pipeline use.

Audit focus:

- `FfResampler` is an empty 7-line placeholder with no current references. It is the strongest current dead-code candidate in AMS Core.
- Keep unsafe FFmpeg code isolated to the integration folder.
- Avoid duplicating downmix, silence, and timeline projection rules between ASR, treatment, and prosody.

### FS05: Alignment, Timing, and Artifact Contracts

Owns anchor discovery, transcript alignment, hydration, timing merge, and the JSON DTOs consumed by pipeline stages and hosts.

Primary concepts:

- Anchor processors find stable book/ASR sync points.
- Transcript alignment maps ASR tokens back to book text.
- Hydration enriches transcript indexes with word-level and diff data.
- Artifact records define canonical persisted shapes.

Audit focus:

- DTO files often have no call edges because they are serialized. Do not classify them as dead only because the call graph is quiet.
- Alignment service decomposition is already partly done: focused services exist under `Services/Alignment`, with `AlignmentService` as a facade.
- Keep artifact compatibility visible when moving files.

### FS06: MFA Forced Alignment

Owns forced-alignment preparation and invocation.

Primary concepts:

- `MfaChunkCorpusBuilder` builds deterministic chunk corpus inputs.
- `MfaService`, `MfaDetachedProcessRunner`, and `MfaProcessSupervisor` run MFA/G2P commands.
- `MfaPronunciationProvider` and `PronunciationLexiconCache` support pronunciation generation and caching.
- `TextGridAggregationService` merges per-chunk TextGrid output.

Audit focus:

- Large files here are workflow-heavy. Refactor behind stable command/result contracts.
- Keep chunk ID and corpus path determinism intact.
- Workstation pickup MFA refinement also uses these models, so usage may be outside Core.

### FS07: Use-Case Commands and Pipeline Entry Points

Owns the public application-level use cases that CLI and Workstation call.

Primary concepts:

- Command classes execute single pipeline operations.
- `PipelineService` orchestrates multi-stage chapter runs.
- `PipelineStage`, `PipelineRunOptions`, `RunState`, `RunFailure`, `RunArtifact`, and module IDs define run contracts.
- `MfaProcessSupervisor` is current process-supervision code for MFA.

Audit focus:

- This is the main cross-slice orchestration layer. High fan-out is expected.
- Keep command classes thin; avoid burying domain algorithms here.
- `ModuleIds`, `RunState`, and result DTOs are used heavily by host projects even if Core-only call graph looks sparse.

### FS08: Benchmark and Determinism

Owns benchmark run/compare flows and determinism contracts.

Primary concepts:

- Benchmark runs execute pipeline chapters with deterministic policy snapshots.
- Metrics collection reads generated artifacts and audio/QC outputs.
- Compare services validate compatibility and metric thresholds between runs.
- Manifest validation records malformed or invalid benchmark artifacts.

Audit focus:

- This slice is new relative to older planning docs and should be treated as current.
- Contracts are large but serialization-heavy; quiet call edges are not dead-code evidence.
- Compare/run service failure shapes reuse FS07 run contracts.

### FS09: Validation and Reporting

Owns validation report generation, script validation, and text diff scoring.

Primary concepts:

- `ValidationReportBuilder` builds report views from transcript and hydrate artifacts.
- `ValidationService` is a small application wrapper.
- `ScriptValidator` compares script text against ASR output.
- `TextDiffAnalyzer` performs detailed hydrated diff/scoring.

Audit focus:

- There are two validation namespaces: application validation report models and older `Ams.Core.Validation` script validation. Keep this split visible during cleanup.
- `TextDiffAnalyzer` is large and algorithmic. Treat changes as high risk.

### FS10: Prosody and Pause Dynamics

Owns pause analysis and pause transformation.

Primary concepts:

- Pause maps model sentence, paragraph, and chapter timing structure.
- Pause policies define class windows and compression behavior.
- `PauseDynamicsService` produces and applies pause adjustment decisions.
- `PauseTimelineApplier` applies pause transform sets to timings.

Audit focus:

- Historical notes correctly kept Prosody as active.
- Preserve policy serialization compatibility.
- Keep pause math deterministic and independently testable.

### FS11: Common Infrastructure

Owns small dependency-light helpers shared across slices.

Primary concepts:

- Path resolution and app-data locations.
- Text normalization and edit-distance metrics.
- Logging facade.
- Natural sort and chapter label helpers.

Audit focus:

- Common must stay small. Do not use it as a dumping ground for domain behavior.
- `ChapterLabelResolver` is a successful consolidation of earlier duplicated chapter-label logic.

### FS12: Embedded Resources and Model Assets

Owns non-code assets bundled or embedded by the Core project.

Primary concepts:

- `english-frequency-82k.txt` supports pronunciation/proper-noun logic.
- FFmpeg model assets are copied to output/publish by the project file.

Audit focus:

- These files dominate line counts but are data assets, not code complexity.
- Check licensing and binary provenance before redistributing.
- Keep asset copy paths in sync with `Ams.Core.csproj`.

## Dead, Stale, and Ambiguous Code

### Current High-Confidence Dead Code

| Item | Slice | Evidence | Action |
| --- | --- | --- | --- |
| `Services/Integrations/FFmpeg/FfResampler.cs` | FS04 | Empty placeholder class; no current references found in Core, CLI, Workstation, or Tests | Delete unless a near-term FFmpeg resampler implementation is planned |

### Current Stale Index/Vault Entries

| Stale item | Evidence | Action |
| --- | --- | --- |
| `Ams.Core/Asr/AsrClient.cs` | Present in Code2Obsidian `file_hashes` and vault notes, absent from filesystem | Remove stale vault notes or fix Code2Obsidian stale-source cleanup |
| `Ams.Core/Application/Processes/AsrProcessSupervisor.cs` | Present in Code2Obsidian `file_hashes` and vault notes, absent from filesystem | Remove stale vault notes or fix Code2Obsidian stale-source cleanup |

### Historical Dead-Code Findings Already Resolved

Older planning notes flagged these items as dead or likely dead. They are not in the current `host/Ams.Core` tree:

- WhisperNet placeholder files: `WnModel`, `WnSession`, `WnTranscriber`, `WnUtils`.
- `AudioService` and `IAudioService`.
- `ManifestV2`.
- `DspDemoRunner`.
- `SentenceTimelineBuilder`.

### Not Dead Despite Quiet Core Call Graph

These categories frequently show zero incoming Core call edges but are intentionally kept:

- DTO and artifact contract files under `Artifacts`, `Application/Runs`, `Application/Pipeline`, and `Application/Benchmark`.
- Interface files used by DI or host projects.
- Serialization models used by JSON load/save rather than direct method calls.
- Runtime manager interfaces and workspace contracts used by CLI/Workstation.
- MFA command/result models used by Workstation pickup refinement and MFA process runners.

## File And Line Ownership Ledger

Each row assigns the entire current file to one feature slice. `Lines 1-N` means every line in that file belongs to the listed slice for audit/refactor planning.

| File | Lines | Slice |
| --- | ---: | --- |
| `Ams.Core.csproj` | 1-166 | FS00 Build and project surface |
| `Application/Benchmark/BenchmarkCompareContracts.cs` | 1-368 | FS08 Benchmark and determinism |
| `Application/Benchmark/BenchmarkCompareService.cs` | 1-1057 | FS08 Benchmark and determinism |
| `Application/Benchmark/BenchmarkDeterminismContracts.cs` | 1-441 | FS08 Benchmark and determinism |
| `Application/Benchmark/BenchmarkDeterminismGate.cs` | 1-410 | FS08 Benchmark and determinism |
| `Application/Benchmark/BenchmarkHydrateTimingReader.cs` | 1-268 | FS08 Benchmark and determinism |
| `Application/Benchmark/BenchmarkMetricsCollector.cs` | 1-549 | FS08 Benchmark and determinism |
| `Application/Benchmark/BenchmarkRunArtifactStore.cs` | 1-173 | FS08 Benchmark and determinism |
| `Application/Benchmark/BenchmarkRunManifestValidator.cs` | 1-322 | FS08 Benchmark and determinism |
| `Application/Benchmark/BenchmarkRunRequest.cs` | 1-82 | FS08 Benchmark and determinism |
| `Application/Benchmark/BenchmarkRunResult.cs` | 1-1270 | FS08 Benchmark and determinism |
| `Application/Benchmark/BenchmarkRunService.cs` | 1-930 | FS08 Benchmark and determinism |
| `Application/Benchmark/IBenchmarkMetricsCollector.cs` | 1-67 | FS08 Benchmark and determinism |
| `Application/Commands/BuildBookIndexCommand.cs` | 1-196 | FS07 Use-case commands and pipeline entry points |
| `Application/Commands/BuildTranscriptIndexCommand.cs` | 1-68 | FS07 Use-case commands and pipeline entry points |
| `Application/Commands/ComputeAnchorsCommand.cs` | 1-30 | FS07 Use-case commands and pipeline entry points |
| `Application/Commands/GenerateTranscriptCommand.cs` | 1-277 | FS07 Use-case commands and pipeline entry points |
| `Application/Commands/HydrateTranscriptCommand.cs` | 1-29 | FS07 Use-case commands and pipeline entry points |
| `Application/Commands/MergeTimingsCommand.cs` | 1-287 | FS07 Use-case commands and pipeline entry points |
| `Application/Commands/RunMfaCommand.cs` | 1-139 | FS07 Use-case commands and pipeline entry points |
| `Application/Mfa/MfaChunkCorpusBuilder.cs` | 1-1140 | FS06 MFA forced alignment |
| `Application/Mfa/MfaDetachedProcessRunner.cs` | 1-210 | FS06 MFA forced alignment |
| `Application/Mfa/MfaInvocationContext.cs` | 1-53 | FS06 MFA forced alignment |
| `Application/Mfa/MfaPronunciationProvider.cs` | 1-459 | FS06 MFA forced alignment |
| `Application/Mfa/MfaService.cs` | 1-202 | FS06 MFA forced alignment |
| `Application/Mfa/MfaWorkflow.cs` | 1-759 | FS06 MFA forced alignment |
| `Application/Mfa/MfaWorkspaceResolver.cs` | 1-346 | FS06 MFA forced alignment |
| `Application/Mfa/Models/MfaBeamProfile.cs` | 1-47 | FS06 MFA forced alignment |
| `Application/Mfa/Models/MfaChapterContext.cs` | 1-17 | FS06 MFA forced alignment |
| `Application/Mfa/Models/MfaCommandResult.cs` | 1-6 | FS06 MFA forced alignment |
| `Application/Mfa/PronunciationLexiconCache.cs` | 1-321 | FS06 MFA forced alignment |
| `Application/Mfa/TextGridAggregationService.cs` | 1-190 | FS06 MFA forced alignment |
| `Application/Pipeline/PipelineChapterResult.cs` | 1-276 | FS07 Use-case commands and pipeline entry points |
| `Application/Pipeline/PipelineConcurrencyControl.cs` | 1-98 | FS07 Use-case commands and pipeline entry points |
| `Application/Pipeline/PipelineRunOptions.cs` | 1-73 | FS07 Use-case commands and pipeline entry points |
| `Application/Pipeline/PipelineStage.cs` | 1-12 | FS07 Use-case commands and pipeline entry points |
| `Application/Pipeline/RecoveryTier.cs` | 1-13 | FS07 Use-case commands and pipeline entry points |
| `Application/Processes/MfaProcessSupervisor.cs` | 1-647 | FS07 Use-case commands and pipeline entry points |
| `Application/Runs/ModuleId.cs` | 1-51 | FS07 Use-case commands and pipeline entry points |
| `Application/Runs/ModuleIds.cs` | 1-12 | FS07 Use-case commands and pipeline entry points |
| `Application/Runs/RunArtifact.cs` | 1-35 | FS07 Use-case commands and pipeline entry points |
| `Application/Runs/RunFailure.cs` | 1-36 | FS07 Use-case commands and pipeline entry points |
| `Application/Runs/RunProgressUpdate.cs` | 1-114 | FS07 Use-case commands and pipeline entry points |
| `Application/Runs/RunState.cs` | 1-9 | FS07 Use-case commands and pipeline entry points |
| `Application/Validation/Models/ValidationModels.cs` | 1-74 | FS09 Validation and reporting |
| `Application/Validation/Models/ValidationReportModels.cs` | 1-40 | FS09 Validation and reporting |
| `Application/Validation/ValidationReportBuilder.cs` | 1-442 | FS09 Validation and reporting |
| `Application/Validation/ValidationService.cs` | 1-22 | FS09 Validation and reporting |
| `Artifacts/Alignment/AnchorDocument.cs` | 1-64 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/Alignment/ChunkAudioDocument.cs` | 1-49 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/Alignment/ChunkPlanDocument.cs` | 1-69 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/Alignment/Mfa/TextGridDocument.cs` | 1-7 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/AudioBuffer.cs` | 1-188 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/AudioBufferMetadata.cs` | 1-59 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/FragmentTiming.cs` | 1-6 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/Hydrate/HydratedTranscript.cs` | 1-108 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/SentenceTiming.cs` | 1-36 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/TimingOverrides.cs` | 1-60 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/TimingRange.cs` | 1-38 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/TranscriptModels.cs` | 1-55 | FS05 Alignment, timing, and artifact contracts |
| `Artifacts/WaveformPeaks.cs` | 1-8 | FS05 Alignment, timing, and artifact contracts |
| `Asr/AmsAsrModel.cs` | 1-93 | FS03 ASR transcription |
| `Asr/AsrEngine.cs` | 1-230 | FS03 ASR transcription |
| `Asr/AsrModels.cs` | 1-91 | FS03 ASR transcription |
| `Asr/AsrTranscriptBuilder.cs` | 1-104 | FS03 ASR transcription |
| `AssemblyInfo.cs` | 1-4 | FS00 Build and project surface |
| `Audio/AsrAudioPreparer.cs` | 1-113 | FS03 ASR transcription |
| `Audio/AudioDefaults.cs` | 1-23 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/AudioIntegrityVerifier.cs` | 1-474 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/AudioSpliceService.cs` | 1-390 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/AudioTreatmentService.cs` | 1-1117 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/ChapterEditModels.cs` | 1-45 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/FeatureExtraction.cs` | 1-655 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/QualityControl/AudioQcAnalyzer.cs` | 1-459 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/QualityControl/AudioQcModels.cs` | 1-43 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/SilenceChunker.cs` | 1-403 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/SpliceBoundaryService.cs` | 1-394 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/TimelineProjection.cs` | 1-81 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/TreatmentOptions.cs` | 1-62 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Audio/WaveformPeakExtractor.cs` | 1-117 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Common/AmsAppDataPaths.cs` | 1-47 | FS11 Common infrastructure |
| `Common/AmsPathResolver.cs` | 1-292 | FS11 Common infrastructure |
| `Common/ChapterLabelResolver.cs` | 1-75 | FS11 Common infrastructure |
| `Common/LevenshteinMetrics.cs` | 1-220 | FS11 Common infrastructure |
| `Common/Log.cs` | 1-160 | FS11 Common infrastructure |
| `Common/NaturalStringComparer.cs` | 1-97 | FS11 Common infrastructure |
| `Common/TextNormalizer.cs` | 1-305 | FS11 Common infrastructure |
| `GlobalUsings.cs` | 1-3 | FS00 Build and project surface |
| `Pipeline/SentenceRefinementService.cs` | 1-223 | FS05 Alignment, timing, and artifact contracts |
| `Processors/Alignment/Anchors/AnchorDiscovery.cs` | 1-213 | FS05 Alignment, timing, and artifact contracts |
| `Processors/Alignment/Anchors/AnchorPipeline.cs` | 1-106 | FS05 Alignment, timing, and artifact contracts |
| `Processors/Alignment/Anchors/AnchorPreprocessor.cs` | 1-108 | FS05 Alignment, timing, and artifact contracts |
| `Processors/Alignment/Anchors/AnchorTokenizer.cs` | 1-25 | FS05 Alignment, timing, and artifact contracts |
| `Processors/Alignment/Anchors/SectionLocator.cs` | 1-536 | FS05 Alignment, timing, and artifact contracts |
| `Processors/Alignment/Anchors/StopwordSets.cs` | 1-11 | FS05 Alignment, timing, and artifact contracts |
| `Processors/Alignment/Mfa/MfaTimingMerger.cs` | 1-485 | FS05 Alignment, timing, and artifact contracts |
| `Processors/Alignment/Mfa/TextGridParser.cs` | 1-122 | FS05 Alignment, timing, and artifact contracts |
| `Processors/Alignment/Tx/TranscriptAligner.cs` | 1-878 | FS05 Alignment, timing, and artifact contracts |
| `Processors/Alignment/Tx/WindowBuilder.cs` | 1-32 | FS05 Alignment, timing, and artifact contracts |
| `Processors/AsrProcessor.cs` | 1-1381 | FS03 ASR transcription |
| `Processors/AudioProcessor.Activity.cs` | 1-230 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Processors/AudioProcessor.Analysis.cs` | 1-586 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Processors/AudioProcessor.cs` | 1-274 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Processors/Diffing/TextDiffAnalyzer.cs` | 1-967 | FS09 Validation and reporting |
| `Processors/DocumentProcessor/DocumentProcessor.Cache.cs` | 1-26 | FS02 Book ingestion, indexing, and pronunciation |
| `Processors/DocumentProcessor/DocumentProcessor.Indexing.cs` | 1-47 | FS02 Book ingestion, indexing, and pronunciation |
| `Processors/DocumentProcessor/DocumentProcessor.Phonemes.cs` | 1-13 | FS02 Book ingestion, indexing, and pronunciation |
| `Prosody/PauseAdjustmentsDocument.cs` | 1-158 | FS10 Prosody and pause dynamics |
| `Prosody/PauseAnalysisReport.cs` | 1-47 | FS10 Prosody and pause dynamics |
| `Prosody/PauseCompressionMath.cs` | 1-245 | FS10 Prosody and pause dynamics |
| `Prosody/PauseDynamicsService.cs` | 1-924 | FS10 Prosody and pause dynamics |
| `Prosody/PauseMapBuilder.cs` | 1-514 | FS10 Prosody and pause dynamics |
| `Prosody/PauseMapModels.cs` | 1-332 | FS10 Prosody and pause dynamics |
| `Prosody/PauseModels.cs` | 1-194 | FS10 Prosody and pause dynamics |
| `Prosody/PausePolicyStorage.cs` | 1-59 | FS10 Prosody and pause dynamics |
| `Prosody/PauseTimelineApplier.cs` | 1-271 | FS10 Prosody and pause dynamics |
| `Resources/english-frequency-82k.txt` | 1-83973 | FS12 Embedded resources and model assets |
| `Runtime/Artifacts/FileArtifactResolver.cs` | 1-285 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Artifacts/IArtifactResolver.cs` | 1-62 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Audio/AppPlaybackAlertSoundService.cs` | 1-253 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Audio/AudioBufferContext.cs` | 1-94 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Audio/AudioBufferManager.cs` | 1-238 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Audio/IAppPlaybackAlertSoundService.cs` | 1-38 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Book/BookAudio.cs` | 1-492 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/BookCache.cs` | 1-302 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/BookContext.cs` | 1-39 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/BookDocuments.cs` | 1-35 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/BookIndexer.cs` | 1-1086 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/BookManager.cs` | 1-239 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/BookModels.cs` | 1-108 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/BookParser.cs` | 1-793 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/BookPhonemePopulator.cs` | 1-118 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/EnglishFrequencyDictionary.cs` | 1-74 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/IBookServices.cs` | 1-183 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/IPronunciationProvider.cs` | 1-19 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/PronunciationHelper.cs` | 1-202 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Book/ProperNounPromptFilter.cs` | 1-258 | FS02 Book ingestion, indexing, and pronunciation |
| `Runtime/Chapter/ChapterContext.cs` | 1-128 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Chapter/ChapterContextHandle.cs` | 1-91 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Chapter/ChapterDiscoveryService.cs` | 1-173 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Chapter/ChapterDocuments.cs` | 1-216 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Chapter/ChapterManager.cs` | 1-726 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Common/DelegateDocumentSlotAdapter.cs` | 1-28 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Common/DocumentSlot.cs` | 1-115 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Common/DocumentSlotOptions.cs` | 1-8 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Common/IDocumentSlotAdapter.cs` | 1-8 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Interfaces/IAudioBufferManager.cs` | 1-15 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Interfaces/IBookManager.cs` | 1-15 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Interfaces/IChapterManager.cs` | 1-31 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Workspace/IWorkspace.cs` | 1-41 | FS01 Runtime workspace and artifact lifecycle |
| `Runtime/Workspace/WorkspaceChapterDiscovery.cs` | 1-48 | FS01 Runtime workspace and artifact lifecycle |
| `Services/Alignment/AlignmentOptions.cs` | 1-29 | FS05 Alignment, timing, and artifact contracts |
| `Services/Alignment/AlignmentService.cs` | 1-55 | FS05 Alignment, timing, and artifact contracts |
| `Services/Alignment/AnchorComputeService.cs` | 1-115 | FS05 Alignment, timing, and artifact contracts |
| `Services/Alignment/ChunkPlanningService.cs` | 1-181 | FS05 Alignment, timing, and artifact contracts |
| `Services/Alignment/IAnchorComputeService.cs` | 1-22 | FS05 Alignment, timing, and artifact contracts |
| `Services/Alignment/ITranscriptHydrationService.cs` | 1-22 | FS05 Alignment, timing, and artifact contracts |
| `Services/Alignment/ITranscriptIndexService.cs` | 1-23 | FS05 Alignment, timing, and artifact contracts |
| `Services/Alignment/TranscriptHydrationService.cs` | 1-466 | FS05 Alignment, timing, and artifact contracts |
| `Services/Alignment/TranscriptIndexService.cs` | 1-457 | FS05 Alignment, timing, and artifact contracts |
| `Services/AsrService.cs` | 1-444 | FS03 ASR transcription |
| `Services/Documents/DocumentService.cs` | 1-148 | FS02 Book ingestion, indexing, and pronunciation |
| `Services/Integrations/FFmpeg/FfDecoder.cs` | 1-609 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Services/Integrations/FFmpeg/FfEncoder.cs` | 1-790 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Services/Integrations/FFmpeg/FfFilterGraph.cs` | 1-617 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Services/Integrations/FFmpeg/FfFilterGraphRunner.cs` | 1-680 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Services/Integrations/FFmpeg/FfLogCapture.cs` | 1-66 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Services/Integrations/FFmpeg/FfResampler.cs` | 1-7 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Services/Integrations/FFmpeg/FfSession.cs` | 1-366 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Services/Integrations/FFmpeg/FfUtils.cs` | 1-149 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Services/Integrations/FFmpeg/FilterSpecs.cs` | 1-67 | FS04 Audio DSP, QC, and FFmpeg integration |
| `Services/Integrations/FFmpeg/models/eng.traineddata` | 1-29678 | FS12 Embedded resources and model assets |
| `Services/Integrations/FFmpeg/models/sh.rnnn` | 1-22 | FS12 Embedded resources and model assets |
| `Services/Integrations/FFmpeg/models/silero_vad.onnx` | 1-9266 | FS12 Embedded resources and model assets |
| `Services/Interfaces/IAlignmentService.cs` | 1-24 | FS05 Alignment, timing, and artifact contracts |
| `Services/Interfaces/IAsrService.cs` | 1-25 | FS03 ASR transcription |
| `Services/Interfaces/IDocumentService.cs` | 1-61 | FS02 Book ingestion, indexing, and pronunciation |
| `Services/PipelineService.cs` | 1-815 | FS07 Use-case commands and pipeline entry points |
| `Validation/ScriptValidator.cs` | 1-416 | FS09 Validation and reporting |
| `global.json` | 1-5 | FS00 Build and project surface |

## Refresh Procedure

Use this when AMS Core changes enough that the ledger may be stale:

1. Run Code2Obsidian against `host`.
2. Compare `host/Ams.Core` filesystem files to Code2Obsidian `file_hashes`.
3. Treat the filesystem as authoritative for current file coverage.
4. Recompute line counts with `wc -l`.
5. Update slice totals and the ownership ledger.
6. Re-check dead/stale findings with `rg` across `Ams.Core`, `Ams.Cli`, `Ams.Workstation.Server`, and `Ams.Tests`.

## Reader-Test Result

A fresh reader can now:

- Pick any current AMS Core file and find its owning feature slice.
- Assign all lines in that file to a labelled home.
- Identify the current high-confidence dead-code candidate.
- Avoid trusting stale Code2Obsidian notes for removed ASR files.
- See which historical dead-code findings are already resolved.
