using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Align.Anchors;
using Ams.Core.Align;
using Ams.Core.Align.Anchors;
using Ams.Core.Align.Tx;

namespace Ams.Core.Pipeline;

public static class SentenceRefinementPreparation
{
    public static (int Start, int End)? TryLoadSectionWordRange(string workDir, BookIndex bookIndex)
    {
        var anchorsPath = Path.Combine(workDir, "anchors", "anchors.json");
        if (!File.Exists(anchorsPath))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(anchorsPath);
            using var doc = JsonDocument.Parse(stream);
            if (!doc.RootElement.TryGetProperty("Stats", out var statsElement))
            {
                return null;
            }

            if (!statsElement.TryGetProperty("Section", out var sectionProperty))
            {
                return null;
            }

            var sectionName = sectionProperty.GetString();
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                return null;
            }

            var sections = bookIndex.Sections;
            if (sections is null || sections.Length == 0)
            {
                return null;
            }

            var match = sections.FirstOrDefault(s => string.Equals(s.Title, sectionName, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                Console.WriteLine($"[refine] section '{sectionName}' not found in book index; using full book");
                return null;
            }

            return (match.StartWord, match.EndWord);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[refine] failed to read anchors/anchors.json: {ex.Message}");
            return null;
        }
    }

    public static (BookIndex ScopedBookIndex, TranscriptIndex Transcript, TokenMapping Mapping) BuildTranscriptArtifacts(
        BookIndex bookIndex,
        AsrResponse asr,
        string audioPath,
        string bookIndexPath,
        (int Start, int End)? sectionWordRange)
    {
        var defaultStart = bookIndex.Words.Length > 0 ? bookIndex.Words[0].WordIndex : 0;
        var defaultEnd = bookIndex.Words.Length > 0 ? bookIndex.Words[^1].WordIndex : int.MaxValue;
        var (startWord, endWord) = sectionWordRange ?? (defaultStart, defaultEnd);

        var filteredWords = bookIndex.Words
            .Where(w => w.WordIndex >= startWord && w.WordIndex <= endWord)
            .ToArray();
        if (filteredWords.Length == 0)
        {
            filteredWords = bookIndex.Words;
        }

        var filteredSentences = bookIndex.Sentences
            .Where(s => s.Start >= startWord && s.End <= endWord)
            .ToArray();
        if (filteredSentences.Length == 0)
        {
            filteredSentences = bookIndex.Sentences;
        }

        var filteredParagraphs = bookIndex.Paragraphs?
            .Where(p => p.Start >= startWord && p.End <= endWord)
            .ToArray();
        if (filteredParagraphs is { Length: 0 })
        {
            filteredParagraphs = bookIndex.Paragraphs;
        }

        var filteredSections = bookIndex.Sections?
            .Where(s => s.StartWord >= startWord && s.EndWord <= endWord)
            .ToArray();

        var wordOffset = filteredWords.Length > 0 ? filteredWords[0].WordIndex : 0;
        var sentenceOffset = filteredSentences.Length > 0 ? filteredSentences[0].Index : 0;
        var paragraphOffset = filteredParagraphs is { Length: > 0 } ? filteredParagraphs[0].Index : 0;

        static int NormalizeIndex(int value, int offset)
            => value >= 0 ? value - offset : value;

        var remappedWords = filteredWords
            .Select((w, idx) => new BookWord(
                Text: w.Text,
                WordIndex: idx,
                SentenceIndex: NormalizeIndex(w.SentenceIndex, sentenceOffset),
                ParagraphIndex: NormalizeIndex(w.ParagraphIndex, paragraphOffset),
                SectionIndex: w.SectionIndex >= 0 ? 0 : w.SectionIndex))
            .ToArray();

        var remappedSentences = filteredSentences
            .Select((s, idx) => new BookSentence(
                Index: idx,
                Start: s.Start - wordOffset,
                End: s.End - wordOffset))
            .ToArray();

        var remappedParagraphs = filteredParagraphs is null
            ? Array.Empty<BookParagraph>()
            : filteredParagraphs
                .Select((p, idx) => new BookParagraph(
                    Index: idx,
                    Start: p.Start - wordOffset,
                    End: p.End - wordOffset,
                    Kind: p.Kind,
                    Style: p.Style))
                .ToArray();

        SectionRange[]? remappedSections = null;
        if (filteredSections is { Length: > 0 })
        {
            remappedSections = filteredSections
                .Select((s, idx) => new SectionRange(
                    Id: idx,
                    Title: s.Title,
                    Level: s.Level,
                    Kind: s.Kind,
                    StartWord: s.StartWord - wordOffset,
                    EndWord: s.EndWord - wordOffset,
                    StartParagraph: NormalizeIndex(s.StartParagraph, paragraphOffset),
                    EndParagraph: NormalizeIndex(s.EndParagraph, paragraphOffset)))
                .ToArray();
        }

        var scopedBookIndex = bookIndex with
        {
            Words = remappedWords,
            Sentences = remappedSentences,
            Paragraphs = remappedParagraphs,
            Sections = remappedSections
        };

        var bookView = AnchorPreprocessor.BuildBookView(scopedBookIndex);
        var asrView = AnchorPreprocessor.BuildAsrView(asr);

        var policy = new AnchorPolicy(Stopwords: StopwordSets.EnglishPlusDomain);
        var anchorResult = AnchorPipeline.ComputeAnchors(
            scopedBookIndex,
            asr,
            policy,
            new SectionDetectOptions(false, 0),
            includeWindows: true);

        var windows = anchorResult.Windows ?? new List<(int bLo, int bHi, int aLo, int aHi)>
        {
            (0, bookView.Tokens.Count, 0, asrView.Tokens.Count)
        };

        var equivalences = new Dictionary<string, string>(StringComparer.Ordinal);
        var fillers = new HashSet<string>(StringComparer.Ordinal) { "uh", "um" };

        var ops = TranscriptAligner.AlignWindows(
            bookView.Tokens,
            asrView.Tokens,
            windows,
            equivalences,
            fillers);

        var wordAligns = new List<WordAlign>(ops.Count);
        foreach (var (bi, aj, op, reason, score) in ops)
        {
            int? bookIdx = bi.HasValue && bi.Value < bookView.FilteredToOriginalWord.Count
                ? bookView.FilteredToOriginalWord[bi.Value]
                : null;
            int? asrIdx = aj.HasValue && aj.Value < asrView.FilteredToOriginalToken.Count
                ? asrView.FilteredToOriginalToken[aj.Value]
                : null;
            wordAligns.Add(new WordAlign(bookIdx, asrIdx, op, reason, score));
        }

        var bookSentences = scopedBookIndex.Sentences
            .Select(s => (s.Index, s.Start, s.End))
            .ToList();

        var bookParagraphs = scopedBookIndex.Paragraphs?.Select(p => (p.Index, p.Start, p.End)).ToList()
                            ?? new List<(int Id, int Start, int End)>();

        var (sentenceAligns, paragraphAligns) = TranscriptAligner.Rollup(
            wordAligns,
            bookSentences,
            bookParagraphs);

        var transcript = new TranscriptIndex(
            audioPath,
            bookIndex.SourceFile,
            bookIndexPath,
            DateTime.UtcNow,
            "v1.0",
            wordAligns,
            sentenceAligns,
            paragraphAligns);

        var sentenceRanges = sentenceAligns
            .Select(s => new SentenceTokenRange(
                s.Id,
                s.ScriptRange?.Start,
                s.ScriptRange?.End))
            .ToList();

        var mapping = new TokenMapping(asr, sentenceRanges, wordAligns);
        return (scopedBookIndex, transcript, mapping);
    }

    public static async Task<IReadOnlyList<ChunkAlignment>> LoadChunkAlignmentsAsync(
        string chunksDir,
        JsonSerializerOptions options,
        CancellationToken ct)
    {
        if (!Directory.Exists(chunksDir))
            throw new InvalidOperationException($"Chunk alignments not found at {chunksDir}.");

        var files = Directory.EnumerateFiles(chunksDir, "*.aeneas.json", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
            throw new InvalidOperationException($"No chunk alignment fragments found under {chunksDir}.");

        var result = new List<ChunkAlignment>(files.Count);
        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, ct);
            var alignment = JsonSerializer.Deserialize<ChunkAlignment>(json, options);
            if (alignment is null)
                throw new InvalidOperationException($"Invalid chunk alignment JSON: {Path.GetFileName(file)}");

            result.Add(alignment);
        }

        return result;
    }

    public static async Task<IReadOnlyList<SilenceEvent>> LoadSilencesAsync(
        string silenceJsonPath,
        JsonSerializerOptions options,
        CancellationToken ct)
    {
        if (!File.Exists(silenceJsonPath))
        {
            return Array.Empty<SilenceEvent>();
        }

        try
        {
            var timelineJson = await File.ReadAllTextAsync(silenceJsonPath, ct);
            var timeline = JsonSerializer.Deserialize<SilenceTimelineV2>(timelineJson, options);
            if (timeline?.Events is null || timeline.Events.Count == 0)
                return Array.Empty<SilenceEvent>();

            return timeline.Events.OrderBy(e => e.Start).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[refine] failed to parse timeline/silence.json: {ex.Message}");
            return Array.Empty<SilenceEvent>();
        }
    }
}



