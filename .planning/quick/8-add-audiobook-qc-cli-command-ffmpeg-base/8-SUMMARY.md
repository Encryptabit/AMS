---
phase: quick
plan: 8
subsystem: audio, cli
tags: [ffmpeg, silencedetect, ffprobe, qc, spectre-console, audiobook]

requires: []
provides:
  - "AudioQcAnalyzer: ffmpeg silencedetect-based chapter structure analysis"
  - "QC CLI command: qc analyze --dir for batch audiobook QC"
  - "ChapterQcResult model for per-file QC reporting"
affects: []

tech-stack:
  added: []
  patterns: ["pure static analyzer + ffmpeg process invocation", "Spectre.Console table for CLI reporting"]

key-files:
  created:
    - host/Ams.Core/Audio/QualityControl/AudioQcModels.cs
    - host/Ams.Core/Audio/QualityControl/AudioQcAnalyzer.cs
    - host/Ams.Cli/Commands/QcCommand.cs
    - host/Ams.Tests/Audio/QualityControl/AudioQcAnalyzerTests.cs
  modified:
    - host/Ams.Cli/Program.cs

key-decisions:
  - "QC noise floor -40dB (not -55dB AudioDefaults): -55 is tuned for ASR chunking, -40 matches user's QC analysis needs"
  - "Open-ended silence sentinel (End=-1, Duration=-1): signals trailing silence_start with no matching silence_end"
  - "InvariantCulture for all double parsing: fixes latent locale bug in existing SentenceRefinementService pattern"

patterns-established:
  - "Pure static analyzer functions testable without ffmpeg + async ffmpeg/ffprobe orchestration method"

requirements-completed: [QC-01]

duration: 5min
completed: 2026-03-02
---

# Quick Task 8: Audiobook QC CLI Command Summary

**ffmpeg silencedetect-based audiobook QC analyzer with head/tail/gap structure detection, anomaly flagging, Spectre.Console table output, and JSON export**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-02T06:58:21Z
- **Completed:** 2026-03-02T07:03:32Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Pure static analysis functions (ParseSilenceRegions, AnalyzeStructure, FlagAnomalies) with 23 unit tests
- Full CLI command: `qc analyze --dir <path>` with configurable thresholds and optional JSON export
- InvariantCulture double parsing throughout (fixes latent locale bug pattern from SentenceRefinementService)

## Task Commits

Each task was committed atomically:

1. **Task 1: Core QC analyzer service with models and tests** - `e8f8823` (test: RED), `38cb524` (feat: GREEN)
2. **Task 2: QC CLI command with console table and JSON export** - `a11fd4b` (feat)

## Files Created/Modified
- `host/Ams.Core/Audio/QualityControl/AudioQcModels.cs` - SilenceRegion, QcThresholds, ChapterQcResult records
- `host/Ams.Core/Audio/QualityControl/AudioQcAnalyzer.cs` - ParseSilenceRegions, AnalyzeStructure, FlagAnomalies, AnalyzeFileAsync
- `host/Ams.Cli/Commands/QcCommand.cs` - CLI verb: qc analyze --dir with Spectre.Console table + JSON export
- `host/Ams.Cli/Program.cs` - Registered QcCommand.Create()
- `host/Ams.Tests/Audio/QualityControl/AudioQcAnalyzerTests.cs` - 23 unit tests for pure analysis functions

## Decisions Made
- QC noise floor set to -40dB (not AudioDefaults -55dB) because -55 is tuned for ASR chunking sensitivity, while -40 matches the user's QC analysis requirements
- Trailing silence_start without matching silence_end tracked with sentinel values (End=-1, Duration=-1) to signal open-ended silence extending to file end
- InvariantCulture used for all double parsing, fixing the locale bug pattern present in SentenceRefinementService.ParseSilenceOutput

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required. ffmpeg/ffprobe must be available (already a project dependency via FFMPEG_EXE env var or PATH).

## Next Phase Readiness
- QC analyzer is standalone -- no pipeline/workspace/book dependencies
- Ready for use against any directory of audiobook chapter files
- JSON export enables downstream tooling integration

## Self-Check: PASSED

All 5 files verified present. All 3 commits (e8f8823, 38cb524, a11fd4b) verified in git log.

---
*Quick task: 8*
*Completed: 2026-03-02*
