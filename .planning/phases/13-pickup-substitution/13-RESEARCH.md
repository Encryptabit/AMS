# Phase 13: Pickup Substitution - Research

**Researched:** 2026-02-24
**Domain:** Blazor Server single-page pickup workflow, cross-chapter navigation, waveform region editing, roomtone operations, audio format preservation
**Confidence:** HIGH

## Summary

Phase 13 redesigns the Polish area from a multi-page chapter-list workflow into a single-page pickup substitution interface. The user sets a pickup session file and roomtone file once (book-wide), then navigates between chapters with CRX entries via flippers, reviewing ASR-matched pickups through a Match, Stage, Commit three-column pipeline. The full chapter waveform is always visible with draggable regions for boundary editing, and roomtone insert/replace/delete operations are supported as manual editing tools.

Phase 12 built all the critical foundation services: `PolishService` (orchestration), `PickupMatchingService` (ASR + MFA matching), `StagingQueueService` (non-destructive queue), `UndoService` (versioned backups), `AudioSpliceService` (crossfade splice), `SpliceBoundaryService` (boundary refinement), `PolishVerificationService` (ASR re-validation), and `PreviewBufferService` (in-memory preview). The JS interop layer already has `addEditableRegion`, `updateRegionBounds`, `playSegment`, `syncPlayheads`, and region callback events. This phase wires these services into a redesigned single-page UI, adds cross-chapter navigation logic, adds roomtone operations (which are variations of the existing splice service), and fixes the 24-bit audio format matching issue in `FfEncoder`.

**Primary recommendation:** Replace the existing `Index.razor`, `ChapterPolish.razor`, and `PickupImporter.razor` with a single `PickupSubstitution.razor` page at `/polish`. Reuse all Phase 12 services directly -- the service layer needs only minor extensions (roomtone operations, cross-chapter pickup matching, format-preserving encode). The largest new work is the UI redesign (three-column pipeline, flippers, breadcrumbs) and the 24-bit WAV encoding fix in Ams.Core.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Single-page workflow -- no navigating back to a chapter list
- **Header bar:** Pickup session file selector + Roomtone file selector (book-wide, set once)
- **Row 1:** Breadcrumb trail showing current chapter + CRX entry position
- **Row 2:** Full chapter waveform with `<` and `>` flippers on left and right sides
- **Row 3:** Three columns -- Matches | Staged | Committed -- each pickup is a single box that moves through the pipeline
- This replaces the Phase 12 Polish page scaffold (Index.razor, ChapterPolish.razor, PickupImporter.razor, StagingQueue.razor)
- Input: One session recording per book (multiple pickups back-to-back, separated by silence)
- Processing is **immediate** when the pickup file is set -- split by silence, run ASR, run MFA alignment on all segments, match to all CRX entries across all chapters upfront
- Each match box: mini waveform thumbnail of the pickup segment + matched sentence text + confidence score
- ASR matching is high-confidence (CRX provides target text) -- no manual reassignment needed
- **Stage actions:** "Stage All" button, individual stage button per match box, drag-and-drop from Matches to Staged
- **Unstage:** Boxes can move backward from Staged to Matches (fully reversible)
- **Commit:** Each commit immediately writes corrected.wav -- waveform reloads to show updated audio
- **Revert:** Committed replacements can be reverted using Phase 12 undo infrastructure
- A single pickup box exists in exactly one column at a time (prevents duplicate staging)
- Staged pickups show as a draggable region on the main chapter waveform marking the segment to replace
- User can adjust region boundaries to control exactly what chunk of audio gets removed
- Separate play/audition button to listen to just the pickup
- Logic: "delete everything inside these boundaries, insert this pickup"
- **Roomtone operations:** User can select any region on the waveform and: Insert roomtone at that point, Replace selection with roomtone, Delete selection
- All operations (pickup substitution + roomtone ops) get crossfade on both edges
- Crossfade duration is adjustable per replacement
- corrected.wav must exactly match source chapter WAV format: sample rate, bit depth, channels
- Flippers skip chapters with no CRX entries -- only navigate between chapters needing pickups
- Full chapter waveform always visible (no auto-zoom to sentence regions)
- Breadcrumb trail tracks current position (chapter + CRX entry)
- Chapter shows a completion badge/checkmark when all CRX entries are committed
- Flippers auto-advance to next incomplete chapter upon completion

### Claude's Discretion
- Mini waveform rendering approach in match boxes
- Exact crossfade default duration value
- Drag-and-drop implementation details
- Breadcrumb format and styling
- Region color coding (staged vs roomtone selection)
- Loading/progress indicators during initial pickup processing

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

## Standard Stack

