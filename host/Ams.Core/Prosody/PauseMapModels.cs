using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ams.Core.Artifacts;

namespace Ams.Core.Prosody;

public sealed record PauseStats(int Count, double Min, double Median, double Max, double Mean, double Total)
{
    public static PauseStats Empty { get; } = new(0, 0d, 0d, 0d, 0d, 0d);

    public static PauseStats FromDurations(IEnumerable<double> durations)
    {
        if (durations is null) throw new ArgumentNullException(nameof(durations));
        var list = durations.Where(value => value >= 0d && double.IsFinite(value)).OrderBy(value => value).ToList();
        if (list.Count == 0)
        {
            return Empty;
        }

        double total = list.Sum();
        double min = list[0];
        double max = list[^1];
        double median = list.Count % 2 == 1
            ? list[list.Count / 2]
            : (list[list.Count / 2 - 1] + list[list.Count / 2]) * 0.5;
        double mean = total / list.Count;

        return new PauseStats(list.Count, min, median, max, mean, total);
    }
}

public sealed class PauseStatsSet
{
    public PauseStatsSet(
        PauseStats comma,
        PauseStats sentence,
        PauseStats paragraph,
        PauseStats chapterHead,
        PauseStats postChapterRead,
        PauseStats tail,
        PauseStats other)
    {
        Comma = comma;
        Sentence = sentence;
        Paragraph = paragraph;
        ChapterHead = chapterHead;
        PostChapterRead = postChapterRead;
        Tail = tail;
        Other = other;
    }

    public PauseStats Comma { get; }
    public PauseStats Sentence { get; }
    public PauseStats Paragraph { get; }
    public PauseStats ChapterHead { get; }
    public PauseStats PostChapterRead { get; }
    public PauseStats Tail { get; }
    public PauseStats Other { get; }

    public PauseStats Get(PauseClass pauseClass) => pauseClass switch
    {
        PauseClass.Comma => Comma,
        PauseClass.Sentence => Sentence,
        PauseClass.Paragraph => Paragraph,
        PauseClass.ChapterHead => ChapterHead,
        PauseClass.PostChapterRead => PostChapterRead,
        PauseClass.Tail => Tail,
        _ => Other
    };

    public static PauseStatsSet FromDurations(IReadOnlyDictionary<PauseClass, List<double>> durations)
    {
        if (durations is null) throw new ArgumentNullException(nameof(durations));

        PauseStats Compute(PauseClass @class)
        {
            return durations.TryGetValue(@class, out var list) && list.Count > 0
                ? PauseStats.FromDurations(list)
                : PauseStats.Empty;
        }

        return new PauseStatsSet(
            Compute(PauseClass.Comma),
            Compute(PauseClass.Sentence),
            Compute(PauseClass.Paragraph),
            Compute(PauseClass.ChapterHead),
            Compute(PauseClass.PostChapterRead),
            Compute(PauseClass.Tail),
            Compute(PauseClass.Other));
    }
}

public abstract class PauseScopeBase
{
    protected PauseScopeBase(PauseStatsSet stats)
    {
        Stats = stats ?? throw new ArgumentNullException(nameof(stats));
    }

    public PauseStatsSet Stats { get; }
    public PauseStats Comma => Stats.Comma;
    public PauseStats Sentence => Stats.Sentence;
    public PauseStats Paragraph => Stats.Paragraph;
    public PauseStats ChapterHead => Stats.ChapterHead;
    public PauseStats PostChapterRead => Stats.PostChapterRead;
    public PauseStats Tail => Stats.Tail;
    public PauseStats Other => Stats.Other;
}

public sealed class PauseInterval
{
    public PauseInterval(PauseClass pauseClass, double originalStart, double originalEnd, bool hasHint)
    {
        if (!double.IsFinite(originalStart)) throw new ArgumentOutOfRangeException(nameof(originalStart));
        if (!double.IsFinite(originalEnd)) throw new ArgumentOutOfRangeException(nameof(originalEnd));
        if (originalEnd < originalStart) originalEnd = originalStart;

        Class = pauseClass;
        OriginalStart = originalStart;
        OriginalEnd = originalEnd;
        CurrentStart = originalStart;
        CurrentEnd = originalEnd;
        HasHint = hasHint;
    }

    public PauseClass Class { get; }
    public double OriginalStart { get; }
    public double OriginalEnd { get; }
    public double OriginalDuration => OriginalEnd - OriginalStart;

    public double CurrentStart { get; private set; }
    public double CurrentEnd { get; private set; }
    public double CurrentDuration => CurrentEnd - CurrentStart;
    public bool HasHint { get; }

    public void SetCurrent(double start, double end)
    {
        if (!double.IsFinite(start)) throw new ArgumentOutOfRangeException(nameof(start));
        if (!double.IsFinite(end)) throw new ArgumentOutOfRangeException(nameof(end));
        if (end < start) end = start;
        CurrentStart = start;
        CurrentEnd = end;
    }
}

public abstract record SentenceTimelineElement(double OriginalStart);

public sealed record SentenceWordElement(
    int WordIndex,
    string Text,
    double OriginalStart,
    double OriginalEnd,
    double CurrentStart,
    double CurrentEnd) : SentenceTimelineElement(OriginalStart);

