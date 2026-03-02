---
phase: quick-5
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - host/Ams.Core/Processors/AsrProcessor.cs
autonomous: true
requirements: [ASR-CHUNKING]

must_haves:
  truths:
    - "Audio longer than 30s is split at silence midpoints before Whisper transcription"
    - "Each chunk is transcribed independently and timestamps are offset by chunk start time"
    - "Short audio (under 30s) bypasses chunking entirely and uses the existing single-pass path"
    - "Stitched result is identical in shape to single-pass AsrResponse (same model, merged tokens+segments)"
    - "Per-chunk prompt carries tail text from previous chunk for context continuity"
  artifacts:
    - path: "host/Ams.Core/Processors/AsrProcessor.cs"
      provides: "Silence-based pre-chunking in TranscribeWithWhisperNetAsync"
      contains: "DetectSilence"
  key_links:
    - from: "AsrProcessor.TranscribeWithWhisperNetAsync"
      to: "AudioProcessor.DetectSilence"
      via: "silence detection on prepared buffer"
      pattern: "AudioProcessor\\.DetectSilence"
    - from: "AsrProcessor.TranscribeWithWhisperNetAsync"
      to: "AsrProcessor.RunWhisperPassAsync"
      via: "per-chunk transcription loop"
      pattern: "RunWhisperPassAsync.*chunk"
---

<objective>
Add silence-based pre-chunking to AsrProcessor so that long audio is split at natural silence boundaries before Whisper transcription, eliminating duplicate token emissions at Whisper's internal 30-second window boundaries.

Purpose: Whisper processes audio in 30-second windows internally. When a single long buffer is sent, tokens near window boundaries can be emitted twice or get timing artifacts. By splitting at silence midpoints and transcribing each chunk independently (with timestamp offsetting), we get clean per-chunk results that stitch into an accurate whole-file transcription.

Output: Modified `AsrProcessor.cs` with chunked transcription path in `TranscribeWithWhisperNetAsync`, silence-based boundary computation, per-chunk timestamp offsetting, cross-chunk prompt continuity, and short-audio bypass.
</objective>

<execution_context>
@/home/cari/.claude/get-shit-done/workflows/execute-plan.md
@/home/cari/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@host/Ams.Core/Processors/AsrProcessor.cs
@host/Ams.Core/Processors/AudioProcessor.cs
@host/Ams.Core/Artifacts/AudioBuffer.cs
@host/Ams.Core/Asr/AsrModels.cs
@host/Ams.Core/Audio/AsrAudioPreparer.cs

<interfaces>
<!-- Key types and contracts the executor needs. -->

From host/Ams.Core/Processors/AudioProcessor.cs:
```csharp
public static IReadOnlyList<SilenceInterval> DetectSilence(AudioBuffer buffer, SilenceDetectOptions? options = null);
public static AudioBuffer Trim(AudioBuffer buffer, TimeSpan start, TimeSpan? end = null);
public sealed record SilenceDetectOptions
{
    public double NoiseDb { get; init; } = -50.0;
    public TimeSpan MinimumDuration { get; init; } = TimeSpan.FromMilliseconds(500);
}
public sealed record SilenceInterval(TimeSpan Start, TimeSpan End, TimeSpan Duration);
```

From host/Ams.Core/Artifacts/AudioBuffer.cs:
```csharp
public sealed class AudioBuffer
{
    public int Channels { get; }
    public int SampleRate { get; }
    public int Length { get; }
    public float[][] Planar { get; }
    public MemoryStream ToWavStream(AudioEncodeOptions? options = null);
}
```

From host/Ams.Core/Asr/AsrModels.cs:
```csharp
public sealed record AsrToken(double StartTime, double Duration, string Word);
public sealed record AsrSegment(double StartSec, double EndSec, string Text);
public sealed record AsrResponse(string ModelVersion, AsrToken[]? Tokens, AsrSegment[]? Segments);
```

