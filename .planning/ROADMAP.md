# Roadmap: AMS Audio Management System

## Overview

A comprehensive Audio Management System for audio processing, ASR, forced alignment, and audiobook mastering. v1.0-1.1 established ground-truth codebase understanding and refactored to 8.0 health score. v2.0 introduces a desktop UI with GPU-native rendering.

## Domain Expertise

- ~/.claude/skills/expertise/audio-dsp/SKILL.md (audio processing patterns)
- Avalonia 12 + GPU rendering (VelloSharp/Impeller - research in Phase 8)

## Milestones

- ✅ [v1.0 AMS Codebase Audit](milestones/v1.0-ROADMAP.md) (Phases 1-4) - SHIPPED 2025-12-30
- ✅ **v1.1 Execute Refactoring** - Phases 5-7 - SHIPPED 2025-12-31
- 🚧 **v2.0 Desktop UI** - Phases 8-12 (in progress)

## Completed Milestones

<details>
<summary>v1.0 AMS Codebase Audit (Phases 1-4) - SHIPPED 2025-12-30</summary>

**Delivered:** Complete codebase understanding with 146 files analyzed, 31 issues catalogued, 6.8/10 health score, and 45-action prioritized roadmap.

### Phase 1: Discovery
- [x] 01-01: File Inventory & Project Structure (3 tasks) - completed 2025-12-28
- [x] 01-02: Call Graph Analysis (3 tasks) - completed 2025-12-28
- [x] 01-03: FFmpeg/P/Invoke Documentation (3 tasks) - completed 2025-12-28
- [x] 01-04: Module Dependency Map (3 tasks) - completed 2025-12-28

### Phase 2: Pipeline Analysis
- [x] 02-01: Pipeline Orchestration Flow (3 tasks) - completed 2025-12-28
- [x] 02-02: Data Flow & Artifacts (3 tasks) - completed 2025-12-28
- [x] 02-03: Indexing Clarification (3 tasks) - completed 2025-12-28

### Phase 3: Code Audit
- [x] 03-01: Dead Code Inventory (3 tasks) - completed 2025-12-29
- [x] 03-02: Responsibility Analysis (3 tasks) - completed 2025-12-29
- [x] 03-03: Project Audit & Synthesis (3 tasks) - completed 2025-12-29

### Phase 4: Recommendations
- [x] 04-01: Pruning & Consolidation Plans (3 tasks) - completed 2025-12-30
- [x] 04-02: Architecture Map & Action Sequencing (3 tasks) - completed 2025-12-30

</details>

<details>
<summary>✅ v1.1 Execute Refactoring (Phases 5-7) - SHIPPED 2025-12-31</summary>

**Milestone Goal:** Execute prioritized refactoring from ACTION-LIST.md to improve codebase health from 6.8 to 8.0

#### Phase 5: Immediate Cleanup
**Goal**: Delete dead code, fix warnings, archive dormant projects - quick wins with zero risk
**Depends on**: v1.0 milestone complete
**Research**: Unlikely (internal deletion/archival)
**Plans**: 3

Plans:
- [x] 05-01: Delete Dead Code (4 tasks) - Delete OverlayTest, Wn*.cs, templates, unused code - completed 2025-12-30
- [x] 05-02: Fix Warning & Remove Interface (2 tasks) - DateTime fix, IMfaService removal - completed 2025-12-30
- [x] 05-03: Archive Dormant Projects (3 tasks) - Move UI.Avalonia, InspectDocX to archive/ - completed 2025-12-30

Issues addressed: AUD-001, AUD-002, AUD-005, AUD-006, AUD-008, AUD-009, AUD-010, AUD-015, AUD-019, AUD-021, AUD-023, AUD-024, AUD-027

#### Phase 6: Utility Extraction
**Goal**: Extract shared utilities, fix failing tests, consolidate ASR buffer preparation
**Depends on**: Phase 5
**Research**: Unlikely (internal refactoring patterns)
**Plans**: 4