public sealed record SentencePauseElement(PauseInterval Pause) : SentenceTimelineElement(Pause.OriginalStart);

public abstract record ParagraphTimelineElement(double OriginalStart);

public sealed record ParagraphSentenceElement(SentencePauseMap Sentence) : ParagraphTimelineElement(Sentence.OriginalTiming.StartSec);

public sealed record ParagraphPauseElement(PauseInterval Pause) : ParagraphTimelineElement(Pause.OriginalStart);

public abstract record ChapterTimelineElement(double OriginalStart);

public sealed record ChapterParagraphElement(ParagraphPauseMap Paragraph) : ChapterTimelineElement(Paragraph.OriginalStart);

public sealed record ChapterPauseElement(PauseInterval Pause) : ChapterTimelineElement(Pause.OriginalStart);

public sealed class SentencePauseMap : PauseScopeBase
{
    private readonly IReadOnlyList<SentenceTimelineElement> _timeline;

    public SentencePauseMap(
        int sentenceId,
        int paragraphId,
        SentenceTiming originalTiming,
        IReadOnlyList<SentenceTimelineElement> timeline,
        PauseStatsSet stats)
        : base(stats)
    {
        SentenceId = sentenceId;
        ParagraphId = paragraphId;
        OriginalTiming = originalTiming;
        CurrentTiming = originalTiming;
        _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
    }

    public int SentenceId { get; }

    public int ParagraphId { get; }

    public SentenceTiming OriginalTiming { get; }

    public SentenceTiming CurrentTiming { get; private set; }

    public IReadOnlyList<SentenceTimelineElement> Timeline => _timeline;

    public void UpdateTiming(SentenceTiming timing)
    {
        CurrentTiming = timing;
    }
}

public sealed class ParagraphPauseMap : PauseScopeBase
{
    private readonly IReadOnlyList<ParagraphTimelineElement> _timeline;
    private readonly IReadOnlyList<SentencePauseMap> _sentences;

    public ParagraphPauseMap(
        int paragraphId,
        IReadOnlyList<ParagraphTimelineElement> timeline,
        IReadOnlyList<SentencePauseMap> sentences,
        PauseStatsSet stats,
        double originalStart,
        double originalEnd)
        : base(stats)
    {
        if (!double.IsFinite(originalStart)) throw new ArgumentOutOfRangeException(nameof(originalStart));
        if (!double.IsFinite(originalEnd)) throw new ArgumentOutOfRangeException(nameof(originalEnd));
        if (originalEnd < originalStart) originalEnd = originalStart;

        ParagraphId = paragraphId;
        _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
        _sentences = sentences ?? throw new ArgumentNullException(nameof(sentences));
        OriginalStart = originalStart;
        OriginalEnd = originalEnd;
        CurrentStart = originalStart;
        CurrentEnd = originalEnd;
    }

    public int ParagraphId { get; }

    public IReadOnlyList<ParagraphTimelineElement> Timeline => _timeline;

    public IReadOnlyList<SentencePauseMap> Sentences => _sentences;

    public double OriginalStart { get; }

    public double OriginalEnd { get; }

    public double CurrentStart { get; private set; }

    public double CurrentEnd { get; private set; }

    public void UpdateBounds(double start, double end)
    {
        if (!double.IsFinite(start)) throw new ArgumentOutOfRangeException(nameof(start));
        if (!double.IsFinite(end)) throw new ArgumentOutOfRangeException(nameof(end));
        if (end < start) end = start;
        CurrentStart = start;
        CurrentEnd = end;
    }
}

public sealed class ChapterPauseMap : PauseScopeBase
{
    private readonly IReadOnlyList<ChapterTimelineElement> _timeline;
    private readonly IReadOnlyList<ParagraphPauseMap> _paragraphs;

    public ChapterPauseMap(
        IReadOnlyList<ChapterTimelineElement> timeline,
        IReadOnlyList<ParagraphPauseMap> paragraphs,
        PauseStatsSet stats,
        double originalStart,
        double originalEnd)
        : base(stats)
    {
        if (!double.IsFinite(originalStart)) throw new ArgumentOutOfRangeException(nameof(originalStart));
        if (!double.IsFinite(originalEnd)) throw new ArgumentOutOfRangeException(nameof(originalEnd));
        if (originalEnd < originalStart) originalEnd = originalStart;

        _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
        _paragraphs = paragraphs ?? throw new ArgumentNullException(nameof(paragraphs));
        OriginalStart = originalStart;
        OriginalEnd = originalEnd;
        CurrentStart = originalStart;
        CurrentEnd = originalEnd;
    }

    public IReadOnlyList<ChapterTimelineElement> Timeline => _timeline;

    public IReadOnlyList<ParagraphPauseMap> Paragraphs => _paragraphs;

    public double OriginalStart { get; }

    public double OriginalEnd { get; }

    public double CurrentStart { get; private set; }

    public double CurrentEnd { get; private set; }

    public void UpdateBounds(double start, double end)
    {
        if (!double.IsFinite(start)) throw new ArgumentOutOfRangeException(nameof(start));
        if (!double.IsFinite(end)) throw new ArgumentOutOfRangeException(nameof(end));
        if (end < start) end = start;
        CurrentStart = start;
        CurrentEnd = end;
    }
}
