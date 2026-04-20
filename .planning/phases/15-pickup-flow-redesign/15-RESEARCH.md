# Phase 15: Pickup Flow Redesign - Research

**Researched:** 2026-03-09
**Domain:** Immutable edit list / timeline projection, unified pickup asset model, MFA-aware segmentation, breath-aware boundary detection, arbitrary-order revert
**Confidence:** HIGH

## Summary

Phase 15 redesigns the pickup substitution system's model and service layer underneath the existing `/polish` page. The three major architectural changes are: (1) replacing the current mutable `ShiftDownstream` + `RebaseTranscriptTimeToCurrentTimeline` timeline management with an immutable edit list and a `TimelineProjection` service, (2) introducing a unified `PickupAsset` model that normalizes both session-file segments and individual pickup files into the same shape, and (3) enhancing boundary detection to be breath-aware rather than relying solely on silence detection.

The existing codebase provides substantial infrastructure from Phases 12-14. `AudioSpliceService` (crossfade splice via FFmpeg `acrossfade`), `SpliceBoundaryService` (silence + energy snapping), `PickupMatchingService` (ASR + MFA positional pairing), `PolishService` (orchestration), `StagingQueueService` (non-destructive queue with JSON persistence), and `UndoService` (versioned segment backups) are all operational. The `FeatureExtraction.Detect()` method already provides frame-level breath detection with spectral analysis. The key insight is that this phase does NOT add new external dependencies — it restructures internal models and services to support order-independent multi-edit application, dual-side boundary editing, and deterministic revert.

**Primary recommendation:** Introduce a `TimelineProjection` service in Ams.Core that accepts an ordered list of immutable `ChapterEdit` records and computes baseline↔current time mappings. Replace `StagingQueueService.ShiftDownstream` and `PolishService.RebaseTranscriptTimeToCurrentTimeline` with projection queries. Use rebuild-from-original for revert (deterministic, correct by construction). Enhance `SpliceBoundaryService` to incorporate `FeatureExtraction.Detect()` for breath-aware placement. Create a `PickupAsset` model and `PickupAssetService` that unify import from both source types.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Pickup Source Handling:**
- Unified PickupAsset model from the start — both session-file segments and individual WAV files normalize into the same `PickupAsset` shape. Import auto-detects source type.
- MFA-aware segmentation — when splitting a session file, use MFA phone boundaries to find cleaner split points between utterances instead of relying solely on the current 0.8s silence-gap heuristic.
- Persist with cache invalidation — pickup assets are cached to disk. Unless the user explicitly re-imports, the cached data is reused. Invalidate on source file change (path + size + modified timestamp).
- Individual file convention — when received as a folder, files follow the same naming as CRX error WAVs (e.g., `001.wav`, `002.wav`). System should match by filename pattern to error number when available.

**Matching Strategy:**
- CRX.json is the source of truth for targeting — each CRX entry has `SentenceId`, `Chapter`, `StartTime`, `EndTime`, `ErrorNumber`, `AudioFile`, and `Comments` (with "Should be" / "Read as" text). This metadata drives deterministic targeting for individual pickup files.
- ASR + text similarity for session files — when a session file contains multiple pickups, ASR each segment and fuzzy-match recognized text against CRX "Should be" text. This handles out-of-order recording.
- Unmatched bucket + manual assignment — pickups that don't confidently match any CRX target appear in a separate "unmatched" list. User can manually assign them to a target via drag-drop or selection.
- Full manual reassignment — any pickup can be reassigned from one CRX target to another after auto-matching, via drag-drop or menu.

**Boundary & Handle Model:**
- Adaptive handles with user final say — system provides smart initial boundaries based on content analysis (breath detection, energy analysis), but the user adjusts edges in the UI and has final authority.
- Both sides editable — chapter-side region handles AND pickup-side trim handles are both adjustable in the UI. Two sets of draggable edges.
- Breath-aware boundary placement — detect breaths near sentence edges and place initial cut points so breaths are not bisected. Keep the breath with whichever sentence it perceptually belongs to.
- Precision slice replacement — the goal is to replace only the recorded speech itself. Do not cut into breaths (either side), do not cut into surrounding speech. Since a pickup typically does not include the same breath context as the original, the replacement should slot in cleanly between the natural pauses/breaths.
- Context playback for audition — when auditioning a staged replacement, play surrounding chapter audio with the pickup spliced in so the user hears the before/after transitions in context.
- Crossfades must live inside preserved handles, not consume speech — the current 80ms pickup padding with 70ms crossfade is fundamentally broken. Handles must be large enough that the crossfade region is entirely outside the speech/breath zone.

**Timeline Projection:**
- Immutable edit list + projection service — each applied edit (pickup replacement or roomtone operation) is an immutable record. A `TimelineProjection` service maps any baseline transcript time to current chapter time by walking the edit list. No in-place mutation of queue items.
- Unified model for pickups and roomtone — roomtone operations (insert, replace, delete) and pickup replacements are both "chapter edits" in the same projection system. One source of truth for timeline state.
- Arbitrary revert — any applied edit can be reverted independently, regardless of application order. System recalculates the chapter from remaining edits.

### Claude's Discretion
- Matching algorithm selection — Claude picks the best approach for content-aware matching that supports arbitrary recording order (hybrid positional + text similarity recommended).
- Rebuild vs surgical revert — Claude decides the revert implementation strategy based on performance/correctness tradeoffs. Rebuild-from-original is the safer default (deterministic, correct by construction); surgical revert is an optimization if needed later.

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope.
</user_constraints>

