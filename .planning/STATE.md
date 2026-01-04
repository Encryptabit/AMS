# AMS Project State

## Brief Summary

Audio Management System - CLI and core library for audio processing, ASR, forced alignment, and audiobook mastering. v2.0 pivoted from native desktop UI to Blazor workstation.

## Current Position

**Milestone**: v2.0 Blazor Workstation
**Phase**: 9 - Blazor Audiobook Workstation
**Plan**: 1/4 - In progress
**Status**: Executing phase plans

## Progress

```
v1.0 Codebase Audit    [████████████████████] 100% - SHIPPED
v1.1 Execute Refactor  [████████████████████] 100% - SHIPPED
v2.0 Blazor Workstation[███░░░░░░░░░░░░░░░░░]  15% - Phase 9 executing (1/4 plans)
```

## Phase 9 Plans

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 09-01 | Project Foundation & DI | 3 | Complete |
| 09-02 | Layout Shell & Navigation | 4 | Pending |
| 09-03 | Waveform Component & JS Interop | 4 | Pending |
| 09-04 | Proof Area & Sentence List | 4 | Pending |

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

Execute Phase 9 Plan 2:
```
/gsd:execute-plan .planning/phases/09-blazor-workstation/09-02-PLAN.md
```

## Deferred Issues

None currently.

## Removed Projects

- `host/Ams.Web*` - Removed in favor of Ams.Workstation.Server (Blazor Server approach)

## Session Continuity

Last session: 2026-01-04
Branch: `blazor-workstation`
Status: Phase 9 Plan 1 complete, ready for 09-02-PLAN.md
