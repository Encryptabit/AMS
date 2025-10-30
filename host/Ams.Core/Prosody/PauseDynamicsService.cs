using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ams.Core.Artifacts;
using Ams.Core.Book;
using Ams.Core.Common;
using Ams.Core.Hydrate;

namespace Ams.Core.Prosody;

public interface IPauseDynamicsService
{
    PauseAnalysisReport AnalyzeChapter(
        TranscriptIndex transcript,
        BookIndex bookIndex,
        HydratedTranscript? hydrated,
        PausePolicy policy,
        IReadOnlyList<(double Start, double End)>? intraSentenceSilences = null,
        bool includeAllIntraSentenceGaps = true);

    PauseTransformSet PlanTransforms(PauseAnalysisReport analysis, PausePolicy policy);

    PauseApplyResult Apply(PauseTransformSet transforms, IReadOnlyDictionary<int, SentenceTiming> baseline);

    PauseDynamicsResult Execute(
        TranscriptIndex transcript,
        BookIndex bookIndex,
        HydratedTranscript? hydrated,
        PausePolicy policy,
        IReadOnlyList<(double Start, double End)>? intraSentenceSilences = null,
        bool includeAllIntraSentenceGaps = true);
}

public sealed record PauseApplyResult(
    IReadOnlyDictionary<int, SentenceTiming> Timeline,
    PauseTransformSet Transforms,
    IReadOnlyList<PauseIntraGap> IntraSentenceGaps);

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
        IReadOnlyList<(double Start, double End)>? intraSentenceSilences = null,
        bool includeAllIntraSentenceGaps = true)
    {
        if (transcript is null) throw new ArgumentNullException(nameof(transcript));
        if (bookIndex is null) throw new ArgumentNullException(nameof(bookIndex));
        if (policy is null) throw new ArgumentNullException(nameof(policy));

        var sentenceToParagraph = BuildSentenceParagraphMap(bookIndex);
        var headingParagraphIds = BuildHeadingParagraphSet(bookIndex);
        var headingSentenceIds = new HashSet<int>(sentenceToParagraph
            .Where(static kvp => kvp.Value >= 0)
            .Where(kvp => headingParagraphIds.Contains(kvp.Value))
            .Select(static kvp => kvp.Key));

        var spans = BuildInterSentenceSpans(transcript, sentenceToParagraph, headingParagraphIds);
        Dictionary<int, HydratedSentence>? hydratedSentenceMap = hydrated?.Sentences.ToDictionary(s => s.Id);

        if (intraSentenceSilences is not null && intraSentenceSilences.Count > 0)
        {
            spans.AddRange(BuildIntraSentenceSpans(
                transcript,
                bookIndex,
                hydratedSentenceMap,
                intraSentenceSilences,
                headingSentenceIds,
                includeAllIntraSentenceGaps));
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
            if (span.CrossesChapterHead || span.Class == PauseClass.ChapterHead)
            {
                continue;
            }

            if (!double.IsFinite(span.DurationSec) || span.DurationSec <= 0d)
            {
                continue;
            }

            bool isIntraSentence = span.LeftSentenceId >= 0 && span.LeftSentenceId == span.RightSentenceId;

            if (PauseCompressionMath.ShouldPreserve(span.DurationSec, span.Class, classProfiles))
            {
                continue;
            }

            double target = PauseCompressionMath.ComputeTargetDuration(span.DurationSec, span.Class, policy, classProfiles);
            if (!double.IsFinite(target))
            {
                continue;
            }

            bool isShrink = target < span.DurationSec - TargetEpsilon;

            if (isIntraSentence && isShrink)
            {
                double minAllowedByRatio = span.DurationSec * IntraSentenceMinRatio;
                double minAllowed = Math.Max(IntraSentenceFloorDuration, minAllowedByRatio);
                if (minAllowed >= span.DurationSec - TargetEpsilon)
                {
                    continue;
                }

                double clampedTarget = Math.Max(target, minAllowed);
                double maxShrink = Math.Min(IntraSentenceMaxShrinkSeconds, span.DurationSec - minAllowed);
                if (maxShrink <= TargetEpsilon)
                {
                    continue;
                }

                double desiredShrink = span.DurationSec - clampedTarget;
                if (desiredShrink > maxShrink)
                {
                    clampedTarget = span.DurationSec - maxShrink;
                }

                clampedTarget = Math.Max(clampedTarget, minAllowed);

                if (span.DurationSec - clampedTarget < TargetEpsilon)
                {
                    continue;
                }

                if (Math.Abs(clampedTarget - target) > TargetEpsilon)
                {
                    Log.Info(
                        "PauseDynamics clamped intra-sentence pause target for sentence {SentenceId} from {Original:F3}s to {Clamped:F3}s",
                        span.LeftSentenceId,
                        target,
                        clampedTarget);
                }

                target = clampedTarget;
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

    public PauseApplyResult Apply(PauseTransformSet transforms, IReadOnlyDictionary<int, SentenceTiming> baseline)
    {
        if (transforms is null) throw new ArgumentNullException(nameof(transforms));
        if (baseline is null) throw new ArgumentNullException(nameof(baseline));

        if (baseline.Count == 0)
        {
            return new PauseApplyResult(
                new ReadOnlyDictionary<int, SentenceTiming>(new Dictionary<int, SentenceTiming>()),
                transforms,
                Array.Empty<PauseIntraGap>());
        }

        if (transforms.PauseAdjusts.Count == 0)
        {
            return new PauseApplyResult(
                new ReadOnlyDictionary<int, SentenceTiming>(CloneBaseline(baseline)),
                transforms,
                Array.Empty<PauseIntraGap>());
        }

        var timelineResult = PauseTimelineApplier.Apply(baseline, transforms.PauseAdjusts);
        return new PauseApplyResult(timelineResult.Timeline, transforms, timelineResult.IntraSentenceGaps);
    }

    public PauseDynamicsResult Execute(
        TranscriptIndex transcript,
        BookIndex bookIndex,
        HydratedTranscript? hydrated,
        PausePolicy policy,
        IReadOnlyList<(double Start, double End)>? intraSentenceSilences = null,
        bool includeAllIntraSentenceGaps = true)
    {
        var analysis = AnalyzeChapter(transcript, bookIndex, hydrated, policy, intraSentenceSilences, includeAllIntraSentenceGaps);
        var plan = PlanTransforms(analysis, policy);
        var sentenceToParagraph = BuildSentenceParagraphMap(bookIndex);
        var filteredPlan = FilterParagraphZeroAdjustments(plan, sentenceToParagraph);
        var baseline = transcript.Sentences
            .OrderBy(s => s.Timing.StartSec)
            .ToDictionary(
                sentence => sentence.Id,
                sentence => new SentenceTiming(sentence.Timing.StartSec, sentence.Timing.EndSec));
        var applyResult = Apply(filteredPlan, baseline);
        return new PauseDynamicsResult(analysis, filteredPlan, applyResult);
    }

    private static List<PauseSpan> BuildInterSentenceSpans(
        TranscriptIndex transcript,
        IReadOnlyDictionary<int, int> sentenceToParagraph,
        HashSet<int> headingParagraphIds)
    {
        var orderedSentences = transcript.Sentences
            .OrderBy(s => s.Timing.StartSec)
            .ThenBy(s => s.Timing.EndSec)
            .ToList();

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

            sentenceToParagraph.TryGetValue(left.Id, out var leftParagraphId);
            sentenceToParagraph.TryGetValue(right.Id, out var rightParagraphId);

            bool crossesParagraph = leftParagraphId >= 0
                && rightParagraphId >= 0
                && leftParagraphId != rightParagraphId;
            bool leftHeading = leftParagraphId >= 0 && headingParagraphIds.Contains(leftParagraphId);
            bool rightHeading = rightParagraphId >= 0 && headingParagraphIds.Contains(rightParagraphId);
            bool crossesChapterHead = leftHeading || rightHeading;

            var pauseClass = crossesParagraph ? PauseClass.Paragraph : PauseClass.Sentence;

            var span = new PauseSpan(
                left.Id,
                right.Id,
                start,
                end,
                end - start,
                pauseClass,
                HasGapHint: false,
                CrossesParagraph: crossesParagraph,
                CrossesChapterHead: crossesChapterHead,
                Provenance: PauseProvenance.ScriptPunctuation);

            spans.Add(span);

            if (span.Class == PauseClass.Comma)
            {
                Log.Info("PauseDynamics comma span from script: sentence {SentenceId} start={Start:F3}s end={End:F3}s", left.Id, span.StartSec, span.EndSec);
            }
        }

        return spans;
    }

    private static IEnumerable<PauseSpan> BuildIntraSentenceSpans(
        TranscriptIndex transcript,
        BookIndex bookIndex,
        Dictionary<int, HydratedSentence>? hydratedSentenceMap,
        IReadOnlyList<(double Start, double End)> silences,
        HashSet<int> headingSentenceIds,
        bool includeAllIntraSentenceGaps)
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

        if (silences.Count == 0)
        {
            yield break;
        }

        if (includeAllIntraSentenceGaps)
        {
            foreach (var span in BuildAllIntraSentenceSpans(sentences, silences, headingSentenceIds))
            {
                yield return span;
            }
            yield break;
        }

        var sentenceSilences = new Dictionary<int, List<(double Start, double End)>>();

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

            if (!IsReliableSentence(sentence))
            {
                continue;
            }

            if (headingSentenceIds.Contains(sentence.Id))
            {
                continue;
            }

            double safeStart = start + IntraSentenceEdgeGuardSeconds;
            double safeEnd = end - IntraSentenceEdgeGuardSeconds;
            if (safeEnd - safeStart < MinGapSeconds)
            {
                continue;
            }

            if (!sentenceSilences.TryGetValue(sentence.Id, out var list))
            {
                list = new List<(double, double)>();
                sentenceSilences[sentence.Id] = list;
            }

            list.Add((safeStart, safeEnd));
        }

        foreach (var sentence in sentences)
        {
            if (!IsReliableSentence(sentence))
            {
                continue;
            }

            if (headingSentenceIds.Contains(sentence.Id))
            {
                continue;
            }

            if (!sentenceSilences.TryGetValue(sentence.Id, out var gaps))
            {
                continue;
            }

            HydratedSentence? hydratedSentence = null;
            hydratedSentenceMap?.TryGetValue(sentence.Id, out hydratedSentence);
            var punctuationTimes = GetPunctuationTimes(sentence, bookIndex, hydratedSentence);
            if (punctuationTimes.Count == 0)
            {
                Log.Info("PauseDynamics punctuation count zero for sentence {SentenceId}", sentence.Id);
                continue;
            }

            var selectedGaps = MatchSilencesToPunctuation(gaps, punctuationTimes, sentence.Id);
            foreach (var (start, end, provenance) in selectedGaps)
            {
                double duration = end - start;
                if (duration < MinGapSeconds)
                {
                    continue;
                }

                var span = new PauseSpan(
                    sentence.Id,
                    sentence.Id,
                    start,
                    end,
                    duration,
                    PauseClass.Comma,
                    HasGapHint: true,
                    CrossesParagraph: false,
                    CrossesChapterHead: false,
                    Provenance: provenance);

                Log.Info("PauseDynamics comma span (script/textgrid) sentence {SentenceId} start={Start:F3}s end={End:F3}s provenance={Prov}",
                    sentence.Id,
                    start,
                    end,
                    span.Provenance);
                yield return span;
            }
        }

        static IEnumerable<PauseSpan> BuildAllIntraSentenceSpans(
            IReadOnlyList<SentenceAlign> sentences,
            IReadOnlyList<(double Start, double End)> silences,
            HashSet<int> headingSentenceIds)
        {
            const double MinGapSecondsLocal = 0.05;
            const double ToleranceLocal = 0.002;

        foreach (var (start, end) in silences)
        {
            if (!double.IsFinite(start) || !double.IsFinite(end))
            {
                continue;
            }

                double duration = end - start;
                if (duration < MinGapSecondsLocal)
                {
                    continue;
                }

                var sentence = sentences.FirstOrDefault(s =>
                    s.Timing.StartSec <= start + ToleranceLocal &&
                    s.Timing.EndSec >= end - ToleranceLocal);

            if (sentence is null)
            {
                continue;
            }

            if (!IsReliableSentence(sentence))
            {
                continue;
            }

            if (headingSentenceIds.Contains(sentence.Id))
            {
                continue;
            }

            double safeStart = start + IntraSentenceEdgeGuardSeconds;
            double safeEnd = end - IntraSentenceEdgeGuardSeconds;
            if (safeEnd - safeStart < MinGapSecondsLocal)
            {
                continue;
            }

            var span = new PauseSpan(
                sentence.Id,
                sentence.Id,
                safeStart,
                safeEnd,
                safeEnd - safeStart,
                PauseClass.Comma,
                HasGapHint: true,
                CrossesParagraph: false,
                CrossesChapterHead: false,
                Provenance: PauseProvenance.TextGridSilence);

            Log.Info("PauseDynamics comma span from TextGrid: sentence {SentenceId} start={Start:F3}s end={End:F3}s", sentence.Id, safeStart, safeEnd);
            yield return span;
        }
        }
    }

    private static List<double> GetPunctuationTimes(SentenceAlign sentence, BookIndex bookIndex, HydratedSentence? hydratedSentence)
    {
        double sentenceStart = sentence.Timing.StartSec;
        double sentenceEnd = sentence.Timing.EndSec;
        double duration = Math.Max(0d, sentenceEnd - sentenceStart);

        var times = ExtractTimesFromScript(sentence.Id, sentenceStart, duration, hydratedSentence?.BookText, "Book");
        if (times.Count > 0)
        {
            return times;
        }

        var fallbackTimes = ExtractTimesFromScript(sentence.Id, sentenceStart, duration, hydratedSentence?.ScriptText, "Script");
        if (fallbackTimes.Count > 0)
        {
            return fallbackTimes;
        }

        return ExtractTimesFromBookWords(sentence, bookIndex);
    }

    private static bool IsIntraSentencePunctuation(char ch) => ch == ',';

    private static List<double> ExtractTimesFromScript(
        int sentenceId,
        double sentenceStart,
        double duration,
        string? scriptText,
        string sourceLabel)
    {
        var times = new List<double>();
        if (string.IsNullOrWhiteSpace(scriptText) || duration <= 0)
        {
            return times;
        }

        var indices = new List<int>();
        for (int i = 0; i < scriptText.Length; i++)
        {
            if (IsIntraSentencePunctuation(scriptText[i]))
            {
                indices.Add(i);
            }
        }

        if (indices.Count == 0)
        {
            return times;
        }

        double length = Math.Max(1, scriptText.Length - 1);
        foreach (var idx in indices)
        {
            double ratio = idx / length;
            times.Add(sentenceStart + ratio * duration);
        }

        Log.Info(
            "PauseDynamics {Source} punctuation count {Count} for sentence {SentenceId}",
            sourceLabel,
            times.Count,
            sentenceId);

        return times;
    }

    private static List<double> ExtractTimesFromBookWords(SentenceAlign sentence, BookIndex bookIndex)
    {
        var times = new List<double>();
        var range = sentence.BookRange;
        int start = Math.Clamp(range.Start, 0, bookIndex.Words.Length - 1);
        int end = Math.Clamp(range.End, start, bookIndex.Words.Length - 1);

        double sentenceStart = sentence.Timing.StartSec;
        double sentenceEnd = sentence.Timing.EndSec;
        double duration = Math.Max(0d, sentenceEnd - sentenceStart);
        if (duration <= 0)
        {
            return times;
        }

        int wordCount = end - start + 1;
        double step = wordCount > 0 ? duration / Math.Max(1, wordCount) : 0d;

        for (int i = start; i <= end; i++)
        {
            var token = bookIndex.Words[i].Text;
            if (string.IsNullOrEmpty(token))
            {
                continue;
            }

            double wordStart = sentenceStart + step * (i - start);
            double wordEnd = (i == end) ? sentenceEnd : sentenceStart + step * (i - start + 1);
            double wordCenter = (wordStart + wordEnd) * 0.5d;

            foreach (char ch in token)
            {
                if (IsIntraSentencePunctuation(ch))
                {
                    times.Add(wordCenter);
                }
            }
        }

        return times;
    }

    private static IReadOnlyList<(double Start, double End, PauseProvenance Provenance)> MatchSilencesToPunctuation(
        List<(double Start, double End)> gaps,
        IReadOnlyList<double> punctuationTimes,
        int sentenceId)
    {
        var results = new List<(double, double, PauseProvenance)>();
        if (gaps.Count == 0 || punctuationTimes.Count == 0)
        {
            return results;
        }

        var available = gaps
            .Select((gap, idx) => new GapCandidate(idx, gap.Start, gap.End))
            .ToDictionary(candidate => candidate.Index);

        var used = new HashSet<int>();

        foreach (var punctuationTime in punctuationTimes)
        {
            double bestScore = double.MaxValue;
            int bestIndex = -1;

            foreach (var candidate in available.Values)
            {
                if (used.Contains(candidate.Index))
                {
                    continue;
                }

                double center = candidate.Center;
                double score = Math.Abs(center - punctuationTime);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = candidate.Index;
                }
            }

            if (bestIndex >= 0)
            {
                used.Add(bestIndex);
                var gap = gaps[bestIndex];
                results.Add((gap.Start, gap.End, PauseProvenance.ScriptAndTextGrid));
            }
        }

        if (results.Count < punctuationTimes.Count)
        {
            Log.Info("PauseDynamics matched {Matched} of {Expected} punctuation-driven pauses for sentence {SentenceId}",
                results.Count,
                punctuationTimes.Count,
                sentenceId);
        }

        return results;
    }

    private readonly record struct GapCandidate(int Index, double Start, double End)
    {
        public double Center => (Start + End) * 0.5d;
    }

    private static bool IsReliableSentence(SentenceAlign sentence)
    {
        return string.Equals(sentence.Status, "ok", StringComparison.OrdinalIgnoreCase);
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

    private static HashSet<int> BuildHeadingParagraphSet(BookIndex bookIndex)
    {
        var headings = new HashSet<int>();
        foreach (var paragraph in bookIndex.Paragraphs)
        {
            if (paragraph is null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(paragraph.Kind)
                && paragraph.Kind.Equals("Heading", StringComparison.OrdinalIgnoreCase))
            {
                headings.Add(paragraph.Index);
            }
        }

        return headings;
    }

    private static PauseTransformSet FilterParagraphZeroAdjustments(
        PauseTransformSet plan,
        IReadOnlyDictionary<int, int> sentenceToParagraph)
    {
        if (plan.PauseAdjusts.Count == 0)
        {
            return plan;
        }

        bool TouchesParagraphZero(int sentenceId)
        {
            if (sentenceId < 0)
            {
                return false;
            }

            return sentenceToParagraph.TryGetValue(sentenceId, out var paragraphId) && paragraphId == 0;
        }

        var filtered = plan.PauseAdjusts
            .Where(adj =>
                adj.Class is PauseClass.ChapterHead or PauseClass.PostChapterRead
                || (!TouchesParagraphZero(adj.LeftSentenceId) && !TouchesParagraphZero(adj.RightSentenceId)))
            .ToList();

        return filtered.Count == plan.PauseAdjusts.Count
            ? plan
            : new PauseTransformSet(plan.BreathCuts, filtered);
    }

    private const double TargetEpsilon = 0.005;
    private const double IntraSentenceFloorDuration = 0.12;
    private const double IntraSentenceMaxShrinkSeconds = 0.12;
    private const double IntraSentenceEdgeGuardSeconds = 0.01;
    private const double IntraSentenceMinRatio = 0.55;

    private static Dictionary<int, SentenceTiming> CloneBaseline(IReadOnlyDictionary<int, SentenceTiming> baseline)
    {
        var clone = new Dictionary<int, SentenceTiming>(baseline.Count);
        foreach (var kvp in baseline)
        {
            var timing = kvp.Value;
            clone[kvp.Key] = new SentenceTiming(timing.StartSec, timing.EndSec, timing.FragmentBacked, timing.Confidence);
        }

        return clone;
    }
}