Plans:
- [x] 06-01: Extract ChapterLabelResolver (2 tasks) - Create utility from duplicated section resolution code - completed 2025-12-31
- [x] 06-02: Relocate & Cleanup (2 tasks) - Move MFA artifacts, remove unused AudioProcessor methods - completed 2025-12-31
- [x] 06-03: Fix FFmpeg Tests (2 tasks) - Fixed afade parameter and filter graph labels - completed 2025-12-31
- [x] 06-04: ASR Buffer Consolidation (2 tasks) - Create AsrAudioPreparer utility - completed 2025-12-31

Issues addressed: AUD-004, AUD-007, AUD-011, AUD-012, AUD-013, AUD-014, AUD-020, AUD-025

#### Phase 7: Service Decomposition
**Goal**: Split AlignmentService god class, consolidate validation, standardize Prosody patterns
**Depends on**: Phase 6 (ChapterLabelResolver required)
**Research**: Unlikely (internal refactoring)
**Plans**: 5

Plans:
- [x] 07-01: AnchorComputeService Extraction (2 tasks) - Extract anchor computation from AlignmentService - completed 2025-12-31
- [x] 07-02: TranscriptIndexService Extraction (2 tasks) - Extract transcript index building - completed 2025-12-31
- [x] 07-03: TranscriptHydrationService & Facade (2 tasks) - Complete decomposition, register DI - completed 2025-12-31
- [x] 07-04: Validation Consolidation (2 tasks) - Relocate validation files, document IBook* interfaces - completed 2025-12-31
- [x] 07-05: Prosody Standardization & Tests (2 tasks) - Standardize patterns, add test coverage - completed 2025-12-31

Issues addressed: AUD-003, AUD-016, AUD-017, AUD-018, AUD-026, AUD-031

</details>

### 🚧 v2.0 Blazor Workstation (In Progress)

**Milestone Goal:** Build a Blazor Server workstation for audiobook production with Prep/Proof/Polish workflow areas, replacing the Python validation-viewer.

<details>
<summary>Phase 8/8.1: Native Desktop UI Research (ON ICE)</summary>

#### Phase 8: GPU Rendering Research
**Goal**: Evaluate VelloSharp vs Impeller - build minimal POCs, compare performance and developer experience
**Status**: Complete - informed decision to pivot to Blazor

Plans:
- [x] 08-01: VelloSharp POC - NO-GO for .NET UI integrations - completed 2025-12-31
- [x] 08-02: Hybrid Window POC - WPF + owned Vello window validates - completed 2026-01-01

#### Phase 8.1: SkiaSharp vs VelloSharp Comparison POC
**Goal**: Compare GPU rendering approaches
**Status**: Paused - Blazor pivot decision made

Plans:
- [x] 08.1-01: SkiaSharp Waveform POC - completed 2026-01-03
- [-] 08.1-02: Comparison & Decision - skipped (pivoted to Blazor)

</details>

#### Phase 9: Blazor Audiobook Workstation
**Goal**: Build Blazor Server workstation with Prep/Proof/Polish architecture, port validation-viewer to Proof area
**Depends on**: v1.1 milestone complete
**Research**: Complete (09-RESEARCH.md)
**Plans**: 5

Plans:
- [x] 09-01: Project Foundation & DI (3 tasks) - Create Blazor Server project, configure Ams.Core integration - completed 2026-01-04
- [x] 09-02: Ams.Core Integration & Layout Shell (6 tasks) - BlazorWorkspace, section loading, persistent state, navigation - completed 2026-01-03
- [x] 09-03: Waveform Component & JS Interop (4 tasks) - wavesurfer.js integration, WaveformPlayer component - completed 2026-01-04
- [x] 09-04: Real Data Integration (4 tasks) - Audio streaming from AudioBuffer, sentences from HydratedTranscript - completed 2026-01-04
- [x] 09-05: Chapter Discovery Consolidation (3 tasks) - ChapterDiscoveryService in Ams.Core, BlazorWorkspace refactor - completed 2026-01-08

