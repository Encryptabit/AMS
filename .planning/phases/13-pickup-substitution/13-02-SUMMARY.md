---
phase: 13-pickup-substitution
plan: 02
subsystem: api, models, ui
tags: [waveform, canvas, rms, pickup-matching, roomtone, cross-chapter]

# Dependency graph
requires:
  - phase: 12-polish-area-foundation
    provides: "PolishModels with PickupMatch, StagedReplacement, UndoRecord"
provides:
  - "CrossChapterPickupMatch with composite key for multi-chapter pickup tracking"
  - "RoomtoneOperation/RoomtoneRequest types for audio region editing"
  - "PickupBoxState enum for Match->Stage->Commit pipeline"
  - "GET /api/audio/waveform-data endpoint returning normalized amplitude array"
  - "drawMiniWaveform and loadAndDrawMiniWaveform JS canvas functions"
affects: [13-03, 13-04, 13-05, 13-06]

# Tech tracking
tech-stack:
  added: []
  patterns: [canvas-based-mini-waveform, rms-amplitude-endpoint, composite-key-pattern]

key-files:
  created: []
  modified:
    - host/Ams.Workstation.Server/Models/PolishModels.cs
    - host/Ams.Workstation.Server/Controllers/AudioController.cs
    - host/Ams.Workstation.Server/wwwroot/js/waveform-interop.js

key-decisions:
  - "Canvas-based mini waveform instead of wavesurfer instances for match box thumbnails"
  - "Composite key pattern (chapterStem:sentenceId) prevents cross-chapter ID collisions"
  - "RMS amplitude with per-block normalization for consistent visual representation"

patterns-established:
  - "Mini waveform pattern: server-side RMS computation + client-side canvas rendering"
  - "Composite key pattern: chapterStem:sentenceId for cross-chapter entity tracking"

requirements-completed: [PS-MATCH, PS-IMPORT]

# Metrics
duration: 3min
completed: 2026-02-24
---

# Phase 13 Plan 02: Cross-Chapter Models & Mini Waveform Summary

**Cross-chapter pickup models with composite keys, waveform amplitude data API, and lightweight canvas-based mini waveform renderer for match box thumbnails**

## Performance

- **Duration:** 3 min
- **Started:** 2026-02-24T19:07:12Z
- **Completed:** 2026-02-24T19:10:55Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Extended PolishModels with CrossChapterPickupMatch (composite key), RoomtoneOperation/RoomtoneRequest, and PickupBoxState types
- Added GET /api/audio/waveform-data endpoint that decodes audio segments and returns normalized RMS amplitude arrays
- Created drawMiniWaveform and loadAndDrawMiniWaveform JS functions for lightweight canvas rendering without wavesurfer overhead

## Task Commits

Each task was committed atomically:

1. **Task 1: Cross-chapter pickup models and roomtone operation types** - `f0a2660` (feat)
2. **Task 2: Waveform amplitude data endpoint and mini waveform JS renderer** - `a543fe5` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Models/PolishModels.cs` - Added CrossChapterPickupMatch, RoomtoneOperation, RoomtoneRequest, PickupBoxState
- `host/Ams.Workstation.Server/Controllers/AudioController.cs` - Added GET waveform-data endpoint with RMS amplitude computation
- `host/Ams.Workstation.Server/wwwroot/js/waveform-interop.js` - Added drawMiniWaveform and loadAndDrawMiniWaveform functions

## Decisions Made
- Used canvas-based mini waveform rendering (drawMiniWaveform) rather than wavesurfer instances per Research Pattern 6, avoiding heavyweight JS library overhead for simple thumbnails
- Composite key format `chapterStem:sentenceId` chosen to prevent ID collisions when processing pickups across all chapters simultaneously
- RMS amplitude normalization divides by max amplitude for 0.0-1.0 range, ensuring consistent visual representation regardless of source audio levels

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Models ready for Phase 13 pickup matching service (plan 13-03+)
- Waveform-data endpoint ready for match box thumbnail rendering in the Pickup Substitution page
- Mini waveform JS functions available for any Blazor component via JS interop

## Self-Check: PASSED

All files exist. All commits verified.

---
*Phase: 13-pickup-substitution*
*Completed: 2026-02-24*
