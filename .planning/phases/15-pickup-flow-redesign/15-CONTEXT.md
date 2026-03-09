# Phase 15: Pickup Flow Redesign - Context

**Gathered:** 2026-03-09
**Status:** Ready for planning

<domain>
## Phase Boundary

Redesign the pickup substitution system (Phase 12/13 work) so that multiple pickups per chapter can be applied in any order without timeline corruption, boundary truncation, or breath clipping. Both session-file and individual-file pickup sources become first-class citizens. The single-page `/polish` workflow is retained but rebuilt on a new model/service foundation.

</domain>

<decisions>
## Implementation Decisions

### Pickup Source Handling
- **Unified PickupAsset model from the start** — both session-file segments and individual WAV files normalize into the same `PickupAsset` shape. Import auto-detects source type.
- **MFA-aware segmentation** — when splitting a session file, use MFA phone boundaries to find cleaner split points between utterances instead of relying solely on the current 0.8s silence-gap heuristic.
- **Persist with cache invalidation** — pickup assets are cached to disk. Unless the user explicitly re-imports, the cached data is reused. Invalidate on source file change (path + size + modified timestamp).
- **Individual file convention** — when received as a folder, files follow the same naming as CRX error WAVs (e.g., `001.wav`, `002.wav`). System should match by filename pattern to error number when available.

### Matching Strategy
- **CRX.json is the source of truth for targeting** — each CRX entry has `SentenceId`, `Chapter`, `StartTime`, `EndTime`, `ErrorNumber`, `AudioFile`, and `Comments` (with "Should be" / "Read as" text). This metadata drives deterministic targeting for individual pickup files.
- **ASR + text similarity for session files** — when a session file contains multiple pickups, ASR each segment and fuzzy-match recognized text against CRX "Should be" text. This handles out-of-order recording.
- **Unmatched bucket + manual assignment** — pickups that don't confidently match any CRX target appear in a separate "unmatched" list. User can manually assign them to a target via drag-drop or selection.
- **Full manual reassignment** — any pickup can be reassigned from one CRX target to another after auto-matching, via drag-drop or menu.

### Boundary & Handle Model
- **Adaptive handles with user final say** — system provides smart initial boundaries based on content analysis (breath detection, energy analysis), but the user adjusts edges in the UI and has final authority.
- **Both sides editable** — chapter-side region handles AND pickup-side trim handles are both adjustable in the UI. Two sets of draggable edges.
- **Breath-aware boundary placement** — detect breaths near sentence edges and place initial cut points so breaths are not bisected. Keep the breath with whichever sentence it perceptually belongs to.
- **Precision slice replacement** — the goal is to replace only the recorded speech itself. Do not cut into breaths (either side), do not cut into surrounding speech. Since a pickup typically does not include the same breath context as the original, the replacement should slot in cleanly between the natural pauses/breaths.
- **Context playback for audition** — when auditioning a staged replacement, play surrounding chapter audio with the pickup spliced in so the user hears the before/after transitions in context.
- **Crossfades must live inside preserved handles, not consume speech** — the current 80ms pickup padding with 70ms crossfade is fundamentally broken. Handles must be large enough that the crossfade region is entirely outside the speech/breath zone.

### Timeline Projection
- **Immutable edit list + projection service** — each applied edit (pickup replacement or roomtone operation) is an immutable record. A `TimelineProjection` service maps any baseline transcript time to current chapter time by walking the edit list. No in-place mutation of queue items.
- **Unified model for pickups and roomtone** — roomtone operations (insert, replace, delete) and pickup replacements are both "chapter edits" in the same projection system. One source of truth for timeline state.
- **Arbitrary revert** — any applied edit can be reverted independently, regardless of application order. System recalculates the chapter from remaining edits.

### Claude's Discretion
- **Matching algorithm selection** — Claude picks the best approach for content-aware matching that supports arbitrary recording order (hybrid positional + text similarity recommended).
- **Rebuild vs surgical revert** — Claude decides the revert implementation strategy based on performance/correctness tradeoffs. Rebuild-from-original is the safer default (deterministic, correct by construction); surgical revert is an optimization if needed later.

</decisions>

<specifics>
## Specific Ideas

- CRX.json structure (real example from `For_The_Glory_Of_Rome_1`): each entry has `ErrorNumber`, `Chapter`, `Timecode`, `ErrorType`, `Comments` (with "Should be" / "Read as" parsed text), `SentenceId`, `StartTime`, `EndTime`, `AudioFile`, `CreatedAt`. This is rich enough for deterministic targeting.
- Individual pickup files follow the same naming as CRX audio exports (e.g., `001.wav` maps to `ErrorNumber: 1`).
- Session files are the common case — one WAV with all pickups back-to-back separated by silence. The narrator may record them out of order relative to the book.
- The user typically does one book at a time. The first book they did used individual files; all subsequent books use a single session file.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 15-pickup-flow-redesign*
*Context gathered: 2026-03-09*
