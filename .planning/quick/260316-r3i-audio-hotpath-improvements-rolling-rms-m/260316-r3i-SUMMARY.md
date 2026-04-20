---
phase: 260316-r3i
plan: 01
subsystem: audio-processing
tags: [performance, simd, hotpath, rolling-rms, managed-copy]
dependency-graph:
  requires: []
  provides: [rolling-rms-silence-detection, simd-sum-of-squares, managed-concat, simd-tomono, pre-indexed-word-lookup, sink-channel-fix]
  affects: [SilenceChunker, AudioBuffer, PipelineCommand, AsrAudioPreparer, MfaChunkCorpusBuilder, FfFilterGraphRunner]
tech-stack:
  added: []
  patterns: [SIMD-Vector-float, rolling-sum-of-squares, binary-search-pre-index, squared-threshold-comparison]
key-files:
  created: []
  modified:
    - host/Ams.Core/Audio/SilenceChunker.cs
    - host/Ams.Tests/Audio/SilenceChunkerTests.cs
    - host/Ams.Core/Artifacts/AudioBuffer.cs
    - host/Ams.Cli/Commands/PipelineCommand.cs
    - host/Ams.Core/Audio/AsrAudioPreparer.cs
    - host/Ams.Core/Application/Mfa/MfaChunkCorpusBuilder.cs
    - host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs
decisions:
  - Rolling sum-of-squares with SIMD SumOfSquaresSimd helper replaces per-hop full-window ComputeRms
  - Squared threshold comparison eliminates Math.Sqrt per hop step
  - Direct managed memory copy in AudioBuffer.Concat replaces FFmpeg filter graph overhead
  - Pre-sorted word midpoints with binary search replaces O(C*W) rescanning in MfaChunkCorpusBuilder
  - Sink-derived _outputChannels field replaces input-derived _channels for accumulator init
metrics:
  duration: 7m8s
  completed: 2026-03-17T02:53:39Z
  tasks_completed: 3
  tasks_total: 3
  test_count: 296
  test_pass: 296
---

# Quick Task 260316-r3i: Audio Hotpath Improvements Summary

Rolling RMS with SIMD acceleration in SilenceChunker, managed-copy AudioBuffer.Concat, SIMD ToMono, pre-indexed MFA corpus word lookup, and sink-derived accumulator channels in FfFilterGraphRunner.

## Task Results

| Task | Name | Commit | Key Changes |
|------|------|--------|-------------|
| 1 | SilenceChunker rolling RMS, SIMD, threshold, tail, split fixes | 2a20902 | Rolling sum-of-squares, SumOfSquaresSimd, squared threshold, tail handling, pre-sized SelectSplitPoints |
| 2 | AudioBuffer.Concat managed copy, ToMono SIMD, metadata fix | e7b8285 | Direct managed copy replaces FFmpeg graph, Vector<float> fold-down, mono metadata correction |
| 3 | MfaChunkCorpusBuilder pre-index, FfFilterGraphRunner channel fix | e80e6a0 | O(W log W + C) pre-indexed lookup, sink-derived _outputChannels |

## Changes Applied

### SilenceChunker (Task 1)
- **Rolling sum-of-squares**: Initial window computed via SumOfSquaresSimd, then each hop subtracts outgoing and adds incoming slice -- O(hopSize) per step instead of O(windowSize)
- **SIMD ComputeRms**: New `SumOfSquaresSimd` helper processes Vector<float>.Count elements at a time with multiply-accumulate, then horizontal sum plus scalar remainder
- **Squared threshold**: Compares `sumOfSquares / windowSize < thresholdSq` instead of `sqrt(sumOfSquares / windowSize) < threshold` -- eliminates Math.Sqrt per hop
- **Tail handling**: After main hop loop, checks unexamined samples when `lastHopEnd < totalSamples` and tail >= RmsHopSize/2
- **SelectSplitPoints**: Pre-sized list allocation; forced splits guard against creating sub-minChunkSamples tail chunks
- **New test**: `TailSilence_DetectedWhenNotAlignedToHopSize` validates tail partial window handling

### AudioBuffer.Concat (Task 2)
- Replaced FFmpeg filter graph (abuffer + aformat + concat filters) with direct per-channel `Span.CopyTo` managed memory copy
- Removed `using Ams.Core.Services.Integrations.FFmpeg` from AudioBuffer.cs (no longer needed)

### PipelineCommand.ToMono (Task 2)
- SIMD fold-down: `Vector<float> scaleVec` multiply-add per channel, scalar remainder for tail

### AsrAudioPreparer.DownmixToMono (Task 2)
- After FFmpeg pan filter, `result.UpdateMetadata(...WithCurrentStream(..., 1, "flt", "mono"))` corrects output channel layout
- Cached `buffer.Channels` into local for repeated access

### MfaChunkCorpusBuilder.Build (Task 3)
- Pre-builds `List<PreIndexedWord>` sorted by midpoint time before chunk loop
- Per-chunk: binary search for lower bound, scan forward until midpoint >= upper bound
- Complexity: O(W log W) sort + O(W) total scanning across all chunks = O(W log W + C) vs previous O(C * W)
- Existing `BuildLabTextFromWordTiming` method preserved unchanged for test compatibility

### FfFilterGraphRunner (Task 3)
- New `_outputChannels` field initialized to `_inputs[0].Channels` as fallback
- `RefreshOutputFormat()` now queries `av_buffersink_get_channels(_sink)` to derive actual output channel count
- Accumulator and frameSink initialization use `_outputChannels` (not `_channels`) after `RefreshOutputFormat()` completes

## Deviations from Plan

None -- plan executed exactly as written.

## Verification

- All 296 tests pass (14 SilenceChunker, 31 MfaChunkCorpusBuilder, 251 others)
- Ams.Core and Ams.Cli build with zero errors

## Self-Check: PASSED

All 8 files verified present. All 3 commit hashes verified in git log.
