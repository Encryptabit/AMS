using Ams.Core.Artifacts.Alignment;
using Ams.Core.Asr;
using Ams.Core.Services;

namespace Ams.Tests.Asr;

public sealed class AsrServiceSpliceTests
{
    [Fact]
    public void Splice_WithSinglePatchedChunk_ReplacesTokensInRangeAndPreservesOthers()
    {
        // 3 chunks: [0,5), [5,10), [10,15). Patch chunk 1 only.
        var plan = BuildPlan(
            (chunkId: 0, start: 0.0, end: 5.0),
            (chunkId: 1, start: 5.0, end: 10.0),
            (chunkId: 2, start: 10.0, end: 15.0));

        var existing = new AsrResponse(
            "model-v1",
            tokens: new[]
            {
                new AsrToken(1.0, 0.5, "hello"),  // chunk 0
                new AsrToken(3.0, 0.5, "world"),  // chunk 0
                new AsrToken(6.0, 0.5, "BAD1"),   // chunk 1 — should be replaced
                new AsrToken(7.5, 0.5, "BAD2"),   // chunk 1 — should be replaced
                new AsrToken(11.0, 0.5, "the"),   // chunk 2
                new AsrToken(12.0, 0.5, "end"),   // chunk 2
            },
            segments: new[]
            {
                new AsrSegment(1.0, 4.0, "hello world"),
                new AsrSegment(6.0, 8.0, "BAD"),
                new AsrSegment(11.0, 13.0, "the end"),
            });

        // New chunk 1 transcribed in isolation: tokens have relative-to-chunk timings.
        var newChunk1 = new AsrResponse(
            "model-v2",
            tokens: new[]
            {
                new AsrToken(0.5, 0.4, "fixed"),  // absolute 5.5
                new AsrToken(2.0, 0.4, "good"),   // absolute 7.0
            },
            segments: new[] { new AsrSegment(0.5, 2.5, "fixed good") });

        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: new[] { 1 },
            newChunks: new[] { (ChunkId: 1, Response: newChunk1, OffsetSec: 5.0) });

        // Tokens: chunk 0 + chunk 2 preserved; chunk 1 replaced with offset-adjusted new tokens.
        Assert.Equal(
            new[] { "hello", "world", "fixed", "good", "the", "end" },
            spliced.Tokens.Select(t => t.Word).ToArray());
        Assert.Equal(5.5, spliced.Tokens[2].StartTime, precision: 4);
        Assert.Equal(7.0, spliced.Tokens[3].StartTime, precision: 4);

        // ModelVersion taken from the new chunk (it's the source of truth for patched ranges).
        Assert.Equal("model-v2", spliced.ModelVersion);

