# AMS Project State

## Brief Summary

Audio Management System - CLI and core library for audio processing, ASR, forced alignment, and audiobook mastering. v2.0 pivoted from native desktop UI to Blazor workstation.

## Current Position

**Milestone**: v2.0 Blazor Workstation
**Phase**: 10 - Proof Feature Parity
**Plan**: 5/6 (complete)
**Status**: Plan 10-05 complete; awaiting human verification checkpoint

## Progress

```
v1.0 Codebase Audit    [████████████████████] 100% - SHIPPED
v1.1 Execute Refactor  [████████████████████] 100% - SHIPPED
v2.0 Blazor Workstation[██████████████████░░]  83% - Plan 10-05 complete
```

## Phase 10 Plans

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 10-01 | Proof Backend Services | 3 | Complete |
| 10-02 | Book Overview Page | 4 | Complete |
| 10-03 | Error Patterns Aggregation | 3 | Complete |
| 10-04 | Errors View Enhancement | 3 | Complete |
| 10-05 | Review Status & Ignored Patterns | 6 | Complete |
| 10-06 | Audio Export & CRX Foundation | 3 | Pending |

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

## Next Action

Plan 10-05 complete. Human verification checkpoint pending.
After verification, continue with Plan 10-06 (Audio Export & CRX Foundation):
```
/gsd:execute-plan .planning/phases/10-proof-feature-parity/10-06-PLAN.md
```

## Deferred UI Refinements (for Plan 10-04)

- Selection flash: Active sentence should briefly flash lighter shade when selected
- Conditional left border: Only show colored border for sentences with errors

## Deferred Issues

None currently.

## Removed Projects

- `host/Ams.Web*` - Removed in favor of Ams.Workstation.Server (Blazor Server approach)

## Session Continuity

Last session: 2026-02-22 16:10 PST
Branch: `blazor-workstation`
Status: Plan 10-05 complete (Review Status & Ignored Patterns persistence)
Note: Plans 10-03 through 10-05 complete. Persistence services for reviewed chapters and ignored patterns now wired into Blazor pages with direct service injection.