### Core (Already in Codebase)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Blazor Server | .NET 9 | UI framework | Already established in workstation |
| Bit.BlazorUI | current | Component library (BitCard, BitStack, BitButton, BitTag, BitText, BitDropdown, BitProgress) | Already used throughout |
| wavesurfer.js | v7 (CDN) | Waveform visualization, regions plugin, playback | Already loaded, regions plugin included |
| FFmpeg.AutoGen | current | Audio processing via libavfilter | Already integrated in Ams.Core |
| Whisper.NET | current | Local ASR (via AsrProcessor) | Already integrated, sole ASR engine |

### Supporting (Already Available from Phase 12)
| Library/Service | Type | Purpose | Status |
|---------|------|---------|--------|
| `PolishService` | Workstation Service | Orchestration: import, match, stage, apply, revert, preview | Fully operational |
| `PickupMatchingService` | Workstation Service | ASR-based pickup-to-sentence matching with MFA refinement | Fully operational, cached |
| `PickupMfaRefinementService` | Workstation Service | MFA forced alignment on pickup recordings | Fully operational |
| `StagingQueueService` | Workstation Singleton | Non-destructive staging queue with persistence | Fully operational |
| `UndoService` | Workstation Singleton | Versioned segment backup/restore | Fully operational |
| `PolishVerificationService` | Workstation Service | Post-replacement ASR re-validation | Fully operational |
| `PreviewBufferService` | Workstation Singleton | In-memory preview buffer for AudioController | Fully operational |
| `AudioSpliceService` | Core Static | Crossfade segment replacement via FfFilterGraph | Fully operational |
| `SpliceBoundaryService` | Core Static | Silence-based boundary refinement | Fully operational |
| `CrxService` | Workstation Service | Excel CRX entry reading | Fully operational |
| `AudioController` | API Controller | Audio streaming: chapter, corrected, preview, region, file | Fully operational |
| `WaveformPlayer` | Blazor Component | wavesurfer.js wrapper with editable regions, playback, zoom | Fully operational |
| `ContextPlayer` | Blazor Component | Listen-with-context verification | Fully operational |
| `waveform-interop.js` | JS Module | All region/playback JS interop | Fully operational |

### Nothing New to Install
All required capabilities exist in the current dependency set. No new NuGet packages or npm modules needed.

## Architecture Patterns

### Current Polish Area Structure (to be replaced)
```
Components/Pages/Polish/
  Index.razor           # Chapter list (REPLACE with single-page)
  ChapterPolish.razor   # Per-chapter view (ABSORB into single page)
  BatchEditor.razor     # Batch operations (KEEP, separate route)
Components/Shared/
  PickupImporter.razor  # File input + match cards (ABSORB)
  StagingQueue.razor    # Queue display (ABSORB)
  WaveformPlayer.razor  # KEEP as-is
  ContextPlayer.razor   # KEEP as-is
  MultiWaveformView.razor # KEEP for batch editor
```

### New Architecture
```
Components/Pages/Polish/
  PickupSubstitution.razor  # NEW: Single-page workflow (/polish)
  BatchEditor.razor         # KEEP: Separate route (/polish/batch)
Components/Shared/
  PickupBox.razor           # NEW: Single match/staged/committed box
  WaveformPlayer.razor      # KEEP: Main waveform + regions
  ContextPlayer.razor       # KEEP: Post-commit verification
  MultiWaveformView.razor   # KEEP: Batch editor support
Services/
  PolishService.cs          # EXTEND: add roomtone operations, cross-chapter matching
  StagingQueueService.cs    # EXTEND: add column state (Matched/Staged/Committed)
  (all other services)      # KEEP: as-is
Models/
  PolishModels.cs           # EXTEND: add PickupBoxState, RoomtoneOperation
```

### Pattern 1: Three-Column Pipeline State Machine
**What:** Each pickup match exists in exactly one state: Matched, Staged, or Committed. The box moves between columns via user actions. The `StagedReplacement` model already has `ReplacementStatus` (Staged, Applied, Reverted, Failed) -- extend the concept with a visual column mapping.
**When to use:** All UI rendering of pickup boxes.
**Mapping:**
```
Matched  = PickupMatch exists but no StagedReplacement created yet
Staged   = StagedReplacement with Status = Staged
Committed = StagedReplacement with Status = Applied
(Reverted items move back to Matched column)
```
**Example:**
```csharp
// Column state is derived from existing service state, not a new persistence layer
private IReadOnlyList<PickupMatch> GetMatchedItems()
    => _allMatches
        .Where(m => !_stagedItems.Any(s => s.SentenceId == m.SentenceId
            && s.Status != ReplacementStatus.Reverted))
        .ToList();

private IReadOnlyList<StagedReplacement> GetStagedItems()
    => _stagedItems.Where(s => s.Status == ReplacementStatus.Staged).ToList();

private IReadOnlyList<StagedReplacement> GetCommittedItems()
    => _stagedItems.Where(s => s.Status == ReplacementStatus.Applied).ToList();
```

