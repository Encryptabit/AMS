# Phase 12: Polish Area Foundation - Context

**Gathered:** 2026-02-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Build the Polish area of the Blazor workstation with take replacement workflow and batch editing foundations. Users import pickup recordings, match them to flagged sentences via ASR, stage replacements non-destructively, and apply them with crossfades. Batch operations (renaming, timing shifts, pre/post roll standardization) use the same staging pattern. A multi-waveform stacked view enables cross-chapter editing. All audio edits go through Ams.Core FFmpeg integration. DSP pipeline integration is deferred to its own phase.

</domain>

<decisions>
## Implementation Decisions

### Take Replacement Flow
- Primary source: pickup recordings (separate audio files recorded to fix errors)
- Accept both: single session file (multiple pickups in one recording) and individual pickup files (one per sentence)
- Session file segmentation: ASR-based matching — run ASR on the pickup file, auto-match recognized text to CRX target sentences
- User can fine-tune pickup boundaries/assignments via waveform regions after auto-matching
- Staging queue: replacements are staged non-destructively, user applies individually or in batch at their discretion
- No forced all-or-nothing — user decides when and how many to apply

### Batch Editing Scope
- Batch operations for Phase 12: pickup replacement, batch renaming, batch shifting of chapter readings/headers, batch pre+post roll standardization
- DSP pipeline: placeholder/hooks only — actual DSP batch processing is its own future phase
- Target selection: manual multi-select of chapters to include in batch operations
- Multi-waveform editor: selected chapters loaded simultaneously in stacked vertical layout (DAW-style), synchronized playhead/markers across all visible waveforms
- Efficient partial buffer loading required — load regions, not full chapters, since multiple chapters are active at once
- All batch operations use non-destructive staging — preview changes, apply when ready, original treated audio intact until commit

### Audio Editing Requirements
- All edits must have crossfades applied at splice points (smooth transitions, no clicks)
- All audio editing goes through the FFmpeg integration in Ams.Core
- If editing functionality (splice, crossfade, segment replacement) doesn't exist in Ams.Core yet, it needs to be added as part of this phase

### Result Verification
- Auto re-validate after replacement: re-run ASR on the affected segment, update diff/error status automatically
- Listen-with-context for user verification: play the replaced segment with surrounding audio (few seconds before/after) to check natural flow
- Auto-sync to Proof: when a fix passes re-validation and user is satisfied, sentence status automatically updates in the Proof area
- Undo supported: keep original segments so replacements can be reverted even after application

### Claude's Discretion
- Staging queue UI design and interaction patterns
- Partial buffer loading strategy for multi-waveform view
- Crossfade duration and profile defaults
- Undo storage mechanism (versioned files vs in-memory snapshots)
- Waveform region interaction details for pickup boundary adjustment

</decisions>

<specifics>
## Specific Ideas

- Multi-waveform view inspired by DAW multi-track layout — chapters stacked vertically with synchronized playhead
- Pickup matching uses ASR recognition to auto-map to CRX target sentences, with manual adjustment via wavesurfer regions
- User emphasized flexibility: "sometimes I might want to apply changes all at once, sometimes one at a time"
- Batch operations include renaming, shifting chapter readings/headers, and pre+post roll standardization
- All edits flow through Ams.Core FFmpeg — no direct audio file manipulation outside the core library

</specifics>

<deferred>
## Deferred Ideas

- Auditioning other in-book utterances of the same word/phrase as replacement candidates — future enhancement to take replacement
- DSP batch pipeline (batch DSP processing across chapters) — deserves its own phase due to intricacy
- Phase 13 (Pickup Substitution) may overlap with take replacement — reconcile scope during planning

</deferred>

---

*Phase: 12-polish-area-foundation*
*Context gathered: 2026-02-23*