#### Phase 10: Proof Feature Parity
**Goal**: Complete validation-viewer feature parity in Proof area - overview, error patterns, diff visualization, review tracking, CRX export
**Depends on**: Phase 9
**Research**: Complete (comprehensive validation-viewer analysis)
**Plans**: 6

Plans:
- [x] 10-01: Proof Backend Services (3 tasks) - ValidationMetricsService, ProofReportService, ProofApiController - completed 2026-01-09

#### Phase 10.1: Audio Treatment (INSERTED)
**Goal**: Create audio treatment pipeline for production-ready chapter WAV files with roomtone spacing
**Depends on**: Phase 10-01 complete
**Research**: None (uses existing FFmpeg/AudioProcessor patterns)
**Plans**: 1
**Status**: Complete

Plans:
- [x] 10.1-01: Audio Treatment Service & CLI (3 tasks) - AudioTreatmentService, TreatCommand, verification - completed 2026-02-02

#### Phase 10.1.1: Treatment Optimization (INSERTED)
**Goal**: Optimize AudioTreatmentService to use in-memory buffer manipulation, remove external FFmpeg process and temp files
**Depends on**: Phase 10.1 complete
**Research**: None (uses existing FfFilterGraph/AudioBuffer patterns)
**Plans**: 1
**Status**: Complete

Plans:
- [x] 10.1.1-01: In-memory buffer concatenation & service refactor (2 tasks) - completed 2026-02-02

**Optimization targets:**
- Remove external FFmpeg process spawning (use internal FfFilterGraph)
- Remove temp file artifacts (no intermediate WAV files)
- In-memory buffer concatenation
- Single encode to disk at the end

#### Phase 10 (continued): Proof Feature Parity
- [x] 10-02: Book Overview Page (4 tasks) - Stats grid, chapter cards, navigation - completed 2026-02-02
- [x] 10-03: Error Patterns Aggregation (3 tasks) - ErrorPatternService, aggregation endpoint, patterns page - completed 2026-02-17
- [x] 10-04: Errors View Enhancement (3 tasks) - WER-sorted sentences, diff visualization, error cards - completed 2026-02-17
- [x] 10-05: Review Status & Ignored Patterns (3 tasks) - Persistence services, API endpoints, UI updates - completed 2026-02-22
- [x] 10-06: Audio Export & CRX Foundation (3 tasks) - Segment export, CRX service, CRX modal with diff-based comment gen - completed 2026-02-23

#### Phase 10.2: CRX Excel Integration & API Cleanup (INSERTED)
**Goal**: Replace JSON CRX tracking with Excel-based workflow matching validation-viewer (BASE_CRX.xlsx template, openpyxl-equivalent append), remove unused ProofApiController
**Depends on**: Phase 10-06 complete
**Research**: None (validation-viewer CRX workflow fully analyzed)
**Plans**: 1
**Status**: Not started

Plans:
- [ ] 10.2-01-PLAN.md -- Excel CRX persistence (ClosedXML) + ProofApiController removal

#### Phase 11: Prep Area Implementation
**Goal**: Pipeline orchestration UI - run ASR, alignment, MFA from workstation
**Depends on**: Phase 10
**Research**: Unlikely (using existing PipelineService)
**Plans**: TBD

#### Phase 12: Polish Area Foundation
**Goal**: Take replacement workflow with pickup import/ASR matching/crossfade splice, batch editing foundations (rename, shift, pre/post roll), multi-waveform stacked view, non-destructive staging queue with undo, and post-replacement verification with Proof sync
**Depends on**: Phase 11
**Research**: Complete (12-RESEARCH.md)
**Plans**: 8 plans
**Requirements:** REQ-TAKE, REQ-BATCH, REQ-MULTI, REQ-SPLICE, REQ-VERIFY, REQ-UNDO, REQ-STAGE

