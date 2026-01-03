# AMS Project State

## Brief Summary

Audio Management System - CLI and core library for audio processing, ASR, forced alignment, and audiobook mastering. v2.0 adds desktop UI with GPU-native rendering.

## Current Position

**Milestone**: v2.0 Desktop UI - ON ICE
**Phase**: 8.1 - SkiaSharp vs VelloSharp Comparison POC (paused)
**Plan**: 1/2 - Exploration complete
**Status**: Desktop UI deferred - pivoting to Blazor validation viewer

## Progress

```
v1.0 Codebase Audit    [████████████████████] 100% - SHIPPED
v1.1 Execute Refactor  [████████████████████] 100% - SHIPPED
v2.0 Desktop UI        [████░░░░░░░░░░░░░░░░]  20% - In Progress (Phase 8.1: 1/2)
```

## Key Decisions (v2.0)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| VelloSharp + Avalonia | NO-GO | Child window surface creation fails |
| VelloSharp + WPF | NO-GO | VelloView composition fails |
| VelloSharp + WinUI | NO-GO | Exit code 22 |
| VelloSharp + pure Winit | WORKS | Standalone window renders perfectly |
| **Hybrid WPF+Vello** | **CONFIRMED GO** | Owned window bypasses wgpu child limitation - runtime validated |
| **Desktop UI v2.0** | **ON ICE** | Complexity too high for current needs - pivot to Blazor |

## Phase 8.1 Conclusions (POC Exploration)

**Decision: Defer desktop UI, pursue Blazor validation viewer instead**

Key findings from POC work:
1. **SkiaSharp GPU (SKGLElement)**: OpenGL context creation fails on WPF (OpenTK/SDL2 compatibility)
2. **SkiaSharp CPU (SKElement)**: Works but not GPU-accelerated
3. **VelloSharp**: Requires complex owned-window architecture due to wgpu limitations
4. **Both**: Required mipmap infrastructure for acceptable waveform performance
5. **Blazor + wavesurfer.js**: Battle-tested solution, works immediately

The effort to build a native desktop waveform viewer exceeds the value for current validation needs.

## Phase 8 Conclusions

**Architecture Validated:** WPF shell + owned Winit/Vello window

1. wgpu requires top-level windows (child windows fail surface creation)
2. Owned windows via `SetWindowLongPtr(GWL_HWNDPARENT)` work
3. Position sync on move/resize via WPF events
4. Build validates (0 errors)
5. **Runtime testing confirmed** - GPU rendering works perfectly

### POC Created

```
poc/HybridVelloPoc/
├── HybridVelloPoc.sln
└── HybridVelloPoc.Shell/
    ├── App.xaml(.cs)
    ├── MainWindow.xaml(.cs)
    └── VelloHostController.cs  (753 lines - core hosting logic)
```

## Next Action

**Blazor Validation Viewer** - Convert validation viewer to Blazor app with wavesurfer.js

Options to discuss:
1. Blazor Server (simpler, real-time connection to AMS pipeline)
2. Blazor WebAssembly (standalone, can run offline)
3. Blazor Hybrid (MAUI shell, native feel)

```
/gsd:discuss-phase blazor-validation-viewer
```

## Deferred Issues

None currently.

## Roadmap Evolution

- Phase 8.1 inserted: SkiaSharp vs VelloSharp comparison POC before committing to architecture

## Session Continuity

Last session: 2026-01-03
Stopped at: Desktop UI v2.0 put on ice after POC exploration
Decision: Pivot to Blazor validation viewer

Resume with `/gsd:discuss-phase blazor-validation-viewer` to plan new approach
