using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core;
using Ams.Core.Align;
using Ams.Core.Align.Tx;
using Xunit;

namespace Ams.Tests.Refinement;

public sealed class SentenceRefinementServiceTests
{
    private readonly SentenceRefinementService _service = new();

    [Fact]
    public async Task RefineAsync_UsesFragmentTimings_WhenAvailable()
    {
        var transcript = CreateTranscriptIndex(new ScriptRange(0, 1));
        var asr = new AsrResponse("model", new[]
        {
            new AsrToken(0.0, 0.5, "hello"),
            new AsrToken(0.5, 0.5, "world")
        });

        var fragment = new FragmentTiming("chunk_001", 0, 0.1, 1.1);
        var context = new SentenceRefinementContext(
            new Dictionary<string, FragmentTiming> { ["1"] = fragment },
            Array.Empty<SilenceEvent>(),
            MinTailSec: 0.05,
            MaxSnapAheadSec: 1.0);

        var result = await _service.RefineAsync("audio.wav", transcript, asr, context, CancellationToken.None);

        var sentence = Assert.Single(result);
        Assert.Equal(0.1, sentence.Start, 6);
        Assert.Equal(1.1, sentence.End, 6);
        Assert.Equal(0, sentence.StartWordIdx);
        Assert.Equal(1, sentence.EndWordIdx);
    }

    [Fact]
    public async Task RefineAsync_SnapsEndToNearestSilence()
    {
        var transcript = CreateTranscriptIndex(new ScriptRange(0, 1));
        var asr = new AsrResponse("model", new[]
        {
            new AsrToken(0.0, 1.0, "first"),
            new AsrToken(1.0, 0.4, "second")
        });

        var silenceEvents = new List<SilenceEvent>
        {
            new SilenceEvent(1.6, 2.0, 0.4, 1.8)
        };

        var context = new SentenceRefinementContext(
            new Dictionary<string, FragmentTiming>(),
            silenceEvents,
            MinTailSec: 0.05,
            MaxSnapAheadSec: 1.0);

        var result = await _service.RefineAsync("audio.wav", transcript, asr, context, CancellationToken.None);

        var sentence = Assert.Single(result);
        Assert.Equal(0.0, sentence.Start, 6);
        Assert.Equal(1.6, sentence.End, 6);
    }

    [Fact]
    public async Task RefineAsync_FallsBackToTokenTimingWhenFragmentMissing()
    {
        var transcript = CreateTranscriptIndex(new ScriptRange(0, 0));
        var asr = new AsrResponse("model", new[]
        {
            new AsrToken(2.0, 0.4, "lone")
        });

        var context = new SentenceRefinementContext(
            new Dictionary<string, FragmentTiming>(),
            Array.Empty<SilenceEvent>(),
            MinTailSec: 0.05,
            MaxSnapAheadSec: 0.5);

        var result = await _service.RefineAsync("audio.wav", transcript, asr, context, CancellationToken.None);

        var sentence = Assert.Single(result);
        Assert.Equal(2.0, sentence.Start, 6);
        Assert.Equal(2.4, sentence.End, 6);
        Assert.Equal(0, sentence.StartWordIdx);
        Assert.Equal(0, sentence.EndWordIdx);
    }

    [Fact]
    public async Task RefineAsync_ClampsStartToPreviousEnd()
    {
        var transcript = CreateTranscriptIndex(new ScriptRange(0, 0), new ScriptRange(1, 1));
        var asr = new AsrResponse("model", new[]
        {
            new AsrToken(0.0, 0.6, "first"),
            new AsrToken(0.7, 0.6, "second")
        });

        var context = new SentenceRefinementContext(
            new Dictionary<string, FragmentTiming>
            {
                ["1"] = new FragmentTiming("chunk", 0, 0.0, 0.6),
                ["2"] = new FragmentTiming("chunk", 1, 0.5, 1.0)
            },
            Array.Empty<SilenceEvent>(),
            MinTailSec: 0.05,
            MaxSnapAheadSec: 0.5);

        var result = await _service.RefineAsync("audio.wav", transcript, asr, context, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(0.0, result[0].Start, 6);
        Assert.Equal(0.6, result[0].End, 6);
        Assert.Equal(0.6, result[1].Start, 6); // clamped to previous end
        Assert.True(result[1].End >= result[1].Start);
    }

    private static TranscriptIndex CreateTranscriptIndex(params ScriptRange[] scriptRanges)
    {
        var sentences = new List<SentenceAlign>();
        for (int i = 0; i < scriptRanges.Length; i++)
        {
            sentences.Add(new SentenceAlign(
                Id: i + 1,
                BookRange: new IntRange(i, i),
                ScriptRange: scriptRanges[i],
                Metrics: new SentenceMetrics(0, 0, 0, 0, 0),
                Status: "ok"));
        }

        return new TranscriptIndex(
            AudioPath: "audio.wav",
            ScriptPath: "script.txt",
            BookIndexPath: "book.json",
            CreatedAtUtc: DateTime.UtcNow,
            NormalizationVersion: "v1",
            Words: new List<WordAlign>(),
            Sentences: sentences,
            Paragraphs: new List<ParagraphAlign>());
    }
}