Plans:
- [ ] 12-01-PLAN.md -- AudioSpliceService (crossfade via FFmpeg acrossfade) + Polish domain models
- [ ] 12-02-PLAN.md -- StagingQueueService + UndoService (non-destructive staging + versioned segment backup)
- [ ] 12-03-PLAN.md -- PickupMatchingService (ASR-based via Whisper.NET) + PolishService orchestrator
- [ ] 12-04-PLAN.md -- Waveform interop extensions (draggable regions, playhead sync, segment playback)
- [ ] 12-05-PLAN.md -- Polish UI: landing page, ChapterPolish view, PickupImporter, StagingQueue components
- [ ] 12-06-PLAN.md -- MultiWaveformView + BatchEditor page + BatchOperationService
- [ ] 12-07-PLAN.md -- PolishVerificationService (auto re-validate, listen-with-context, Proof sync)
- [ ] 12-08-PLAN.md -- End-to-end verification checkpoint

#### Phase 12.1: MFA Refinement for Pickup Timings (INSERTED)
**Goal**: Run pickup audio through MFA forced alignment to get phoneme-accurate sentence boundaries, replacing rough ASR-based timing estimates. Reuses existing MfaWorkflow/MfaProcessSupervisor infrastructure.
**Depends on**: Phase 12 (PickupMatchingService, single-pass ASR)
**Research**: None (existing MFA infra fully understood)
**Plans**: 1 plan

Plans:
- [ ] 12.1-01-PLAN.md -- PickupMfaRefinementService + PickupMatchingService integration

### Quick Plans (Ad-hoc)

- [x] quick-001: Simplify alignment/diff pipeline errors (deterministic UI diff rendering, glue-heuristic removal, stricter sentence ownership) - completed 2026-02-17

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → ... → 12

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1. Discovery | v1.0 | 4/4 | Complete | 2025-12-28 |
| 2. Pipeline Analysis | v1.0 | 3/3 | Complete | 2025-12-28 |
| 3. Code Audit | v1.0 | 3/3 | Complete | 2025-12-29 |
| 4. Recommendations | v1.0 | 2/2 | Complete | 2025-12-30 |
| 5. Immediate Cleanup | v1.1 | 3/3 | Complete | 2025-12-30 |
| 6. Utility Extraction | v1.1 | 4/4 | Complete | 2025-12-31 |
| 7. Service Decomposition | v1.1 | 5/5 | Complete | 2025-12-31 |
| 8. GPU Rendering Research | v2.0 | 2/2 | Complete (on ice) | 2026-01-01 |
| 8.1. SkiaSharp vs VelloSharp POC | v2.0 | 1/2 | Paused (pivoted) | 2026-01-03 |
| 9. Blazor Workstation | v2.0 | 5/5 | Complete | 2026-01-08 |
| 10. Proof Feature Parity | v2.0 | 6/6 | Complete | 2026-02-23 |
| 10.1. Audio Treatment | v2.0 | 1/1 | Complete | 2026-02-02 |
| 10.1.1. Treatment Optimization | v2.0 | 1/1 | Complete | 2026-02-02 |
| 10.2. CRX Excel & Cleanup | v2.0 | Complete    | 2026-02-23 | - |
| 11. Prep Area Implementation | v2.0 | 0/? | Not started | - |
| 12. Polish Area Foundation | v2.0 | Complete    | 2026-02-24 | - |
| 12.1. MFA Pickup Refinement | v2.0 | Complete    | 2026-02-24 | - |

### Phase 13: Pickup Substitution

**Goal:** [To be planned]
**Depends on:** Phase 12
**Plans:** 8/8 plans complete

Plans:
- [ ] TBD (run /gsd:plan-phase 13 to break down)
