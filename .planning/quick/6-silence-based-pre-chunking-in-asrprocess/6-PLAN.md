---
phase: quick-6
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - host/Ams.Core/Audio/SilenceChunker.cs
  - host/Ams.Core/Services/AsrService.cs
  - host/Ams.Tests/Audio/SilenceChunkerTests.cs
autonomous: true
requirements: [ASR-PRE-CHUNK]

must_haves:
  truths:
    - "SilenceChunker splits an AudioBuffer at silence boundaries using a single O(n) pass"
    - "Silence regions are detected using AudioDefaults thresholds (-55dB, 200ms minimum)"
    - "Split points land at the midpoint of each qualifying silence region for clean boundaries"
    - "Short audio (under a configurable minimum) is returned as a single chunk without splitting"
    - "AsrService transcribes each chunk independently and merges results with correct timestamp offsets"
    - "Merged AsrResponse has contiguous, monotonically increasing token timestamps"
    - "Existing single-buffer transcription behavior is preserved when no silence boundaries are found"
  artifacts:
    - path: "host/Ams.Core/Audio/SilenceChunker.cs"
      provides: "O(n) silence detection and chunk boundary selection"
      contains: "FindChunkBoundaries"
    - path: "host/Ams.Core/Services/AsrService.cs"
      provides: "Multi-chunk transcription with timestamp merging"
      contains: "SilenceChunker"
    - path: "host/Ams.Tests/Audio/SilenceChunkerTests.cs"
      provides: "Unit tests for silence detection and chunking"
      contains: "SilenceChunkerTests"
  key_links:
    - from: "AsrService.TranscribeAsync"
      to: "SilenceChunker.FindChunkBoundaries"
      via: "Splits buffer before transcription loop"
      pattern: "SilenceChunker\\.FindChunkBoundaries"
    - from: "SilenceChunker"
      to: "AudioBuffer.Slice"
      via: "Zero-copy chunk extraction at silence midpoints"
      pattern: "buffer\\.Slice"
    - from: "SilenceChunker"
      to: "AudioDefaults"
      via: "Shared silence threshold and minimum duration constants"
      pattern: "AudioDefaults\\."
---

<objective>
Add silence-based pre-chunking to the ASR pipeline so chapter audio is split at natural pause points before transcription instead of being sent as one monolithic buffer.

Purpose: Whisper performs better on shorter segments with clean silence boundaries. Splitting at natural pauses (sentence/paragraph breaks in narration) improves word-level timestamp accuracy and reduces hallucination at chunk edges. This uses the zero-copy AudioBuffer.Slice() from quick-5 to avoid per-chunk memory allocation.

Output: SilenceChunker utility class, updated AsrService with multi-chunk transcription, comprehensive tests.
</objective>

<execution_context>
@/home/cari/.claude/get-shit-done/workflows/execute-plan.md
@/home/cari/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@host/Ams.Core/Audio/AudioDefaults.cs
@host/Ams.Core/Artifacts/AudioBuffer.cs
@host/Ams.Core/Services/AsrService.cs
@host/Ams.Core/Processors/AsrProcessor.cs
@host/Ams.Core/Services/Interfaces/IAsrService.cs

<interfaces>
From host/Ams.Core/Artifacts/AudioBuffer.cs:
```csharp
public sealed class AudioBuffer
{
    public int Channels { get; }
    public int SampleRate { get; }
    public int Length { get; }
    public ReadOnlyMemory<float> GetChannel(int channel);
    public AudioBuffer Slice(int startSample, int length);
    public AudioBuffer Slice(TimeSpan start, TimeSpan end);
}
```

From host/Ams.Core/Audio/AudioDefaults.cs:
```csharp
public static class AudioDefaults
{
    public const double SilenceThresholdDb = -55.0;
    public static readonly TimeSpan MinimumSilenceDuration = TimeSpan.FromMilliseconds(200);
}
```

From host/Ams.Core/Processors/AsrProcessor.cs:
```csharp
public static class AsrProcessor
{
    public static Task<AsrResponse> TranscribeBufferAsync(
        AudioBuffer buffer, AsrOptions options, CancellationToken ct);
}

public sealed record AsrResponse(string ModelVersion, AsrToken[] Tokens, AsrSegment[] Segments);
public sealed record AsrToken(double StartTime, double Duration, string Text);
public sealed record AsrSegment(double StartSec, double EndSec, string Text);
```

