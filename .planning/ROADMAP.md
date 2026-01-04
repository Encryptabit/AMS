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
**Plans**: 4

Plans:
- [x] 09-01: Project Foundation & DI (3 tasks) - Create Blazor Server project, configure Ams.Core integration - completed 2026-01-04
- [x] 09-02: Ams.Core Integration & Layout Shell (6 tasks) - BlazorWorkspace, section loading, persistent state, navigation - completed 2026-01-03
- [x] 09-03: Waveform Component & JS Interop (4 tasks) - wavesurfer.js integration, WaveformPlayer component - completed 2026-01-04
- [ ] 09-03.2: Layout Lockdown (TBD tasks) - BitGrid adoption for all pages and components (INSERTED)
- [ ] 09-04: Proof Area & Sentence List (4 tasks) - Chapter review workflow with keyboard shortcuts

#### Phase 10: Ams.Core Data Integration
**Goal**: Connect workstation to real Ams.Core data - load books, chapters, hydrate.json, audio files
**Depends on**: Phase 9
**Research**: Unlikely (using existing Ams.Core types)
**Plans**: TBD

#### Phase 11: Prep Area Implementation
**Goal**: Pipeline orchestration UI - run ASR, alignment, MFA from workstation
**Depends on**: Phase 10
**Research**: Unlikely (using existing PipelineService)
**Plans**: TBD

#### Phase 12: Polish Area Foundation
**Goal**: Take replacement workflow, batch editing foundations
**Depends on**: Phase 11
**Research**: Unlikely (extending Proof patterns)
**Plans**: TBD

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
| 9. Blazor Workstation | v2.0 | 3/4 | In progress | - |
| 10. Ams.Core Data Integration | v2.0 | 0/? | Not started | - |
| 11. Prep Area Implementation | v2.0 | 0/? | Not started | - |
| 12. Polish Area Foundation | v2.0 | 0/? | Not started | - |
