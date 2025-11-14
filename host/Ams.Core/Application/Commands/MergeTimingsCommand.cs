using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment.Mfa;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Core.Processors.Alignment.Mfa;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Application.Commands;

public sealed class MergeTimingsCommand
{
    public Task ExecuteAsync(
        ChapterContext chapter,
        MergeTimingsOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);

        var updateHydrate = options?.ApplyToHydrate ?? true;
        var updateTranscript = options?.ApplyToTranscript ?? true;
        if (!updateHydrate && !updateTranscript)
        {
            Log.Debug("merge-timings invoked with both hydrate/transcript targets disabled; skipping.");
            return Task.CompletedTask;
        }

        var textGridIntervals = TextGridParser.ParseWordIntervals(
            (options?.TextGridFile ?? chapter.Documents.GetTextGridFile()
                ?? throw new InvalidOperationException("TextGrid artifact path is not available."))
            .FullName);

        var textGridWords = textGridIntervals
            .Where(iv => !string.IsNullOrWhiteSpace(iv.Text) && iv.End > iv.Start)
            .Select(iv => new TextGridWord(iv.Text, iv.Start, iv.End))
            .ToList();

        if (textGridWords.Count == 0)
        {
            Log.Debug("TextGrid contained no usable word intervals; skipping merge.");
            return Task.CompletedTask;
        }

        var bookIndex = chapter.Book.Documents.BookIndex
                        ?? throw new InvalidOperationException("BookIndex is not loaded.");
        if (bookIndex.Words.Length == 0)
        {
            Log.Debug("BookIndex contains no words; skipping merge.");
            return Task.CompletedTask;
        }

        var (chapterStart, chapterEnd) = ResolveChapterWordWindow(chapter, bookIndex);
        if (chapterEnd < chapterStart)
        {
            Log.Debug("Chapter word window is invalid; skipping merge.");
            return Task.CompletedTask;
        }

        var hydrate = chapter.Documents.HydratedTranscript;
        var hydrateWordTargets = new List<WordTarget>();
        var sentenceTargets = new List<SentenceTarget>();
        List<HydratedWord>? updatedHydrateWords = null;
        List<HydratedSentence>? updatedHydrateSentences = null;
        var hydrateWordsUpdated = false;
        var hydrateSentencesUpdated = false;

        if (updateHydrate && hydrate is null)
        {
            Log.Debug("Hydrated transcript not loaded; hydrate targets will be skipped.");
        }
        else if (updateHydrate && hydrate is not null)
        {
            updatedHydrateWords = hydrate.Words.ToList();
            for (var i = 0; i < updatedHydrateWords.Count; i++)
            {
                var localIndex = i;
                var word = updatedHydrateWords[localIndex];
                if (word.BookIdx is not int bookIdx)
                {
                    continue;
                }

                if (bookIdx < chapterStart || bookIdx > chapterEnd)
                {
                    continue;
                }

                hydrateWordTargets.Add(new WordTarget(bookIdx, (start, end, duration) =>
                {
                    var current = updatedHydrateWords[localIndex];
                    var updated = current with
                    {
                        StartSec = start,
                        EndSec = end,
                        DurationSec = duration
                    };
                    if (!ReferenceEquals(updated, current))
                    {
                        updatedHydrateWords[localIndex] = updated;
                        hydrateWordsUpdated = true;
                    }
                }));
            }

            updatedHydrateSentences = hydrate.Sentences.ToList();
            for (var i = 0; i < updatedHydrateSentences.Count; i++)
            {
                var localIndex = i;
                var bookRange = updatedHydrateSentences[localIndex].BookRange;
                sentenceTargets.Add(new SentenceTarget(
                    bookRange.Start,
                    bookRange.End,
                    (start, end, _) =>
                    {
                        var current = updatedHydrateSentences[localIndex];
                        var newTiming = new TimingRange(start, end);
                        if (current.Timing is { } existing && existing.Equals(newTiming))
                        {
                            return;
                        }

                        updatedHydrateSentences[localIndex] = current with { Timing = newTiming };
                        hydrateSentencesUpdated = true;
                    }));
            }
        }

