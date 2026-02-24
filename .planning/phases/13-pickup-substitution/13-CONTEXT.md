# Phase 13: Pickup Substitution - Context

**Gathered:** 2026-02-24
**Status:** Ready for planning

<domain>
## Phase Boundary

End-to-end pickup substitution workflow in the Blazor workstation. User sets a pickup session file and roomtone file once (book-wide), then navigates chapters with CRX entries via flippers, reviewing ASR-matched pickups through a Match → Stage → Commit pipeline. Includes waveform region editing for roomtone insert/replace/delete operations. The corrected WAV output must match the source chapter audio format exactly.

Phase 12 built the foundation services (AudioSpliceService, StagingQueueService, UndoService, PickupMatchingService, PolishService). This phase builds the complete user-facing workflow on top of those services, with a redesigned single-page layout.

</domain>

<decisions>
## Implementation Decisions

### Page Layout (redesigned from Phase 12 scaffold)
- Single-page workflow — no navigating back to a chapter list
- **Header bar:** Pickup session file selector + Roomtone file selector (book-wide, set once)
- **Row 1:** Breadcrumb trail showing current chapter + CRX entry position
- **Row 2:** Full chapter waveform with `<` and `>` flippers on left and right sides
- **Row 3:** Three columns — Matches | Staged | Committed — each pickup is a single box that moves through the pipeline
- This replaces the Phase 12 Polish page scaffold (Index.razor, ChapterPolish.razor, PickupImporter.razor, StagingQueue.razor)

### Pickup Import Flow
- Input: One session recording per book (multiple pickups back-to-back, separated by silence)
- Processing is **immediate** when the pickup file is set — split by silence, run ASR, run MFA alignment on all segments, match to all CRX entries across all chapters upfront
- Reuses existing infrastructure: PickupMatchingService, ASR, MFA pipeline from Phase 12/12.1

### Match Boxes
- Each box in the Matches column represents one pickup segment matched to a CRX sentence
- Box contents: mini waveform thumbnail of the pickup segment + matched sentence text + confidence score
- ASR matching is high-confidence (CRX provides target text) — no manual reassignment needed

### Match → Stage → Commit Pipeline
- **Stage actions:** "Stage All" button, individual stage button per match box, drag-and-drop from Matches to Staged
- **Unstage:** Boxes can move backward from Staged to Matches (fully reversible)
- **Commit:** Each commit immediately writes corrected.wav — waveform reloads to show updated audio
- **Revert:** Committed replacements can be reverted using Phase 12 undo infrastructure
- A single pickup box exists in exactly one column at a time (prevents duplicate staging)

### Waveform Region Editing
- Staged pickups show as a draggable region on the main chapter waveform marking the segment to replace
- User can adjust region boundaries to control exactly what chunk of audio gets removed
- Separate play/audition button to listen to just the pickup
- Logic: "delete everything inside these boundaries, insert this pickup"
- **Roomtone operations:** User can select any region on the waveform and:
  - Insert roomtone at that point (pushes content apart)
  - Replace selection with roomtone (fills region with room tone)
  - Delete selection (removes region, pulls content together)
- All operations (pickup substitution + roomtone ops) get crossfade on both edges
- Crossfade duration is adjustable per replacement

### Audio Format Matching
- corrected.wav must exactly match source chapter WAV format: sample rate, bit depth, channels
- If input is 24-bit/44.1kHz, output must be 24-bit/44.1kHz — no format conversion
- This fixes the current issue where output bit depth/rate doesn't match the raw chapter audio

### Chapter Navigation
- Flippers skip chapters with no CRX entries — only navigate between chapters needing pickups
- Full chapter waveform always visible (no auto-zoom to sentence regions)
- Breadcrumb trail tracks current position (chapter + CRX entry)

### Completion & Progression
- Chapter shows a completion badge/checkmark when all CRX entries are committed
- Flippers auto-advance to next incomplete chapter upon completion

### Claude's Discretion
- Mini waveform rendering approach in match boxes
- Exact crossfade default duration value
- Drag-and-drop implementation details
- Breadcrumb format and styling
- Region color coding (staged vs roomtone selection)
- Loading/progress indicators during initial pickup processing

</decisions>

<specifics>
## Specific Ideas

- User provided a hand-drawn UI sketch showing the 3-row layout: breadcrumbs / waveform+flippers / three-column pipeline
- "I want to be able to adjust the region boundaries which essentially equates to, remove this chunk of audio and insert the pickup in question"
- Roomtone is a manual editing tool, not automatic padding — user highlights a region and chooses insert/replace/delete
- The pickup file and roomtone file belong in the header bar "as this is where book-wide values have naturally ended up"
- All waveform editing operations incur crossfade on both sides

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 13-pickup-substitution*
*Context gathered: 2026-02-24*