## Standard Stack

### Core (Already in Codebase — Zero New Dependencies)
| Library/Service | Location | Purpose | Status |
|---------|---------|---------|--------|
| `AudioSpliceService` | `Ams.Core/Audio/` | Crossfade splice (`ReplaceSegment`, `DeleteRegion`, `InsertAtPoint`, `GenerateRoomtoneFill`) | Fully operational |
| `SpliceBoundaryService` | `Ams.Core/Audio/` | Silence-based boundary refinement with energy snapping | Fully operational |
| `FeatureExtraction` | `Ams.Core/Audio/` | Frame-level breath detection (`Detect()`) via spectral analysis | Fully operational |
| `PickupMatchingService` | Workstation Services | ASR + MFA positional pairing with Levenshtein confidence | Fully operational |
| `PickupMfaRefinementService` | Workstation Services | MFA forced alignment on pickup recordings | Fully operational |
| `PolishService` | Workstation Services | Orchestration: import, stage, apply, revert, audition | Fully operational — needs refactoring |
| `StagingQueueService` | Workstation Services | JSON-persisted per-chapter queue | Fully operational — needs refactoring |
| `UndoService` | Workstation Services | Versioned segment backup/restore | Fully operational — needs refactoring |
| `AudioProcessor` | `Ams.Core/Processors/` | Trim, Resample, DetectSilence, SnapToEnergy, Decode, EncodeWav | Fully operational |
| `LevenshteinMetrics` | `Ams.Core/Common/` | String similarity scoring | Fully operational |
| `FfFilterGraphRunner` | `Ams.Core/Services/Integrations/FFmpeg/` | Multi-input FFmpeg filter graph execution | Fully operational |
| wavesurfer.js v7 | CDN | Waveform with draggable/resizable regions | Fully operational |
| Whisper.NET | NuGet | In-process ASR via `AsrProcessor` | Fully operational |

### Nothing New to Install
All required capabilities exist in the current dependency set. No new NuGet packages or npm modules needed. The `LevenshteinMetrics` utility already handles text similarity (no need for FuzzySharp or other external libraries).

## Architecture Patterns

### Current Architecture (What Exists)

The current system uses **mutable timeline management** with two mechanisms:
1. `StagingQueueService.ShiftDownstream()` — after each apply/revert, shifts `OriginalStartSec`/`OriginalEndSec` of downstream items in-place
2. `PolishService.RebaseTranscriptTimeToCurrentTimeline()` — walks the applied items list to map transcript time → current time

