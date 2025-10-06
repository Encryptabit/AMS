using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Artifacts;
using Ams.Core.Book;
using Ams.Core.Hydrate;

namespace Ams.Core.Prosody;

public interface IPauseDynamicsService
{
    PauseAnalysisReport AnalyzeChapter(
        TranscriptIndex transcript,
        BookIndex bookIndex,
        HydratedTranscript? hydrated,
        PausePolicy policy,
        IReadOnlyList<(double Start, double End)>? intraSentenceSilences = null);

    PauseTransformSet PlanTransforms(PauseAnalysisReport analysis, PausePolicy policy);

    PauseApplyResult Apply(PauseTransformSet transforms, IReadOnlyList<SentenceTiming> baseline);

    PauseDynamicsResult Execute(
        TranscriptIndex transcript,
        BookIndex bookIndex,
        HydratedTranscript? hydrated,
        PausePolicy policy,
        IReadOnlyList<(double Start, double End)>? intraSentenceSilences = null);
}

public sealed record PauseApplyResult(
    IReadOnlyList<SentenceTiming> Timeline,
    PauseTransformSet Transforms);

public sealed record PauseDynamicsResult(
    PauseAnalysisReport Analysis,
    PauseTransformSet Plan,
    PauseApplyResult ApplyResult);

public sealed class PauseDynamicsService : IPauseDynamicsService
{
    public PauseAnalysisReport AnalyzeChapter(
        TranscriptIndex transcript,
        BookIndex bookIndex,
        HydratedTranscript? hydrated,
        PausePolicy policy,
        IReadOnlyList<(double Start, double End)>? intraSentenceSilences = null)
    {
        if (transcript is null) throw new ArgumentNullException(nameof(transcript));
        if (bookIndex is null) throw new ArgumentNullException(nameof(bookIndex));
        if (policy is null) throw new ArgumentNullException(nameof(policy));

        var spans = BuildInterSentenceSpans(transcript, bookIndex, hydrated);

        if (intraSentenceSilences is not null && intraSentenceSilences.Count > 0)
        {
            spans.AddRange(BuildIntraSentenceSpans(transcript, intraSentenceSilences));
        }

        var classStats = new Dictionary<PauseClass, PauseClassSummary>();
        foreach (var group in spans.GroupBy(span => span.Class))
        {
            classStats[group.Key] = PauseClassSummary.FromDurations(group.Select(x => x.DurationSec));
        }

        if (!classStats.ContainsKey(PauseClass.Comma))
        {
            classStats[PauseClass.Comma] = PauseClassSummary.Empty;
        }

        return new PauseAnalysisReport(spans, classStats);
    }

    public PauseTransformSet PlanTransforms(PauseAnalysisReport analysis, PausePolicy policy)
    {
        if (analysis is null) throw new ArgumentNullException(nameof(analysis));
        if (policy is null) throw new ArgumentNullException(nameof(policy));

        if (analysis.Spans.Count == 0)
        {
            return PauseTransformSet.Empty;
        }

        var classProfiles = PauseCompressionMath.BuildProfiles(analysis.Spans, policy);
        if (classProfiles.Count == 0)
        {
            return PauseTransformSet.Empty;
        }

        var adjustments = new List<PauseAdjust>();

        foreach (var span in analysis.Spans)
        {
            if (!double.IsFinite(span.DurationSec) || span.DurationSec <= 0d)
            {
                continue;
            }

            if (PauseCompressionMath.ShouldPreserve(span.DurationSec, span.Class, classProfiles))
            {
                continue;
            }

            double target = PauseCompressionMath.ComputeTargetDuration(span.DurationSec, span.Class, policy, classProfiles);
            if (!double.IsFinite(target))
            {
                continue;
            }

            if (Math.Abs(target - span.DurationSec) < TargetEpsilon)
            {
                continue;
            }

            adjustments.Add(new PauseAdjust(
                span.LeftSentenceId,
                span.RightSentenceId,
                span.Class,
                span.DurationSec,
                target,
                span.StartSec,
                span.EndSec,
                span.HasGapHint));
        }

        if (adjustments.Count == 0)
        {
            return PauseTransformSet.Empty;
        }

        return new PauseTransformSet(Array.Empty<BreathCut>(), adjustments);
    }

