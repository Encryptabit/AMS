---
phase: 12-polish-area-foundation
plan: 02
subsystem: services
tags: [staging-queue, undo, json-persistence, singleton, blazor]

requires:
  - phase: 12-01
    provides: "PolishModels (StagedReplacement, UndoRecord, ReplacementStatus)"
provides:
  - "StagingQueueService for non-destructive replacement staging"
  - "UndoService for versioned original segment backups"
  - "DI registration of both Polish services"
affects: [12-03, 12-04, 12-05, 12-06, 12-07, 12-08]

tech-stack:
  added: []
  patterns: ["Lazy-load JSON persistence keyed by chapter stem", "Versioned backup files with manifest"]

key-files:
  created:
    - host/Ams.Workstation.Server/Services/StagingQueueService.cs
    - host/Ams.Workstation.Server/Services/UndoService.cs
  modified:
    - host/Ams.Workstation.Server/Program.cs

key-decisions:
  - "StagingQueue persists to {workDir}/.polish/staging-queue.json (workspace-local, not AppData)"
  - "UndoService uses versioned segment files with per-chapter manifest.json"
  - "Both services use simple lock synchronization (single-user app)"

patterns-established:
  - "Polish services persist to workspace .polish/ and .polish-undo/ directories"
  - "Lazy loading per chapter with in-memory caching and save-on-mutate"

requirements-completed: [REQ-STAGE, REQ-UNDO]

duration: 4min
completed: 2026-02-23
---

# Phase 12 Plan 02: Staging Queue & Undo Services Summary

**Non-destructive staging queue and versioned undo backup services with JSON persistence and DI wiring**

## Performance

- **Duration:** 4 min
- **Started:** 2026-02-23T01:46:09Z
- **Completed:** 2026-02-23T01:50:17Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- StagingQueueService manages per-chapter replacement queue with stage/unstage/update-status/clear operations
- UndoService saves original audio segments as versioned WAV files with JSON manifest for app-restart persistence
- Both services registered as singletons in DI, ready for downstream Polish area consumers

## Task Commits

Each task was committed atomically:

1. **Task 1: StagingQueueService** - `9ce04ba` (feat)
2. **Task 2: UndoService and DI registration** - `4f93be5` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Services/StagingQueueService.cs` - Non-destructive staging queue with JSON persistence to .polish/staging-queue.json
- `host/Ams.Workstation.Server/Services/UndoService.cs` - Versioned segment backup with per-chapter manifest.json in .polish-undo/
- `host/Ams.Workstation.Server/Program.cs` - DI registration of StagingQueueService and UndoService as singletons

## Decisions Made
- Staging queue persists to workspace-local `.polish/staging-queue.json` (not AppData) since it is book/workspace-specific data
- Undo backups use versioned filenames (`sent{id}.v{N}.original.wav`) with next-version determined by scanning both manifest records and filesystem
- Thread safety via simple `lock` -- adequate for single-user Blazor workstation

## Deviations from Plan

None - plan executed exactly as written. PolishModels.cs was already created by plan 12-01 (executed in parallel).

## Issues Encountered
- Running Ams.Workstation.Server process (PID 16900) locked Ams.Core.dll, causing initial build failure. Resolved by stopping the server process.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- StagingQueueService and UndoService are ready for use by plan 12-03 (PolishApiController) and beyond
- Services follow the established singleton + workspace pattern used by other workstation services

## Self-Check: PASSED

- [x] StagingQueueService.cs exists
- [x] UndoService.cs exists
- [x] Program.cs modified with DI registration
- [x] Commit 9ce04ba verified in git log
- [x] Commit 4f93be5 verified in git log
- [x] Solution builds with 0 errors, 0 warnings

---
*Phase: 12-polish-area-foundation*
*Completed: 2026-02-23*