From host/Ams.Core/Services/AsrService.cs:
```csharp
public sealed class AsrService : IAsrService
{
    public Task<AsrResponse> TranscribeAsync(ChapterContext chapter, AsrOptions options, CancellationToken ct);
    public AudioBuffer ResolveAsrReadyBuffer(ChapterContext chapter);
}
```
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Implement SilenceChunker with O(n) silence detection</name>
  <files>host/Ams.Core/Audio/SilenceChunker.cs, host/Ams.Tests/Audio/SilenceChunkerTests.cs</files>
  <behavior>
    - FindChunkBoundaries on all-silence buffer returns single chunk (0, Length) — no splitting within pure silence
    - FindChunkBoundaries on buffer with no silence returns single chunk (0, Length) — nothing to split on
    - FindChunkBoundaries on buffer with 300ms silence region at midpoint returns two chunks split at silence midpoint
    - FindChunkBoundaries on buffer with multiple silence regions returns N+1 chunks (N silences)
    - FindChunkBoundaries on short buffer (under MinChunkDuration) returns single chunk regardless of silence
    - Silence region shorter than MinimumSilenceDuration (200ms) is ignored
    - Split point lands at midpoint of silence region (not start or end)
    - Returned chunk boundaries cover entire buffer with no gaps or overlaps (sum of lengths == buffer.Length)
    - RMS-to-dB conversion: silence at -55dB threshold means RMS amplitude ~= 0.00178 (10^(-55/20))
  </behavior>
  <action>
Create `host/Ams.Tests/Audio/SilenceChunkerTests.cs` first with failing tests, then implement `host/Ams.Core/Audio/SilenceChunker.cs`.

**SilenceChunker API:**

```csharp
namespace Ams.Core.Audio;

public static class SilenceChunker
{
    public readonly record struct ChunkBoundary(int StartSample, int Length);

    /// Returns chunk boundaries for splitting an AudioBuffer at silence points.
    /// Uses a sliding RMS window to detect silence regions, then splits at their midpoints.
    public static IReadOnlyList<ChunkBoundary> FindChunkBoundaries(
        AudioBuffer buffer,
        double silenceThresholdDb = AudioDefaults.SilenceThresholdDb,
        TimeSpan? minSilenceDuration = null,
        TimeSpan? minChunkDuration = null);
}
```

**Algorithm (single O(n) pass):**

1. Convert `silenceThresholdDb` to linear RMS threshold: `threshold = Math.Pow(10, silenceThresholdDb / 20.0)` (~0.00178 for -55dB).
2. Set `minSilenceSamples` from `minSilenceDuration` (default `AudioDefaults.MinimumSilenceDuration` = 200ms) converted to samples.
3. Set `minChunkSamples` from `minChunkDuration` (default 30 seconds) converted to samples. If total buffer is shorter, return single chunk.
4. Use a sliding RMS window of 1024 samples (hop = 512) for noise resilience vs per-sample comparison.
5. Single forward pass: track `silenceStart` (or -1 if not in silence). When RMS drops below threshold, record `silenceStart`. When RMS rises above threshold (or end of buffer), check if silence duration >= `minSilenceSamples`. If yes, record the silence region midpoint as a split candidate.
6. From the list of split candidates, greedily select boundaries that keep chunks between `minChunkDuration` and a reasonable max (e.g., 5 minutes). Skip split points that would create too-small chunks.
7. Return `List<ChunkBoundary>` covering [0..Length) with no gaps.

**Important details:**
- Operate on channel 0 only (buffer is mono by the time it reaches ASR — `AsrAudioPreparer.PrepareForAsr` ensures this).
- The RMS window approach avoids false positives from single-sample noise spikes.
- Default `minChunkDuration` of 30 seconds prevents excessive fragmentation on audiobooks with frequent pauses.
- No maximum chunk size enforcement — if narration has no qualifying silences for 20+ minutes, that's fine; Whisper handles long audio. The goal is to split where natural pauses exist, not to force fixed-size chunks.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet test host/Ams.Tests/Ams.Tests.csproj --no-restore --filter "FullyQualifiedName~SilenceChunkerTests" -v minimal 2>&amp;1 | tail -20</automated>
  </verify>
  <done>
    - SilenceChunker.FindChunkBoundaries returns correct boundaries for: all-silence, no-silence, single-silence, multi-silence, short-buffer, sub-threshold-silence scenarios
    - O(n) complexity — single pass with sliding RMS window
    - Uses AudioDefaults constants as defaults
    - All chunk boundaries are contiguous (no gaps, no overlaps, total == buffer.Length)
  </done>
</task>