From host/Ams.Core/Processors/AsrProcessor.cs:
```csharp
// Current flow: TranscribeWithWhisperNetAsync -> RunWhisperPassAsync (single pass, entire buffer)
// RunWhisperPassAsync encodes buffer to WAV stream, runs processor.ProcessAsync, collects tokens+segments
public sealed record AsrOptions(
    string ModelPath, string Language = "auto", int Threads = 8,
    bool UseGpu = true, bool EnableWordTimestamps = true, bool SplitOnWord = true,
    int BeamSize = 5, int BestOf = 1, float Temperature = 0.0f,
    bool NoSpeechBoost = true, int GpuDevice = 0, bool UseFlashAttention = true,
    bool UseDtwTimestamps = false);
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Add silence-based chunk boundary computation and chunked transcription to AsrProcessor</name>
  <files>host/Ams.Core/Processors/AsrProcessor.cs</files>
  <action>
Modify `TranscribeWithWhisperNetAsync` to detect silences and split long audio into chunks before calling `RunWhisperPassAsync`. The change is entirely within `AsrProcessor.cs` -- no new files needed.

**1a. Add chunking constants and a chunk record.**

Add near the existing constants (`DtwFallbackMinAudioSeconds` etc.):

```csharp
/// Minimum audio duration (seconds) to trigger chunking. Audio shorter than this
/// goes through the existing single-pass path.
private const double ChunkingMinAudioSeconds = 30.0;

/// Target maximum chunk duration. Silence boundaries near this target are preferred.
private const double ChunkTargetMaxSeconds = 25.0;

/// Silence detection options tuned for chunk boundary detection.
/// Lower noise threshold and shorter minimum duration than speech boundary detection
/// because we want to find ANY natural pause, not just long silences.
private static readonly SilenceDetectOptions ChunkSilenceOptions = new()
{
    NoiseDb = -40.0,
    MinimumDuration = TimeSpan.FromMilliseconds(300)
};

