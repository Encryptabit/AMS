# VelloSharp POC Findings

**Date:** 2025-12-31
**Verdict:** NO-GO for Windows/Avalonia

## Executive Summary

VelloSharp 0.5.0-alpha.3 is **not ready for production use** on Windows with Avalonia. The library has fundamental integration issues that prevent continuous animation - a core requirement for audio visualization.

## Test Configuration

- **Avalonia:** 11.3.6 (matches VelloSharp's build target)
- **VelloSharp:** 0.5.0-alpha.3
- **Platform:** Windows 11, .NET 9.0
- **GPU:** (user's system)

## Issues Discovered

### 1. Animation Does Not Work (Critical)

**Symptom:** VelloAnimatedCanvasControl only redraws when the window is resized. Setting `IsPlaying="True"` and `FrameRate="60"` has no effect.

**Root Cause:** The control's internal animation loop (either composition-based or DispatcherTimer fallback) does not trigger on Windows without the Winit backend.

**Workaround Attempted:** Manual `DispatcherTimer` calling `InvalidateVisual()` every 16ms - **did not work**. The control ignores invalidation requests.

### 2. Winit Backend Has Threading Bug (Critical)

**Symptom:** Using `.UseWinit()` crashes immediately with:
```
System.InvalidOperationException: Call from invalid thread
   at Avalonia.Threading.Dispatcher.VerifyAccess()
   at Avalonia.Rendering.RenderLoop.Add(IRenderLoopTask i)
```

**Root Cause:** VelloSharp.Avalonia.Winit initializes the compositor from the wrong thread on Windows.

### 3. Version Compatibility Issues (Moderate)

- VelloSharp 0.5.0-alpha.3 is built against Avalonia 11.3.6
- Using Avalonia 11.3.10 causes `TypeLoadException` (API changes in IClipboard, IFontManager)
- VelloSharp does **not** support Avalonia 12.x (nightly)

## What Works

- Static rendering works (shapes appear on initial draw and resize)
- Path building API (`PathBuilder`, `FillPath`, `StrokePath`) is functional
- GPU rendering appears active (not CPU fallback)
- Color/brush APIs work as documented

## Performance (Limited Data)

Due to animation issues, proper FPS measurement was not possible:
- FPS counter showed 1-2 FPS (only updating on resize events)
- When forcing redraws via resize, rendering appeared smooth
- No GPU performance profiling was conducted

## NImpeller Evaluation

NImpeller (github.com/AvaloniaUI/NImpeller) was also evaluated:

| Aspect | Status |
|--------|--------|
| What is it | Raw .NET bindings for Flutter's Impeller engine |
| Avalonia integration | **None** - SDL-based samples only |
| NuGet packages | Not published |
| Platform testing | Linux only (Intel iGPU) |
| Production ready | No - experimental, bindings regenerate per-build |

**Key insight:** Avalonia is officially partnering with Google/Flutter on Impeller. Avalonia 12 will introduce experimental GPU-first rendering options including Impeller. However, this is not yet available in nightly builds.

## Recommendation

### For Phase 9 (Avalonia Foundation)

**Option A: Use Standard Avalonia 11.x + SkiaSharp GPU (Recommended)**
- Avalonia's built-in Skia renderer is mature and stable
- SkiaSharp supports GPU acceleration via D3D11/Vulkan/Metal
- Lowest risk, proven technology
- Can upgrade to Impeller when Avalonia 12 stabilizes

**Option B: Track Avalonia 12 + Impeller**
- Monitor Avalonia 12 nightly for Impeller experimental support
- Re-evaluate when UseImpeller() or equivalent appears
- Official Avalonia team is working on this

**Option C: Wait for VelloSharp Maturity**
- Monitor VelloSharp GitHub for Windows fixes
- Re-evaluate when version 0.6+ or 1.0 releases
- Winit threading issue needs upstream fix

### Suggested Path Forward

Proceed with **Option A** for v2.0 milestone. Use Avalonia 11.x with standard SkiaSharp rendering. GPU acceleration for audio visualization can be achieved via:
1. SkiaSharp's GPU backend (already available)
2. Custom compute shaders if needed for spectrogram/heavy workloads
3. Re-evaluate VelloSharp for v3.0 when it matures

## Files Created

- `poc/VelloSharpPoc/` - Complete POC project (can be deleted or archived)
- `poc/VelloSharpPoc/FINDINGS.md` - This document

## References

- [VelloSharp GitHub](https://github.com/wieslawsoltes/VelloSharp)
- [Avalonia NuGet](https://www.nuget.org/packages/Avalonia)
- [Avalonia Nightly Feed](https://nuget-feed-nightly.avaloniaui.net/v3/index.json)