<task type="auto">
  <name>Task 2: Wire SilenceChunker into AsrService with timestamp-corrected merging</name>
  <files>host/Ams.Core/Services/AsrService.cs</files>
  <action>
Update `AsrService.TranscribeAsync` to use `SilenceChunker` for pre-chunking the audio buffer before sending to Whisper. The Whisper path currently sends the entire buffer as one shot. After this change, it will:

1. Call `SilenceChunker.FindChunkBoundaries(buffer)` on the ASR-ready buffer.
2. If only one chunk is returned (no qualifying silences found, or buffer too short), fall through to existing single-buffer path — no behavior change.
3. If multiple chunks, iterate: for each `ChunkBoundary`, call `buffer.Slice(boundary.StartSample, boundary.Length)` to get a zero-copy view, then call `AsrProcessor.TranscribeBufferAsync(slice, options, ct)`.
4. Merge the per-chunk `AsrResponse` results into a single `AsrResponse`:
   - For each chunk's tokens, add the chunk's start time offset: `new AsrToken(token.StartTime + chunkOffsetSec, token.Duration, token.Text)` where `chunkOffsetSec = boundary.StartSample / (double)buffer.SampleRate`.
   - For each chunk's segments, add the offset: `new AsrSegment(seg.StartSec + chunkOffsetSec, seg.EndSec + chunkOffsetSec, seg.Text)`.
   - Concatenate all offset-adjusted tokens and segments into a single response.
   - Use the `ModelVersion` from the first chunk's response.
5. Log chunk count and durations at Debug level: `"Pre-chunking: {ChunkCount} chunks from {TotalDuration:F1}s audio"`.

**Merging helper** — add a private static method:

```csharp
private static AsrResponse MergeChunkResponses(
    IReadOnlyList<(AsrResponse Response, double OffsetSec)> chunks)
```

This keeps AsrService's TranscribeAsync clean. The method collects all tokens/segments, applies offsets, and returns a single AsrResponse.

**Key constraints:**
- Do NOT change the `IAsrService` interface — the chunking is an internal optimization.
- Do NOT change the Nemo path (`RunNemoAsync` in GenerateTranscriptCommand) — only Whisper benefits from pre-chunking since Nemo has its own server-side chunking.
- The Whisper path in `AsrService.TranscribeAsync` is the only call site that changes. `AsrProcessor` remains unmodified.
- Preserve the existing `ResolveAsrReadyBuffer` method unchanged — it still prepares mono 16kHz. The chunking happens after preparation.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet build host/Ams.Core/Ams.Core.csproj --no-restore 2>&amp;1 | tail -10</automated>
  </verify>
  <done>
    - AsrService.TranscribeAsync pre-chunks audio at silence boundaries before Whisper transcription
    - Single-chunk case (no silences found) falls through to original behavior — zero regression risk
    - Multi-chunk case merges responses with monotonically increasing, correctly offset timestamps
    - IAsrService interface unchanged
    - Nemo path unchanged
    - Debug logging shows chunk count and durations
  </done>
</task>

</tasks>

<verification>
```bash
# Full test suite — no regressions
cd /home/cari/repos/AMS && dotnet test host/Ams.Tests/Ams.Tests.csproj --no-restore -v minimal

# SilenceChunker tests specifically
cd /home/cari/repos/AMS && dotnet test host/Ams.Tests/Ams.Tests.csproj --no-restore --filter "FullyQualifiedName~SilenceChunkerTests" -v minimal

# Verify SilenceChunker uses AudioDefaults
grep -n "AudioDefaults" host/Ams.Core/Audio/SilenceChunker.cs

# Verify AsrService uses SilenceChunker
grep -n "SilenceChunker" host/Ams.Core/Services/AsrService.cs

# Verify IAsrService interface unchanged
grep -c "TranscribeAsync\|ResolveAsrReadyBuffer" host/Ams.Core/Services/Interfaces/IAsrService.cs
# Should still be 2

# Full solution build
cd /home/cari/repos/AMS && dotnet build host/Ams.sln --no-restore
```
</verification>

<success_criteria>
- SilenceChunker detects silence regions via O(n) sliding RMS window
- Chunk boundaries land at silence midpoints for clean ASR segmentation
- AsrService pre-chunks audio before Whisper transcription
- Merged response has correct, monotonically increasing timestamps
- Single-chunk fallback preserves existing behavior exactly
- All existing tests pass (no regressions)
- New SilenceChunker tests pass
</success_criteria>

<output>
After completion, create `.planning/quick/6-silence-based-pre-chunking-in-asrprocess/6-SUMMARY.md`
</output>