private readonly record struct ChunkBoundary(int StartSample, int EndSample, double StartTimeSec);
```

**1b. Implement `ComputeChunkBoundaries`.**

Add a static method that takes the prepared `AudioBuffer` and returns chunk boundaries:

```csharp
private static List<ChunkBoundary> ComputeChunkBoundaries(AudioBuffer buffer)
{
    var durationSec = ComputeAudioDurationSeconds(buffer);
    if (durationSec < ChunkingMinAudioSeconds)
        return []; // empty = single-pass

    var silences = AudioProcessor.DetectSilence(buffer, ChunkSilenceOptions);
    if (silences.Count == 0)
        return []; // no silence found, fall back to single-pass

    var boundaries = new List<ChunkBoundary>();
    int currentStart = 0;
    double currentStartSec = 0;

    foreach (var silence in silences)
    {
        var midpointSec = silence.Start.TotalSeconds + silence.Duration.TotalSeconds / 2.0;
        var chunkDurationSec = midpointSec - currentStartSec;

        if (chunkDurationSec < ChunkTargetMaxSeconds * 0.5)
            continue; // chunk too short, skip this silence

        // Cut at silence midpoint
        var midpointSample = (int)(midpointSec * buffer.SampleRate);
        midpointSample = Math.Clamp(midpointSample, 0, buffer.Length);

        boundaries.Add(new ChunkBoundary(currentStart, midpointSample, currentStartSec));
        currentStart = midpointSample;
        currentStartSec = midpointSec;
    }

    // Final chunk: from last cut point to end of audio
    if (currentStart < buffer.Length)
    {
        boundaries.Add(new ChunkBoundary(currentStart, buffer.Length, currentStartSec));
    }

    // If we only got 1 boundary (no splits happened), return empty to use single-pass
    return boundaries.Count <= 1 ? [] : boundaries;
}
```

**1c. Implement `ExtractChunk` to create a sub-buffer from sample range.**

Use direct `Array.Copy` on `Planar` data -- avoid `AudioProcessor.Trim` which round-trips through FFmpeg:

```csharp
private static AudioBuffer ExtractChunk(AudioBuffer source, int startSample, int endSample)
{
    var length = endSample - startSample;
    var chunk = new AudioBuffer(source.Channels, source.SampleRate, length);
    for (int ch = 0; ch < source.Channels; ch++)
    {
        Array.Copy(source.Planar[ch], startSample, chunk.Planar[ch], 0, length);
    }
    return chunk;
}
```

**1d. Implement `OffsetResponse` to shift all timestamps by chunk start time.**

```csharp
private static AsrResponse OffsetResponse(AsrResponse response, double offsetSec)
{
    if (offsetSec <= 0)
        return response;

    var tokens = new AsrToken[response.Tokens.Length];
    for (int i = 0; i < response.Tokens.Length; i++)
    {
        var t = response.Tokens[i];
        tokens[i] = new AsrToken(t.StartTime + offsetSec, t.Duration, t.Word);
    }

    var segments = new AsrSegment[response.Segments.Length];
    for (int i = 0; i < response.Segments.Length; i++)
    {
        var s = response.Segments[i];
        segments[i] = new AsrSegment(s.StartSec + offsetSec, s.EndSec + offsetSec, s.Text);
    }

    return new AsrResponse(response.ModelVersion, tokens, segments);
}
```

**1e. Implement `MergeResponses` to stitch chunk results.**

```csharp
private static AsrResponse MergeResponses(List<AsrResponse> responses)
{
    if (responses.Count == 0)
        return new AsrResponse("whisper", Array.Empty<AsrToken>(), Array.Empty<AsrSegment>());
    if (responses.Count == 1)
        return responses[0];

    var allTokens = new List<AsrToken>();
    var allSegments = new List<AsrSegment>();
    string modelVersion = responses[0].ModelVersion;

    foreach (var r in responses)
    {
        allTokens.AddRange(r.Tokens);
        allSegments.AddRange(r.Segments);
    }

    return new AsrResponse(modelVersion, allTokens.ToArray(), allSegments.ToArray());
}
```

**1f. Implement `BuildTailPrompt` to extract last N words from a response for cross-chunk context.**

```csharp
private const int PromptTailWordCount = 8;

