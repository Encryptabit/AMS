# AMS Project State

## Brief Summary

Audio Management System - CLI and core library for audio processing, ASR, forced alignment, and audiobook mastering. v2.0 pivoted from native desktop UI to Blazor workstation.

## Current Position

**Milestone**: v2.0 Blazor Workstation
**Phase**: 12 - Polish Area Foundation
**Plan**: 5/8 (complete)
**Status**: Plan 12-02 complete; staging queue and undo services delivered

## Progress

```
v1.0 Codebase Audit    [████████████████████] 100% - SHIPPED
v1.1 Execute Refactor  [████████████████████] 100% - SHIPPED
v2.0 Blazor Workstation[████████████████████] 100% - Phase 10 complete
```

## Phase 12 Plans (Polish Area Foundation)

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 12-01 | Audio Splice Service & Domain Models | 2 | Complete |
| 12-02 | Staging Queue & Undo Services | 2 | Complete |
| 12-03 | Polish API Endpoints | - | Pending |
| 12-04 | Waveform Region Editing | 2 | Complete |
| 12-05 | Polish Page Layout | - | Pending |
| 12-06 | Pickup Matching UI | - | Pending |
| 12-07 | Batch Operations | - | Pending |
| 12-08 | Result Verification | - | Pending |

## Phase 10 Plans

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 10-01 | Proof Backend Services | 3 | Complete |
| 10-02 | Book Overview Page | 4 | Complete |
| 10-03 | Error Patterns Aggregation | 3 | Complete |
| 10-04 | Errors View Enhancement | 3 | Complete |
| 10-05 | Review Status & Ignored Patterns | 6 | Complete |
| 10-06 | Audio Export & CRX Foundation | 3 | Complete |

## Phase 10.2 Plans (CRX Excel & API Cleanup - Complete)

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 10.2-01 | CRX Excel Integration & API Cleanup | 2 | Complete |

## Phase 10.1 Plans (INSERTED - Complete)

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 10.1-01 | AudioTreatmentService + TreatCommand | 3 | Complete |

## Phase 9 Plans

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 09-01 | Project Foundation & DI | 3 | Complete |
| 09-02 | Ams.Core Integration & Layout Shell | 6 | Complete |
| 09-03 | Waveform Component & JS Interop | 4 | Complete |
| 09-04 | Real Data Integration (Audio + Sentences) | 4 | Complete |
| 09-05 | Chapter Discovery Consolidation | 3 | Complete |
| 09-06 | Layout Lockdown (BitGrid) | 5 | Deferred |

## Future Objectives (Unscheduled)

- Keyboard Navigation & SentenceList Component enhancements
- Switchable alignment mode (DTW chunked vs MFA) for workflow-specific precision/speed tradeoffs
  - Plan: `.planning/quick/002-switchable-dtw-mfa-alignment-mode/002-PLAN.md`

## Key Decisions (v2.0)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| VelloSharp + Avalonia | NO-GO | Child window surface creation fails |
| VelloSharp + WPF | NO-GO | VelloView composition fails |
| VelloSharp + WinUI | NO-GO | Exit code 22 |
| VelloSharp + pure Winit | WORKS | Standalone window renders perfectly |
| Hybrid WPF+Vello | CONFIRMED GO | Owned window bypasses wgpu child limitation |
| **Desktop UI v2.0** | **ON ICE** | Complexity too high for current needs |
| **Blazor Server** | **CHOSEN** | Direct Ams.Core integration, wavesurfer.js for audio |
| book-index sections | sections[].title | Chapter list from sections array, not chapters |
| State persistence | LocalAppData | %LOCALAPPDATA%\AMS\workstation-state.json |
| Diff pipeline stabilization | Deterministic op-order + strict sentence ownership | Remove synthetic diff artifacts (hybrids/reorder/boundary bleed) in reviewer workflows |
| Persistence services | Singleton + LocalAppData JSON | ReviewedStatusService and IgnoredPatternsService persist per-book state |
| AggregatePatterns param type | IReadOnlySet<string> | Compatible with IgnoredPatternsService.GetIgnoredKeys() return type |
| Audio segment export | AudioProcessor.Trim + ToWavStream | FFmpeg atrim for segment extraction, sequential WAV numbering in CRX folder |
| CRX tracking format | Excel (.xlsx) via ClosedXML | Matches Python validation-viewer column layout for publisher submission |
| AudioSpliceService pattern | Static/stateless like AudioProcessor | FFmpeg acrossfade for crossfaded splicing, no DI needed |
| Crossfade clamping | 30% of shorter segment | Prevents boundary overflow per research Pitfall 2 |
| Region callback naming | OnRegionBoundsUpdated JSInvokable | Distinct from OnRegionUpdated EventCallback to avoid C# ambiguity |
| Region update event | update-end (not update) | Avoids continuous drag event overhead on Blazor circuit |
| Polish services persistence | Workspace-local .polish/ dirs | StagingQueue and UndoService persist to workspace, not AppData |
| Undo versioning | Versioned segment files + manifest.json | sent{id}.v{N}.original.wav with per-chapter JSON manifest |

## Phase 8/8.1 Conclusions (Archived)

<details>
<summary>Native Desktop UI POC Results</summary>

**Decision: Defer desktop UI, pursue Blazor workstation instead**

Key findings from POC work:
1. **SkiaSharp GPU (SKGLElement)**: OpenGL context creation fails on WPF
2. **SkiaSharp CPU (SKElement)**: Works but not GPU-accelerated
3. **VelloSharp**: Requires complex owned-window architecture due to wgpu limitations
4. **Both**: Required mipmap infrastructure for acceptable waveform performance
5. **Blazor + wavesurfer.js**: Battle-tested solution, works immediately

The effort to build a native desktop waveform viewer exceeds the value for current validation needs.

### POC Created

```
poc/HybridVelloPoc/    - WPF + owned Vello window (validated)
poc/SkiaSharpPoc/      - WPF + SkiaSharp (CPU only worked)
poc/VelloSharpPoc/     - Avalonia + VelloSharp (child window fails)
```

</details>

## Accumulated Context

### Roadmap Evolution

- Phase 13 added: Pickup Substitution

## Next Action

Plan 12-02 complete. Staging queue and undo services delivered. Ready for next pending plan.

## Deferred UI Refinements (for Plan 10-04)

- Selection flash: Active sentence should briefly flash lighter shade when selected
- Conditional left border: Only show colored border for sentences with errors

## Deferred Issues

None currently.

## Removed Projects

- `host/Ams.Web*` - Removed in favor of Ams.Workstation.Server (Blazor Server approach)

## Session Continuity

Last session: 2026-02-23 09:50 PST
Branch: `blazor-workstation`
Stopped at: Completed 12-02-PLAN.md
Note: Phase 12 plan 02 executed -- StagingQueueService and UndoService with JSON persistence and DI registration.
