# Phase 12: Polish Area Foundation - Research

**Researched:** 2026-02-23
**Domain:** Blazor Server audio editing UI + Ams.Core audio manipulation + wavesurfer.js multi-instance
**Confidence:** HIGH

## Summary

Phase 12 builds the Polish area of the Blazor workstation, transforming the current placeholder into a full take-replacement workflow with batch editing foundations. The core technical challenges are: (1) building an ASR-based pickup matching service that auto-maps pickup recordings to CRX target sentences, (2) implementing audio splice/crossfade operations in Ams.Core using the existing FFmpeg filter graph infrastructure, (3) extending the wavesurfer.js interop layer to support draggable/resizable regions for boundary adjustment and multiple synchronized waveform instances for the multi-chapter stacked view, and (4) building a non-destructive staging queue pattern with undo support.

The existing codebase provides strong foundations. `AudioProcessor` already has `Trim`, `FadeIn`, and `AudioBuffer.Concat`. The `FfFilterGraph` already supports multiple labeled inputs (via `WithInput`) and a `Custom()` clause, which can compose the FFmpeg `acrossfade` filter for crossfade splicing. The `AsrClient` (Nemo) and `AsrProcessor` (Whisper) provide two ASR paths for pickup text recognition. The wavesurfer.js v7 regions plugin is already loaded via CDN and the JS interop already includes `initRegions`, `addRegion`, `clearRegions`, `removeRegion`, `highlightRegion`, and `playRegion` -- though regions are currently non-draggable/non-resizable and need extension for boundary editing.

**Primary recommendation:** Build a `PolishService` in the workstation and an `AudioSpliceService` in Ams.Core. The splice service handles crossfade segment replacement using `FfFilterGraph` with two inputs + `acrossfade`. The Polish service orchestrates pickup import, ASR matching, staging, and application. Extend `WaveformPlayer` to support draggable regions via new JS interop functions, and create a `MultiWaveformView` component for the stacked chapter layout.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Primary source: pickup recordings (separate audio files recorded to fix errors)
- Accept both: single session file (multiple pickups in one recording) and individual pickup files (one per sentence)
- Session file segmentation: ASR-based matching -- run ASR on the pickup file, auto-match recognized text to CRX target sentences
- User can fine-tune pickup boundaries/assignments via waveform regions after auto-matching
- Staging queue: replacements are staged non-destructively, user applies individually or in batch at their discretion
- No forced all-or-nothing -- user decides when and how many to apply
- Batch operations for Phase 12: pickup replacement, batch renaming, batch shifting of chapter readings/headers, batch pre+post roll standardization
- DSP pipeline: placeholder/hooks only -- actual DSP batch processing is its own future phase
- Target selection: manual multi-select of chapters to include in batch operations
- Multi-waveform editor: selected chapters loaded simultaneously in stacked vertical layout (DAW-style), synchronized playhead/markers across all visible waveforms
- Efficient partial buffer loading required -- load regions, not full chapters, since multiple chapters are active at once
- All batch operations use non-destructive staging -- preview changes, apply when ready, original treated audio intact until commit
- All edits must have crossfades applied at splice points (smooth transitions, no clicks)
- All audio editing goes through the FFmpeg integration in Ams.Core
- If editing functionality (splice, crossfade, segment replacement) doesn't exist in Ams.Core yet, it needs to be added as part of this phase
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

### Deferred Ideas (OUT OF SCOPE)
- Auditioning other in-book utterances of the same word/phrase as replacement candidates -- future enhancement to take replacement
- DSP batch pipeline (batch DSP processing across chapters) -- deserves its own phase due to intricacy
- Phase 13 (Pickup Substitution) may overlap with take replacement -- reconcile scope during planning
</user_constraints>

## Standard Stack

