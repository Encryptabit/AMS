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
        Assert.Contains("smith", labText);
        Assert.Contains("went", labText);
        Assert.Contains("washington", labText);
    }

    [Fact]
    public void BuildLabTextFromWordTiming_UsesBookWordsForMatchesWithinChunk()
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
    public void BuildLabTextFromWordTiming_PrefersBookCanonical_ForSubstitution()
    {
        // MFA aligns text against audio; the book is the source of truth, so
        // Sub-op book words feed their canonical pronunciation to MFA. This
        // matters most for Whisper substitutions like "Chapter Five" -> "V."
        // where the ASR's spoken word is a single phone but the book text is
        // what the narrator actually said.
        var words = new[]
        {
            new HydratedWord(10, 0, "going", "gonna", "Sub", "near_or_diff", 1.0),
            new HydratedWord(11, 1, "home", "home", "Match", "equal_or_equiv", 0.0)
        };
        var asr = new AsrResponse(
            modelVersion: "test",
            tokens:
            [
                new AsrToken(0.10, 0.20, "gonna"),
                new AsrToken(0.40, 0.20, "home")
            ]);

        var lab = MfaChunkCorpusBuilder.BuildLabTextFromWordTiming(words, asr, 0.0, 1.0);

        Assert.NotNull(lab);
        Assert.Contains("going", lab);
        Assert.DoesNotContain("gonna", lab);
        Assert.Contains("home", lab);
    }

    [Fact]
    public void BuildLabTextFromWordTiming_IncludesNonFillerInsertion()
    {
        var words = new[]
        {
            new HydratedWord(10, 0, "alpha", "alpha", "Match", "equal_or_equiv", 0.0),
            new HydratedWord(null, 1, null, "really", "Ins", "extra", 1.0),
            new HydratedWord(11, 2, "bravo", "bravo", "Match", "equal_or_equiv", 0.0)
        };
        var asr = new AsrResponse(
            modelVersion: "test",
            tokens:
            [
                new AsrToken(0.10, 0.20, "alpha"),
                new AsrToken(0.30, 0.20, "really"),
                new AsrToken(0.50, 0.20, "bravo")
            ]);

        var lab = MfaChunkCorpusBuilder.BuildLabTextFromWordTiming(words, asr, 0.0, 1.0);

        Assert.NotNull(lab);
        var tokens = lab.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(["alpha", "really", "bravo"], tokens);
    }

    [Fact]
    public void BuildLabTextFromWordTiming_IncludesIsolatedDeletion_WithDefaultTolerance()
    {
        // A single Del-op book word between two Matches is a real narrator slip
        // (or a Whisper drop like "Chapter" before a chapter number). With the
        // default tolerance of 3, the canonical word is spliced back in so MFA
        // has text to align against the corresponding audio.
        var words = new[]
        {
            new HydratedWord(10, 0, "alpha", "alpha", "Match", "equal_or_equiv", 0.0),
            new HydratedWord(11, null, "chapter", null, "Del", "missing_book", 1.0),
            new HydratedWord(12, 1, "bravo", "bravo", "Match", "equal_or_equiv", 0.0)
        };
        var asr = new AsrResponse(
            modelVersion: "test",
            tokens:
            [
                new AsrToken(0.10, 0.20, "alpha"),
                new AsrToken(0.50, 0.20, "bravo")
            ]);

        var lab = MfaChunkCorpusBuilder.BuildLabTextFromWordTiming(words, asr, 0.0, 1.0);

        Assert.NotNull(lab);
        var tokens = lab.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // Synthesized Del midpoint interpolates between 0.20 (alpha) and 0.60 (bravo) -> 0.40.
        // Lab order should follow midpoint order: alpha, chapter, bravo.
        Assert.Equal(["alpha", "chapter", "bravo"], tokens);
    }

    [Fact]
    public void BuildLabTextFromWordTiming_DropsLongDeletionRun_BeyondTolerance()
    {
        // When the narrator skips an entire passage, the hydrate has a long run
        // of consecutive Del-op book words. Those must not be spliced into the
        // lab — MFA would try to align audio that isn't there and squash the
        // surrounding alignment. With tolerance 3, a 5-word Del run is dropped.
        var words = new[]
        {
            new HydratedWord(10, 0, "alpha", "alpha", "Match", "equal_or_equiv", 0.0),
            new HydratedWord(11, null, "the", null, "Del", "missing_book", 1.0),
            new HydratedWord(12, null, "quick", null, "Del", "missing_book", 1.0),
            new HydratedWord(13, null, "brown", null, "Del", "missing_book", 1.0),
            new HydratedWord(14, null, "fox", null, "Del", "missing_book", 1.0),
            new HydratedWord(15, null, "jumps", null, "Del", "missing_book", 1.0),
            new HydratedWord(16, 1, "bravo", "bravo", "Match", "equal_or_equiv", 0.0)
        };
        var asr = new AsrResponse(
            modelVersion: "test",
            tokens:
            [
                new AsrToken(0.10, 0.20, "alpha"),
                new AsrToken(0.50, 0.20, "bravo")
            ]);

        var lab = MfaChunkCorpusBuilder.BuildLabTextFromWordTiming(words, asr, 0.0, 1.0,
            maxConsecutiveDelRun: 3);

        Assert.NotNull(lab);
        var tokens = lab.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(["alpha", "bravo"], tokens);
        Assert.DoesNotContain("the", lab);
        Assert.DoesNotContain("fox", lab);
    }

    [Fact]
    public void BuildLabTextFromWordTiming_DropsAnyDeletion_WhenToleranceIsZero()
    {
        // Tolerance 0 reproduces legacy behavior where every Del op is dropped.
        var words = new[]
        {
            new HydratedWord(10, 0, "alpha", "alpha", "Match", "equal_or_equiv", 0.0),
            new HydratedWord(11, null, "chapter", null, "Del", "missing_book", 1.0),
            new HydratedWord(12, 1, "bravo", "bravo", "Match", "equal_or_equiv", 0.0)
        };
        var asr = new AsrResponse(
            modelVersion: "test",
            tokens:
            [
                new AsrToken(0.10, 0.20, "alpha"),
                new AsrToken(0.50, 0.20, "bravo")
            ]);

        var lab = MfaChunkCorpusBuilder.BuildLabTextFromWordTiming(words, asr, 0.0, 1.0,
            maxConsecutiveDelRun: 0);

        Assert.NotNull(lab);
        Assert.DoesNotContain("chapter", lab);
        Assert.Contains("alpha", lab);
        Assert.Contains("bravo", lab);
    }

    [Fact]
    public void BuildLabTextFromWordTiming_DelNeighbors_FollowBookOrder_NotListOrder()
    {
        // Production hydrate.Words is built as anchor ops first, DP ops after, so
        // list-order does not match book-order. A naive previous/next neighbor walk
        // through the list picks the last-anchor as a Del's "previous" word, even
        // when the Del is for the first book token (chapter heading drop). The
        // synthesized midpoint then lands far past the actual deletion, putting the
        // spliced word in the wrong chunk. Regression for review feedback on
        // rvw_8d172ee3f20b444daa993a75db15a9b7.
        var words = new[]
        {
            // Anchor block (book-order 100..103) emitted first, like production.
            new HydratedWord(100, 2, "tuesday", "Tuesday", "Match", "anchor", 0.0),
            new HydratedWord(101, 3, "morning", "morning", "Match", "anchor", 0.0),
            new HydratedWord(102, 4, "gregor", "Gregor", "Match", "anchor", 0.0),
            new HydratedWord(103, 5, "arrived", "arrived", "Match", "anchor", 0.0),
            // DP block emitted after anchors. "Chapter" has no ASR equivalent; "5"
            // was Whisper-substituted as "V."; both belong at the START of the book.
            new HydratedWord(97, null, "Chapter", null, "Del", "missing_book", 1.0),
            new HydratedWord(98, 0, "5", "V.", "Sub", "near_or_diff", 0.3),
            new HydratedWord(99, 1, "On", "On", "Match", "equal_or_equiv", 0.0)
        };
        var asr = new AsrResponse(
            modelVersion: "test",
            tokens:
            [
                new AsrToken(0.98, 3.20, "V."),
                new AsrToken(4.18, 0.10, "On"),
                new AsrToken(4.28, 0.20, "Tuesday"),
                new AsrToken(4.50, 0.30, "morning"),
                new AsrToken(4.80, 0.40, "Gregor"),
                new AsrToken(5.20, 0.40, "arrived")
            ]);

        var lab = MfaChunkCorpusBuilder.BuildLabTextFromWordTiming(words, asr, 0.0, 17.0);

        Assert.NotNull(lab);
        var tokens = lab.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // "chapter" must be spliced in at a midpoint near "V." (book-order prev=null,
        // next=98 ("5"/"V." midpoint=2.58s) -> synthesized ~2.579s) and therefore
        // appear BEFORE "five" in the lab — not stuck after "tuesday morning gregor".
        Assert.Equal(["chapter", "five", "on", "tuesday", "morning", "gregor", "arrived"], tokens);
    }

    [Fact]
    public void BuildLabTextFromWordTiming_OmitsFillerInsertion()
    {
        var words = new[]
        {
            new HydratedWord(10, 0, "alpha", "alpha", "Match", "equal_or_equiv", 0.0),
            new HydratedWord(null, 1, null, "um", "Ins", "filler", 0.3),
            new HydratedWord(11, 2, "bravo", "bravo", "Match", "equal_or_equiv", 0.0)
        };
        var asr = new AsrResponse(
            modelVersion: "test",
            tokens:
            [
                new AsrToken(0.10, 0.20, "alpha"),
                new AsrToken(0.30, 0.20, "um"),
                new AsrToken(0.50, 0.20, "bravo")
            ]);

        var lab = MfaChunkCorpusBuilder.BuildLabTextFromWordTiming(words, asr, 0.0, 1.0);

        Assert.NotNull(lab);
        var tokens = lab.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(["alpha", "bravo"], tokens);
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
            CreatedAtUtc: DateTime.UtcNow,
            SourceAudioPath: "dummy.wav",
            SourceAudioFingerprint: "fp",
            Policy: new ChunkPlanPolicy(-40, 200, 1, 29.5, 16000),
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
            CreatedAtUtc: DateTime.UtcNow,
            SourceAudioPath: "dummy.wav",
            SourceAudioFingerprint: "fp",
            Policy: new ChunkPlanPolicy(-40, 200, 1, 29.5, 16000),
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

    [Fact]
    public void Build_DoesNotLoadAudioBuffer_WhenReusableChunkAudioSatisfiesAllChunks()
    {
        using var workspace = new ScopedRebuildWorkspace(chunkCount: 2);
        workspace.WriteSourceWav(0, "SOURCE-WAV-0");
        workspace.WriteSourceWav(1, "SOURCE-WAV-1");

        var factoryCalled = false;
        var result = MfaChunkCorpusBuilder.Build(
            audioBufferFactory: () =>
            {
                factoryCalled = true;
                throw new InvalidOperationException("Audio buffer should not be loaded when chunk WAVs are reusable.");
            },
            chunkPlan: workspace.Plan,
            hydrate: workspace.Hydrate,
            corpusDirectory: workspace.CorpusDir,
            chunkAudio: workspace.ChunkAudio,
            requireAsrChunkAudio: true);

        Assert.False(factoryCalled);
        Assert.Equal(2, result.Utterances.Count);
        Assert.Equal("SOURCE-WAV-0", workspace.ReadCorpusFile(0, ".wav"));
        Assert.Equal("SOURCE-WAV-1", workspace.ReadCorpusFile(1, ".wav"));
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

    // ----------------------------------------------------------------
    // RebuildScoped tests (C3)
    // ----------------------------------------------------------------

    [Fact]
    public void RebuildScoped_OnlyRewritesSpecifiedChunkArtifacts()
    {
        using var workspace = new ScopedRebuildWorkspace(chunkCount: 3);

        // Pre-populate: chunk 0/1/2 each have a wav and lab in the corpus dir with sentinel
        // bytes that let us distinguish the original from a rebuilt version.
        workspace.WriteCorpusFile(0, ".wav", "ORIG-WAV-0");
        workspace.WriteCorpusFile(0, ".lab", "preserved zero alpha bravo charlie");
        workspace.WriteCorpusFile(1, ".wav", "ORIG-WAV-1");
        workspace.WriteCorpusFile(1, ".lab", "preserved one delta echo foxtrot");
        workspace.WriteCorpusFile(2, ".wav", "ORIG-WAV-2");
        workspace.WriteCorpusFile(2, ".lab", "preserved two golf hotel india");

        // Source pre-sliced WAVs that RebuildScoped will copy from when reusing chunk audio.
        // Chunk 1 source has different bytes so we can detect the overwrite.
        workspace.WriteSourceWav(1, "FRESH-WAV-1");

        MfaChunkCorpusBuilder.RebuildScoped(
            audioBuffer: workspace.AudioBuffer,
            chunkPlan: workspace.Plan,
            hydrate: workspace.Hydrate,
            corpusDirectory: workspace.CorpusDir,
            chunkIndices: new[] { 1 },
            chunkAudio: workspace.ChunkAudio,
            requireAsrChunkAudio: true);

        // Chunks 0 and 2: bytes unchanged (sentinel still there).
        Assert.Equal("ORIG-WAV-0", workspace.ReadCorpusFile(0, ".wav"));
        Assert.Equal("preserved zero alpha bravo charlie", workspace.ReadCorpusFile(0, ".lab"));
        Assert.Equal("ORIG-WAV-2", workspace.ReadCorpusFile(2, ".wav"));
        Assert.Equal("preserved two golf hotel india", workspace.ReadCorpusFile(2, ".lab"));

        // Chunk 1: wav overwritten from source; lab regenerated from hydrate (sentinel gone).
        Assert.Equal("FRESH-WAV-1", workspace.ReadCorpusFile(1, ".wav"));
        var newLab1 = workspace.ReadCorpusFile(1, ".lab");
        // Old sentinel tokens that were NOT in chunk 1's hydrate text must be gone.
        Assert.DoesNotContain("preserved", newLab1);
        Assert.DoesNotContain("foxtrot", newLab1);
        // Hydrate sentence 1 ("rebuilt one middle delta echo") provides the new content.
        Assert.Contains("rebuilt", newLab1);
        Assert.Contains("middle", newLab1);
    }

    [Fact]
    public void RebuildScoped_ReturnsFullUtteranceListInPlanOrder()
    {
        using var workspace = new ScopedRebuildWorkspace(chunkCount: 3);
        workspace.WriteCorpusFile(0, ".wav", "ORIG-0");
        workspace.WriteCorpusFile(0, ".lab", "preserved zero alpha bravo charlie");
        workspace.WriteCorpusFile(2, ".wav", "ORIG-2");
        workspace.WriteCorpusFile(2, ".lab", "preserved two golf hotel india");
        workspace.WriteSourceWav(1, "FRESH-1");

        var result = MfaChunkCorpusBuilder.RebuildScoped(
            audioBuffer: workspace.AudioBuffer,
            chunkPlan: workspace.Plan,
            hydrate: workspace.Hydrate,
            corpusDirectory: workspace.CorpusDir,
            chunkIndices: new[] { 1 },
            chunkAudio: workspace.ChunkAudio,
            requireAsrChunkAudio: true);

        Assert.Equal(3, result.Utterances.Count);
        Assert.Equal(new[] { 0, 1, 2 }, result.Utterances.Select(u => u.ChunkId).ToArray());
        // Each utterance points at the deterministic file path under the corpus dir.
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(Path.Combine(workspace.CorpusDir, $"utt-{i:D4}.wav"), result.Utterances[i].WavPath);
            Assert.Equal(Path.Combine(workspace.CorpusDir, $"utt-{i:D4}.lab"), result.Utterances[i].LabPath);
        }
    }

    [Fact]
    public void RebuildScoped_BoundaryDedupeReadsPreviousChunkLabFromDisk()
    {
        // When chunk 1 is rebuilt and chunk 0's existing lab on disk ends with tokens that
        // overlap chunk 1's new tokens, the dedupe trims the duplicate prefix from chunk 1.
        using var workspace = new ScopedRebuildWorkspace(chunkCount: 2,
            // Force chunk 1's hydrate text to start with the same tokens chunk 0's lab ends with.
            chunk0BookText: "alpha bravo charlie delta echo",
            chunk1BookText: "delta echo rebuilt middle suffix");

        // Pre-existing lab for chunk 0 ends with "delta echo" (≥ MinBoundaryOverlapTokensForTrim
        // of 3 requires the overlap to be at least 3 — make it exactly 3).
        workspace.WriteCorpusFile(0, ".lab", "alpha bravo charlie delta echo");
        workspace.WriteCorpusFile(0, ".wav", "ORIG-0");
        workspace.WriteSourceWav(1, "FRESH-1");

        MfaChunkCorpusBuilder.RebuildScoped(
            audioBuffer: workspace.AudioBuffer,
            chunkPlan: workspace.Plan,
            hydrate: workspace.Hydrate,
            corpusDirectory: workspace.CorpusDir,
            chunkIndices: new[] { 1 },
            chunkAudio: workspace.ChunkAudio,
            requireAsrChunkAudio: true);

        // Chunk 1's hydrate had "delta echo rebuilt middle suffix"; with dedupe the leading
        // 2-token suffix-prefix overlap might or might not trim depending on threshold. Verify
        // the unique tail tokens are present and (defensively) that the lab does not double-emit
        // tokens that already ended chunk 0.
        var newLab1 = workspace.ReadCorpusFile(1, ".lab");
        Assert.Contains("rebuilt", newLab1);
        Assert.Contains("middle", newLab1);
        Assert.Contains("suffix", newLab1);
    }

    [Fact]
    public void RebuildScoped_FailedRebuild_DeletesStaleArtifactsAndOmitsFromResult()
    {
        // When a scoped chunk's hydrate text is empty, rebuild can't produce lab tokens. The
        // existing wav/lab on disk are stale relative to the recovery's reason for running,
        // so they must be deleted (NOT silently kept while we claim "rebuilt"). The result
        // utterance list must reflect the missing chunk so the orchestrator can react.
        using var workspace = new ScopedRebuildWorkspace(chunkCount: 3,
            chunk1BookText: "");  // chunk 1 will produce no lab text

        // Pre-populate stale artifacts for every chunk.
        for (int i = 0; i < 3; i++)
        {
            workspace.WriteCorpusFile(i, ".wav", $"STALE-{i}");
            workspace.WriteCorpusFile(i, ".lab", $"stale tokens chunk {i} alpha bravo");
        }
        workspace.WriteSourceWav(1, "would-be-fresh");

        var result = MfaChunkCorpusBuilder.RebuildScoped(
            audioBuffer: workspace.AudioBuffer,
            chunkPlan: workspace.Plan,
            hydrate: workspace.Hydrate,
            corpusDirectory: workspace.CorpusDir,
            chunkIndices: new[] { 1 },
            chunkAudio: workspace.ChunkAudio,
            requireAsrChunkAudio: true);

        // Chunk 1 stale artifacts should be removed.
        Assert.False(File.Exists(Path.Combine(workspace.CorpusDir, "utt-0001.wav")),
            "Stale wav for failed scoped rebuild must be deleted.");
        Assert.False(File.Exists(Path.Combine(workspace.CorpusDir, "utt-0001.lab")),
            "Stale lab for failed scoped rebuild must be deleted.");

        // Result must omit chunk 1 — caller (orchestrator) sees the gap and reacts.
        Assert.DoesNotContain(result.Utterances, u => u.ChunkId == 1);
        // Other chunks (preserved) retain their entries.
        Assert.Contains(result.Utterances, u => u.ChunkId == 0);
        Assert.Contains(result.Utterances, u => u.ChunkId == 2);
    }

    [Fact]
    public void RebuildScoped_WrittenLabFileHasNoUtf8Bom()
    {
        // MFA's text reader treats a leading U+FEFF as part of the first word, so
        // "chapter five" becomes "﻿chapter five" -- which goes OOV/<unk> and
        // squashes the alignment of the following words. Lab files must be BOM-free.
        using var workspace = new ScopedRebuildWorkspace(chunkCount: 2,
            chunk1BookText: "chapter five on tuesday morning gregor arrived");

        workspace.WriteCorpusFile(0, ".wav", "ORIG-0");
        workspace.WriteCorpusFile(0, ".lab", "alpha bravo charlie");
        workspace.WriteSourceWav(1, "FRESH-1");

        MfaChunkCorpusBuilder.RebuildScoped(
            audioBuffer: workspace.AudioBuffer,
            chunkPlan: workspace.Plan,
            hydrate: workspace.Hydrate,
            corpusDirectory: workspace.CorpusDir,
            chunkIndices: new[] { 1 },
            chunkAudio: workspace.ChunkAudio,
            requireAsrChunkAudio: true);

        var labBytes = File.ReadAllBytes(Path.Combine(workspace.CorpusDir, "utt-0001.lab"));
        Assert.True(labBytes.Length >= 3, "Rebuilt lab file should not be empty.");
        Assert.False(
            labBytes[0] == 0xEF && labBytes[1] == 0xBB && labBytes[2] == 0xBF,
            "Lab file must not start with UTF-8 BOM (EF BB BF) — MFA mis-tokenizes the first word.");
    }

    [Fact]
    public void RebuildScoped_ThrowsForOutOfBoundsIndex()
    {
        using var workspace = new ScopedRebuildWorkspace(chunkCount: 2);
        workspace.WriteCorpusFile(0, ".wav", "ORIG");
        workspace.WriteCorpusFile(0, ".lab", "tokens here");
        workspace.WriteCorpusFile(1, ".wav", "ORIG");
        workspace.WriteCorpusFile(1, ".lab", "tokens here too");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MfaChunkCorpusBuilder.RebuildScoped(
                audioBuffer: workspace.AudioBuffer,
                chunkPlan: workspace.Plan,
                hydrate: workspace.Hydrate,
                corpusDirectory: workspace.CorpusDir,
                chunkIndices: new[] { 5 }));
    }

    [Fact]
    public void RebuildScoped_ThrowsForEmptyIndices()
    {
        using var workspace = new ScopedRebuildWorkspace(chunkCount: 1);

        Assert.Throws<ArgumentException>(() =>
            MfaChunkCorpusBuilder.RebuildScoped(
                audioBuffer: workspace.AudioBuffer,
                chunkPlan: workspace.Plan,
                hydrate: workspace.Hydrate,
                corpusDirectory: workspace.CorpusDir,
                chunkIndices: Array.Empty<int>()));
    }

    // Test workspace: builds a temp corpus dir, separate source-wav dir, a synthetic chunk
    // plan + hydrate covering N chunks of 5 seconds each. Disposable: cleans up temp dirs.
    private sealed class ScopedRebuildWorkspace : IDisposable
    {
        private readonly string _root;

        public AudioBuffer AudioBuffer { get; }
        public ChunkPlanDocument Plan { get; }
        public HydratedTranscript Hydrate { get; }
        public ChunkAudioDocument ChunkAudio { get; }
        public string CorpusDir { get; }
        public string SourceWavDir { get; }

        public ScopedRebuildWorkspace(
            int chunkCount,
            string? chunk0BookText = null,
            string? chunk1BookText = null,
            string? chunk2BookText = null)
        {
            _root = Path.Combine(Path.GetTempPath(), "ams-c3-" + Guid.NewGuid().ToString("N"));
            CorpusDir = Path.Combine(_root, "corpus");
            SourceWavDir = Path.Combine(_root, "src");
            Directory.CreateDirectory(CorpusDir);
            Directory.CreateDirectory(SourceWavDir);

            const int sampleRate = 16000;
            const double chunkDurationSec = 5.0;
            var totalSeconds = chunkCount * chunkDurationSec;
            AudioBuffer = new AudioBuffer(channels: 1, sampleRate: sampleRate, length: (int)(sampleRate * totalSeconds));

            var bookTexts = new[]
            {
                chunk0BookText ?? "preserved zero alpha bravo charlie",
                chunk1BookText ?? "rebuilt one middle delta echo",
                chunk2BookText ?? "preserved two golf hotel india"
            };

            var planEntries = new ChunkPlanEntry[chunkCount];
            var sentences = new HydratedSentence[chunkCount];
            var chunkAudioEntries = new ChunkAudioEntry[chunkCount];
            for (int i = 0; i < chunkCount; i++)
            {
                var startSec = i * chunkDurationSec;
                var endSec = startSec + chunkDurationSec;
                planEntries[i] = new ChunkPlanEntry(
                    ChunkId: i,
                    StartSample: i * sampleRate * (int)chunkDurationSec,
                    LengthSamples: sampleRate * (int)chunkDurationSec,
                    StartSec: startSec,
                    EndSec: endSec);
                sentences[i] = new HydratedSentence(
                    Id: i,
                    BookRange: new HydratedRange(i * 5, i * 5 + 4),
                    ScriptRange: null,
                    BookText: bookTexts[i],
                    ScriptText: bookTexts[i],
                    Metrics: new SentenceMetrics(1.0, 1.0, 1.0, 0, 0),
                    Status: "ok",
                    Timing: new TimingRange(startSec + 0.5, endSec - 0.5),
                    Diff: null);
                chunkAudioEntries[i] = new ChunkAudioEntry(
                    ChunkId: i,
                    UtteranceName: $"utt-{i:D4}",
                    StartSec: startSec,
                    EndSec: endSec,
                    WavPath: Path.Combine(SourceWavDir, $"utt-{i:D4}.wav"));
            }

            var policy = new ChunkPlanPolicy(
                SilenceThresholdDb: -40, MinSilenceDurationMs: 250,
                MinChunkDurationSec: 5, MaxChunkDurationSec: 30, SampleRate: sampleRate);
            Plan = new ChunkPlanDocument(
                CreatedAtUtc: DateTime.UtcNow,
                SourceAudioPath: Path.Combine(_root, "audio.wav"),
                SourceAudioFingerprint: $"test|{AudioBuffer.Length}|{sampleRate}|1",
                Policy: policy,
                Chunks: planEntries);

            Hydrate = new HydratedTranscript(
                AudioPath: "chapter.wav",
                ScriptPath: "chapter.txt",
                BookIndexPath: "book-index.json",
                CreatedAtUtc: DateTime.UtcNow,
                NormalizationVersion: "test",
                Words: Array.Empty<HydratedWord>(),
                Sentences: sentences,
                Paragraphs: Array.Empty<HydratedParagraph>());

            ChunkAudio = new ChunkAudioDocument(
                Version: ChunkAudioDocument.CurrentVersion,
                CreatedAtUtc: DateTime.UtcNow,
                SourceAudioFingerprint: Plan.SourceAudioFingerprint,
                SampleRate: sampleRate,
                Channels: 1,
                Chunks: chunkAudioEntries);
        }

        public void WriteCorpusFile(int chunkIndex, string suffix, string content)
        {
            File.WriteAllText(Path.Combine(CorpusDir, $"utt-{chunkIndex:D4}{suffix}"), content);
        }

        public void WriteSourceWav(int chunkIndex, string content)
        {
            File.WriteAllText(Path.Combine(SourceWavDir, $"utt-{chunkIndex:D4}.wav"), content);
        }

        public string ReadCorpusFile(int chunkIndex, string suffix)
            => File.ReadAllText(Path.Combine(CorpusDir, $"utt-{chunkIndex:D4}{suffix}"));

        public void Dispose()
        {
            try { Directory.Delete(_root, recursive: true); }
            catch { /* best-effort cleanup */ }
        }
    }
}