private static string? BuildTailPrompt(AsrResponse response)
{
    if (response.Tokens is not { Length: > 0 })
        return null;

    var startIdx = Math.Max(0, response.Tokens.Length - PromptTailWordCount);
    var sb = new StringBuilder();
    for (int i = startIdx; i < response.Tokens.Length; i++)
    {
        if (sb.Length > 0) sb.Append(' ');
        sb.Append(response.Tokens[i].Word);
    }

    return sb.Length > 0 ? sb.ToString() : null;
}
```

**1g. Modify `TranscribeWithWhisperNetAsync` to use chunked path.**

Replace the current body of `TranscribeWithWhisperNetAsync` with:

```csharp
private static async Task<AsrResponse> TranscribeWithWhisperNetAsync(
    AudioBuffer buffer,
    AsrOptions options,
    CancellationToken cancellationToken)
{
    var chunks = ComputeChunkBoundaries(buffer);

    // Short audio or no silence boundaries: use existing single-pass path
    if (chunks.Count == 0)
    {
        var response = await RunWhisperPassAsync(buffer, options, cancellationToken).ConfigureAwait(false);

        if (!ShouldRetryWithoutDtw(options, buffer, response, out var audioDurationSec, out var transcriptEndSec,
                out var coverage))
        {
            return response;
        }

        Log.Warn(
            "DTW timestamps appear truncated for model '{Model}' (end={TranscriptEnd:F2}s, audio={AudioDuration:F2}s, coverage={Coverage:P1}). Retrying once with DTW disabled.",
            options.ModelPath, transcriptEndSec, audioDurationSec, coverage);

        var fallbackOptions = options with { UseDtwTimestamps = false };
        return await RunWhisperPassAsync(buffer, fallbackOptions, cancellationToken).ConfigureAwait(false);
    }

    // Chunked transcription path
    Log.Debug("Chunking audio into {ChunkCount} segments for Whisper transcription", chunks.Count);

    var responses = new List<AsrResponse>(chunks.Count);
    var currentOptions = options;

    for (int i = 0; i < chunks.Count; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var chunk = chunks[i];
        var chunkBuffer = ExtractChunk(buffer, chunk.StartSample, chunk.EndSample);
        var chunkDurationSec = (double)chunkBuffer.Length / chunkBuffer.SampleRate;

        Log.Debug("Transcribing chunk {Index}/{Total} (start={Start:F2}s, duration={Duration:F2}s)",
            i + 1, chunks.Count, chunk.StartTimeSec, chunkDurationSec);

        var chunkResponse = await RunWhisperPassAsync(chunkBuffer, currentOptions, cancellationToken)
            .ConfigureAwait(false);

        // Offset timestamps to global timeline
        var offsetResponse = OffsetResponse(chunkResponse, chunk.StartTimeSec);
        responses.Add(offsetResponse);

        // Build tail prompt for next chunk's context continuity
        var tailPrompt = BuildTailPrompt(chunkResponse);
        if (tailPrompt != null)
        {
            // Future: when AsrOptions gains Prompt field (Task 4), wire it here:
            // currentOptions = currentOptions with { Prompt = tailPrompt };
            // For now, just log the tail for diagnostics
            Log.Debug("Chunk {Index} tail prompt: \"{Tail}\"", i + 1, tailPrompt);
        }
    }

    return MergeResponses(responses);
}
```

**Important notes for the executor:**

- The DTW retry logic stays in the single-pass branch only. Per-chunk transcription does not use DTW retry because chunks are short enough that DTW truncation does not occur.
- `ExtractChunk` uses direct `Array.Copy` on `Planar` arrays, NOT `AudioProcessor.Trim` (which would needlessly round-trip through FFmpeg).
- Do NOT add a `Prompt` property to `AsrOptions` in this task. That is Task 4's responsibility. The tail prompt building is preparatory -- it logs the prompt text but does not pass it to Whisper yet. When Task 4 lands, the commented-out line becomes active.
- The `using System.Text` import is already present at the top of AsrProcessor.cs.
- Keep all new methods `private static` consistent with the existing codebase style.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet build host/Ams.Core/Ams.Core.csproj --no-restore 2>&1 | tail -5</automated>
  </verify>
  <done>
    - Audio >= 30s is split at silence midpoints into chunks, each transcribed independently
    - Timestamps on each chunk's tokens and segments are offset by the chunk's global start time
    - Merged response has all tokens and segments in correct temporal order
    - Audio < 30s uses existing single-pass path (including DTW retry logic) unchanged
    - Tail prompt text is computed and logged per chunk (wiring to WithPrompt deferred to Task 4)
    - All existing callers (AsrService.TranscribeAsync, PickupMatchingService, TranscribeFileAsync) benefit automatically since chunking is inside TranscribeWithWhisperNetAsync
    - Project compiles without errors or warnings
  </done>
</task>

<task type="auto">
  <name>Task 2: Add unit tests for chunk boundary computation, offset, and merge logic</name>
  <files>host/Ams.Tests/Processors/AsrChunkingTests.cs</files>
  <action>
Create a new test file for the chunking helpers. Since `ComputeChunkBoundaries`, `OffsetResponse`, `MergeResponses`, `ExtractChunk`, and `BuildTailPrompt` are `private static`, test them via `InternalsVisibleTo` or by temporarily making them `internal static` with `[assembly: InternalsVisibleTo("Ams.Tests")]` (check if this attribute already exists on `Ams.Core`).

**2a. Check InternalsVisibleTo setup.**

Look for `InternalsVisibleTo` in `Ams.Core.csproj` or an `AssemblyInfo.cs`. If it already exists for `Ams.Tests`, use it. If not, add `[assembly: InternalsVisibleTo("Ams.Tests")]` to a file like `host/Ams.Core/Properties/AssemblyInfo.cs` or inline in the csproj.

Then change the five new methods in Task 1 from `private static` to `internal static` so tests can call them directly.

**2b. Create test class `AsrChunkingTests` with these test cases:**

1. **OffsetResponse_ShiftsAllTimestamps** -- Create an `AsrResponse` with 2 tokens and 1 segment, call `OffsetResponse` with offset 10.0, assert all `StartTime`, `StartSec`, `EndSec` are shifted by 10.0, `Duration` and `Word`/`Text` are unchanged.

2. **OffsetResponse_ZeroOffset_ReturnsSameInstance** -- Offset of 0.0 returns the original response object (reference equality).

3. **MergeResponses_CombinesTokensAndSegments** -- Create 3 responses with 2 tokens each, merge, assert result has 6 tokens in order, ModelVersion from first response.

4. **MergeResponses_SingleResponse_ReturnsSame** -- Single response returns that response.

5. **MergeResponses_Empty_ReturnsEmptyResponse** -- Empty list returns response with empty arrays.

6. **ExtractChunk_CopiesSampleRange** -- Create a mono AudioBuffer with 100 samples (values 0..99), extract samples 20-50, assert chunk has 30 samples with correct values.

7. **BuildTailPrompt_TakesLastNWords** -- Create response with 12 tokens, assert tail prompt contains last 8 words space-separated.

8. **BuildTailPrompt_EmptyTokens_ReturnsNull** -- Response with no tokens returns null.

9. **ComputeChunkBoundaries_ShortAudio_ReturnsEmpty** -- Buffer with < 30s of audio returns empty list (single-pass).

Use `Ams.Core.Asr` for `AsrResponse`/`AsrToken`/`AsrSegment`, `Ams.Core.Artifacts` for `AudioBuffer`, `Ams.Core.Processors` for `AsrProcessor`.

**Do NOT test `TranscribeWithWhisperNetAsync` or `RunWhisperPassAsync` directly** -- those require a Whisper model. Only test the pure computational helpers.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet test host/Ams.Tests/Ams.Tests.csproj --filter "FullyQualifiedName~AsrChunking" --no-restore 2>&1 | tail -15</automated>
  </verify>
  <done>
    - All 9 unit tests pass
    - Tests cover: offset arithmetic, merge concatenation, chunk extraction, tail prompt, short-audio bypass
    - No test touches Whisper model or FFmpeg -- purely computational
    - Project compiles and all existing tests still pass
  </done>
</task>

</tasks>

<verification>
```bash
# Full build
cd /home/cari/repos/AMS && dotnet build host/Ams.Core/Ams.Core.csproj --no-restore

# Run chunking tests
dotnet test host/Ams.Tests/Ams.Tests.csproj --filter "FullyQualifiedName~AsrChunking" --no-restore

# Run all tests to check for regressions
dotnet test host/Ams.Tests/Ams.Tests.csproj --no-restore

# Verify DetectSilence is referenced in the chunking code
grep -n "DetectSilence" host/Ams.Core/Processors/AsrProcessor.cs

# Verify chunk boundary computation exists
grep -n "ComputeChunkBoundaries" host/Ams.Core/Processors/AsrProcessor.cs
```
</verification>

<success_criteria>
- Audio longer than 30s is split at silence midpoints before Whisper transcription
- Each chunk transcribed independently with timestamps offset to global timeline
- Short audio (< 30s) uses existing single-pass path unchanged (including DTW retry)
- Tail prompt text is built per chunk and logged (prompt wiring deferred to Task 4)
- All new computational helpers have unit tests
- Full test suite passes with no regressions
- All existing callers (AsrService, PickupMatchingService, TranscribeFileAsync) benefit automatically
</success_criteria>

<output>
After completion, create `.planning/quick/5-silence-based-pre-chunking-in-asrprocess/5-SUMMARY.md`
</output>
