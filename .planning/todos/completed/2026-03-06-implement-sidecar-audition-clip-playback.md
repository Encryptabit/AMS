---
created: 2026-03-06T06:30:55.209Z
title: Implement sidecar audition clip playback
area: ui
files:
  - host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor:789
  - host/Ams.Workstation.Server/Services/PolishService.cs:248
  - host/Ams.Workstation.Server/Components/Shared/WaveformPlayer.razor:331
  - host/Ams.Workstation.Server/wwwroot/js/waveform-interop.js:528
---

## Problem

On `/polish`, staged-item audition currently blocks on `PolishService.GeneratePreview(...)`, which performs a full chapter splice before playback begins. Logs show about 7-8 seconds between boundary refinement and audition start. The generated preview buffer is also not the active waveform source during `PlaySegment`, so we pay heavy precompute cost with little immediate playback benefit.

## Solution

Implement a sidecar audition clip path:
- Keep chapter waveform audio loaded (`/api/audio/chapter/.../corrected`) and unchanged.
- Build a short audition clip around splice boundaries (context before + pickup + context after) using FFmpeg-backed splice operations.
- Play that clip through a separate hidden player/sidecar audio path so audition starts quickly without reloading the chapter WaveSurfer instance.
- Ensure staged audition and committed audition behaviors remain explicit and deterministic.
