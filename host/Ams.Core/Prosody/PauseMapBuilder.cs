using System.Collections.ObjectModel;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Runtime.Book;
using SentenceTiming = Ams.Core.Artifacts.SentenceTiming;

namespace Ams.Core.Prosody;

/// <summary>
/// Builds hierarchical pause maps from transcript data and detected silences.
/// </summary>
public static class PauseMapBuilder
{
    public static ChapterPauseMap Build(
        TranscriptIndex transcript,
        BookIndex bookIndex,
        HydratedTranscript hydrated,
        PausePolicy? policy = null,
        IReadOnlyList<(double Start, double End)>? intraSentenceSilences = null,
        bool includeAllIntraSentenceGaps = false)
    {
        if (transcript is null) throw new ArgumentNullException(nameof(transcript));
        if (bookIndex is null) throw new ArgumentNullException(nameof(bookIndex));
        if (hydrated is null) throw new ArgumentNullException(nameof(hydrated));

        policy ??= PausePolicyPresets.House();

        var sentenceToParagraph = BuildSentenceToParagraphMap(hydrated);
        var paragraphSentenceOrder = BuildParagraphSentenceOrder(hydrated);
        var hydratedSentenceMap = hydrated.Sentences.ToDictionary(sentence => sentence.Id);

        var spanReport = new PauseDynamicsService().AnalyzeChapter(
            transcript,
            bookIndex,
            hydrated,
            policy,
            intraSentenceSilences,
            includeAllIntraSentenceGaps);

        var sentenceCollectors =
            CreateSentenceCollectors(transcript, hydratedSentenceMap, bookIndex, sentenceToParagraph);
        var paragraphCollectors = CreateParagraphCollectors(paragraphSentenceOrder);
        var chapterCollector = new ChapterCollector(paragraphSentenceOrder.Keys);

        foreach (var span in spanReport.Spans)
        {
            if (span.Class == PauseClass.Comma && span.LeftSentenceId == span.RightSentenceId)
            {
                if (sentenceCollectors.TryGetValue(span.LeftSentenceId, out var sentenceCollector))
                {
                    sentenceCollector.AddPause(span);
                }

                continue;
            }

            if (!sentenceCollectors.TryGetValue(span.LeftSentenceId, out var leftSentence))
            {
                continue;
            }

            int leftParagraphId = leftSentence.ParagraphId;
            int rightParagraphId = span.RightSentenceId >= 0 &&
                                   sentenceCollectors.TryGetValue(span.RightSentenceId, out var rightSentence)
                ? rightSentence.ParagraphId
                : leftParagraphId;

            var interval = new PauseInterval(span.Class, span.StartSec, span.EndSec, span.HasGapHint);

            if (leftParagraphId >= 0 && leftParagraphId == rightParagraphId &&
                paragraphCollectors.TryGetValue(leftParagraphId, out var paragraphCollector))
            {
                paragraphCollector.AddPause(leftSentence.SentenceId, interval);
            }
            else
            {
                chapterCollector.AddPause(leftParagraphId, interval);
            }
        }

        var sentenceMaps = sentenceCollectors.Values
            .Select(collector => collector.Build())
            .ToDictionary(sentence => sentence.SentenceId);

        foreach (var paragraphCollector in paragraphCollectors.Values)
        {
            var sentenceCollectorSequence = paragraphCollector.SentenceIds
                .Select(id => sentenceCollectors.TryGetValue(id, out var collector) ? collector : null)
                .Where(static collector => collector is not null)
                .Select(static collector => collector!);

            paragraphCollector.AbsorbSentenceDurations(sentenceCollectorSequence);
        }

        var paragraphMaps = paragraphCollectors.Values
            .Select(collector => collector.Build(sentenceMaps))
            .OrderBy(map => map.OriginalStart)
            .ToList();
        var paragraphMapById = paragraphMaps.ToDictionary(map => map.ParagraphId);

        foreach (var paragraphCollector in paragraphCollectors.Values)
        {
            chapterCollector.AbsorbDurations(paragraphCollector.Durations);
        }

        var chapterMap = chapterCollector.Build(paragraphMapById);

        return chapterMap;
    }

