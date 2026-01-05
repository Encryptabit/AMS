# AMS Project State

## Brief Summary

Audio Management System - CLI and core library for audio processing, ASR, forced alignment, and audiobook mastering. v2.0 pivoted from native desktop UI to Blazor workstation.

## Current Position

**Milestone**: v2.0 Blazor Workstation
**Phase**: 9 - Blazor Audiobook Workstation
**Plan**: 4/4 - Complete
**Status**: Phase complete

## Progress

```
v1.0 Codebase Audit    [████████████████████] 100% - SHIPPED
v1.1 Execute Refactor  [████████████████████] 100% - SHIPPED
v2.0 Blazor Workstation[██████████░░░░░░░░░░]  50% - Phase 9 complete
```

## Phase 9 Plans

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 09-01 | Project Foundation & DI | 3 | Complete |
| 09-02 | Ams.Core Integration & Layout Shell | 6 | Complete |
| 09-03 | Waveform Component & JS Interop | 4 | Complete |
| 09-04 | Real Data Integration (Audio + Sentences) | 4 | Complete |
| 09-05 | Keyboard Nav & SentenceList Component | - | Pending |
| 09-06 | Layout Lockdown (BitGrid) | 5 | Deferred |

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

Phase 9 complete. Plan next phase:
```
/gsd:plan-phase 10
```

## Deferred Issues

None currently.

## Removed Projects

- `host/Ams.Web*` - Removed in favor of Ams.Workstation.Server (Blazor Server approach)

## Session Continuity

Last session: 2026-01-04
Branch: `blazor-workstation`
Status: Phase 9 complete (all 4 plans executed)
Note: Real data integration working - audio streams from AudioBuffer, sentences from HydratedTranscript