    public PauseApplyResult Apply(PauseTransformSet transforms, IReadOnlyList<SentenceTiming> baseline)
    {
        if (transforms is null) throw new ArgumentNullException(nameof(transforms));
        if (baseline is null) throw new ArgumentNullException(nameof(baseline));

        // Application logic will be layered on once transforms are produced. Returning the baseline timeline
        // keeps the operation a no-op for now.
        return new PauseApplyResult(baseline, transforms);
    }

    public PauseDynamicsResult Execute(
        TranscriptIndex transcript,
        BookIndex bookIndex,
        HydratedTranscript? hydrated,
        PausePolicy policy,
        IReadOnlyList<(double Start, double End)>? intraSentenceSilences = null)
    {
        var analysis = AnalyzeChapter(transcript, bookIndex, hydrated, policy, intraSentenceSilences);
        var plan = PlanTransforms(analysis, policy);
        var baseline = transcript.Sentences
            .OrderBy(s => s.Timing.StartSec)
            .Select(s => new SentenceTiming(s.Timing.StartSec, s.Timing.EndSec))
            .ToList();
        var applyResult = Apply(plan, baseline);
        return new PauseDynamicsResult(analysis, plan, applyResult);
    }

    private static List<PauseSpan> BuildInterSentenceSpans(
        TranscriptIndex transcript,
        BookIndex bookIndex,
        HydratedTranscript? hydrated)
    {
        var orderedSentences = transcript.Sentences
            .OrderBy(s => s.Timing.StartSec)
            .ThenBy(s => s.Timing.EndSec)
            .ToList();

        var sentenceToParagraph = hydrated is not null
            ? hydrated.Paragraphs.SelectMany(paragraph => paragraph.SentenceIds.Select(id => (id, paragraph.Id)))
                .ToDictionary(tuple => tuple.id, tuple => tuple.Id)
            : BuildSentenceParagraphMap(bookIndex);
        var spans = new List<PauseSpan>(Math.Max(0, orderedSentences.Count - 1));

        for (int i = 0; i < orderedSentences.Count - 1; i++)
        {
            var left = orderedSentences[i];
            var right = orderedSentences[i + 1];

            double start = left.Timing.EndSec;
            double end = right.Timing.StartSec;
            if (!double.IsFinite(start) || !double.IsFinite(end) || end <= start)
            {
                continue;
            }

            bool crossesParagraph = sentenceToParagraph.TryGetValue(left.Id, out var leftParagraph)
                && sentenceToParagraph.TryGetValue(right.Id, out var rightParagraph)
                && leftParagraph != rightParagraph;

            var pauseClass = crossesParagraph ? PauseClass.Paragraph : PauseClass.Sentence;

            spans.Add(new PauseSpan(
                left.Id,
                right.Id,
                start,
                end,
                end - start,
                pauseClass,
                HasGapHint: false,
                CrossesParagraph: crossesParagraph,
                CrossesChapterHead: false));
        }

        return spans;
    }

    private static IEnumerable<PauseSpan> BuildIntraSentenceSpans(
        TranscriptIndex transcript,
        IReadOnlyList<(double Start, double End)> silences)
    {
        const double MinGapSeconds = 0.05;
        const double Tolerance = 0.002;

        var sentences = transcript.Sentences
            .Where(s => s.Timing.Duration > 0)
            .OrderBy(s => s.Timing.StartSec)
            .ToList();

        if (sentences.Count == 0)
        {
            yield break;
        }

        foreach (var (start, end) in silences)
        {
            if (!double.IsFinite(start) || !double.IsFinite(end))
            {
                continue;
            }

            double duration = end - start;
            if (duration < MinGapSeconds)
            {
                continue;
            }

            var sentence = sentences.FirstOrDefault(s =>
                s.Timing.StartSec <= start + Tolerance &&
                s.Timing.EndSec >= end - Tolerance);

            if (sentence is null)
            {
                continue;
            }

            yield return new PauseSpan(
                sentence.Id,
                sentence.Id,
                start,
                end,
                duration,
                PauseClass.Comma,
                HasGapHint: true,
                CrossesParagraph: false,
                CrossesChapterHead: false);
        }
    }

    private static Dictionary<int, int> BuildSentenceParagraphMap(BookIndex bookIndex)
    {
        var map = new Dictionary<int, int>();

        foreach (var word in bookIndex.Words)
        {
            if (word.SentenceIndex >= 0 && word.ParagraphIndex >= 0)
            {
                map[word.SentenceIndex] = word.ParagraphIndex;
            }
        }

        return map;
    }

    private const double TargetEpsilon = 0.005;
}