### Core (Already in Codebase)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Blazor Server | .NET 9 | UI framework | Already established in workstation |
| Bit.BlazorUI | current | Component library | Already used throughout workstation |
| wavesurfer.js | v7 (CDN) | Waveform visualization & regions | Already loaded, regions plugin included |
| FFmpeg.AutoGen | current | Audio processing via libavfilter | Already integrated in Ams.Core |
| ClosedXML | current | Excel CRX tracking | Already used by CrxService |
| Whisper.NET | current | Local ASR (Whisper) | Already integrated via AsrProcessor |

### Supporting (Already Available)
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| AsrClient (Nemo HTTP) | internal | External ASR service | Pickup file transcription via Nemo |
| AsrProcessor (Whisper.NET) | internal | Local ASR engine | Pickup file transcription via Whisper |
| FfFilterGraph | internal | FFmpeg filter composition | All audio splice/crossfade operations |
| AudioBuffer | internal | In-memory audio representation | Buffer manipulation, concat, encode |
| AudioProcessor | internal | Trim, FadeIn, DetectSilence, etc. | Segment extraction and analysis |

### Nothing New to Install
All required capabilities exist in the current dependency set. No new NuGet packages or npm modules needed.

## Architecture Patterns

### Recommended Project Structure
```
host/Ams.Workstation.Server/
├── Components/
│   ├── Pages/Polish/
│   │   ├── Index.razor              # Polish area landing (chapter list + batch tools)
│   │   ├── ChapterPolish.razor      # Per-chapter polish view (staging + waveform)
│   │   └── BatchEditor.razor        # Multi-chapter batch operations view
│   └── Shared/
│       ├── WaveformPlayer.razor     # Enhanced: draggable regions, zoom regions
│       ├── PickupImporter.razor     # Pickup file upload + ASR matching UI
│       ├── StagingQueue.razor       # Non-destructive edit queue display
│       ├── MultiWaveformView.razor  # Stacked DAW-style multi-chapter waveforms
│       └── ContextPlayer.razor      # Listen-with-context verification player
├── Services/
│   ├── PolishService.cs             # Orchestrator: import, match, stage, apply
│   ├── PickupMatchingService.cs     # ASR-based pickup-to-sentence matching
│   ├── StagingQueueService.cs       # Non-destructive staging state management
│   └── BatchOperationService.cs     # Batch rename, shift, pre/post roll ops
└── Models/
    ├── PolishModels.cs              # PickupMatch, StagedReplacement, etc.
    └── BatchModels.cs               # BatchOperation, BatchTarget, etc.

host/Ams.Core/
├── Audio/
│   └── AudioSpliceService.cs        # NEW: Splice, crossfade, segment replacement
└── (existing files unchanged)
```

### Pattern 1: Non-Destructive Staging Queue
**What:** All audio edits are staged as descriptors (not applied immediately). Each staged edit records: target chapter, sentence range, replacement buffer source, crossfade params. Only when user clicks "Apply" does the actual audio manipulation occur.
**When to use:** Every edit operation in the Polish area.
**Example:**
```csharp
// Staged replacement descriptor -- does NOT modify audio
public sealed record StagedReplacement(
    string Id,
    string ChapterStem,
    int SentenceId,
    double OriginalStartSec,
    double OriginalEndSec,
    string PickupSourcePath,      // path to pickup WAV
    double PickupStartSec,        // pickup boundary (post-adjustment)
    double PickupEndSec,
    double CrossfadeDurationSec,  // default 0.030 (30ms)
    string CrossfadeCurve,        // default "tri" (triangle/linear)
    DateTime StagedAtUtc,
    ReplacementStatus Status);    // Staged, Applied, Reverted

public enum ReplacementStatus { Staged, Applied, Reverted, Failed }
```