        var transcript = chapter.Documents.Transcript;
        List<SentenceAlign>? updatedTranscriptSentences = null;
        var transcriptUpdated = false;
        if (updateTranscript && transcript is null)
        {
            Log.Debug("Transcript index not loaded; transcript targets will be skipped.");
        }
        else if (updateTranscript && transcript is not null)
        {
            updatedTranscriptSentences = transcript.Sentences.ToList();
            for (var i = 0; i < updatedTranscriptSentences.Count; i++)
            {
                var localIndex = i;
                var bookRange = updatedTranscriptSentences[localIndex].BookRange;
                sentenceTargets.Add(new SentenceTarget(
                    bookRange.Start,
                    bookRange.End,
                    (start, end, _) =>
                    {
                        var current = updatedTranscriptSentences[localIndex];
                        var newTiming = new TimingRange(start, end);
                        if (current.Timing.Equals(newTiming))
                        {
                            return;
                        }

                        updatedTranscriptSentences[localIndex] = current with { Timing = newTiming };
                        transcriptUpdated = true;
                    }));
            }
        }

        if (hydrateWordTargets.Count == 0 && sentenceTargets.Count == 0)
        {
            Log.Debug("No hydrate/transcript targets available for timing merge; skipping.");
            return Task.CompletedTask;
        }

        var report = MfaTimingMerger.MergeAndApply(
            textGridWords,
            idx => bookIndex.Words[idx].Text ?? string.Empty,
            chapterStart,
            chapterEnd,
            hydrateWordTargets,
            sentenceTargets,
            s => Log.Debug("{TimingMergeDetail}", s));

        if (hydrate is not null && (hydrateWordsUpdated || hydrateSentencesUpdated))
        {
            var newHydrate = hydrate with
            {
                Words = hydrateWordsUpdated ? updatedHydrateWords! : hydrate.Words,
                Sentences = hydrateSentencesUpdated ? updatedHydrateSentences! : hydrate.Sentences
            };
            chapter.Documents.HydratedTranscript = newHydrate;
        }

        var transcriptChanged = transcript is not null && transcriptUpdated;
        if (transcriptChanged && updatedTranscriptSentences is not null)
        {
            var transcriptInstance = transcript!;
            var newTranscript = transcriptInstance with { Sentences = updatedTranscriptSentences };
            chapter.Documents.Transcript = newTranscript;
        }

        if ((hydrate is not null && (hydrateWordsUpdated || hydrateSentencesUpdated)) || transcriptChanged)
        {
            chapter.Documents.SaveChanges();
        }

        Log.Debug(
            "MFA merge report: tgTokens={TgTokens}, bookTokens={BookTokens}, pairs={Pairs}, matches={Matches}, wildMatches={Wild}, ins={Insertions}, del={Deletions}, wordsUpdated={WordsUpdated}, sentencesUpdated={SentencesUpdated}",
            report.TextGridTokenCount,
            report.BookTokenCount,
            report.Pairs,
            report.Matches,
            report.WildMatches,
            report.Insertions,
            report.Deletions,
            report.WordsUpdated,
            report.SentencesUpdated);

        return Task.CompletedTask;
    }

    private static (int startWord, int endWord) ResolveChapterWordWindow(ChapterContext chapter, BookIndex bookIndex)
    {
        var start = chapter.Descriptor.BookStartWord;
        var end = chapter.Descriptor.BookEndWord;

        if (!start.HasValue || !end.HasValue)
        {
            foreach (var label in EnumerateChapterLabels(chapter))
            {
                if (string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                var section = SectionLocator.ResolveSectionByTitle(bookIndex, label);
                if (section is not null)
                {
                    start = section.StartWord;
                    end = section.EndWord;
                    break;
                }
            }
        }

        var maxIndex = Math.Max(0, bookIndex.Words.Length - 1);
        if (!start.HasValue || !end.HasValue)
        {
            return (0, maxIndex);
        }

        var normalizedStart = Math.Clamp(start.Value, 0, maxIndex);
        var normalizedEnd = Math.Clamp(end.Value, normalizedStart, maxIndex);
        return (normalizedStart, normalizedEnd);
    }

    private static IEnumerable<string> EnumerateChapterLabels(ChapterContext chapter)
    {
        if (!string.IsNullOrWhiteSpace(chapter.Descriptor.ChapterId))
        {
            yield return chapter.Descriptor.ChapterId;
        }

        foreach (var alias in chapter.Descriptor.Aliases ?? Array.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(alias))
            {
                yield return alias;
            }
        }

        if (!string.IsNullOrWhiteSpace(chapter.Descriptor.RootPath))
        {
            var label = Path.GetFileName(chapter.Descriptor.RootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!string.IsNullOrWhiteSpace(label))
            {
                yield return label;
            }
        }
    }
}

public sealed record MergeTimingsOptions
{
    public FileInfo? HydrateFile { get; init; }
    public FileInfo? TranscriptFile { get; init; }
    public FileInfo? TextGridFile { get; init; }
    public DirectoryInfo? AlignmentRootDirectory { get; init; }
    public bool ApplyToHydrate { get; init; } = true;
    public bool ApplyToTranscript { get; init; } = true;
}
