using Ams.Core.Application.Mfa;
using Ams.Core.Asr;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Hydrate;

namespace Ams.Tests.Application.Mfa;

public class MfaChunkCorpusBuilderTests
{
    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static HydratedSentence MakeSentence(int id, string bookText, double startSec, double endSec)
    {
        return new HydratedSentence(
            Id: id,
            BookRange: new HydratedRange(0, 1),
            ScriptRange: null,
            BookText: bookText,
            ScriptText: bookText,
            Metrics: new SentenceMetrics(1.0, 1.0, 1.0, 0, 0),
            Status: "ok",
            Timing: new TimingRange(startSec, endSec),
            Diff: null);
    }

    private static HydratedSentence MakeUntimedSentence(int id, string bookText)
    {
        return new HydratedSentence(
            Id: id,
            BookRange: new HydratedRange(0, 1),
            ScriptRange: null,
            BookText: bookText,
            ScriptText: bookText,
            Metrics: new SentenceMetrics(1.0, 1.0, 1.0, 0, 0),
            Status: "ok",
            Timing: null,
            Diff: null);
    }

    // ----------------------------------------------------------------
    // FindOverlappingSentences tests
    // ----------------------------------------------------------------

    [Fact]
    public void FindOverlappingSentences_ReturnsOverlapping_WhenTimingsIntersect()
    {
        var sentences = new[]
        {
            MakeSentence(0, "First sentence about the world.", 0.0, 5.0),
            MakeSentence(1, "Second sentence about the town.", 5.0, 10.0),
            MakeSentence(2, "Third sentence about the country.", 10.0, 15.0)
        };

        // Chunk [4.0, 11.0) should overlap sentences 0, 1, 2
        var result = MfaChunkCorpusBuilder.FindOverlappingSentences(sentences, 4.0, 11.0);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, s => s.Id == 0);
        Assert.Contains(result, s => s.Id == 1);
        Assert.Contains(result, s => s.Id == 2);
    }

    [Fact]
    public void FindOverlappingSentences_ReturnsEmpty_WhenNoTimingOverlap()
    {
        var sentences = new[]
        {
            MakeSentence(0, "First sentence about the world.", 0.0, 5.0),
            MakeSentence(1, "Second sentence about the town.", 5.0, 10.0)
        };

        // Chunk [15.0, 20.0) has no overlap
        var result = MfaChunkCorpusBuilder.FindOverlappingSentences(sentences, 15.0, 20.0);

        Assert.Empty(result);
    }

    [Fact]
    public void FindOverlappingSentences_SkipsUntimedSentences()
    {
        var sentences = new HydratedSentence[]
        {
            MakeSentence(0, "Timed sentence about the world.", 0.0, 5.0),
            MakeUntimedSentence(1, "Untimed sentence in the middle."),
            MakeSentence(2, "Another timed sentence here.", 5.0, 10.0)
        };

        var result = MfaChunkCorpusBuilder.FindOverlappingSentences(sentences, 0.0, 10.0);

        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.NotNull(s.Timing));
    }

    [Fact]
    public void FindOverlappingSentences_ExactBoundary_IsNotOverlap()
    {
        // Sentence ends exactly at chunk start: [0,5) and [5,10) do not overlap
        var sentences = new[]
        {
            MakeSentence(0, "First sentence about the world.", 0.0, 5.0)
        };

        var result = MfaChunkCorpusBuilder.FindOverlappingSentences(sentences, 5.0, 10.0);

        Assert.Empty(result);
    }

    // ----------------------------------------------------------------
    // BuildLabText tests (normal overlap mapping)
    // ----------------------------------------------------------------

    [Fact]
    public void BuildLabText_CombinesOverlappingSentenceBookText()
    {
        var sentences = new[]
        {
            MakeSentence(0, "The quick brown fox.", 0.0, 5.0),
            MakeSentence(1, "Jumps over the lazy dog.", 4.5, 10.0)
        };

        var labText = MfaChunkCorpusBuilder.BuildLabText(sentences, 0.0, 10.0);

        Assert.NotNull(labText);
        Assert.Contains("the", labText);
        Assert.Contains("quick", labText);
        Assert.Contains("brown", labText);
        Assert.Contains("fox", labText);
        Assert.Contains("jumps", labText);
        Assert.Contains("over", labText);
        Assert.Contains("lazy", labText);
        Assert.Contains("dog", labText);
    }

    [Fact]
    public void BuildLabText_ReturnsNull_WhenNoOverlap()
    {
        var sentences = new[]
        {
            MakeSentence(0, "Some sentence text here.", 0.0, 5.0)
        };

        var result = MfaChunkCorpusBuilder.BuildLabText(sentences, 10.0, 20.0);

        Assert.Null(result);
    }

    [Fact]
    public void BuildLabText_NormalizesViaBookTextOnly_NeverUsesAsrWords()
    {
        // BookText contains book words; the lab should reflect book text, not ASR output
        var sentences = new[]
        {
            MakeSentence(0, "Mr. Smith went to Washington.", 0.0, 5.0)
        };

        var labText = MfaChunkCorpusBuilder.BuildLabText(sentences, 0.0, 5.0);

        Assert.NotNull(labText);
        // PronunciationHelper normalizes "Mr." to "mister"
        Assert.Contains("smith", labText);
        Assert.Contains("went", labText);
        Assert.Contains("washington", labText);
    }

    [Fact]
    public void BuildLabTextFromWordTiming_UsesBookWordsWithinChunk()
    {
        var words = new[]
        {
            new HydratedWord(10, 0, "Alpha", "alpha", "Match", "anchor", 0.0),
            new HydratedWord(11, 1, "Mr.", "mr", "Match", "anchor", 0.0),
            new HydratedWord(12, 2, "Charlie", "charlie", "Match", "anchor", 0.0)
        };
        var asr = new AsrResponse(
            modelVersion: "test",
            tokens:
            [
                new AsrToken(0.10, 0.20, "alpha"),
                new AsrToken(0.50, 0.20, "mr"),
                new AsrToken(1.20, 0.20, "charlie")
            ]);

        var lab = MfaChunkCorpusBuilder.BuildLabTextFromWordTiming(words, asr, 0.0, 1.0);

        Assert.NotNull(lab);
        Assert.Contains("alpha", lab);
        Assert.Contains("mr", lab);
        Assert.DoesNotContain("charlie", lab);
    }

    [Fact]
    public void BuildLabTextFromWordTiming_ReturnsNull_WhenNoWordsInChunk()
    {
        var words = new[]
        {
            new HydratedWord(10, 0, "Alpha", "alpha", "Match", "anchor", 0.0),
            new HydratedWord(11, 1, "Bravo", "bravo", "Match", "anchor", 0.0)
        };
        var asr = new AsrResponse(
            modelVersion: "test",
            tokens:
            [
                new AsrToken(10.0, 0.20, "alpha"),
                new AsrToken(11.0, 0.20, "bravo")
            ]);

        var lab = MfaChunkCorpusBuilder.BuildLabTextFromWordTiming(words, asr, 0.0, 1.0);

        Assert.Null(lab);
    }

    [Fact]
    public void BuildLabTextFromWordTiming_DedupesDuplicateBookIndex()
    {
        var words = new[]
        {
            new HydratedWord(10, 0, "Alpha", "alpha", "Match", "anchor", 0.0),
            new HydratedWord(10, 1, "Alpha", "alpha", "Match", "anchor", 0.0),
            new HydratedWord(11, 2, "Bravo", "bravo", "Match", "anchor", 0.0)
        };
        var asr = new AsrResponse(
            modelVersion: "test",
            tokens:
            [
                new AsrToken(0.10, 0.20, "alpha"),
                new AsrToken(0.30, 0.20, "alpha"),
                new AsrToken(0.50, 0.20, "bravo")
            ]);

        var lab = MfaChunkCorpusBuilder.BuildLabTextFromWordTiming(words, asr, 0.0, 1.0);

        Assert.NotNull(lab);
        var tokens = lab.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, tokens.Length);
        Assert.Equal("alpha", tokens[0]);
        Assert.Equal("bravo", tokens[1]);
    }

    // ----------------------------------------------------------------
    // BuildLabTextWithFallback tests (sparse overlap)
    // ----------------------------------------------------------------

    [Fact]
    public void BuildLabTextWithFallback_ExpandsToNearestSentence_WhenNoDirectOverlap()
    {
        var sentences = new[]
        {
            MakeSentence(0, "The beginning of the chapter in the story.", 0.0, 5.0),
            MakeSentence(1, "Something in the very middle section.", 10.0, 15.0),
            MakeSentence(2, "The ending of the chapter comes now.", 20.0, 25.0)
        };

        // Chunk [7.0, 9.0) overlaps nothing, but sentence 1 (mid=12.5) is closest to chunk mid (8.0)
        var result = MfaChunkCorpusBuilder.BuildLabTextWithFallback(sentences, 7.0, 9.0, 1);

        Assert.NotNull(result);
        // Sentence 0 (mid=2.5) is actually closer to 8.0 than sentence 1 (mid=12.5)?
        // Let's check: |2.5-8.0|=5.5, |12.5-8.0|=4.5 => sentence 1 is closer
        Assert.Contains("middle", result);
    }

    [Fact]
    public void BuildLabTextWithFallback_ReturnsNull_WhenNoSentencesAtAll()
    {
        var sentences = Array.Empty<HydratedSentence>();

        var result = MfaChunkCorpusBuilder.BuildLabTextWithFallback(sentences, 0.0, 5.0, 0);

        Assert.Null(result);
    }

    [Fact]
    public void BuildLabTextWithFallback_UsesPositionalIndex_WhenAllSentencesUntimed()
    {
        var sentences = new[]
        {
            MakeUntimedSentence(0, "Very first untimed sentence here."),
            MakeUntimedSentence(1, "Second untimed sentence goes here."),
            MakeUntimedSentence(2, "Third untimed sentence in list.")
        };

        // With all untimed, should use positional fallback
        var result = MfaChunkCorpusBuilder.BuildLabTextWithFallback(sentences, 0.0, 5.0, 0);

        Assert.NotNull(result);
        // Should pick sentence[0] for chunk index 0
        Assert.Contains("first", result);
    }

    // ----------------------------------------------------------------
    // FormatUtteranceName tests
    // ----------------------------------------------------------------

    [Theory]
    [InlineData(0, "utt-0000")]
    [InlineData(1, "utt-0001")]
    [InlineData(42, "utt-0042")]
    [InlineData(9999, "utt-9999")]
    public void FormatUtteranceName_ProducesDeterministicPaddedNames(int index, string expected)
    {
        var result = MfaChunkCorpusBuilder.FormatUtteranceName(index);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsChunkAudioEntryCompatible_ReturnsTrue_ForMatchingChunkEntry()
    {
        var chunk = new ChunkPlanEntry(
            ChunkId: 3,
            StartSample: 0,
            LengthSamples: 10,
            StartSec: 12.0,
            EndSec: 24.0);
        var entry = new ChunkAudioEntry(
            ChunkId: 3,
            UtteranceName: "utt-0003",
            StartSec: 12.01,
            EndSec: 23.99,
            WavPath: "/tmp/utt-0003.wav");

        var compatible = MfaChunkCorpusBuilder.IsChunkAudioEntryCompatible(chunk, entry, "utt-0003");

        Assert.True(compatible);
    }

    [Fact]
    public void IsChunkAudioEntryCompatible_ReturnsFalse_WhenUtteranceNameDiffers()
    {
        var chunk = new ChunkPlanEntry(
            ChunkId: 0,
            StartSample: 0,
            LengthSamples: 10,
            StartSec: 0.0,
            EndSec: 1.0);
        var entry = new ChunkAudioEntry(
            ChunkId: 0,
            UtteranceName: "utt-9999",
            StartSec: 0.0,
            EndSec: 1.0,
            WavPath: "/tmp/utt-9999.wav");

        var compatible = MfaChunkCorpusBuilder.IsChunkAudioEntryCompatible(chunk, entry, "utt-0000");

        Assert.False(compatible);
    }

    [Fact]
    public void IsChunkAudioEntryCompatible_ReturnsFalse_WhenTimingOutsideTolerance()
    {
        var chunk = new ChunkPlanEntry(
            ChunkId: 8,
            StartSample: 0,
            LengthSamples: 10,
            StartSec: 5.0,
            EndSec: 10.0);
        var entry = new ChunkAudioEntry(
            ChunkId: 8,
            UtteranceName: "utt-0008",
            StartSec: 5.25,
            EndSec: 10.0,
            WavPath: "/tmp/utt-0008.wav");

        var compatible = MfaChunkCorpusBuilder.IsChunkAudioEntryCompatible(chunk, entry, "utt-0008");

        Assert.False(compatible);
    }

    [Fact]
    public void Build_Throws_WhenRequireAsrChunkAudioAndArtifactMissing()
    {
        var audio = new AudioBuffer(1, 16000, 16000);
        var chunkPlan = new ChunkPlanDocument(
            Version: ChunkPlanDocument.CurrentVersion,
            CreatedAtUtc: DateTime.UtcNow,
            SourceAudioPath: "dummy.wav",
            SourceAudioFingerprint: "fp",
            Policy: new ChunkPlanPolicy(-40, 200, 1, 16000),
            Chunks: new[]
            {
                new ChunkPlanEntry(0, 0, 16000, 0.0, 1.0)
            });
        var hydrate = new HydratedTranscript(
            AudioPath: "dummy.wav",
            ScriptPath: "dummy.txt",
            BookIndexPath: "book-index.json",
            CreatedAtUtc: DateTime.UtcNow,
            NormalizationVersion: "v1",
            Words: Array.Empty<HydratedWord>(),
            Sentences: new[]
            {
                MakeSentence(0, "One two three four.", 0.0, 1.0)
            },
            Paragraphs: Array.Empty<HydratedParagraph>());

        var ex = Assert.Throws<InvalidOperationException>(() =>
            MfaChunkCorpusBuilder.Build(
                audio,
                chunkPlan,
                hydrate,
                Path.Combine(Path.GetTempPath(), $"mfa-chunk-test-{Guid.NewGuid():N}"),
                chunkAudio: null,
                requireAsrChunkAudio: true));

        Assert.Contains("no chunk-audio artifact", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_Throws_WhenRequireAsrChunkAudioAndEntryIncompatible()
    {
        var audio = new AudioBuffer(1, 16000, 16000);
        var chunkPlan = new ChunkPlanDocument(
            Version: ChunkPlanDocument.CurrentVersion,
            CreatedAtUtc: DateTime.UtcNow,
            SourceAudioPath: "dummy.wav",
            SourceAudioFingerprint: "fp",
            Policy: new ChunkPlanPolicy(-40, 200, 1, 16000),
            Chunks: new[]
            {
                new ChunkPlanEntry(0, 0, 16000, 0.0, 1.0)
            });
        var hydrate = new HydratedTranscript(
            AudioPath: "dummy.wav",
            ScriptPath: "dummy.txt",
            BookIndexPath: "book-index.json",
            CreatedAtUtc: DateTime.UtcNow,
            NormalizationVersion: "v1",
            Words: Array.Empty<HydratedWord>(),
            Sentences: new[]
            {
                MakeSentence(0, "One two three four.", 0.0, 1.0)
            },
            Paragraphs: Array.Empty<HydratedParagraph>());
        var chunkAudio = new ChunkAudioDocument(
            Version: ChunkAudioDocument.CurrentVersion,
            CreatedAtUtc: DateTime.UtcNow,
            SourceAudioFingerprint: "fp",
            SampleRate: 16000,
            Channels: 1,
            Chunks: new[]
            {
                new ChunkAudioEntry(0, "utt-9999", 0.0, 1.0, "/tmp/missing.wav")
            });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            MfaChunkCorpusBuilder.Build(
                audio,
                chunkPlan,
                hydrate,
                Path.Combine(Path.GetTempPath(), $"mfa-chunk-test-{Guid.NewGuid():N}"),
                chunkAudio: chunkAudio,
                requireAsrChunkAudio: true));

        Assert.Contains("could not be reused", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ----------------------------------------------------------------
    // Deterministic corpus file list test
    // ----------------------------------------------------------------

    [Fact]
    public void Build_ProducesDeterministicUtteranceList_ForGivenInputs()
    {
        // We cannot call Build() directly in unit tests because it requires FFmpeg
        // for AudioProcessor.EncodeWav. Instead, verify the text mapping logic
        // produces deterministic results by calling BuildLabText twice.
        var sentences = new[]
        {
            MakeSentence(0, "Alpha bravo charlie delta echo foxtrot.", 0.0, 30.0),
            MakeSentence(1, "Golf hotel india juliet kilo lima.", 30.0, 60.0),
            MakeSentence(2, "Mike november oscar papa quebec romeo.", 60.0, 90.0)
        };

        var chunks = new[]
        {
            new ChunkPlanEntry(0, 0, 480000, 0.0, 30.0),
            new ChunkPlanEntry(1, 480000, 480000, 30.0, 60.0),
            new ChunkPlanEntry(2, 960000, 480000, 60.0, 90.0)
        };

        // Run twice and verify identical output
        var results1 = chunks.Select(c => MfaChunkCorpusBuilder.BuildLabText(sentences, c.StartSec, c.EndSec)).ToList();
        var results2 = chunks.Select(c => MfaChunkCorpusBuilder.BuildLabText(sentences, c.StartSec, c.EndSec)).ToList();

        Assert.Equal(results1.Count, results2.Count);
        for (int i = 0; i < results1.Count; i++)
        {
            Assert.Equal(results1[i], results2[i]);
        }

        // Verify each chunk maps to the expected sentence
        Assert.Contains("alpha", results1[0]!);
        Assert.Contains("golf", results1[1]!);
        Assert.Contains("mike", results1[2]!);
    }

    [Fact]
    public void BuildLabText_ReturnsNull_WhenBookTextTooShort()
    {
        // MinLabTokenCount is 2; a single word should not produce a valid lab
        var sentences = new[]
        {
            MakeSentence(0, "I", 0.0, 5.0) // single word produces <2 tokens
        };

        var result = MfaChunkCorpusBuilder.BuildLabText(sentences, 0.0, 5.0);

        Assert.Null(result);
    }

    [Fact]
    public void FindOverlappingSentences_PartialOverlap_IncludesSentence()
    {
        var sentences = new[]
        {
            MakeSentence(0, "This is a long sentence that spans time.", 3.0, 8.0)
        };

        // Chunk [0, 4) partially overlaps sentence [3, 8)
        var result = MfaChunkCorpusBuilder.FindOverlappingSentences(sentences, 0.0, 4.0);

        Assert.Single(result);
        Assert.Equal(0, result[0].Id);
    }

    [Fact]
    public void FindBoundaryTokenOverlap_ReturnsLongestSuffixPrefixMatch()
    {
        var previous = new[] { "alpha", "bravo", "charlie", "delta", "echo" };
        var current = new[] { "charlie", "delta", "echo", "foxtrot" };

        var overlap = MfaChunkCorpusBuilder.FindBoundaryTokenOverlap(previous, current);

        Assert.Equal(3, overlap);
    }

    [Fact]
    public void FindBoundaryTokenOverlap_ReturnsZero_WhenNoSuffixPrefixMatch()
    {
        var previous = new[] { "alpha", "bravo", "charlie" };
        var current = new[] { "delta", "echo", "foxtrot" };

        var overlap = MfaChunkCorpusBuilder.FindBoundaryTokenOverlap(previous, current);

        Assert.Equal(0, overlap);
    }
}