### Pattern 2: Crossfade Splice via FfFilterGraph
**What:** Replace a segment in a chapter audio by trimming three parts (before, pickup, after) and joining with crossfade at splice points. Uses the existing `FfFilterGraph` multi-input capability with FFmpeg's `acrossfade` filter.
**When to use:** When applying any staged replacement.
**Example:**
```csharp
// In AudioSpliceService (new, in Ams.Core)
public AudioBuffer SpliceWithCrossfade(
    AudioBuffer original,
    double spliceStartSec,
    double spliceEndSec,
    AudioBuffer replacement,
    double crossfadeSec = 0.030,
    string curve = "tri")
{
    // 1. Trim before-segment: [0 .. spliceStart]
    var before = AudioProcessor.Trim(original, TimeSpan.Zero, TimeSpan.FromSeconds(spliceStartSec));

    // 2. Trim after-segment: [spliceEnd .. duration]
    var after = AudioProcessor.Trim(original, TimeSpan.FromSeconds(spliceEndSec));

    // 3. Crossfade: before -> replacement
    var firstJoin = CrossfadeBuffers(before, replacement, crossfadeSec, curve);

    // 4. Crossfade: (before+replacement) -> after
    var result = CrossfadeBuffers(firstJoin, after, crossfadeSec, curve);

    return result;
}

private AudioBuffer CrossfadeBuffers(AudioBuffer a, AudioBuffer b, double durationSec, string curve)
{
    // Use FfFilterGraph with two labeled inputs and acrossfade
    var spec = $"[a]aformat=sample_fmts=flt[af];[b]aformat=sample_fmts=flt[bf];" +
               $"[af][bf]acrossfade=d={durationSec:F6}:c1={curve}:c2={curve}[out]";

    return FfFilterGraphRunner.Apply(
        new[] {
            new FfFilterGraphRunner.GraphInput("a", a),
            new FfFilterGraphRunner.GraphInput("b", b)
        },
        spec);
}
```

### Pattern 3: Partial Buffer Loading for Multi-Waveform
**What:** When loading multiple chapters simultaneously in the multi-waveform view, load only the visible region of each chapter audio (e.g., around flagged sentences) rather than the full file. Use `AudioProcessor.Decode` with `Start`/`Duration` options, or `AudioProcessor.Trim` on already-loaded buffers.
**When to use:** Multi-waveform stacked view with 3+ chapters loaded.
**Example:**
```csharp
// Load only a region around a flagged sentence
var decodeOptions = new AudioDecodeOptions(
    Start: TimeSpan.FromSeconds(Math.Max(0, sentenceStart - 5.0)),
    Duration: TimeSpan.FromSeconds(sentenceEnd - sentenceStart + 10.0));
var regionBuffer = AudioProcessor.Decode(chapterAudioPath, decodeOptions);
```

### Pattern 4: ASR-Based Pickup Matching
**What:** Run ASR on a pickup recording file, extract recognized text with timings, then fuzzy-match each recognized segment against the CRX target sentence book text. For session files with multiple pickups, use silence detection to segment first, then ASR each segment.
**When to use:** Importing pickup recordings (single or session files).
**Example:**
```csharp
// PickupMatchingService
public async Task<List<PickupMatch>> MatchPickupToSentences(
    AudioBuffer pickupBuffer,
    IReadOnlyList<HydratedSentence> targetSentences,
    CancellationToken ct)
{
    // 1. Run ASR on pickup
    var asrReady = AsrAudioPreparer.PrepareForAsr(pickupBuffer);
    var asrResponse = await RunAsrAsync(asrReady, ct);

    // 2. For session files: segment by silence gaps
    var segments = SegmentByPauses(asrResponse, pauseThresholdSec: 2.0);

    // 3. Fuzzy-match each segment to target sentences
    var matches = new List<PickupMatch>();
    foreach (var segment in segments)
    {
        var bestMatch = FindBestSentenceMatch(segment.Text, targetSentences);
        if (bestMatch != null)
        {
            matches.Add(new PickupMatch(
                SentenceId: bestMatch.Id,
                PickupStartSec: segment.StartSec,
                PickupEndSec: segment.EndSec,
                Confidence: bestMatch.Score,
                RecognizedText: segment.Text));
        }
    }
    return matches;
}
```