### Pattern 2: Cross-Chapter Flipper Navigation
**What:** Maintain a pre-computed list of chapters that have CRX entries. Flippers skip to next/previous chapter in this filtered list, load that chapter's data, and refresh the waveform.
**When to use:** The `<` and `>` flipper buttons flanking the waveform.
**Example:**
```csharp
// Pre-compute on startup: chapters with CRX entries, ordered by book position
private List<string> _chaptersWithCrx;

private void InitializeFlipperNavigation()
{
    var allCrx = CrxService.GetEntries();
    _chaptersWithCrx = Workspace.AvailableChapters
        .Where(ch => allCrx.Any(e => ChapterMatches(e.Chapter, ch)))
        .ToList();
    _currentChapterIndex = _chaptersWithCrx.IndexOf(currentChapter);
}

private async Task NavigateToChapter(int index)
{
    if (index < 0 || index >= _chaptersWithCrx.Count) return;
    _currentChapterIndex = index;
    var chapterName = _chaptersWithCrx[index];
    Workspace.SelectChapter(chapterName);
    // Refresh waveform, matches, staged items, committed items
    await LoadChapterDataAsync(chapterName);
}
```

### Pattern 3: Upfront Cross-Chapter Pickup Processing
**What:** When the user sets a pickup session file, immediately run ASR + MFA on the entire file, then match against ALL CRX entries across ALL chapters. Cache results per chapter for instant display when flipping between chapters.
**When to use:** Pickup file selection in the header bar.
**Example:**
```csharp
// Book-wide pickup processing on file selection
private Dictionary<string, List<PickupMatch>> _matchesByChapter = new();

private async Task ProcessPickupSessionAsync(string pickupFilePath)
{
    // 1. Gather ALL flagged sentences across ALL chapters with CRX entries
    var allFlagged = new List<HydratedSentence>();
    var sentenceToChapter = new Dictionary<int, string>();

    foreach (var chapter in _chaptersWithCrx)
    {
        var flagged = LoadFlaggedSentences(chapter);
        foreach (var s in flagged)
        {
            allFlagged.Add(s);
            sentenceToChapter[s.Id] = chapter;
        }
    }

    // 2. Single ASR + MFA pass on entire pickup file (already cached by PickupMatchingService)
    var allMatches = await PickupMatchingService.MatchPickupAsync(
        pickupFilePath, allFlagged, ct);

    // 3. Distribute matches by chapter for per-chapter display
    _matchesByChapter.Clear();
    foreach (var match in allMatches)
    {
        if (sentenceToChapter.TryGetValue(match.SentenceId, out var chapter))
        {
            if (!_matchesByChapter.ContainsKey(chapter))
                _matchesByChapter[chapter] = new();
            _matchesByChapter[chapter].Add(match);
        }
    }
}
```

### Pattern 4: Roomtone Operations as AudioSpliceService Extensions
**What:** Roomtone insert, replace, and delete are all variations of the existing `AudioSpliceService.ReplaceSegment`. Use the same crossfade splice infrastructure with different inputs.
**When to use:** When user selects a waveform region and chooses a roomtone operation.
**Implementation:**
```csharp
// All three roomtone operations map to ReplaceSegment:

// INSERT: splice in roomtone at a point (zero-width original range)
AudioSpliceService.ReplaceSegment(chapter, insertPoint, insertPoint, roomtoneBuffer, crossfade);

// REPLACE: replace selected region with roomtone of matching duration
var roomtone = GenerateRoomtone(regionDuration); // loop/trim roomtone file to match
AudioSpliceService.ReplaceSegment(chapter, regionStart, regionEnd, roomtone, crossfade);

// DELETE: replace selected region with nothing (zero-length replacement)
// Use empty buffer or just crossfade the before and after segments directly
var empty = new AudioBuffer(chapter.Channels, chapter.SampleRate, 0);
AudioSpliceService.ReplaceSegment(chapter, regionStart, regionEnd, empty, crossfade);
```

