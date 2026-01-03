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

### 🚧 v2.0 Desktop UI (In Progress)

**Milestone Goal:** Build a desktop application using Avalonia 12 with GPU-native rendering (Vello or Impeller), connecting to the existing AMS pipeline.

#### Phase 8: GPU Rendering Research
**Goal**: Evaluate VelloSharp vs Impeller - build minimal POCs, compare performance and developer experience
**Depends on**: v1.1 milestone complete
**Research**: Likely (new tech, fast-moving APIs)
**Research topics**: VelloSharp/Avalonia integration, Impeller .NET bindings status, wgpu setup on Windows, GPU memory management
**Plans**: 2

Plans:
- [x] 08-01: VelloSharp POC (4 tasks) - NO-GO for .NET UI integrations, pure Winit works - completed 2025-12-31
- [x] 08-02: Hybrid Window POC (4 tasks) - WPF shell + owned Winit/Vello window validates - completed 2026-01-01

#### Phase 8.1: SkiaSharp vs VelloSharp Comparison POC
**Goal**: Compare SkiaSharp GPU-accelerated rendering against the validated WPF+VelloSharp owned window approach to make final renderer decision before committing to architecture
**Depends on**: Phase 8
**Research**: Likely (SkiaSharp GPU backends, performance benchmarking)
**Research topics**: SkiaSharp hardware acceleration on Windows, GPU backend configuration (OpenGL/Vulkan/D3D), performance comparison methodology
**Plans**: 2

Plans:
- [x] 08.1-01: SkiaSharp Waveform POC (3 tasks) - Build GPU-accelerated waveform renderer - completed 2026-01-03
- [ ] 08.1-02: Comparison & Decision (3 tasks) - Side-by-side comparison, make renderer choice

#### Phase 9: Avalonia 12 Foundation
**Goal**: Set up Avalonia 12 project with chosen GPU renderer, basic application shell, DI integration with Ams.Core
**Depends on**: Phase 8.1
**Research**: Likely (Avalonia 12 preview patterns, renderer integration)
**Research topics**: Avalonia 12 preview APIs, DI patterns, renderer configuration
**Plans**: TBD

Plans:
- [ ] 09-01: TBD

#### Phase 10: Core UI Components
**Goal**: Book/chapter navigation, pipeline status display, audio waveform visualization shell
**Depends on**: Phase 9
**Research**: Unlikely (internal UI patterns)
**Plans**: TBD

Plans:
- [ ] 10-01: TBD

#### Phase 11: Pipeline Integration
**Goal**: Wire existing Ams.Core commands to UI, real-time pipeline progress, chapter context management
**Depends on**: Phase 10
**Research**: Unlikely (using existing services)
**Plans**: TBD

Plans:
- [ ] 11-01: TBD

#### Phase 12: Audio Visualization
**Goal**: GPU-rendered waveform display, spectrogram visualization, playback position synchronization
**Depends on**: Phase 11
**Research**: Likely (GPU rendering for real-time audio viz)
**Research topics**: GPU buffer management for audio data, real-time waveform rendering, spectrogram computation
**Plans**: TBD

Plans:
- [ ] 12-01: TBD

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
| 8. GPU Rendering Research | v2.0 | 2/2 | Complete | 2026-01-01 |
| 8.1. SkiaSharp vs VelloSharp POC | v2.0 | 1/2 | In progress | - |
| 9. Avalonia 12 Foundation | v2.0 | 0/? | Not started | - |
| 10. Core UI Components | v2.0 | 0/? | Not started | - |
| 11. Pipeline Integration | v2.0 | 0/? | Not started | - |
| 12. Audio Visualization | v2.0 | 0/? | Not started | - |