    private static Dictionary<int, SentenceCollector> CreateSentenceCollectors(
        TranscriptIndex transcript,
        IReadOnlyDictionary<int, HydratedSentence> hydratedSentences,
        BookIndex bookIndex,
        IReadOnlyDictionary<int, int> sentenceToParagraph)
    {
        var collectors = new Dictionary<int, SentenceCollector>();
        foreach (var sentence in transcript.Sentences)
        {
            int paragraphId = sentenceToParagraph.TryGetValue(sentence.Id, out var pid) ? pid : -1;
            hydratedSentences.TryGetValue(sentence.Id, out var hydrated);
            collectors[sentence.Id] = new SentenceCollector(sentence, hydrated, paragraphId, bookIndex);
        }

        return collectors;
    }

    private static Dictionary<int, ParagraphCollector> CreateParagraphCollectors(
        IReadOnlyDictionary<int, IReadOnlyList<int>> paragraphSentenceOrder)
    {
        var collectors = new Dictionary<int, ParagraphCollector>();
        foreach (var kvp in paragraphSentenceOrder)
        {
            collectors[kvp.Key] = new ParagraphCollector(kvp.Key, kvp.Value);
        }

        return collectors;
    }

    private static Dictionary<int, int> BuildSentenceToParagraphMap(HydratedTranscript hydrated)
    {
        var map = new Dictionary<int, int>();

        foreach (var paragraph in hydrated.Paragraphs)
        {
            foreach (var sentenceId in paragraph.SentenceIds)
            {
                map[sentenceId] = paragraph.Id;
            }
        }

        return map;
    }

    private static Dictionary<int, IReadOnlyList<int>> BuildParagraphSentenceOrder(HydratedTranscript hydrated)
    {
        return hydrated.Paragraphs
            .ToDictionary(
                paragraph => paragraph.Id,
                paragraph => (IReadOnlyList<int>)paragraph.SentenceIds.ToList());
    }

    private sealed class SentenceCollector
    {
        private readonly List<SentenceTimelineElement> _timeline = new();
        private readonly Dictionary<PauseClass, List<double>> _durations = new();
        private readonly List<PauseInterval> _pauseIntervals = new();

        public SentenceCollector(SentenceAlign sentence, HydratedSentence? hydrated, int paragraphId,
            BookIndex bookIndex)
        {
            SentenceId = sentence.Id;
            ParagraphId = paragraphId;
            OriginalTiming = ResolveTiming(sentence, hydrated);

            BuildWordTimeline(sentence, bookIndex);
        }

        public int SentenceId { get; }

        public int ParagraphId { get; }

        public SentenceTiming OriginalTiming { get; }

        public IReadOnlyDictionary<PauseClass, List<double>> Durations => _durations;

        public void AddPause(PauseSpan span)
        {
            var interval = new PauseInterval(span.Class, span.StartSec, span.EndSec, span.HasGapHint);
            _pauseIntervals.Add(interval);
            AddDuration(span.Class, interval.OriginalDuration);
        }

        public SentencePauseMap Build()
        {
            var pauses = _pauseIntervals.OrderBy(p => p.OriginalStart).ToList();
            var mergedTimeline = MergeTimeline(pauses);
            var stats = PauseStatsSet.FromDurations(_durations);
            return new SentencePauseMap(SentenceId, ParagraphId, OriginalTiming, mergedTimeline, stats);
        }

        private static SentenceTiming ResolveTiming(SentenceAlign sentence, HydratedSentence? hydrated)
        {
            if (hydrated?.Timing is TimingRange timing && timing.Duration > 0)
            {
                return new SentenceTiming(timing);
            }

            return new SentenceTiming(sentence.Timing);
        }

        private void BuildWordTimeline(SentenceAlign sentence, BookIndex bookIndex)
        {
            int start = Math.Clamp(sentence.BookRange.Start, 0, bookIndex.Words.Length - 1);
            int end = Math.Clamp(sentence.BookRange.End, start, bookIndex.Words.Length - 1);
            int wordCount = end >= start ? end - start + 1 : 0;

            double sentenceStart = OriginalTiming.StartSec;
            double sentenceEnd = OriginalTiming.EndSec;
            double duration = Math.Max(0d, sentenceEnd - sentenceStart);
            double step = wordCount > 0 ? duration / Math.Max(1, wordCount) : 0d;

            for (int i = 0; i < wordCount; i++)
            {
                var bookWord = bookIndex.Words[start + i];
                double originalStart = sentenceStart + step * i;
                double originalEnd = i == wordCount - 1 ? sentenceEnd : sentenceStart + step * (i + 1);
                _timeline.Add(new SentenceWordElement(
                    bookWord.WordIndex,
                    bookWord.Text,
                    originalStart,
                    originalEnd,
                    originalStart,
                    originalEnd));
            }
        }

