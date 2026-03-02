---
phase: quick-5
plan: 01
subsystem: audio-infrastructure
tags: [audiobuffer, refactor, memory, zero-copy, slice]
dependency_graph:
  requires: []
  provides: [AudioBuffer.Slice, AudioDefaults, contiguous-backing]
  affects: [AsrProcessor, FeatureExtraction, AsrAudioPreparer, AudioSpliceService, FfDecoder, FfEncoder, FfFilterGraphRunner, AudioProcessor.Analysis, PipelineCommand, AudioController]
tech_stack:
  added: []
  patterns: [Memory<float>-views, Span-based-copies, CollectionsMarshal.AsSpan, contiguous-backing-store]
key_files:
  created:
    - host/Ams.Core/Audio/AudioDefaults.cs
    - host/Ams.Tests/AudioBufferSliceTests.cs
  modified:
    - host/Ams.Core/Artifacts/AudioBuffer.cs
    - host/Ams.Core/Processors/AsrProcessor.cs
    - host/Ams.Core/Audio/FeatureExtraction.cs
    - host/Ams.Core/Audio/AsrAudioPreparer.cs
    - host/Ams.Core/Audio/AudioSpliceService.cs
    - host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs
    - host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs
    - host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs
    - host/Ams.Core/Processors/AudioProcessor.Analysis.cs
    - host/Ams.Cli/Commands/PipelineCommand.cs
    - host/Ams.Workstation.Server/Controllers/AudioController.cs
    - host/Ams.Tests/WavIoTests.cs
    - host/Ams.Tests/AudioProcessorFilterTests.cs
decisions:
  - Pin single contiguous backing array once in FfEncoder instead of per-channel pinning
  - Use CollectionsMarshal.AsSpan for zero-copy List<float> to Span copies in FfDecoder/FfFilterGraphRunner
  - AudioDefaults with -55dB threshold and 200ms minimum silence duration
metrics:
  duration: 6m
  completed: 2026-03-02T02:56:00Z
  tasks: 3/3
  files_modified: 15
  tests_added: 8
  tests_passing: 97
---

# Phase quick-5 Plan 01: AudioBuffer Contiguous Backing + Memory<float> Slicing Summary

Contiguous float[] backing store with Memory<float> channel views, zero-copy Slice(), and shared AudioDefaults for silence detection constants.

## What Changed

### AudioBuffer Refactor (Task 1)
- Replaced `float[][] Planar` with single contiguous `float[]` backing array
- Channel data accessed via `Memory<float>` slices at stride offsets (`ch * strideLength + offset`)
- `GetChannel(ch)` returns `ReadOnlyMemory<float>` (public, prevents external mutation)
- `GetChannelSpan(ch)` returns writable `Span<float>` (internal, for producers)
- `Slice(startSample, length)` returns zero-copy view sharing parent backing
- `Slice(TimeSpan, TimeSpan)` time-based overload
- `GetBackingArray()` / `GetChannelOffset(ch)` for FFmpeg pinning interop
- Per-sample indexer `buffer[channel, sample]` for convenient access
- Constructor now validates: channels > 0, sampleRate > 0, length >= 0
- `Concat()` updated to span-based copies (resolving the existing TODO)

### Consumer Migration (Task 2)
All 14 consumer files migrated from `.Planar[ch]` to new API:

| File | Pattern | Migration |
|------|---------|-----------|
| AsrProcessor | Array.Copy, loop reads | GetChannelSpan, GetChannel().Span |
| FeatureExtraction | Planar.Length, Planar[0], loop | Channels, GetChannel().ToArray(), .Span |
| AsrAudioPreparer | Direct index write/read | GetChannelSpan, GetChannel().Span |
| AudioSpliceService | Array.Copy loop | Span.CopyTo + zero-length guard |
| FfDecoder | List.CopyTo array | CollectionsMarshal.AsSpan().CopyTo |
| FfEncoder | GCHandle.Alloc per-channel | Pin single backing, offset pointers |
| FfFilterGraphRunner | Planar[ch][i] interleave, List.CopyTo | GetChannel().Span, CollectionsMarshal |
| AudioProcessor.Analysis | Planar[ch] loop | GetChannel().Span |
| PipelineCommand | Clone(), loop, Planar reference | GetChannel().ToArray(), .Span |
| AudioController | Planar[ch][s] | GetChannel().Span[s] |
| WavIoTests | Planar[0][n] assertions | GetChannel(0).Span[n] |
| AudioProcessorFilterTests | Planar[0] writes + LINQ | GetChannelSpan writes, ToArray() LINQ |

### FfEncoder Pinning (SAFETY CRITICAL)
Changed from pinning N separate `float[]` arrays to pinning the single contiguous backing array once, then computing per-channel `IntPtr` offsets. This is both safer (single pin) and more efficient (one GCHandle instead of N).

### AudioDefaults (Task 3)
New `AudioDefaults` static class with shared constants:
- `SilenceThresholdDb = -55.0` (matches existing TreatmentOptions/SpliceBoundaryOptions)
- `MinimumSilenceDuration = 200ms` (bumped from 50ms for better ASR pre-chunking)

### New Tests
8 tests in `AudioBufferSliceTests.cs`:
1. Slice_SharesBacking -- zero-copy verification
2. Slice_DoesNotAffectParent -- parent immutability
3. Slice_OutOfBounds_Throws -- bounds validation
4. Slice_TimeOverload -- TimeSpan-based slicing
5. Slice_ToWavStream_ProducesValidWav -- WAV encoding from sliced buffer
6. Slice_GetChannel_ReturnsCorrectRange -- channel view correctness
7. GetChannelSpan_WriteThrough -- write-through to parent backing
8. Contiguous_MultiChannel -- multi-channel stride correctness

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical Functionality] Zero-length roomtone guard in GenerateRoomtoneFill**
- **Found during:** Task 2
- **Issue:** If `roomtone.Length == 0` and `targetDurationSec > 0`, the while loop (`cursor < targetSamples`) never progresses because `toCopy` is always 0
- **Fix:** Added `throw new ArgumentException("Roomtone source buffer is empty")` before the loop
- **Files modified:** `host/Ams.Core/Audio/AudioSpliceService.cs`
- **Commit:** a8b52f2

No other deviations. Plan executed as written.

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | 9beca93 | Refactor AudioBuffer internals to contiguous backing |
| 2 | a8b52f2 | Migrate all 14 consumer files |
| 3 | 1007565 | Add AudioDefaults and 8 slice tests |

## Verification Results

- Full solution build: 0 errors, 5 pre-existing warnings
- Test suite: 97/97 pass (3 pre-existing ChapterLabelResolver failures excluded -- platform-specific path separator issue)
- Zero remaining `.Planar` references in production code
- Slice method confirmed in AudioBuffer.cs
- AudioDefaults confirmed with SilenceThresholdDb constant

## Self-Check: PASSED

All files exist, all commits verified.
