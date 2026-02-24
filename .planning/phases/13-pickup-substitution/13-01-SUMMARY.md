---
phase: 13-pickup-substitution
plan: 01
subsystem: audio
tags: [ffmpeg, pcm, wav, 24-bit, roomtone, crossfade, splice]

requires:
  - phase: 12-polish-area-foundation
    provides: AudioSpliceService.ReplaceSegment, AudioProcessor encode/decode, PolishService
provides:
  - 24-bit PCM WAV encoding via FfEncoder (PCM_S24LE codec)
  - AudioInfo.BitsPerSample field from FfDecoder.Probe
  - Format-preserving corrected.wav output in PolishService
  - AudioSpliceService.GenerateRoomtoneFill (loop roomtone to target duration)
  - AudioSpliceService.DeleteRegion (crossfade join without replacement)
  - AudioSpliceService.InsertAtPoint (zero-width splice insertion)
affects: [13-pickup-substitution, polish-workflow]

tech-stack:
  added: []
  patterns:
    - "FFmpeg PCM_S24LE codec with AV_SAMPLE_FMT_S32 input for 24-bit WAV encoding"
    - "Source bit-depth probing before encode for format preservation"
    - "Roomtone looping via sample-level Array.Copy for efficiency"

key-files:
  created: []
  modified:
    - host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs
    - host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs
    - host/Ams.Core/Processors/AudioProcessor.cs
    - host/Ams.Core/Audio/AudioSpliceService.cs
    - host/Ams.Workstation.Server/Services/PolishService.cs

key-decisions:
  - "24-bit WAV uses PCM_S24LE codec with S32 input format (FFmpeg standard pattern for 24-bit)"
  - "BitsPerSample resolves from bits_per_raw_sample > bits_per_coded_sample > sample format inference"
  - "GenerateRoomtoneFill uses sample-level Array.Copy loop rather than FFmpeg filter graph for efficiency"

patterns-established:
  - "Format-preserving encode: probe source bit depth, pass to AudioEncodeOptions.TargetBitDepth"
  - "Zero-width splice: split at point then crossfade before+insertion+after (avoids start==end validation)"

requirements-completed: [PS-FORMAT, PS-ROOMTONE]

duration: 4min
completed: 2026-02-24
---

# Phase 13 Plan 01: Audio Infrastructure Summary

**24-bit WAV encoding support and roomtone splice helpers (fill/delete/insert) for pickup substitution pipeline**

## Performance

- **Duration:** 4 min
- **Started:** 2026-02-24T19:07:08Z
- **Completed:** 2026-02-24T19:11:15Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments

- FfEncoder.ResolveEncoding now handles 16/24/32-bit WAV output, enabling format-preserving corrected.wav writes for 24-bit audiobook masters
- AudioInfo record struct extended with BitsPerSample, populated by FfDecoder.Probe via codec parameter inspection with multi-level fallback
- PolishService.PersistCorrectedBuffer probes source chapter WAV bit depth before encoding, ensuring corrected.wav matches original format
- AudioSpliceService gained three static helpers: GenerateRoomtoneFill (loop to duration), DeleteRegion (crossfade join), InsertAtPoint (zero-width splice)

## Task Commits

Each task was committed atomically:

1. **Task 1: 24-bit WAV encoding and format-preserving output** - `124709b` (feat)
2. **Task 2: Roomtone operation helpers in AudioSpliceService** - `d672442` (feat)

## Files Created/Modified

- `host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs` - Added 24-bit PCM_S24LE case to ResolveEncoding switch
- `host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs` - Extract BitsPerSample in Probe via bits_per_raw_sample/coded_sample/format inference
- `host/Ams.Core/Processors/AudioProcessor.cs` - Added BitsPerSample field to AudioInfo record struct (default 0)
- `host/Ams.Core/Audio/AudioSpliceService.cs` - Added GenerateRoomtoneFill, DeleteRegion, InsertAtPoint static helpers
- `host/Ams.Workstation.Server/Services/PolishService.cs` - PersistCorrectedBuffer now probes source and passes TargetBitDepth

## Decisions Made

- **24-bit encoding approach:** FFmpeg has no native 24-bit sample format; PCM_S24LE codec with AV_SAMPLE_FMT_S32 input samples is the standard FFmpeg pattern where the codec truncates 32-bit input to 24-bit in the WAV container.
- **Bit depth detection fallback chain:** bits_per_raw_sample (most accurate for PCM) > bits_per_coded_sample > sample format inference. This handles all common WAV/FLAC/MP3 scenarios.
- **GenerateRoomtoneFill implementation:** Uses direct sample-level Array.Copy looping rather than FFmpeg filter graph, since roomtone looping is a simple memory operation that doesn't benefit from FFmpeg's overhead.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Core audio infrastructure complete for pickup substitution pipeline
- 24-bit format preservation ready for corrected.wav writes
- Roomtone helpers (fill/delete/insert) available for Phase 13 plans 02+ that implement the substitution UI
- AudioSpliceService now covers all splice operation types needed: replace (existing), delete, insert, and roomtone fill

## Self-Check: PASSED

- All 5 modified files exist on disk
- Both task commits verified (124709b, d672442)
- Key content verified: PCM_S24LE in FfEncoder, BitsPerSample in AudioProcessor, 3 helpers in AudioSpliceService, TargetBitDepth in PolishService

---
*Phase: 13-pickup-substitution*
*Completed: 2026-02-24*