        private IReadOnlyList<SentenceTimelineElement> MergeTimeline(List<PauseInterval> pauses)
        {
            if (pauses.Count == 0)
            {
                return new ReadOnlyCollection<SentenceTimelineElement>(_timeline);
            }

            var result = new List<SentenceTimelineElement>(_timeline.Count + pauses.Count);
            var queue = new Queue<PauseInterval>(pauses.OrderBy(p => p.OriginalStart));
            var orderedWords = _timeline.OfType<SentenceWordElement>().OrderBy(word => word.OriginalStart).ToList();

            foreach (var word in orderedWords)
            {
                result.Add(word);
                while (queue.Count > 0 && queue.Peek().OriginalStart <= word.OriginalEnd)
                {
                    result.Add(new SentencePauseElement(queue.Dequeue()));
                }
            }

            while (queue.Count > 0)
            {
                result.Add(new SentencePauseElement(queue.Dequeue()));
            }

            if (result.Count == 0)
            {
                return Array.Empty<SentenceTimelineElement>();
            }

            return result
                .OrderBy(entry => entry.OriginalStart)
                .ToList()
                .AsReadOnly();
        }

        private void AddDuration(PauseClass pauseClass, double duration)
        {
            if (!_durations.TryGetValue(pauseClass, out var list))
            {
                list = new List<double>();
                _durations[pauseClass] = list;
            }

            if (duration >= 0d && double.IsFinite(duration))
            {
                list.Add(duration);
            }
        }
    }

    private sealed class ParagraphCollector
    {
        private readonly Dictionary<int, List<PauseInterval>> _pausesBySentence = new();
        private readonly Dictionary<PauseClass, List<double>> _durations = new();

        public ParagraphCollector(int paragraphId, IReadOnlyList<int> sentenceIds)
        {
            ParagraphId = paragraphId;
            SentenceIds = sentenceIds;
        }

        public int ParagraphId { get; }

        public IReadOnlyList<int> SentenceIds { get; }

        public IReadOnlyDictionary<PauseClass, List<double>> Durations => _durations;

        public void AddPause(int leftSentenceId, PauseInterval interval)
        {
            if (!_pausesBySentence.TryGetValue(leftSentenceId, out var list))
            {
                list = new List<PauseInterval>();
                _pausesBySentence[leftSentenceId] = list;
            }

            list.Add(interval);
            AddDuration(interval.Class, interval.OriginalDuration);
        }

        public void AbsorbSentenceDurations(IEnumerable<SentenceCollector> sentenceCollectors)
        {
            foreach (var collector in sentenceCollectors)
            {
                foreach (var kvp in collector.Durations)
                {
                    AddDurationRange(kvp.Key, kvp.Value);
                }
            }
        }

        public ParagraphPauseMap Build(IDictionary<int, SentencePauseMap> sentenceMaps)
        {
            var timeline = new List<ParagraphTimelineElement>();
            double originalStart = double.PositiveInfinity;
            double originalEnd = double.NegativeInfinity;

            foreach (var sentenceId in SentenceIds)
            {
                if (!sentenceMaps.TryGetValue(sentenceId, out var sentenceMap))
                {
                    continue;
                }

                originalStart = Math.Min(originalStart, sentenceMap.OriginalTiming.StartSec);
                originalEnd = Math.Max(originalEnd, sentenceMap.OriginalTiming.EndSec);
                timeline.Add(new ParagraphSentenceElement(sentenceMap));

                if (_pausesBySentence.TryGetValue(sentenceId, out var pauseList))
                {
                    foreach (var pause in pauseList.OrderBy(p => p.OriginalStart))
                    {
                        timeline.Add(new ParagraphPauseElement(pause));
                    }
                }
            }

            if (!double.IsFinite(originalStart)) originalStart = 0d;
            if (!double.IsFinite(originalEnd)) originalEnd = originalStart;

            var orderedTimeline = timeline
                .OrderBy(entry => entry.OriginalStart)
                .ToList()
                .AsReadOnly();

            var stats = PauseStatsSet.FromDurations(_durations);
            return new ParagraphPauseMap(
                ParagraphId,
                orderedTimeline,
                SentenceIds
                    .Where(sentenceMaps.ContainsKey)
                    .Select(id => sentenceMaps[id])
                    .ToList()
                    .AsReadOnly(),
                stats,
                originalStart,
                originalEnd);
        }

