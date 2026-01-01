# Phase 8 Discovery: GPU Rendering Research

**Date:** 2025-12-31
**Depth:** Level 2 (Standard Research)

## Research Question

Which GPU rendering technology should AMS v2.0 use for its desktop UI: VelloSharp, ImpellerSharp, or another approach?

## Findings

### Option A: VelloSharp

**Status:** v0.5.0-alpha.1 (Released October 11, 2025)

**What it is:** .NET bindings for Vello, a GPU compute-centric 2D renderer built on wgpu.

**Avalonia Integration:**
- `VelloSharp.Avalonia.Vello` — Avalonia UI integration layer
- `VelloSharp.Avalonia.Winit` — Avalonia windowing interop helpers
- `VelloSharp.Avalonia.Controls` — Reusable controls (VelloCanvasControl, VelloAnimatedCanvasControl, VelloSvgControl)

**Performance Claims:**
- Up to 100x faster than SkiaSharp in certain workloads
- 8x improvement even through Skia-compatibility shim
- Tens of thousands of animated vector paths at 120 FPS

**Backend:** wgpu with DX12, Vulkan, Metal auto-negotiated at runtime

**Pros:**
- Most mature .NET GPU renderer available today
- First-class Avalonia support with dedicated packages
- Rust-based Vello is actively developed by Linebender
- wgpu backend handles cross-platform GPU abstraction

**Cons:**
- Alpha status - APIs may change
- Requires wgpu-native binaries bundled with app
- Less community adoption (new technology)

**Source:** [VelloSharp GitHub](https://github.com/wieslawsoltes/VelloSharp)

### Option B: ImpellerSharp

**Status:** Active R&D (APIs may change)

**What it is:** .NET bindings for Flutter's Impeller renderer, with official Avalonia partnership.

**Avalonia Integration:**
- Dedicated NuGet packages for macOS (Metal), Windows, Linux
- SafeHandle-first interop for predictable lifetimes
- Backend flexibility: Metal, OpenGL(ES), Vulkan

**Partnership:**
- Avalonia team partnering directly with Google's Flutter engineers
- Chinmay Garde (Impeller creator) actively collaborating
- Early Avalonia backend already running on Impeller

**Pros:**
- Official Google/Avalonia partnership suggests long-term viability
- Impeller proven in Flutter ecosystem (default on iOS/Android in 2025)
- Precompiled shaders eliminate jank

**Cons:**
- Earlier stage than VelloSharp
- APIs explicitly marked as unstable
- Windows support via OpenGL(ES) or Vulkan (no DX12)

**Source:** [ImpellerSharp GitHub](https://github.com/wieslawsoltes/ImpellerSharp), [Avalonia Blog](https://avaloniaui.net/blog/avalonia-partners-with-google-s-flutter-t-eam-to-bring-impeller-rendering-to-net)

### Option C: Direct wgpu-native (WGPU.NET)

**Status:** v0.17.0.2 (Last updated August 2023)

**What it is:** Raw .NET bindings for wgpu-native, lower-level than VelloSharp.

**Pros:**
- Maximum control over GPU rendering
- Could build custom 2D renderer

**Cons:**
- Stale (2023), may not target latest wgpu-native
- Much more work - would need to build 2D primitives ourselves
- VelloSharp already provides this layer with 2D abstractions

**Recommendation:** Skip - VelloSharp already wraps wgpu with proper 2D rendering.

**Source:** [WGPU.NET GitHub](https://github.com/Trivaxy/WGPU.NET)

## Avalonia 12 Context

From [Avalonia's rendering roadmap](https://avaloniaui.net/blog/the-future-of-avalonia-s-rendering):
- SkiaSharp remains default in v12
- Experimental GPU-first rendering options introduced in v12
- Vello showing "particularly interesting results"
- Long-term transition planned, not immediate replacement

## Recommendation

**Primary evaluation: VelloSharp**
- More mature with concrete Avalonia packages
- Better documented integration path
- Proven 2D rendering primitives

**Secondary evaluation: ImpellerSharp**
- Worth monitoring due to official partnership
- May catch up or surpass VelloSharp
- Evaluate if VelloSharp proves insufficient

## POC Approach

1. **VelloSharp POC** - Build minimal Avalonia app with VelloCanvasControl
2. **Performance baseline** - Render test patterns, measure FPS
3. **Audio viz test** - Render waveform-like graphics at 60Hz
4. **Document findings** - DX pain points, performance, stability

If VelloSharp works well, defer ImpellerSharp evaluation to future phase.
