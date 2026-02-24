---
phase: 13-pickup-substitution
plan: 04
subsystem: services, ui
tags: [cross-chapter, pickup-matching, drag-and-drop, staging, blazor]

# Dependency graph
requires:
  - phase: 13-pickup-substitution
    provides: "CrossChapterPickupMatch model, PickupMatchingService, PolishService, PickupSubstitution page scaffold"
provides:
  - "ImportPickupsCrossChapterAsync for book-wide single-pass pickup processing"
  - "Upfront processing pipeline triggered on pickup file set"
  - "Stage/Unstage/Stage All actions for pickup box column movement"
  - "HTML5 drag-and-drop from Matches to Staged column"
affects: [13-05, 13-06, 13-07]

# Tech tracking
tech-stack:
  added: []
  patterns: [cross-chapter-processing-pipeline, html5-drag-drop-staging, text-similarity-disambiguation]

key-files:
  created: []
  modified:
    - host/Ams.Workstation.Server/Services/PolishService.cs
    - host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor

key-decisions:
  - "Cross-chapter sentence ID disambiguation via Levenshtein text similarity on recognized vs book text"
  - "Single ASR+MFA pass for all chapters' flagged sentences for efficiency"
  - "Compiled Regex for text normalization in PolishService (PunctuationRegex, WhitespaceRegex)"

patterns-established:
  - "Cross-chapter processing: gather all targets, single-pass process, distribute results by chapter stem"
  - "Drag-and-drop staging: ondragstart captures CrossChapterPickupMatch, ondrop on Staged column triggers staging"

requirements-completed: [PS-IMPORT, PS-PIPELINE]

# Metrics
duration: 6min
completed: 2026-02-24
---

# Phase 13 Plan 04: Upfront Processing & Stage/Unstage Actions Summary

**Cross-chapter pickup processing pipeline with single-pass ASR+MFA, Stage/Unstage/Stage All actions, and HTML5 drag-and-drop between Matches and Staged columns**

## Performance

- **Duration:** 6 min
- **Started:** 2026-02-24T19:14:38Z
- **Completed:** 2026-02-24T19:21:00Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- Added ImportPickupsCrossChapterAsync to PolishService: processes all chapters' flagged sentences in a single ASR+MFA pass, distributes matches by chapter stem with sentence ID collision disambiguation
- Wired HandlePickupFileSet to trigger upfront cross-chapter processing with progress reporting when pickup session file is set
- Implemented Stage/Unstage/Stage All actions that move pickup boxes between columns and refresh staging queue state
- Added HTML5 drag-and-drop from Matches column to Staged column with visual feedback (opacity reduction during drag)

## Task Commits

Each task was committed atomically:

1. **Task 1: Cross-chapter pickup import in PolishService** - `6bbf211` (feat)
2. **Task 2: Upfront processing, stage/unstage actions, and drag-and-drop** - `f073f4f` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Services/PolishService.cs` - Added ImportPickupsCrossChapterAsync with cross-chapter disambiguation, NormalizeForCompare helper, compiled Regex fields
- `host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor` - Wired upfront processing pipeline, Stage All button, drag-and-drop handlers, RefreshCurrentChapterState, LoadFlaggedSentencesForChapter

## Decisions Made
- Used Levenshtein text similarity (LevenshteinMetrics.Similarity) to disambiguate when the same sentence ID appears in multiple chapters, matching recognized text against each chapter's book text
- Single-pass processing: all chapters' flagged sentences gathered into one list for PickupMatchingService.MatchPickupAsync, avoiding redundant ASR/MFA runs
- Added compiled static Regex fields (PunctuationRegex, WhitespaceRegex) to PolishService for efficient text normalization, matching the pattern used by PickupMatchingService

## Deviations from Plan

None - plan executed exactly as written. The PickupSubstitution.razor page scaffold already existed from a previous session (plan 13-03 work), so Task 2 modifications applied cleanly.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Upfront processing pipeline ready for use with the Stage/Unstage/Stage All interactions
- HTML5 drag-and-drop functional for intuitive box movement
- Ready for plan 13-05 (Pickup Matching UI enhancements) and plan 13-06 (Staging & Commit UI)

## Self-Check: PASSED

All files exist. All commits verified.

---
*Phase: 13-pickup-substitution*
*Completed: 2026-02-24*