### Pattern 5: HTML5 Drag-and-Drop for Match-to-Staged
**What:** BitUI does not have a native drag-and-drop component. Use standard HTML5 drag events (`ondragstart`, `ondragover`, `ondrop`, `draggable`) on the pickup box elements. The Blazor interop for these events is built-in.
**When to use:** Moving boxes from Matches column to Staged column.
**Example:**
```razor
@* Match box (draggable) *@
<div class="pickup-box"
     draggable="true"
     @ondragstart="@(() => OnDragStart(match))"
     @ondragend="OnDragEnd">
    @* Box content *@
</div>

@* Staged column (drop target) *@
<div class="staged-column"
     @ondragover="OnDragOver"
     @ondragover:preventDefault="true"
     @ondrop="@(() => OnDropToStaged())">
    @* Staged items *@
</div>

@code {
    private PickupMatch? _draggedMatch;

    private void OnDragStart(PickupMatch match) => _draggedMatch = match;
    private void OnDragEnd() => _draggedMatch = null;
    private void OnDragOver() { } // Required for drop to work

    private void OnDropToStaged()
    {
        if (_draggedMatch is null) return;
        StageMatch(_draggedMatch);
        _draggedMatch = null;
    }
}
```

### Pattern 6: Mini Waveform Thumbnails in Match Boxes
**Recommendation (Claude's Discretion):** Use a small `<canvas>` element per match box rendered via JS, NOT a full wavesurfer.js instance. Full wavesurfer instances are heavyweight (each creates a MediaElement, registers plugins, etc.). For thumbnails, decode the pickup segment's waveform data once on the server, generate a normalized amplitude array (e.g., 100 points), and render via a lightweight JS function that draws bars on a canvas.

**Why not wavesurfer for thumbnails:** Creating 10+ wavesurfer instances (one per match box) would be resource-heavy and slow. A simple canvas-based mini waveform is the standard pattern for audio thumbnail displays.

**Example:**
```javascript
// Lightweight mini waveform renderer
export function drawMiniWaveform(canvasId, amplitudeData, color) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    const w = canvas.width;
    const h = canvas.height;
    ctx.clearRect(0, 0, w, h);
    ctx.fillStyle = color || '#4a9eff';
    const barW = w / amplitudeData.length;
    for (let i = 0; i < amplitudeData.length; i++) {
        const barH = amplitudeData[i] * h;
        ctx.fillRect(i * barW, (h - barH) / 2, Math.max(1, barW - 1), barH);
    }
}
```

### Anti-Patterns to Avoid
- **Multiple wavesurfer instances for match box thumbnails:** Each wavesurfer instance creates a MediaElement and loads audio via HTTP. Use canvas-based mini waveforms instead (see Pattern 6).
- **Navigating away from the Polish page:** The entire workflow is a single page. Chapter changes happen via flippers within the page, not via URL navigation. Only the initial route is `/polish`.
- **Processing pickups per-chapter:** The locked decision says process ALL chapters upfront when pickup file is set. Do not re-run ASR/MFA when flipping chapters.
- **Separate persistence for column state:** The Match/Staged/Committed state is derivable from `PickupMatch` list + `StagingQueueService` state. Do not create a separate state file.
- **Encoding corrected.wav at default 16-bit:** Must match source format. See "Audio Format Matching" section.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Pickup-to-sentence matching | Custom text comparison | `PickupMatchingService` (already built) | MFA-refined timings, cached, battle-tested |
| Audio crossfade splice | Manual sample math | `AudioSpliceService.ReplaceSegment` | FFmpeg acrossfade filter handles all edge cases |
| Boundary refinement | Manual silence scanning | `SpliceBoundaryService.RefineBoundaries` | Silence detection + energy snapping, already working |
| Undo/revert | Custom backup system | `UndoService` | Versioned segment files, manifest-based, persisted |
| Non-destructive staging | Custom queue | `StagingQueueService` | JSON-persisted, timing cascade, status transitions |
| ASR re-validation | Custom text comparison | `PolishVerificationService` | Whisper.NET + Levenshtein, threshold-based |
| Waveform draggable regions | Custom canvas drag | wavesurfer.js regions plugin (drag: true, resize: true) | Already in `addEditableRegion` JS interop |
| Mini waveform thumbnails | Full wavesurfer instances | Canvas-based amplitude rendering | 10x lighter, no MediaElement overhead |
| Drag-and-drop | Third-party library | HTML5 native `draggable` + Blazor events | No external dependency needed for simple column-to-column drag |

**Key insight:** Phase 12 built nearly all the service infrastructure. Phase 13 is primarily a UI redesign that wires existing services into a new layout, plus three focused extensions: (1) cross-chapter matching, (2) roomtone operations, and (3) 24-bit audio format preservation.

## Common Pitfalls

### Pitfall 1: 24-bit Audio Format Not Supported in FfEncoder
**What goes wrong:** `FfEncoder.ResolveEncoding` only supports 16-bit (PCM_S16LE) and 32-bit (PCM_F32LE). Audiobook source files are commonly 24-bit/44.1kHz. When `PolishService.PersistCorrectedBuffer` calls `AudioProcessor.EncodeWav`, it defaults to 16-bit, silently downgrading the audio.
**Why it happens:** The `AudioEncodeOptions` record only has `TargetSampleRate` and `TargetBitDepth`. `TargetBitDepth` defaults to null, and `FfEncoder` falls back to 16. The source format metadata (`SourceSampleFormat`) is tracked in `AudioBufferMetadata` but never consulted during encoding.
**How to avoid:** Add 24-bit support to `FfEncoder.ResolveEncoding` (PCM_S24LE / AV_CODEC_ID_PCM_S24LE with AV_SAMPLE_FMT_S32 since there's no native 24-bit sample format in FFmpeg -- encode uses 32-bit samples truncated to 24-bit in the WAV container). Then modify `PersistCorrectedBuffer` to read the source file's bit depth from `AudioBufferMetadata.SourceSampleFormat` and pass it through `AudioEncodeOptions`.
**Warning signs:** Output WAV file has different bit depth than source (probe with `ffprobe` shows 16-bit when source is 24-bit).

### Pitfall 2: Cross-Chapter Sentence ID Collisions
**What goes wrong:** When matching pickups across ALL chapters upfront, sentence IDs may not be globally unique -- chapter 1 and chapter 2 could both have a sentence ID 5.
**Why it happens:** `HydratedSentence.Id` is per-chapter, not global.
**How to avoid:** Use a composite key of (chapterStem, sentenceId) when distributing matches across chapters. The `PickupMatch.SentenceId` alone is insufficient -- must track which chapter each match belongs to.
**Warning signs:** Pickup matched to wrong chapter's sentence, or duplicate entries in the staging queue.

### Pitfall 3: Waveform Reload After Commit
**What goes wrong:** After committing a pickup replacement, the chapter waveform needs to reload from the updated `corrected.wav` but stays showing the old audio.
**Why it happens:** wavesurfer.js caches the audio URL. Simply updating the URL with the same path won't trigger a reload.
**How to avoid:** Append a version query parameter to the audio URL (already done in ChapterPolish.razor with `_correctedVersion++`). The `GetAudioUrl()` pattern with `?v={version}` forces wavesurfer to reload.
**Warning signs:** Waveform doesn't update after commit; playback still plays old audio.

### Pitfall 4: Flipper Navigation Without Deallocating Previous Chapter
**What goes wrong:** Flipping through 20+ chapters without deallocating audio buffers causes memory pressure.
**Why it happens:** `BlazorWorkspace.SelectChapter` opens a new chapter context but `ChapterContextHandle`s may keep previous chapter's audio buffer in memory.
**How to avoid:** When navigating away from a chapter via flippers, explicitly call `handle.Chapter.Audio.Deallocate("corrected")` on the previous chapter's handle. The `BlazorWorkspace` already caches handles in `_chapterHandles` so they can be accessed for cleanup.
**Warning signs:** Memory usage steadily increases as user flips through chapters.

### Pitfall 5: Empty Buffer in Splice for Delete Operation
**What goes wrong:** The "delete selection" roomtone operation creates a zero-length AudioBuffer. `AudioSpliceService.ReplaceSegment` checks `endSec > startSec` and `replacement.Length` in the crossfade logic, which may fail or produce unexpected results with a zero-length buffer.
**Why it happens:** The delete operation means "remove this region, pull content together" -- there's no replacement audio.
**How to avoid:** For delete, don't call `ReplaceSegment` with a zero-length replacement. Instead, trim the before and after segments and crossfade them directly: `Crossfade(before, after, crossfadeSec)`. This is a targeted helper alongside `ReplaceSegment`.
**Warning signs:** Exception from `AudioSpliceService` or `AudioBuffer.Concat` with zero-length buffers.

### Pitfall 6: Roomtone File Duration Mismatch for Replace Operation
**What goes wrong:** The user selects a 5-second region but the roomtone file is only 2 seconds long. Directly using the roomtone buffer for a "replace" operation creates a shorter result, shifting all downstream timings unexpectedly.
**Why it happens:** Roomtone files are typically short recordings (10-30 seconds of room ambient noise).
**How to avoid:** For "replace with roomtone," loop the roomtone buffer to match the selected region's duration. Trim to exact length after looping. This produces a same-duration replacement with zero timing shift.
**Warning signs:** Timeline jumps or downstream sentence desync after roomtone replace.

### Pitfall 7: Single-Page Route Conflict with Existing Polish Routes
**What goes wrong:** The new `PickupSubstitution.razor` at `/polish` conflicts with existing `Index.razor` at the same route, and `ChapterPolish.razor` at `/polish/{ChapterName}`.
**Why it happens:** Blazor routing requires unique route patterns per page.
**How to avoid:** Remove the `@page "/polish"` from `Index.razor` and `@page "/polish/{ChapterName}"` from `ChapterPolish.razor` when the new page replaces them. Keep `BatchEditor.razor` at `/polish/batch`.
**Warning signs:** Blazor ambiguous route exception at startup.

## Code Examples

### Roomtone Loop-to-Duration Helper
```csharp
// Generate roomtone buffer of a specific duration by looping the source file
public static AudioBuffer GenerateRoomtoneFill(AudioBuffer roomtone, double targetDurationSec)
{
    var targetLength = (int)(targetDurationSec * roomtone.SampleRate);
    if (targetLength <= 0) return roomtone;

    var result = new AudioBuffer(roomtone.Channels, roomtone.SampleRate, targetLength, roomtone.Metadata);

    int offset = 0;
    while (offset < targetLength)
    {
        int copyLen = Math.Min(roomtone.Length, targetLength - offset);
        for (int ch = 0; ch < roomtone.Channels; ch++)
        {
            Array.Copy(roomtone.Planar[ch], 0, result.Planar[ch], offset, copyLen);
        }
        offset += copyLen;
    }

    return result;
}
```

### Delete Region (Crossfade Join)
```csharp
// Delete a region: trim before + after, crossfade directly
public static AudioBuffer DeleteRegion(
    AudioBuffer original, double startSec, double endSec, double crossfadeSec = 0.030)
{
    var before = AudioProcessor.Trim(original, TimeSpan.Zero, TimeSpan.FromSeconds(startSec));
    var after = AudioProcessor.Trim(original, TimeSpan.FromSeconds(endSec));

    // Direct crossfade of before and after (no replacement in between)
    return Crossfade(before, after, crossfadeSec, "tri");
}
```

### 24-bit WAV Encoding Extension
```csharp
// Add to FfEncoder.ResolveEncoding:
private static (AVCodecID CodecId, AVSampleFormat SampleFormat) ResolveEncoding(int bitDepth)
{
    return bitDepth switch
    {
        16 => (AVCodecID.AV_CODEC_ID_PCM_S16LE, AVSampleFormat.AV_SAMPLE_FMT_S16),
        24 => (AVCodecID.AV_CODEC_ID_PCM_S24LE, AVSampleFormat.AV_SAMPLE_FMT_S32), // 24-bit in 32-bit container
        32 => (AVCodecID.AV_CODEC_ID_PCM_F32LE, AVSampleFormat.AV_SAMPLE_FMT_FLT),
        _ => throw new NotSupportedException($"Unsupported PCM bit depth {bitDepth}.")
    };
}

// In PersistCorrectedBuffer, determine source bit depth:
private void PersistCorrectedBuffer(AudioBuffer buffer)
{
    var descriptor = handle.Chapter.Descriptor;
    var sourcePath = Path.Combine(descriptor.RootPath, $"{descriptor.ChapterId}.treated.wav");

    // Probe source to get original bit depth
    int? sourceBitDepth = null;
    if (File.Exists(sourcePath))
    {
        var info = AudioProcessor.Probe(sourcePath);
        sourceBitDepth = InferBitDepthFromSampleFormat(info.SampleFormat);
    }

    var options = new AudioEncodeOptions(
        TargetSampleRate: buffer.SampleRate,
        TargetBitDepth: sourceBitDepth);

    AudioProcessor.EncodeWav(correctedPath, buffer, options);
}
```

### Flipper Navigation State
```csharp
// Flipper-aware chapter list
private int _currentIndex;
private List<ChapterCrxInfo> _chaptersWithCrx = new();

private record ChapterCrxInfo(string ChapterName, string Stem, int CrxCount, bool AllCommitted);

private bool CanGoNext => _currentIndex < _chaptersWithCrx.Count - 1;
private bool CanGoPrev => _currentIndex > 0;

private async Task GoNext()
{
    if (!CanGoNext) return;
    await NavigateToChapter(_currentIndex + 1);
}

private async Task GoPrev()
{
    if (!CanGoPrev) return;
    await NavigateToChapter(_currentIndex - 1);
}

// Auto-advance on chapter completion
private void CheckCompletionAndAdvance()
{
    var currentMatches = GetMatchesForCurrentChapter();
    var currentCommitted = GetCommittedItems();

    if (currentMatches.Count > 0 && currentCommitted.Count == currentMatches.Count)
    {
        // All CRX entries committed -- mark complete, advance
        _chaptersWithCrx[_currentIndex] = _chaptersWithCrx[_currentIndex] with { AllCommitted = true };
        if (CanGoNext) _ = GoNext();
    }
}
```

### Breadcrumb Trail
```csharp
// Breadcrumb format: "Chapter 5 (3/12) - Entry 2 of 4"
private string GetBreadcrumbText()
{
    var chapterPos = _currentIndex + 1;
    var totalChapters = _chaptersWithCrx.Count;
    var chapterName = _chaptersWithCrx[_currentIndex].ChapterName;
    var crxCount = _chaptersWithCrx[_currentIndex].CrxCount;
    var committed = GetCommittedItems().Count;

    return $"{chapterName} ({chapterPos}/{totalChapters}) - {committed}/{crxCount} committed";
}
```

## Discretionary Recommendations

### Mini Waveform Rendering (Claude's Discretion)
**Recommendation:** Use a canvas-based amplitude bar renderer (not wavesurfer instances). Generate a normalized amplitude array (100-120 data points per match box) server-side from the pickup segment, send as a float array via JS interop, render on a small `<canvas>` element (200x40px). This is the standard pattern for audio thumbnails in web audio applications.

**Implementation:** Add an endpoint `/api/audio/waveform-data?path=X&start=Y&end=Z&points=100` that decodes the segment, computes RMS amplitudes per block, and returns a JSON array. The JS function `drawMiniWaveform` renders it on the canvas.

### Crossfade Default Duration (Claude's Discretion)
**Recommendation:** Default to **30ms** (0.030 seconds) with **triangle (linear)** curve. This is already the default in `PolishService.StageReplacement` and `AudioSpliceService`. Expose a per-replacement slider (5ms - 200ms) in the box detail view, with presets: Tight (15ms), Normal (30ms), Smooth (60ms).

### Drag-and-Drop Implementation (Claude's Discretion)
**Recommendation:** Use HTML5 native `draggable` attribute with Blazor `@ondragstart`, `@ondragover`, `@ondrop` events. No third-party library needed. Add a CSS class for visual feedback during drag (`opacity: 0.5` on source, border highlight on drop target). The interaction is simple: single source to single target column.

### Breadcrumb Format (Claude's Discretion)
**Recommendation:** Use `BitStack Horizontal` with `BitText` segments separated by chevron icons. Format: `[Chapter Name] > [N of M entries] > [committed/total]`. The entire breadcrumb row is compact (single line).

### Region Color Coding (Claude's Discretion)
**Recommendation:**
- **Staged pickup region:** Green with 30% alpha (`rgba(59, 200, 120, 0.3)`) -- already the default in `addEditableRegion`
- **Roomtone selection region:** Blue with 30% alpha (`rgba(59, 130, 246, 0.3)`) -- distinct from pickup regions
- **Active/selected region:** Brighter variant of same color with 50% alpha
- **Committed region:** Gray/muted (`rgba(150, 150, 150, 0.2)`) -- shows where replacements occurred but no longer editable

### Loading/Progress During Pickup Processing (Claude's Discretion)
**Recommendation:** Show a `BitProgress` bar (linear, not circular) below the pickup file selector in the header bar. Show status text: "Splitting by silence...", "Running ASR...", "Running MFA alignment...", "Matching to CRX entries...". Each stage updates via `StateHasChanged()` during the async processing. Total time for a typical session file (20 pickups, ~5 minutes of audio): 30-90 seconds.

## State of the Art

| Old Approach (Phase 12) | New Approach (Phase 13) | Impact |
|--------------------------|-------------------------|--------|
| Chapter list page -> per-chapter polish | Single-page with flippers | No context-switching, faster workflow |
| Import pickup per chapter | Process all chapters upfront | One-time ASR/MFA, instant chapter flips |
| Two-column layout (importer + queue) | Three-column pipeline (Match/Stage/Commit) | Clearer visual state machine |
| Default 16-bit WAV output | Format-matching output (24-bit support) | No quality loss in corrected audio |
| No roomtone operations | Insert/Replace/Delete roomtone | Manual audio cleanup without external tools |
| PickupImporter + StagingQueue components | Unified PickupBox component | Single reusable box that moves through columns |

**Existing capabilities that just need wiring:**
- `PolishService.ImportPickupAsync` -- works, just needs to be called with all chapters' sentences
- `PolishService.StageReplacement` -- works, creates staging queue entries
- `PolishService.ApplyReplacementAsync` -- works, writes corrected.wav, cascades timing
- `PolishService.RevertReplacementAsync` -- works, restores from undo backup
- `PolishService.GeneratePreview` -- works, in-memory splice for audition
- `WaveformPlayer.AddEditableRegion` -- works, draggable/resizable regions
- `WaveformPlayer.PlaySegment` -- works, plays time range
- `AudioController.GetCorrectedChapterAudio` -- works, serves corrected or treated

## Open Questions

1. **AudioInfo.SampleFormat reliability for bit depth inference**
   - What we know: `FfDecoder.Probe` returns `AudioInfo` which includes format details. The `SourceSampleFormat` in `AudioBufferMetadata` records the source format string (e.g., "s24", "s16", "flt").
   - What's unclear: Whether `AudioInfo` from `Probe` directly exposes bit depth as an integer, or if we need to parse it from the sample format string.
   - Recommendation: Check `AudioInfo` struct. If bit depth isn't directly available, parse from codec name (e.g., "pcm_s24le" -> 24). Alternatively, add `BitsPerSample` to `AudioInfo`.

2. **Roomtone insert at a single point (zero-width splice)**
   - What we know: `AudioSpliceService.ReplaceSegment` requires `endSec > startSec`. An "insert" operation means inserting audio at a single point (start == end).
   - What's unclear: Whether `ReplaceSegment` handles the edge case where start equals end gracefully.
   - Recommendation: Add a dedicated `InsertAtPoint` method that trims before/after at the same point and crossfades with the insertion, avoiding the start < end validation.

3. **Concurrent pickup processing and UI responsiveness**
   - What we know: ASR + MFA on a session file takes 30-90 seconds. The Blazor Server circuit must stay alive.
   - What's unclear: Whether the current `PickupMatchingService` progress can be reported incrementally to the UI.
   - Recommendation: Run the processing in a `Task.Run` with `CancellationToken` from the Blazor circuit. Use `InvokeAsync(StateHasChanged)` for periodic progress updates. The existing ASR cache means re-imports are instant.

## Sources

### Primary (HIGH confidence)
- Codebase: `PolishService.cs` -- full orchestration service with import, stage, apply, revert, preview
- Codebase: `PickupMatchingService.cs` -- ASR + MFA matching with caching
- Codebase: `PickupMfaRefinementService.cs` -- MFA forced alignment refinement pipeline
- Codebase: `StagingQueueService.cs` -- non-destructive queue with persistence, timing cascade
- Codebase: `UndoService.cs` -- versioned segment backup with manifest
- Codebase: `AudioSpliceService.cs` -- crossfade splice via FfFilterGraph
- Codebase: `SpliceBoundaryService.cs` -- silence-based boundary refinement
- Codebase: `FfEncoder.cs` -- WAV encoding, currently only 16-bit and 32-bit (lines 370-377)
- Codebase: `AudioEncodeOptions` -- record with TargetSampleRate and TargetBitDepth
- Codebase: `AudioBufferMetadata.cs` -- tracks SourceSampleFormat
- Codebase: `waveform-interop.js` -- full JS interop with editable regions, playback, sync
- Codebase: `WaveformPlayer.razor` -- Blazor wrapper with region callbacks
- Codebase: `AudioController.cs` -- all audio streaming endpoints
- Codebase: `ChapterPolish.razor` -- existing per-chapter polish UI (reference for patterns)
- Codebase: `PickupImporter.razor` -- existing pickup import UI (reference)
- Codebase: `StagingQueue.razor` -- existing staging queue UI (reference)
- Codebase: `HeaderControls.razor` -- existing header bar layout
- Codebase: `CrxService.cs` -- CRX entry reading from Excel
- Codebase: `BlazorWorkspace.cs` -- chapter selection and context management

### Secondary (MEDIUM confidence)
- Phase 12 Research (`12-RESEARCH.md`) -- foundation architecture patterns
- Phase 12 Context (`12-CONTEXT.md`) -- original design decisions

### Tertiary (LOW confidence)
- None -- all findings verified against codebase.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all libraries already in codebase, zero new dependencies
- Architecture (UI redesign): HIGH -- patterns follow existing Blazor conventions, all components identified
- Service layer reuse: HIGH -- Phase 12 services verified operational, minimal extensions needed
- Audio format matching (24-bit): HIGH -- gap identified precisely (FfEncoder line 370-377), fix approach verified against FFmpeg API
- Roomtone operations: HIGH -- all three ops map directly to existing AudioSpliceService patterns
- Cross-chapter matching: HIGH -- PickupMatchingService accepts any sentence list, just needs broader input
- Drag-and-drop: MEDIUM -- HTML5 native approach standard but not yet used in this codebase
- Mini waveform thumbnails: MEDIUM -- canvas approach standard but implementation is new

**Research date:** 2026-02-24
**Valid until:** 2026-03-24 (stable -- all technologies already integrated)