        private void AddDuration(PauseClass pauseClass, double duration)
        {
            if (!_durations.TryGetValue(pauseClass, out var list))
            {
                list = new List<double>();
                _durations[pauseClass] = list;
            }

            if (duration >= 0d && double.IsFinite(duration))
            {
                list.Add(duration);
            }
        }

        private void AddDurationRange(PauseClass pauseClass, IEnumerable<double> durations)
        {
            if (!_durations.TryGetValue(pauseClass, out var list))
            {
                list = new List<double>();
                _durations[pauseClass] = list;
            }

            foreach (var duration in durations)
            {
                if (duration >= 0d && double.IsFinite(duration))
                {
                    list.Add(duration);
                }
            }
        }
    }

    private sealed class ChapterCollector
    {
        private readonly List<int> _orderedParagraphIds;
        private readonly Dictionary<int, List<PauseInterval>> _pausesByParagraph = new();
        private readonly Dictionary<PauseClass, List<double>> _durations = new();

        public ChapterCollector(IEnumerable<int> paragraphIds)
        {
            _orderedParagraphIds = paragraphIds.ToList();
        }

        public void AddPause(int leftParagraphId, PauseInterval interval)
        {
            if (!_pausesByParagraph.TryGetValue(leftParagraphId, out var list))
            {
                list = new List<PauseInterval>();
                _pausesByParagraph[leftParagraphId] = list;
            }

            list.Add(interval);
            AddDuration(interval.Class, interval.OriginalDuration);
        }

        public void AbsorbDurations(IReadOnlyDictionary<PauseClass, List<double>> durations)
        {
            foreach (var kvp in durations)
            {
                AddDurationRange(kvp.Key, kvp.Value);
            }
        }

        public ChapterPauseMap Build(IDictionary<int, ParagraphPauseMap> paragraphMaps)
        {
            var timeline = new List<ChapterTimelineElement>();
            double originalStart = double.PositiveInfinity;
            double originalEnd = double.NegativeInfinity;

            foreach (var paragraphId in _orderedParagraphIds)
            {
                if (!paragraphMaps.TryGetValue(paragraphId, out var paragraphMap))
                {
                    continue;
                }

                originalStart = Math.Min(originalStart, paragraphMap.OriginalStart);
                originalEnd = Math.Max(originalEnd, paragraphMap.OriginalEnd);
                timeline.Add(new ChapterParagraphElement(paragraphMap));

                if (_pausesByParagraph.TryGetValue(paragraphId, out var pauseList))
                {
                    foreach (var pause in pauseList.OrderBy(p => p.OriginalStart))
                    {
                        timeline.Add(new ChapterPauseElement(pause));
                    }
                }
            }

            if (!double.IsFinite(originalStart)) originalStart = 0d;
            if (!double.IsFinite(originalEnd)) originalEnd = originalStart;

            var orderedTimeline = timeline
                .OrderBy(entry => entry.OriginalStart)
                .ToList()
                .AsReadOnly();

            var stats = PauseStatsSet.FromDurations(_durations);
            var paragraphs = _orderedParagraphIds
                .Where(paragraphMaps.ContainsKey)
                .Select(id => paragraphMaps[id])
                .ToList()
                .AsReadOnly();

            return new ChapterPauseMap(orderedTimeline, paragraphs, stats, originalStart, originalEnd);
        }

        private void AddDuration(PauseClass pauseClass, double duration)
        {
            if (!_durations.TryGetValue(pauseClass, out var list))
            {
                list = new List<double>();
                _durations[pauseClass] = list;
            }

            if (duration >= 0d && double.IsFinite(duration))
            {
                list.Add(duration);
            }
        }

        private void AddDurationRange(PauseClass pauseClass, IEnumerable<double> durations)
        {
            if (!_durations.TryGetValue(pauseClass, out var list))
            {
                list = new List<double>();
                _durations[pauseClass] = list;
            }

            foreach (var duration in durations)
            {
                if (duration >= 0d && double.IsFinite(duration))
                {
                    list.Add(duration);
                }
            }
        }
    }
}