### Pattern 5: Undo via Versioned Segment Files
**What:** Before applying a replacement, save the original segment to a versioned file in a `.polish-undo` subdirectory. Each applied replacement gets a backup file named `{chapterStem}.sent{id}.v{version}.wav`. Revert loads the backup and re-splices.
**When to use:** Every Apply operation in the staging queue.
**Recommendation (Claude's Discretion):** Use versioned files on disk rather than in-memory snapshots. Rationale: chapters are large (30+ minutes of audio at 44.1kHz), holding multiple full-chapter snapshots in memory is wasteful. Storing only the replaced segment (typically 2-15 seconds) as a small WAV file is negligible on disk.

### Anti-Patterns to Avoid
- **Direct file mutation without staging:** Never modify the treated WAV directly. Always go through the staging queue so operations are non-destructive and undoable.
- **Loading full chapter buffers for multi-waveform:** Multiple 30+ minute WAV files decoded simultaneously will exhaust memory. Use partial decoding with `AudioDecodeOptions.Start/Duration`.
- **Synchronous ASR calls on UI thread:** ASR (whether Nemo HTTP or Whisper local) is slow. Always use `async Task` with progress indication.
- **Custom audio manipulation outside Ams.Core:** All audio operations MUST go through `AudioProcessor` / `FfFilterGraph`. No raw sample manipulation in the Workstation project.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Audio crossfade | Manual sample-level fade math | FFmpeg `acrossfade` via `FfFilterGraph` | Handles all edge cases (different sample rates, channel counts), supports multiple fade curves |
| Audio segment extraction | Manual array slicing | `AudioProcessor.Trim` (FFmpeg `atrim`) | Handles sub-sample precision, maintains metadata |
| Audio concatenation | Manual array copy | `AudioBuffer.Concat` | Already validates format matching, handles metadata |
| Silence detection for session segmentation | Amplitude threshold scanning | `AudioProcessor.DetectSilence` (FFmpeg `silencedetect`) | Proven reliability, configurable threshold/duration |
| ASR text matching | Simple string equality | Levenshtein distance / token-level WER | Pickup recordings rarely match book text exactly -- ASR errors, pronunciation differences, natural variation |
| WAV encoding | Manual header writing | `AudioProcessor.EncodeWav` / `buffer.ToWavStream()` | Correct header generation for any sample rate/channels |

**Key insight:** The Ams.Core FFmpeg integration (`FfFilterGraph` + `FfFilterGraphRunner`) already supports multi-input filter graphs. The `acrossfade` filter takes two audio inputs and produces a crossfaded output. This is the correct approach for splice-point crossfades rather than attempting manual sample-level fade math.

## Common Pitfalls

### Pitfall 1: Memory Pressure from Multiple Chapter Buffers
**What goes wrong:** Loading 5+ full chapters (each ~200MB decoded) simultaneously for the multi-waveform view exhausts available RAM.
**Why it happens:** `AudioProcessor.Decode` loads the entire file into memory as float32 planar arrays.
**How to avoid:** Use `AudioDecodeOptions(Start, Duration)` for partial decoding. For multi-waveform view, load only the region around each flagged sentence (e.g., sentence timing +/- 10 seconds padding). Deallocate buffers when chapters are deselected using `AudioBufferManager.Deallocate()`.
**Warning signs:** Application slowdown, GC pressure, out-of-memory exceptions.

### Pitfall 2: Crossfade Duration vs Segment Length
**What goes wrong:** If the crossfade duration exceeds the length of either segment being joined, FFmpeg's `acrossfade` filter fails or produces artifacts.
**Why it happens:** Very short pickup recordings (< 100ms) or tiny segment remainders at splice boundaries.
**How to avoid:** Clamp crossfade duration to `min(crossfadeSec, segmentA_duration * 0.3, segmentB_duration * 0.3)`. Never crossfade more than 30% of either segment.
**Warning signs:** FFmpeg filter graph errors, audible artifacts at splice points.

### Pitfall 3: Sample Rate Mismatch Between Chapter and Pickup
**What goes wrong:** Pickup recordings may have a different sample rate than the treated chapter audio. `AudioBuffer.Concat` and `acrossfade` require matching formats.
**Why it happens:** Pickup files recorded with different equipment/settings than the original chapter.
**How to avoid:** Always resample pickup audio to match the chapter's sample rate before any splice operation: `AudioProcessor.Resample(pickupBuffer, chapterBuffer.SampleRate)`.
**Warning signs:** `InvalidOperationException` from `AudioBuffer.Concat` about mismatched SampleRate.

### Pitfall 4: Blazor Server Circuit Disconnection During Long Operations
**What goes wrong:** ASR processing on a pickup file can take 30+ seconds. If the user navigates away or the circuit disconnects, the operation is orphaned.
**Why it happens:** Blazor Server keeps state on the server; circuit timeout is typically 5 minutes but operations should handle disconnection gracefully.
**How to avoid:** Use `CancellationToken` from the circuit. Run long operations as background tasks with progress polling. Show a progress indicator with the option to cancel.
**Warning signs:** Ghost ASR processes consuming resources after user navigation.

### Pitfall 5: Timing Drift After Splice
**What goes wrong:** After replacing a segment, all subsequent sentence timings in the chapter shift because the replacement may have a different duration than the original.
**Why it happens:** The pickup recording for a sentence is rarely exactly the same length as the original.
**How to avoid:** After applying a replacement, calculate the delta (`replacementDuration - originalDuration`) and shift all subsequent sentence timings in the hydrated transcript accordingly. This is essential for maintaining accurate sentence-level playback.
**Warning signs:** Sentence highlighting desynchronizes from audio after a replacement is applied.

### Pitfall 6: wavesurfer.js Region Events and Blazor Render Cycle
**What goes wrong:** Rapid region drag/resize events fire many JS-to-.NET callbacks, causing excessive Blazor re-renders and UI jank.
**Why it happens:** wavesurfer.js emits `region-update` events continuously during drag, each triggering `StateHasChanged()`.
**How to avoid:** Debounce region update events in JavaScript (e.g., 50ms throttle). Only send final position on `region-update-end`. Use `region-update` only for visual feedback that stays on the JS side.
**Warning signs:** UI becomes sluggish during region dragging.

## Code Examples

### Crossfade Splice (New Ams.Core Capability)
```csharp
// AudioSpliceService.cs -- NEW file in Ams.Core.Audio
// Uses FfFilterGraph with multiple inputs for crossfade

public sealed class AudioSpliceService
{
    /// <summary>
    /// Replace a time range in the original buffer with a replacement buffer,
    /// applying crossfades at both splice points.
    /// </summary>
    public AudioBuffer ReplaceSegment(
        AudioBuffer original,
        double startSec,
        double endSec,
        AudioBuffer replacement,
        double crossfadeSec = 0.030)
    {
        // Ensure matching formats
        if (replacement.SampleRate != original.SampleRate)
            replacement = AudioProcessor.Resample(replacement, (ulong)original.SampleRate);

        var before = AudioProcessor.Trim(original, TimeSpan.Zero, TimeSpan.FromSeconds(startSec));
        var after = AudioProcessor.Trim(original, TimeSpan.FromSeconds(endSec));

        // Clamp crossfade to safe bounds
        double maxCf = Math.Min(
            crossfadeSec,
            Math.Min(
                before.Length / (double)before.SampleRate * 0.3,
                replacement.Length / (double)replacement.SampleRate * 0.3));

        var joined = Crossfade(before, replacement, maxCf);
        maxCf = Math.Min(maxCf, after.Length / (double)after.SampleRate * 0.3);
        return Crossfade(joined, after, maxCf);
    }

    private static AudioBuffer Crossfade(AudioBuffer a, AudioBuffer b, double durationSec)
    {
        if (durationSec <= 0.001 || a.Length == 0 || b.Length == 0)
            return AudioBuffer.Concat(a, b);

        var spec = FormattableString.Invariant(
            $"[a]aformat=sample_fmts=flt[af];[b]aformat=sample_fmts=flt[bf];" +
            $"[af][bf]acrossfade=d={durationSec:F6}:c1=tri:c2=tri[out]");

        return FfFilterGraphRunner.Apply(
            new[] {
                new FfFilterGraphRunner.GraphInput("a", a),
                new FfFilterGraphRunner.GraphInput("b", b)
            },
            spec);
    }
}
```

### Pickup Matching via ASR
```csharp
// PickupMatchingService.cs -- NEW in Workstation Services
// Uses existing AsrClient (Nemo) or AsrProcessor (Whisper)

public sealed class PickupMatchingService
{
    private readonly BlazorWorkspace _workspace;

    public async Task<List<PickupMatch>> MatchAsync(
        string pickupFilePath,
        IReadOnlyList<HydratedSentence> flaggedSentences,
        CancellationToken ct)
    {
        // Decode pickup file
        var pickupBuffer = AudioProcessor.Decode(pickupFilePath);

        // Segment by silence (for session files with multiple pickups)
        var silences = AudioProcessor.DetectSilence(pickupBuffer, new SilenceDetectOptions
        {
            NoiseDb = -45.0,
            MinimumDuration = TimeSpan.FromSeconds(1.5)
        });

        var segments = DeriveSegmentsFromSilence(pickupBuffer, silences);

        // Run ASR on each segment
        var matches = new List<PickupMatch>();
        foreach (var seg in segments)
        {
            var segBuffer = AudioProcessor.Trim(pickupBuffer,
                TimeSpan.FromSeconds(seg.StartSec),
                TimeSpan.FromSeconds(seg.EndSec));

            var asrText = await TranscribeSegmentAsync(segBuffer, ct);
            var bestMatch = FuzzyMatchToSentence(asrText, flaggedSentences);

            if (bestMatch != null)
            {
                matches.Add(new PickupMatch(
                    bestMatch.SentenceId, seg.StartSec, seg.EndSec,
                    bestMatch.Score, asrText));
            }
        }

        return matches;
    }
}
```

### Extended Waveform JS Interop for Draggable Regions
```javascript
// Extensions to waveform-interop.js

/**
 * Add a draggable, resizable region for pickup boundary editing.
 * Calls back to .NET on region-update-end with final boundaries.
 */
export function addEditableRegion(elementId, id, start, end, color, dotNetRef) {
    const instance = window.wavesurferInstances[elementId];
    if (!instance || !instance.regionsPlugin) return;

    const region = instance.regionsPlugin.addRegion({
        id: id,
        start: start,
        end: end,
        color: color || 'rgba(59, 200, 120, 0.3)',
        drag: true,
        resize: true,
        minLength: 0.1
    });

    // Debounced callback on drag/resize end
    region.on('update-end', () => {
        dotNetRef.invokeMethodAsync('OnRegionUpdated', id, region.start, region.end)
            .catch(err => console.warn('[waveform-interop] Region update error:', err));
    });
}

/**
 * Synchronize playhead across multiple waveform instances.
 * Seeks all instances in the group to the same relative position.
 */
export function syncPlayheads(elementIds, timeSeconds) {
    for (const eid of elementIds) {
        const instance = window.wavesurferInstances[eid];
        if (!instance) continue;
        const duration = instance.wavesurfer.getDuration();
        if (duration > 0) {
            instance.wavesurfer.seekTo(timeSeconds / duration);
        }
    }
}
```

### Context Playback (Listen-with-Context)
```csharp
// Play replaced segment with surrounding audio for verification
// Uses existing AudioController endpoint with start/end params

private async Task PlayWithContext(StagedReplacement replacement, double contextSec = 3.0)
{
    var start = Math.Max(0, replacement.OriginalStartSec - contextSec);
    var end = replacement.OriginalEndSec + contextSec;

    // Build URL with segment params -- AudioController already supports this
    var url = $"/api/audio/chapter/{Uri.EscapeDataString(chapterName)}?start={start}&end={end}";
    await _snippetPlayer.InvokeVoidAsync("playSegment", url, 0, end - start);
}
```

## Discretionary Recommendations

### Staging Queue UI Design
**Recommendation:** Use a side panel (right-docked) that shows a vertical list of staged replacements. Each item shows: sentence excerpt, pickup preview waveform (small), status badge (Staged/Applied/Reverted), and action buttons (Apply, Revert, Remove). Group by chapter when in batch mode. Use `BitStack Vertical` with `BitCard` per item, collapsible chapter groups.

### Partial Buffer Loading Strategy
**Recommendation:** For multi-waveform view, do NOT load full chapter audio for each waveform. Instead:
1. Identify flagged sentences per chapter from the hydrated transcript
2. Compute a "region of interest" per chapter: `(firstFlaggedStart - 30s, lastFlaggedEnd + 30s)`
3. Use `AudioProcessor.Decode(path, new AudioDecodeOptions(Start: regionStart, Duration: regionDuration))` for partial decode
4. Serve these region buffers via new API endpoints: `/api/audio/chapter/{name}/region?start=X&end=Y`
5. wavesurfer loads the region audio, with offset tracking for correct absolute time display

### Crossfade Duration and Profile Defaults
**Recommendation:** Default crossfade of **30ms** (0.030 seconds) with **triangle (linear)** curve. This is standard for speech splice editing -- short enough to be imperceptible, long enough to eliminate clicks. Expose as a setting in the Polish UI with presets:
- "Tight" = 15ms (minimal crossfade, nearly hard cut)
- "Normal" = 30ms (default, good for speech)
- "Smooth" = 60ms (longer blend, good for transitions with different recording conditions)
- Custom value with slider (5ms - 200ms)

### Undo Storage Mechanism
**Recommendation:** Versioned segment files on disk. Before applying replacement:
1. Create `.polish-undo/{chapterStem}/` directory
2. Export original segment to `sent{id}.v{version}.original.wav` (small: just the replaced range)
3. Record undo metadata in a JSON manifest: `.polish-undo/{chapterStem}/manifest.json`
4. On revert: load the original segment backup, re-splice into the chapter, update manifest

This approach is memory-efficient (only stores the small replaced segments, not full chapters) and survives application restarts.

### Waveform Region Interaction for Pickup Boundary Adjustment
**Recommendation:** After auto-matching via ASR, display the pickup waveform with green draggable/resizable regions marking each detected segment. The user can:
1. Drag region edges to fine-tune start/end boundaries
2. Click a region to preview-play just that segment
3. Double-click to toggle assignment (assign/unassign from a target sentence)
4. See the matched sentence text overlaid on the region

Use `region-update-end` event (not `region-update`) to send final boundaries to Blazor, avoiding excessive re-renders during drag.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| External FFmpeg process for audio ops | In-process FfFilterGraph (libavfilter) | Phase 10.1.1 | No temp files, no process spawning |
| Single waveform per page | Multiple waveforms possible (v7 instances) | Existing | Enables multi-chapter stacked view |
| Non-draggable regions only | Regions with drag=true, resize=true | Available in wavesurfer.js v7 | Enables boundary adjustment UI |
| Full chapter buffer loading | Partial decode via AudioDecodeOptions | Existing capability | Enables memory-efficient multi-chapter |

**Existing capabilities that just need wiring:**
- `FfFilterGraph.WithInput(buffer, label)` -- multi-input filter graphs: already works
- `AudioProcessor.Trim` with start/end: already works
- `AudioBuffer.Concat`: already works
- `AudioProcessor.DetectSilence`: already works for session file segmentation
- `AsrClient.TranscribeAsync`: already works for Nemo-based pickup ASR
- `AsrProcessor.TranscribeBufferAsync`: already works for Whisper-based pickup ASR
- wavesurfer.js regions with `drag: true, resize: true`: CDN loaded, just not used in current interop

## Open Questions

1. **Which ASR engine for pickup matching?**
   - What we know: Both Nemo (HTTP service at localhost:8765) and Whisper (local .NET) are available. Nemo requires the service running; Whisper requires a model file.
   - What's unclear: Which engine the user prefers for the workstation context. Nemo may already be running for pipeline work.
   - Recommendation: Default to Nemo if the service is reachable (health check), fall back to Whisper. Make configurable via Polish settings.

2. **Batch pre/post roll standardization specifics**
   - What we know: `AudioTreatmentService` already handles pre/post roll with roomtone. Batch standardization means applying consistent pre/post roll across selected chapters.
   - What's unclear: Whether this means re-running treatment or just adjusting existing treated files.
   - Recommendation: Re-run `AudioTreatmentService.TreatChapterAsync` with standardized `TreatmentOptions` for selected chapters. This is safer than trying to trim/extend existing treated files.

3. **Timing shift for chapter readings/headers**
   - What we know: Batch shifting of chapter readings/headers is in scope. `AudioTreatmentService` already detects title/content boundaries.
   - What's unclear: Whether "shifting" means adjusting the gap between title and content, or moving the reading start point.
   - Recommendation: Implement as adjustable `ChapterToContentGapSeconds` in `TreatmentOptions`, with a batch UI to set the gap consistently across chapters and re-treat.

## Sources

### Primary (HIGH confidence)
- Codebase analysis: `Ams.Core/Processors/AudioProcessor.cs` -- existing Trim, FadeIn, DetectSilence
- Codebase analysis: `Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs` -- multi-input support via WithInput()
- Codebase analysis: `Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs` -- Apply(IReadOnlyList<GraphInput>) already supports multi-input
- Codebase analysis: `Ams.Core/Artifacts/AudioBuffer.cs` -- Concat, format validation
- Codebase analysis: `Ams.Core/Asr/AsrClient.cs` -- Nemo HTTP ASR path
- Codebase analysis: `Ams.Core/Processors/AsrProcessor.cs` -- Whisper.NET local ASR path
- Codebase analysis: `Ams.Workstation.Server/wwwroot/js/waveform-interop.js` -- existing regions support (non-draggable)
- Codebase analysis: `Ams.Workstation.Server/Components/Shared/WaveformPlayer.razor` -- existing wavesurfer Blazor wrapper
- Codebase analysis: `Ams.Workstation.Server/Controllers/AudioController.cs` -- existing segment streaming with start/end params

### Secondary (MEDIUM confidence)
- [wavesurfer.js Regions Plugin docs](https://wavesurfer.xyz/plugins/regions) -- region options (drag, resize, events)
- [wavesurfer.js v7 RegionsPlugin API](https://wavesurfer-js.pages.dev/docs/classes/plugins_regions.RegionsPlugin) -- addRegion, getRegions, enableDragSelection
- [FFmpeg acrossfade filter docs](https://ayosec.github.io/ffmpeg-filters-docs/7.1/Filters/Audio/acrossfade.html) -- two-input crossfade, curve types, duration options
- [wavesurfer.js multiple instances discussion](https://github.com/katspaugh/wavesurfer.js/discussions/3591) -- management patterns for multiple players

### Tertiary (LOW confidence)
- None -- all critical findings verified against codebase or official docs.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all libraries already in codebase, no new dependencies
- Architecture: HIGH -- patterns follow established Proof area conventions, FFmpeg multi-input verified in code
- Audio splice/crossfade: HIGH -- `FfFilterGraphRunner.Apply(IReadOnlyList<GraphInput>)` verified to accept multiple inputs; `acrossfade` filter documented
- ASR pickup matching: HIGH -- both Nemo and Whisper ASR paths verified in codebase
- Wavesurfer regions (draggable): MEDIUM -- v7 supports `drag: true, resize: true` per docs, but not yet tested in this codebase's interop layer
- Multi-waveform sync: MEDIUM -- no built-in sync in wavesurfer.js, requires custom JS implementation
- Pitfalls: HIGH -- based on direct codebase analysis and known FFmpeg constraints

**Research date:** 2026-02-23
**Valid until:** 2026-03-23 (stable -- all technologies already integrated)
