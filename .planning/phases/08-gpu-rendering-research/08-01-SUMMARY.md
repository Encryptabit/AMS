# Phase 8 Plan 1: VelloSharp POC Summary

**VelloSharp POC reveals critical Windows issues - NO-GO, recommend SkiaSharp GPU path**

## Accomplishments

- Created POC project with VelloSharp.Avalonia integration
- Tested VelloCanvasControl and VelloAnimatedCanvasControl
- Implemented waveform/spectrogram test patterns
- Discovered critical animation and threading issues on Windows
- Documented findings with clear recommendation

## Files Created/Modified

- `poc/VelloSharpPoc/` - Complete POC project
- `poc/VelloSharpPoc/FINDINGS.md` - Detailed research results
- `.planning/phases/08-gpu-rendering-research/08-01-SUMMARY.md` - This summary

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| **NO-GO on VelloSharp** | Animation doesn't work on Windows; Winit has threading bug |
| **Recommend SkiaSharp GPU** | Mature, stable, already integrated with Avalonia |
| **Defer VelloSharp to v3.0+** | Wait for library maturity (0.6+ or 1.0 release) |

## Issues Encountered

1. **Animation broken**: VelloAnimatedCanvasControl only redraws on window resize
2. **Winit threading crash**: `InvalidOperationException: Call from invalid thread`
3. **Version lock**: VelloSharp requires exact Avalonia 11.3.6
4. **No Avalonia 12 support**: VelloSharp incompatible with Avalonia 12 nightly

## NImpeller Also Evaluated

- Raw .NET bindings for Flutter's Impeller - no Avalonia integration
- No NuGet packages, Linux-only testing
- Avalonia 12 will have official Impeller support (not yet in nightly)

## Technical Details

- VelloSharp 0.5.0-alpha.3 built against Avalonia 11.3.6
- InvalidateVisual() does not trigger redraws
- IsPlaying/FrameRate properties have no effect without Winit
- Static rendering works; continuous animation does not

## Next Steps

**Phase 9 should proceed with standard Avalonia + SkiaSharp:**

1. Use Avalonia 11.3.x (stable) or evaluate 12.x nightly cautiously
2. Leverage SkiaSharp's GPU backend for performance
3. Consider custom compute shaders only if SkiaSharp proves insufficient
4. Archive VelloSharp POC for future reference

## Time Spent

~2 hours debugging VelloSharp integration issues