**Problems with current approach:**
- In-place mutation of queue items makes arbitrary revert incorrect (reverting item B that was applied before item C corrupts C's shifted boundaries)
- `ShiftDownstream` only shifts items downstream of the pivot, but doesn't account for items that were staged AFTER the shift occurred
- Timeline mapping is fragile — the applied-items ordering assumption breaks when edits are applied/reverted out of order
- Roomtone operations use synthetic IDs and aren't first-class in the staging queue
- The 80ms `PickupSlicePaddingSec` padding + 70ms crossfade means crossfade overlaps speech content

### New Architecture (Phase 15)

```
host/Ams.Core/
├── Audio/
│   ├── AudioSpliceService.cs          # KEEP: unchanged
│   ├── SpliceBoundaryService.cs       # EXTEND: add breath-aware boundary
│   ├── FeatureExtraction.cs           # KEEP: breath detection already exists
│   └── TimelineProjection.cs          # NEW: immutable edit list + projection
│
host/Ams.Workstation.Server/
├── Models/
│   ├── PolishModels.cs                # EXTEND: add PickupAsset, ChapterEdit, EditOperation
│   └── CrxModels.cs                   # KEEP: unchanged
├── Services/
│   ├── PickupAssetService.cs          # NEW: unified import from session/individual/folder
│   ├── PickupMatchingService.cs       # EXTEND: add text-similarity matching for session files
│   ├── PolishService.cs               # REFACTOR: use TimelineProjection, rebuild-based revert
│   ├── StagingQueueService.cs         # REFACTOR: immutable edit records, no ShiftDownstream
│   ├── UndoService.cs                 # SIMPLIFY: rebuild-based revert changes undo storage needs
│   └── EditListService.cs             # NEW: manages immutable ChapterEdit records
└── Components/
    ├── Pages/Polish/
    │   └── PickupSubstitution.razor   # REFACTOR: dual-side handles, unmatched bucket, manual reassignment
    └── Shared/
        └── PickupBox.razor            # EXTEND: add pickup-side trim handles, reassignment UI
```

### Pattern 1: Immutable Edit List + TimelineProjection

**What:** Each applied edit (pickup replacement or roomtone operation) is stored as an immutable `ChapterEdit` record. A `TimelineProjection` service computes baseline↔current time mappings by walking the edit list. No in-place mutation.

**When to use:** Every time the system needs to map a transcript/baseline time to the current chapter timeline (boundary rendering, audition clip, staging).

**Data Model:**
```csharp
// In PolishModels.cs
public enum EditOperation { PickupReplace, RoomtoneInsert, RoomtoneReplace, RoomtoneDelete }

/// <summary>
/// Immutable record of a single chapter edit. Once created, never mutated.
/// The edit list is append-only; reverts remove entries from the list.
/// </summary>
public sealed record ChapterEdit(
    string Id,
    string ChapterStem,
    EditOperation Operation,
    double BaselineStartSec,    // position in the ORIGINAL (baseline) timeline
    double BaselineEndSec,      // position in the ORIGINAL (baseline) timeline
    double ReplacementDurationSec, // duration of what was inserted
    int? SentenceId,            // null for roomtone ops
    int? ErrorNumber,           // CRX error number, if applicable
    string? PickupAssetId,      // reference to the PickupAsset used
    double CrossfadeDurationSec,
    string CrossfadeCurve,
    DateTime AppliedAtUtc);
```

**Projection Service:**
```csharp
// In Ams.Core/Audio/TimelineProjection.cs
public static class TimelineProjection
{
    /// <summary>
    /// Maps a baseline (original transcript) time to the current chapter timeline,
    /// accounting for all applied edits (in application order).
    /// </summary>
    public static double BaselineToCurrentTime(
        double baselineTimeSec,
        IReadOnlyList<ChapterEdit> appliedEdits)
    {
        var current = baselineTimeSec;
        foreach (var edit in appliedEdits)
        {
            if (edit.BaselineStartSec >= current)
                continue; // edit is downstream, no effect yet
                
            var originalDuration = edit.BaselineEndSec - edit.BaselineStartSec;
            var delta = edit.ReplacementDurationSec - originalDuration;
            
            if (edit.BaselineEndSec <= current)
            {
                // edit is fully upstream — shift by delta
                current += delta;
            }
            else
            {
                // Time falls inside the edit region — clamp to edit boundary
                current = edit.BaselineStartSec + edit.ReplacementDurationSec;
            }
        }
        return Math.Max(0, current);
    }
    
    /// <summary>
    /// Computes the total duration of the chapter after all edits.
    /// </summary>
    public static double ProjectedDuration(
        double baselineDurationSec,
        IReadOnlyList<ChapterEdit> appliedEdits)
    {
        return BaselineToCurrentTime(baselineDurationSec, appliedEdits);
    }
}
```

**Key insight:** Because all edits reference the BASELINE (original) timeline, they are independent of each other. Removing any edit from the list and recalculating produces correct results regardless of the order edits were applied.

### Pattern 2: Rebuild-from-Original Revert

**What:** When reverting any edit (regardless of application order), rebuild the entire chapter by replaying all remaining edits against the original (treated) audio. This is deterministic, correct by construction, and avoids the "surgical revert" problem entirely.

**When to use:** Any revert operation. Also used for multi-pickup commit (apply all remaining edits in batch).

**Recommendation (Claude's Discretion — Rebuild chosen):** Rebuild-from-original is the clear winner for correctness. The performance cost is acceptable because:
1. Each chapter is typically 20-40 minutes of audio — a full rebuild takes 2-5 seconds (FFmpeg crossfade is fast)
2. The user applies/reverts individual pickups interactively, not in tight loops
3. Surgical revert is inherently fragile (crossfade artifacts compound, boundary precision degrades)

**Example:**
```csharp
public async Task<AudioBuffer> RebuildChapterAsync(
    ChapterContextHandle handle,
    IReadOnlyList<ChapterEdit> editsToApply,
    CancellationToken ct)
{
    // Start from the original treated audio
    var buffer = handle.Chapter.Audio.Treated?.Buffer
        ?? throw new InvalidOperationException("No treated audio available.");
    
    // Sort edits by baseline position (front to back)
    // Apply in REVERSE order (back to front) to preserve positions
    var sorted = editsToApply
        .OrderByDescending(e => e.BaselineStartSec)
        .ToList();
    
    foreach (var edit in sorted)
    {
        ct.ThrowIfCancellationRequested();
        var replacement = LoadReplacementForEdit(handle, edit);
        
        buffer = edit.Operation switch
        {
            EditOperation.PickupReplace => AudioSpliceService.ReplaceSegment(
                buffer, edit.BaselineStartSec, edit.BaselineEndSec,
                replacement, edit.CrossfadeDurationSec, edit.CrossfadeCurve),
            EditOperation.RoomtoneInsert => AudioSpliceService.InsertAtPoint(
                buffer, edit.BaselineStartSec, replacement,
                edit.CrossfadeDurationSec, edit.CrossfadeCurve),
            EditOperation.RoomtoneReplace => AudioSpliceService.ReplaceSegment(
                buffer, edit.BaselineStartSec, edit.BaselineEndSec,
                replacement, edit.CrossfadeDurationSec, edit.CrossfadeCurve),
            EditOperation.RoomtoneDelete => AudioSpliceService.DeleteRegion(
                buffer, edit.BaselineStartSec, edit.BaselineEndSec,
                edit.CrossfadeDurationSec, edit.CrossfadeCurve),
            _ => buffer
        };
    }
    
    return buffer;
}
```

**Critical detail — apply back-to-front:** When edits reference baseline times and are applied sequentially to a buffer that's being modified, we must apply from the END of the file toward the START. This ensures each edit's baseline coordinates remain valid because modifications downstream haven't shifted the positions yet.

### Pattern 3: Unified PickupAsset Model

**What:** Both session-file segments and individual WAV files normalize into the same `PickupAsset` shape. Import auto-detects source type.

**Data Model:**
```csharp
public enum PickupSourceType { SessionSegment, IndividualFile }

/// <summary>
/// A normalized pickup recording segment, regardless of source type.
/// Immutable after creation. References the source file + trim boundaries.
/// </summary>
public sealed record PickupAsset(
    string Id,                      // GUID
    PickupSourceType SourceType,
    string SourceFilePath,          // original WAV path
    double TrimStartSec,            // within the source file
    double TrimEndSec,              // within the source file
    string TranscribedText,         // ASR result
    double Confidence,              // match confidence (0.0-1.0)
    int? MatchedErrorNumber,        // CRX error number if matched
    int? MatchedSentenceId,         // target sentence if matched
    string? MatchedChapterStem,     // target chapter if matched
    DateTime ImportedAtUtc);

/// <summary>
/// Cached collection of pickup assets with source file identity for staleness checks.
/// </summary>
public sealed record PickupAssetCache(
    string SourceFilePath,
    long SourceFileSizeBytes,
    DateTime SourceFileModifiedUtc,
    string CrxTargetsFingerprint,
    IReadOnlyList<PickupAsset> Assets,
    DateTime ProcessedAtUtc);
```

### Pattern 4: MFA-Aware Segmentation

**What:** When splitting a session file, use MFA phone boundaries to find cleaner split points instead of the current 0.8s silence-gap heuristic.

**Current approach:** `PickupMatchingService.SegmentUtterances()` groups ASR tokens by 0.8s silence gaps between token end/start times.

**Improved approach:** After MFA refinement (which already runs via `PickupMfaRefinementService`), the tokens have phone-level timing precision. Use MFA phone boundaries to identify true utterance boundaries:

```csharp
// Enhanced segmentation using MFA-refined token timings
// 1. MFA gives precise phone boundaries (start/end of each word)
// 2. Find gaps between the LAST phone of word N and FIRST phone of word N+1
// 3. A gap > 0.3s between phone boundaries = utterance break (lower threshold than 0.8s)
// 4. For ambiguous breaks, check if the gap contains silence using AudioProcessor.DetectSilence
//    on just the gap region

private static List<PickupSegment> SegmentByMfaBoundaries(
    AsrToken[] mfaRefinedTokens,
    double utteranceGapThresholdSec = 0.4)
{
    // MFA-refined tokens already have precise start/end from phone alignment
    // The threshold can be lower (0.4s vs 0.8s) because MFA timings are more accurate
    // This catches cases where the narrator paused briefly between pickups
}
```

**Key insight:** The `PickupMfaRefinementService` already runs MFA on the entire pickup WAV and rewrites token timings. The segmentation step just needs to use a lower gap threshold with the improved timings.

### Pattern 5: Breath-Aware Boundary Placement

**What:** Enhance `SpliceBoundaryService` to use `FeatureExtraction.Detect()` for breath detection near splice boundaries, placing cut points so breaths are not bisected.

**How it integrates:**
```csharp
// Enhanced SpliceBoundaryService flow:
// 1. Current: find silence → snap to energy → fallback to original
// 2. New step between 1 and 2: detect breaths in the search region
// 3. If a breath overlaps the proposed cut point, shift the cut to:
//    - BEFORE the breath (keep breath with the following sentence)
//    - AFTER the breath (keep breath with the preceding sentence)
//    - Decision: keep breath with whichever side it temporally belongs to
//      (closer to the previous sentence end → keep with previous)

public static SpliceBoundaryResult RefineBoundariesBreathAware(
    AudioBuffer chapterBuffer,
    double roughStartSec,
    double roughEndSec,
    double? prevSentenceEndSec,
    double? nextSentenceStartSec,
    SpliceBoundaryOptions? options = null)
{
    // 1. Existing silence-based refinement
    var initial = RefineBoundaries(chapterBuffer, roughStartSec, roughEndSec,
        prevSentenceEndSec, nextSentenceStartSec, options);
    
    // 2. Run breath detection in the region around each boundary
    var breathOptions = new FrameBreathDetectorOptions();
    
    // Check start boundary for breath overlap
    var startSearchRegion = (
        Math.Max(0, initial.RefinedStartSec - 0.2),
        initial.RefinedStartSec + 0.2);
    var startBreaths = FeatureExtraction.Detect(
        chapterBuffer, startSearchRegion.Item1, startSearchRegion.Item2, breathOptions);
    
    var refinedStart = initial.RefinedStartSec;
    foreach (var breath in startBreaths)
    {
        if (breath.StartSec <= refinedStart && breath.EndSec >= refinedStart)
        {
            // Breath straddles the cut point — move cut to avoid bisection
            refinedStart = breath.EndSec + 0.005; // just after breath
        }
    }
    
    // Similar for end boundary...
    
    return new SpliceBoundaryResult(refinedStart, refinedEnd, ...);
}
```

**The `FeatureExtraction.Detect()` method** already exists and returns `IReadOnlyList<Region>` with `(StartSec, EndSec)` pairs. It uses spectral analysis (high-frequency ratio, flatness, ZCR) combined with temporal hysteresis to identify breath sounds. This is the same infrastructure used by the pause processing pipeline.

### Pattern 6: Dual-Side Boundary Editing (Chapter + Pickup)

**What:** The UI provides two sets of draggable edges: (1) chapter-side region handles defining what gets REMOVED from the chapter, (2) pickup-side trim handles defining what portion of the pickup gets INSERTED.

**Implementation approach:**
```
Chapter waveform:   [=====|▓▓▓REGION▓▓▓|=====]  ← green draggable region (chapter side)
                          ^               ^
                    chapter start    chapter end

Pickup mini-view:   [--|▓▓TRIM▓▓|--]              ← orange handles (pickup side)
                       ^          ^
                  pickup start  pickup end
```

- **Chapter-side handles** already work via `WaveformPlayer.AddEditableRegion()` and `HandleRegionUpdated`
- **Pickup-side handles** need a small wavesurfer instance in the PickupBox or a canvas-based trim control
- Both sets of boundaries are stored in the `StagedReplacement` (which already has `OriginalStartSec`/`OriginalEndSec` for chapter-side and `PickupStartSec`/`PickupEndSec` for pickup-side)

### Pattern 7: Hybrid Matching for Session Files

**Recommendation (Claude's Discretion):** Use a hybrid positional + text-similarity approach for matching session-file segments to CRX targets when narrators record out of order.

**Current approach:** Purely positional — `PairSegmentsToTargets()` aligns segments to CRX targets by ErrorNumber order with a sliding offset to find the best alignment.

**Improved approach for out-of-order recording:**
```csharp
// 1. For each segment, compute similarity against ALL CRX targets
// 2. Build a score matrix: segments × targets
// 3. Use greedy best-match assignment (not Hungarian — overkill for 5-30 items)
//    - Pick highest-confidence pair, assign, remove both from pool, repeat
// 4. Unmatched segments → unmatched bucket
// 5. Unmatched targets → available for manual assignment

private static List<PickupMatch> MatchByTextSimilarity(
    List<PickupSegment> segments,
    List<CrxPickupTarget> targets)
{
    var matches = new List<PickupMatch>();
    var availableTargets = new List<CrxPickupTarget>(targets);
    var availableSegments = new List<(int Index, PickupSegment Segment)>(
        segments.Select((s, i) => (i, s)));
    
    while (availableTargets.Count > 0 && availableSegments.Count > 0)
    {
        double bestScore = -1;
        int bestSegIdx = -1, bestTgtIdx = -1;
        
        for (int s = 0; s < availableSegments.Count; s++)
        {
            var normalized = NormalizeForMatch(availableSegments[s].Segment.TranscribedText);
            for (int t = 0; t < availableTargets.Count; t++)
            {
                var targetText = NormalizeForMatch(availableTargets[t].ShouldBeText);
                var score = LevenshteinMetrics.Similarity(normalized, targetText);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestSegIdx = s;
                    bestTgtIdx = t;
                }
            }
        }
        
        if (bestScore < 0.2) break; // No useful matches remain
        
        var seg = availableSegments[bestSegIdx].Segment;
        var tgt = availableTargets[bestTgtIdx];
        
        matches.Add(new PickupMatch(
            SentenceId: tgt.SentenceId,
            PickupStartSec: seg.StartSec,
            PickupEndSec: seg.EndSec,
            Confidence: bestScore,
            RecognizedText: seg.TranscribedText,
            ErrorNumber: tgt.ErrorNumber,
            IsLowConfidence: bestScore < 0.4));
        
        availableTargets.RemoveAt(bestTgtIdx);
        availableSegments.RemoveAt(bestSegIdx);
    }
    
    return matches;
}
```

**Why greedy over Hungarian:** The match pool is small (typically 5-30 items). Greedy best-first is O(n²·m) with n segments and m targets — negligible for these sizes. Hungarian algorithm adds complexity with no measurable benefit. The existing `LevenshteinMetrics.Similarity()` provides the scoring function.

### Anti-Patterns to Avoid

- **In-place mutation of edit boundaries:** Never modify an edit's baseline coordinates after creation. The baseline is the source of truth. Current `ShiftDownstream()` is the pattern being replaced.
- **Surgical revert for single-edit removal:** Don't try to "undo" a single splice in the middle of a chain. The crossfade artifacts from the original apply are baked in — reversing them introduces quality loss. Always rebuild from original.
- **Padding-based crossfade placement:** The current `PickupSlicePaddingSec = 0.080` approach adds 80ms of padding around pickup boundaries, then applies a 70ms crossfade. This means 70ms of the 80ms pad is consumed by crossfade, leaving only 10ms of actual handle. Instead, handles should be large enough that the crossfade fits entirely within the handle region.
- **Separate undo paths for pickups vs roomtone:** Both are "chapter edits" in the new model. One code path for both.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Breath detection | Amplitude-based heuristic | `FeatureExtraction.Detect()` | Already has spectral analysis (HF ratio, flatness, ZCR), hysteresis, guard margins |
| Audio crossfade splice | Manual sample math | `AudioSpliceService.ReplaceSegment()` | FFmpeg `acrossfade` handles all edge cases |
| Text similarity scoring | External NLP library | `LevenshteinMetrics.Similarity()` | Already in codebase, handles all matching needs |
| File staleness checking | Custom hash comparison | Path + size + modified timestamp triple | Same pattern already used in `PolishService.ImportPickupsCrxAsync` |
| MFA on pickup files | Custom alignment pipeline | `PickupMfaRefinementService.RefineAsrTimingsAsync()` | Full MFA pipeline with caching already operational |
| Silence gap detection | Manual amplitude scan | `AudioProcessor.DetectSilence()` | FFmpeg `silencedetect` with configurable thresholds |
| Energy edge snapping | Manual RMS calculation | `AudioProcessor.SnapToEnergy()` | Already used by `SpliceBoundaryService` |
| WAV encoding with bit depth preservation | Manual header writing | `AudioProcessor.EncodeWav()` with `AudioEncodeOptions.TargetBitDepth` | 24-bit support already added in Phase 13 |
| CRX comment parsing | Regex from scratch | `CrxCommentParser.TryParseShouldBe()` / `TryParseReadAs()` | Already handles "Should be:" / "Read as:" extraction |

**Key insight:** The new model layer (PickupAsset, ChapterEdit, TimelineProjection) is the genuinely new work. Everything else is wiring existing infrastructure through the new model.

## Common Pitfalls

### Pitfall 1: Back-to-Front Application Order in Rebuild

**What goes wrong:** If edits referencing baseline times are applied front-to-back, the first edit shifts all downstream positions. The second edit's baseline coordinates no longer correspond to the modified buffer.
**Why it happens:** Each `ReplaceSegment` call changes the buffer length — `baselineStartSec` for edit 2 is now wrong.
**How to avoid:** Apply edits in REVERSE order (highest `BaselineStartSec` first). Each edit modifies only content after it in the buffer, so preceding edits' baseline positions remain valid.
**Warning signs:** Second and subsequent edits splice at wrong positions; audio sounds jumbled.

### Pitfall 2: Crossfade Duration Consuming Speech in Handles

**What goes wrong:** The crossfade region extends into the speech/breath zone, causing audible artifacts.
**Why it happens:** Current code uses `PickupSlicePaddingSec = 0.080` (80ms padding) with default crossfade of 70ms. The crossfade consumes nearly all the padding, leaving no "clean" handle zone.
**How to avoid:** Handles must extend at least `crossfadeDuration + 30ms` beyond the speech boundary in both directions. For a 70ms crossfade, the handle needs to be at least 100ms wide. Use breath detection to find the actual speech edge, then add crossfade + guard padding.
**Warning signs:** Audible clicks, breath truncation, or speech onset cutting at splice points.

### Pitfall 3: Rebuild Performance on Long Chapters

**What goes wrong:** Rebuilding a 40-minute chapter from 15 edits takes too long for interactive use.
**Why it happens:** Each `AudioSpliceService.ReplaceSegment()` call runs an FFmpeg `acrossfade` filter graph. 15 sequential FFmpeg operations on large buffers add up.
**How to avoid:** Measure actual rebuild time early. If > 5 seconds, consider: (a) caching the most recent rebuild and only re-splicing from the affected edit onward, (b) using concat instead of acrossfade when the crossfade is negligible, (c) parallelizing independent segments. Expected realistic time: 2-4 seconds for 15 edits on a 40-minute chapter (each FFmpeg acrossfade on a ~30-minute buffer takes ~200ms).
**Warning signs:** UI freezes during revert; user complains about lag.

### Pitfall 4: Stale PickupAsset Cache After Re-Recording

**What goes wrong:** User re-records pickups, producing a new WAV file with the same name. The cache returns stale matches.
**Why it happens:** Cache key only checks path — same path returns cached results.
**How to avoid:** Cache key MUST include path + file size + modified timestamp (the triple). This pattern already exists in `PolishService.ImportPickupsCrxAsync` — use the same approach for `PickupAssetCache`.
**Warning signs:** Matches don't reflect the new recording; user sees old recognized text.

### Pitfall 5: Individual File Naming Convention Mismatch

**What goes wrong:** User provides a folder of individual pickup WAVs but filenames don't follow `001.wav` convention (e.g., they use `error_1.wav`, `Chapter3_Error001.wav`).
**How to avoid:** Support multiple naming conventions:
1. `NNN.wav` (e.g., `001.wav`) — direct CRX ErrorNumber match
2. `error_NNN.wav` or `err_NNN.wav` — extract trailing digits
3. Fallback: sort files alphabetically, pair positionally with CRX entries
**Warning signs:** Zero matches when loading individual files; all land in "unmatched" bucket.

### Pitfall 6: Breath Detection False Positives on Fricatives

**What goes wrong:** `FeatureExtraction.Detect()` may classify fricative consonants (f, s, sh, th) as breaths because they share spectral characteristics (high-frequency energy, spectral flatness).
**Why it happens:** Breaths and voiceless fricatives both have aperiodic high-frequency content.
**How to avoid:** The existing `FrameBreathDetectorOptions` already has `FricativeGuardMs = 25` and `GuardLeftMs`/`GuardRightMs` to avoid false positives near speech. Use the default options rather than aggressive custom settings. Additionally, only run breath detection in the inter-sentence gap region (where breaths naturally occur), not inside the speech region.
**Warning signs:** Boundary placement clips speech onset/offset consonants.

### Pitfall 7: Unmatched Pickup Bucket Not Updating After Manual Assignment

**What goes wrong:** After manually assigning an unmatched pickup to a CRX target, the pickup still appears in the unmatched bucket.
**Why it happens:** The unmatched list is derived from `PickupAssets where MatchedErrorNumber == null`. Manual assignment needs to update the asset's match fields.
**How to avoid:** Manual assignment should create a new `PickupAsset` record (immutable) with the updated match fields, replacing the old one in the asset collection. The asset list itself can be mutable (it's an import artifact, not an edit).
**Warning signs:** Duplicate entries — pickup appears in both unmatched list and matched list.

## Code Examples

### ChapterEdit Lifecycle

```csharp
// 1. When user commits a pickup replacement:
var edit = new ChapterEdit(
    Id: Guid.NewGuid().ToString("N"),
    ChapterStem: "Chapter_01",
    Operation: EditOperation.PickupReplace,
    BaselineStartSec: 45.230,    // transcript sentence start
    BaselineEndSec: 47.890,      // transcript sentence end
    ReplacementDurationSec: 2.150, // pickup duration
    SentenceId: 42,
    ErrorNumber: 7,
    PickupAssetId: pickupAsset.Id,
    CrossfadeDurationSec: 0.070,
    CrossfadeCurve: "hsin",
    AppliedAtUtc: DateTime.UtcNow);

editListService.Add(edit);

// 2. Rebuild chapter from all active edits:
var allEdits = editListService.GetEdits("Chapter_01");
var rebuilt = await RebuildChapterAsync(handle, allEdits, ct);
PersistCorrectedBuffer(handle, rebuilt);

// 3. Revert: just remove the edit and rebuild
editListService.Remove(edit.Id);
var remaining = editListService.GetEdits("Chapter_01");
var rebuilt = await RebuildChapterAsync(handle, remaining, ct);
PersistCorrectedBuffer(handle, rebuilt);

// 4. Query current time for a transcript position:
var currentTime = TimelineProjection.BaselineToCurrentTime(45.230, allEdits);
```

### PickupAsset Import from Folder (Individual Files)

```csharp
public async Task<IReadOnlyList<PickupAsset>> ImportFromFolderAsync(
    string folderPath,
    IReadOnlyList<CrxPickupTarget> crxTargets,
    CancellationToken ct)
{
    var files = Directory.GetFiles(folderPath, "*.wav")
        .OrderBy(f => f)
        .ToList();
    
    var assets = new List<PickupAsset>();
    
    foreach (var file in files)
    {
        ct.ThrowIfCancellationRequested();
        var fileName = Path.GetFileNameWithoutExtension(file);
        
        // Try to extract error number from filename
        var errorNumber = TryExtractErrorNumber(fileName);
        var matchedTarget = errorNumber.HasValue
            ? crxTargets.FirstOrDefault(t => t.ErrorNumber == errorNumber.Value)
            : null;
        
        // Run ASR for text recognition
        var buffer = AudioProcessor.Decode(file);
        var duration = (double)buffer.Length / buffer.SampleRate;
        var asrText = await TranscribeAsync(buffer, ct);
        
        var confidence = matchedTarget != null
            ? LevenshteinMetrics.Similarity(
                PickupMatchingService.NormalizeForMatch(asrText),
                PickupMatchingService.NormalizeForMatch(matchedTarget.ShouldBeText))
            : 0.0;
        
        assets.Add(new PickupAsset(
            Id: Guid.NewGuid().ToString("N"),
            SourceType: PickupSourceType.IndividualFile,
            SourceFilePath: file,
            TrimStartSec: 0,
            TrimEndSec: duration,
            TranscribedText: asrText,
            Confidence: confidence,
            MatchedErrorNumber: matchedTarget?.ErrorNumber,
            MatchedSentenceId: matchedTarget?.SentenceId,
            MatchedChapterStem: matchedTarget?.ChapterStem,
            ImportedAtUtc: DateTime.UtcNow));
    }
    
    return assets;
}

private static int? TryExtractErrorNumber(string fileName)
{
    // Try patterns: "001", "error_1", "err_001"
    var digitsOnly = Regex.Match(fileName, @"^(\d+)$");
    if (digitsOnly.Success) return int.Parse(digitsOnly.Value);
    
    var errorPrefix = Regex.Match(fileName, @"(?:error|err)[_-]?(\d+)", RegexOptions.IgnoreCase);
    if (errorPrefix.Success) return int.Parse(errorPrefix.Groups[1].Value);
    
    // Extract trailing digits
    var trailing = Regex.Match(fileName, @"(\d+)$");
    if (trailing.Success) return int.Parse(trailing.Value);
    
    return null;
}
```

### Breath-Aware Boundary Enhancement

```csharp
// Enhancement to SpliceBoundaryService.RefineBoundary
// Insert between step 1 (silence detection) and step 5 (energy snapping)

// After finding the initial refined position from silence:
var breathRegions = FeatureExtraction.Detect(
    chapterBuffer,
    searchLeftSec, searchRightSec,
    new FrameBreathDetectorOptions());

// Check if any breath straddles the proposed cut point
foreach (var breath in breathRegions)
{
    var absoluteStart = searchLeftSec + breath.StartSec;
    var absoluteEnd = searchLeftSec + breath.EndSec;
    
    if (absoluteStart <= refinedPosition && absoluteEnd >= refinedPosition)
    {
        // Breath straddles the cut — decide which side keeps it
        double breathCenter = (absoluteStart + absoluteEnd) / 2.0;
        if (isStartBoundary)
        {
            // For start boundary: place cut AFTER breath (breath stays with previous)
            refinedPosition = absoluteEnd + 0.005;
        }
        else
        {
            // For end boundary: place cut BEFORE breath (breath stays with following)
            refinedPosition = absoluteStart - 0.005;
        }
        method = BoundaryMethod.BreathAware; // new enum value
        break;
    }
}
```

## State of the Art

| Old Approach (Phase 12-13) | New Approach (Phase 15) | Impact |
|----------------------------|-------------------------|--------|
| `ShiftDownstream` mutates queue items in-place | Immutable edit list with projection queries | Enables arbitrary-order revert without corruption |
| `RebaseTranscriptTimeToCurrentTimeline` walks applied items | `TimelineProjection.BaselineToCurrentTime` pure function | Deterministic, testable, no side effects |
| Separate code paths for pickups vs roomtone | Unified `ChapterEdit` + `EditOperation` enum | One timeline system for all chapter modifications |
| Session-file segments only; individual files through `MatchSinglePickupAsync` | Unified `PickupAsset` model for both source types | Auto-detect import, consistent data model |
| Silence-only boundary detection (silence center / energy snap) | Breath-aware boundary detection using `FeatureExtraction` | No breath bisection, cleaner splice points |
| 80ms padding + 70ms crossfade overlapping speech | Handles sized by content analysis, crossfade inside handle zone | No speech consumption by crossfade |
| Surgical revert using stored original segments | Rebuild-from-original replaying remaining edits | Correct by construction, no accumulated artifacts |
| Current positional-only matching | Hybrid positional + text-similarity matching | Handles out-of-order session recordings |

## Open Questions

1. **Rebuild performance with many edits**
   - What we know: Each `AudioSpliceService.ReplaceSegment` call runs an FFmpeg filter graph. For 15 edits on a 40-minute chapter, estimated ~3 seconds total.
   - What's unclear: Exact real-world timing with the back-to-front approach on actual book chapters.
   - Recommendation: Implement rebuild, measure timing early, optimize only if > 5 seconds. If needed, consider caching the "last good rebuild" and only replaying from the affected edit.

2. **Pickup-side trim UI for individual PickupBox**
   - What we know: Chapter-side handles use wavesurfer.js regions. The `PickupBox` currently shows a static mini waveform.
   - What's unclear: Whether to use a small wavesurfer instance per pickup (interactive but heavyweight) or a canvas-based trim control (lighter but more implementation work).
   - Recommendation: Use a single shared "pickup detail panel" that opens when a PickupBox is selected, showing a full-size wavesurfer with draggable trim handles. This avoids N wavesurfer instances.

3. **Migration path from current StagingQueue to EditList**
   - What we know: `StagingQueueService` persists to `staging-queue.json`. The new `EditListService` will persist to a different format.
   - What's unclear: Whether to auto-migrate existing staged items or require a clean slate.
   - Recommendation: On Phase 15 rollout, detect existing `staging-queue.json`, warn the user, and offer to clear it. The new format is incompatible with old format anyway because the new system uses baseline coordinates, not shifted coordinates.

## Sources

### Primary (HIGH confidence)
- Codebase: `PolishService.cs` — full current orchestration (952 lines), `RebaseTranscriptTimeToCurrentTimeline`, `ApplyReplacementAsync`, `RevertReplacementAsync`
- Codebase: `StagingQueueService.cs` — current mutable queue with `ShiftDownstream` (446 lines)
- Codebase: `UndoService.cs` — versioned segment backup/restore (322 lines)
- Codebase: `AudioSpliceService.cs` — `ReplaceSegment`, `DeleteRegion`, `InsertAtPoint`, `GenerateRoomtoneFill` (251 lines)
- Codebase: `SpliceBoundaryService.cs` — silence + energy boundary refinement (276 lines)
- Codebase: `FeatureExtraction.cs` — frame-level breath detector with `Detect()` (656 lines)
- Codebase: `PickupMatchingService.cs` — ASR + MFA matching with positional pairing (478 lines)
- Codebase: `PickupMfaRefinementService.cs` — MFA forced alignment on pickup recordings (475 lines)
- Codebase: `PolishModels.cs` — current model definitions (195 lines)
- Codebase: `CrxModels.cs` — CRX entry structure (76 lines)
- Codebase: `PickupSubstitution.razor` — current single-page Polish UI (1214 lines)
- Codebase: `PickupBox.razor` — current pickup box component (177 lines)
- Codebase: `LevenshteinMetrics.cs` — string similarity utility (220 lines)
- Phase 12 Research (`12-RESEARCH.md`) — foundation architecture patterns and decisions
- Phase 13 Research (`13-RESEARCH.md`) — single-page workflow patterns, cross-chapter matching

### Secondary (MEDIUM confidence)
- Phase 14 Verification (`14-VERIFICATION.md`) — chunked ASR/MFA infrastructure status

### Tertiary (LOW confidence)
- None — all findings verified against codebase.

## Metadata

**Confidence breakdown:**
- Timeline projection / immutable edit list: HIGH — architecture is well-defined by the decisions, implementation is straightforward pure-function design
- Rebuild-from-original revert: HIGH — correct by construction, existing `AudioSpliceService` operations are deterministic
- PickupAsset model: HIGH — direct translation of user decisions into data model
- Breath-aware boundaries: HIGH — `FeatureExtraction.Detect()` already exists and is battle-tested
- MFA-aware segmentation: HIGH — `PickupMfaRefinementService` already runs MFA and refines token timings
- Hybrid text-similarity matching: HIGH — existing `LevenshteinMetrics` + greedy assignment is straightforward
- Dual-side boundary editing (UI): MEDIUM — chapter-side handles exist, pickup-side UI approach needs design decision
- Migration from old StagingQueue: MEDIUM — clean-slate approach is simplest, migration logic is optional

**Research date:** 2026-03-09
**Valid until:** 2026-04-09 (stable — all technologies already integrated)