        // Segments: same pattern.
        Assert.Equal(3, spliced.Segments.Length);
        Assert.Equal("fixed good", spliced.Segments[1].Text);
        Assert.Equal(5.5, spliced.Segments[1].StartSec, precision: 4);
        Assert.Equal(7.5, spliced.Segments[1].EndSec, precision: 4);
    }

    [Fact]
    public void Splice_TokensOutsidePatchedRange_PreservedByteIdentical()
    {
        // Round-trip check: if the new chunks' tokens happen to be empty (degenerate), the
        // result must still contain every token from outside the patched ranges, byte-identical.
        var plan = BuildPlan((0, 0.0, 5.0), (1, 5.0, 10.0));
        var preservedToken = new AsrToken(2.345, 0.678, "preserved");
        var existing = new AsrResponse(
            "model-v1",
            tokens: new[] { preservedToken, new AsrToken(6.0, 0.5, "drop") });

        var newEmpty = new AsrResponse("model-v2", tokens: Array.Empty<AsrToken>());
        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: new[] { 1 },
            newChunks: new[] { (ChunkId: 1, Response: newEmpty, OffsetSec: 5.0) });

        Assert.Single(spliced.Tokens);
        // Same record reference: SpliceChunkResponses doesn't allocate when not adjusting.
        Assert.Same(preservedToken, spliced.Tokens[0]);
    }

    [Fact]
    public void Splice_BoundaryTokens_AssignedByHalfOpenRange()
    {
        // Half-open [start, end) — token at StartTime == chunkEnd belongs to the NEXT chunk,
        // not the current one. Verify: token at exactly 5.0 is NOT replaced when patching chunk 0.
        var plan = BuildPlan((0, 0.0, 5.0), (1, 5.0, 10.0));
        var atBoundary = new AsrToken(5.0, 0.3, "boundary");
        var existing = new AsrResponse(
            "model-v1",
            tokens: new[] { new AsrToken(1.0, 0.3, "early"), atBoundary, new AsrToken(7.0, 0.3, "late") });

        var newChunk0 = new AsrResponse("model-v2", tokens: new[] { new AsrToken(2.0, 0.3, "patched") });
        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: new[] { 0 },
            newChunks: new[] { (ChunkId: 0, Response: newChunk0, OffsetSec: 0.0) });

        // 'boundary' (StartTime=5.0) is in chunk 1's range, not chunk 0's. Patching chunk 0
        // must preserve it.
        Assert.Contains(spliced.Tokens, t => t.Word == "boundary" && t.StartTime == 5.0);
        Assert.DoesNotContain(spliced.Tokens, t => t.Word == "early");
        Assert.Contains(spliced.Tokens, t => t.Word == "patched");
        Assert.Contains(spliced.Tokens, t => t.Word == "late");
    }

    [Fact]
    public void Splice_AdjacentPatchedChunks_BothReplaced()
    {
        // Adjacent chunks 1 and 2 both patched. Tokens in either range must be removed, new
        // tokens from both must be merged.
        var plan = BuildPlan((0, 0.0, 5.0), (1, 5.0, 10.0), (2, 10.0, 15.0));
        var existing = new AsrResponse(
            "model-v1",
            tokens: new[]
            {
                new AsrToken(2.0, 0.3, "keep0"),
                new AsrToken(6.0, 0.3, "drop1"),
                new AsrToken(11.0, 0.3, "drop2"),
            });

        var newC1 = new AsrResponse("model-v2", tokens: new[] { new AsrToken(0.5, 0.3, "new1") });
        var newC2 = new AsrResponse("model-v2", tokens: new[] { new AsrToken(0.5, 0.3, "new2") });
        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: new[] { 1, 2 },
            newChunks: new[]
            {
                (ChunkId: 1, Response: newC1, OffsetSec: 5.0),
                (ChunkId: 2, Response: newC2, OffsetSec: 10.0),
            });

        Assert.Equal(
            new[] { "keep0", "new1", "new2" },
            spliced.Tokens.Select(t => t.Word).ToArray());
        Assert.Equal(5.5, spliced.Tokens[1].StartTime, precision: 4);
        Assert.Equal(10.5, spliced.Tokens[2].StartTime, precision: 4);
    }

    [Fact]
    public void Splice_NonAdjacentPatchedChunks_OnlyTheirTokensReplaced()
    {
        // Patch chunks 0 and 2 — middle chunk 1 untouched.
        var plan = BuildPlan((0, 0.0, 5.0), (1, 5.0, 10.0), (2, 10.0, 15.0));
        var middleToken = new AsrToken(7.5, 0.3, "middle");
        var existing = new AsrResponse(
            "model-v1",
            tokens: new[]
            {
                new AsrToken(2.0, 0.3, "drop0"),
                middleToken,
                new AsrToken(12.0, 0.3, "drop2"),
            });

        var newC0 = new AsrResponse("model-v2", tokens: new[] { new AsrToken(1.0, 0.3, "fix0") });
        var newC2 = new AsrResponse("model-v2", tokens: new[] { new AsrToken(2.0, 0.3, "fix2") });
        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: new[] { 0, 2 },
            newChunks: new[]
            {
                (ChunkId: 0, Response: newC0, OffsetSec: 0.0),
                (ChunkId: 2, Response: newC2, OffsetSec: 10.0),
            });

        Assert.Equal(
            new[] { "fix0", "middle", "fix2" },
            spliced.Tokens.Select(t => t.Word).ToArray());
        Assert.Same(middleToken, spliced.Tokens[1]);
    }

    [Fact]
    public void Splice_OutOfOrderNewToken_MonotonicityClampApplied()
    {
        // Defensive: even when both kept and new tokens land in legitimate windows, a kept
        // token's End may overrun the next new token's Start. The monotonicity clamp pulls the
        // later token forward (matching MergeChunkResponses semantics).
        var plan = BuildPlan((0, 0.0, 5.0), (1, 5.0, 10.0));
        var existing = new AsrResponse(
            "model-v1",
            tokens: new[]
            {
                // Long-Duration kept token from chunk 0 ends at 5.10 (overruns into chunk 1).
                new AsrToken(4.5, 0.6, "tail"),  // End = 5.10
            });
        var newC1 = new AsrResponse("model-v2",
            // Relative 0.0 → absolute 5.0, in chunk 1's window. Raw start (5.0) < lastTokenEnd
            // (5.10), so the clamp must push it forward to 5.10.
            tokens: new[] { new AsrToken(0.0, 0.3, "first") });
        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: new[] { 1 },
            newChunks: new[] { (ChunkId: 1, Response: newC1, OffsetSec: 5.0) });

        Assert.Equal(2, spliced.Tokens.Length);
        Assert.Equal("tail", spliced.Tokens[0].Word);
        Assert.Equal(4.5, spliced.Tokens[0].StartTime, precision: 4);
        Assert.Equal("first", spliced.Tokens[1].Word);
        Assert.True(spliced.Tokens[1].StartTime >= 5.10 - 1e-6,
            $"Expected monotonicity clamp; got StartTime={spliced.Tokens[1].StartTime}");
    }

    [Fact]
    public void Splice_NewTokenAtChunkEndBoundary_Discarded()
    {
        // Whisper sometimes emits a token at exactly the slice end. With a half-open
        // [start, end) window, such a token would land at the next chunk's StartSec, colliding
        // with the boundary token we preserved. Splice must drop these.
        var plan = BuildPlan((0, 0.0, 5.0), (1, 5.0, 10.0));
        var keptBoundary = new AsrToken(5.0, 0.3, "kept-boundary");
        var existing = new AsrResponse(
            "model-v1",
            tokens: new[] { new AsrToken(2.0, 0.3, "old0"), keptBoundary });

        // New chunk 0 emits one valid token + one at exactly the slice end (relative 5.0).
        var newC0 = new AsrResponse("model-v2",
            tokens: new[]
            {
                new AsrToken(1.0, 0.3, "new0"),  // absolute 1.0 — in [0, 5)
                new AsrToken(5.0, 0.3, "edge"),  // absolute 5.0 — at chunk end, OUTSIDE [0, 5)
            });
        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: new[] { 0 },
            newChunks: new[] { (ChunkId: 0, Response: newC0, OffsetSec: 0.0) });

        // 'edge' was dropped (at chunk end); 'kept-boundary' preserved (chunk 1's territory).
        Assert.DoesNotContain(spliced.Tokens, t => t.Word == "edge");
        Assert.Contains(spliced.Tokens, t => t.Word == "new0");
        Assert.Contains(spliced.Tokens, t => t.Word == "kept-boundary");
        Assert.Same(keptBoundary, spliced.Tokens.First(t => t.Word == "kept-boundary"));
    }

    [Fact]
    public void Splice_NewTokenPastChunkEnd_Discarded()
    {
        // Whisper sometimes emits a token slightly past the slice's audio (relative time
        // exceeds chunk duration). Without clipping, the offset-adjusted token would inject
        // into the next chunk's range. Splice must drop it.
        var plan = BuildPlan((0, 0.0, 5.0), (1, 5.0, 10.0));
        var existing = new AsrResponse(
            "model-v1",
            tokens: new[] { new AsrToken(6.0, 0.3, "kept-next-chunk") });

        var newC0 = new AsrResponse("model-v2",
            tokens: new[]
            {
                new AsrToken(1.0, 0.3, "in-window"),
                new AsrToken(5.5, 0.3, "overflow"),  // absolute 5.5 — past chunk 0's end
            });
        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: new[] { 0 },
            newChunks: new[] { (ChunkId: 0, Response: newC0, OffsetSec: 0.0) });

        Assert.Contains(spliced.Tokens, t => t.Word == "in-window");
        Assert.DoesNotContain(spliced.Tokens, t => t.Word == "overflow");
        Assert.Contains(spliced.Tokens, t => t.Word == "kept-next-chunk");
    }

    [Fact]
    public void Splice_NewSegmentOvershootsChunkEnd_ClampedToChunkEnd()
    {
        // A new segment whose StartSec is in-window but EndSec extends past the chunk end gets
        // its EndSec clamped to chunkEnd so it doesn't span into adjacent chunks' territory.
        var plan = BuildPlan((0, 0.0, 5.0), (1, 5.0, 10.0));
        var existing = new AsrResponse("model-v1", segments: Array.Empty<AsrSegment>());

        var newC0 = new AsrResponse("model-v2",
            segments: new[] { new AsrSegment(1.0, 5.5, "spans-end") });  // absolute end = 5.5
        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: new[] { 0 },
            newChunks: new[] { (ChunkId: 0, Response: newC0, OffsetSec: 0.0) });

        Assert.Single(spliced.Segments);
        Assert.Equal("spans-end", spliced.Segments[0].Text);
        Assert.Equal(1.0, spliced.Segments[0].StartSec, precision: 4);
        // EndSec clamped to chunk end (5.0), NOT 5.5.
        Assert.Equal(5.0, spliced.Segments[0].EndSec, precision: 4);
    }

    [Fact]
    public void Splice_NoNewChunks_PreservesExistingModelVersion()
    {
        var plan = BuildPlan((0, 0.0, 5.0));
        var existing = new AsrResponse("model-v1", tokens: new[] { new AsrToken(1.0, 0.3, "x") });
        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: Array.Empty<int>(),
            newChunks: Array.Empty<(int, AsrResponse, double)>());

        Assert.Equal("model-v1", spliced.ModelVersion);
        Assert.Single(spliced.Tokens);
        Assert.Same(existing.Tokens[0], spliced.Tokens[0]);
    }

    [Fact]
    public void Splice_SegmentsFollowSameRules()
    {
        var plan = BuildPlan((0, 0.0, 5.0), (1, 5.0, 10.0), (2, 10.0, 15.0));
        var existing = new AsrResponse(
            "model-v1",
            segments: new[]
            {
                new AsrSegment(1.0, 4.0, "keep0"),
                new AsrSegment(6.0, 9.0, "drop1"),
                new AsrSegment(11.0, 14.0, "keep2"),
            });

        var newC1 = new AsrResponse("model-v2",
            segments: new[] { new AsrSegment(0.5, 4.0, "fix1") });
        var spliced = AsrService.SpliceChunkResponses(
            existing, plan,
            patchedChunkIndices: new[] { 1 },
            newChunks: new[] { (ChunkId: 1, Response: newC1, OffsetSec: 5.0) });

        Assert.Equal(
            new[] { "keep0", "fix1", "keep2" },
            spliced.Segments.Select(s => s.Text).ToArray());
        Assert.Equal(5.5, spliced.Segments[1].StartSec, precision: 4);
        Assert.Equal(9.0, spliced.Segments[1].EndSec, precision: 4);
    }

    private static ChunkPlanDocument BuildPlan(params (int chunkId, double start, double end)[] entries)
    {
        var policy = new ChunkPlanPolicy(
            SilenceThresholdDb: -40,
            MinSilenceDurationMs: 250,
            MinChunkDurationSec: 5,
            MaxChunkDurationSec: 30,
            SampleRate: 16000);

        var chunks = entries.Select(e => new ChunkPlanEntry(
            ChunkId: e.chunkId,
            StartSample: (int)(e.start * 16000),
            LengthSamples: (int)((e.end - e.start) * 16000),
            StartSec: e.start,
            EndSec: e.end)).ToArray();

        return new ChunkPlanDocument(
            CreatedAtUtc: DateTime.UtcNow,
            SourceAudioPath: "/test/audio.wav",
            SourceAudioFingerprint: "test|0|0|0",
            Policy: policy,
            Chunks: chunks);
    }
}
