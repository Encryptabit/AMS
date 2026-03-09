---
phase: 15-pickup-flow-redesign
plan: 02
subsystem: audio
tags: [breath-detection, splice-boundary, audio-processing, feature-extraction]

# Dependency graph
requires:
  - phase: 12-polish-area-foundation
    provides: "SpliceBoundaryService with silence-center and snap-energy methods"
provides:
  - "RefineBoundariesBreathAware public method for breath-aware boundary placement"
  - "BreathAware BoundaryMethod enum value"
  - "Configurable breath detection options on SpliceBoundaryOptions"
affects: [15-pickup-flow-redesign]

# Tech tracking
tech-stack:
  added: []
  patterns: ["Breath detection overlay wrapping existing boundary refinement"]

key-files:
  created: []
  modified:
    - host/Ams.Core/Audio/SpliceBoundaryService.cs

key-decisions:
  - "Wrap existing RefineBoundaries rather than modifying it — breath-aware is an overlay"
  - "Default FrameBreathDetectorOptions used (FricativeGuardMs=25) to prevent false positives on fricatives"
  - "Start boundary shifted after breath (keeps breath with preceding sentence), end boundary shifted before breath (keeps breath with following sentence)"

patterns-established:
  - "Breath-aware boundary: detect in narrow window around cut point, shift to avoid bisecting"

requirements-completed: [PFR-BREATH]

# Metrics
duration: 3min
completed: 2026-03-09
---

# Phase 15 Plan 02: Breath-Aware Splice Boundaries Summary

**SpliceBoundaryService enhanced with breath-aware boundary placement using FeatureExtraction.Detect to avoid bisecting breaths at cut points**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-09T09:29:02Z
- **Completed:** 2026-03-09T09:32:03Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- Added `BreathAware` value to `BoundaryMethod` enum
- Implemented `RefineBoundariesBreathAware` public static method that wraps existing `RefineBoundaries` then applies breath detection overlay
- Added configurable breath detection options (`EnableBreathDetection`, `BreathSearchRadiusSec`, `BreathGuardSec`) to `SpliceBoundaryOptions`

## Task Commits

Each task was committed atomically:

1. **Task 1: Add BreathAware BoundaryMethod and breath detection to RefineBoundary** - `964d476` (feat)
2. **Task 2: Add breath-aware options to SpliceBoundaryOptions** - `cd011aa` (feat)

## Files Created/Modified
- `host/Ams.Core/Audio/SpliceBoundaryService.cs` - Added BreathAware enum value, RefineBoundariesBreathAware method, and breath detection options to SpliceBoundaryOptions

## Decisions Made
- Wrapped existing `RefineBoundaries` rather than modifying it — breath-aware placement is a non-invasive overlay
- Used default `FrameBreathDetectorOptions` (FricativeGuardMs=25) to avoid false positives on fricative consonants
- Start boundary shifted after straddling breath (keeps breath with preceding sentence); end boundary shifted before breath (keeps breath with following sentence)
- 5ms guard gap between breath edge and cut point prevents clipping artifacts

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Breath-aware boundary refinement is callable from downstream pickup flow services
- Existing silence-center and snap-energy methods unchanged and fully functional
- Ready for Plan 03 integration

## Self-Check: PASSED

- FOUND: host/Ams.Core/Audio/SpliceBoundaryService.cs
- FOUND: 964d476 (Task 1 commit)
- FOUND: cd011aa (Task 2 commit)

---
*Phase: 15-pickup-flow-redesign*
*Completed: 2026-03-09*
