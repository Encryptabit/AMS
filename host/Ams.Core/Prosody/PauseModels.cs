namespace Ams.Core.Prosody;

/// <summary>
/// Classifies pause types by their structural role in narration.
/// </summary>
public enum PauseClass
{
    Comma,
    Sentence,
    Paragraph,
    ChapterHead,
    PostChapterRead,
    Tail,
    Other
}

/// <summary>
/// Defines minimum and maximum duration bounds for a pause category.
/// </summary>
public sealed class PauseWindow
{
    public PauseWindow(double min, double max)
    {
        if (double.IsNaN(min) || double.IsNaN(max))
        {
            throw new ArgumentException("Pause window bounds cannot be NaN.");
        }

        if (double.IsInfinity(min) || double.IsInfinity(max))
        {
            throw new ArgumentException("Pause window bounds cannot be infinite.");
        }

        if (max < min)
        {
            throw new ArgumentException("Pause window max must be greater than or equal to min.");
        }

        Min = min;
        Max = max;
    }

    public double Min { get; }
    public double Max { get; }
}

/// <summary>
/// Configuration for pause compression and dynamics processing.
/// Defines target windows for each pause class and compression parameters.
/// </summary>
public sealed class PausePolicy
{
    public PausePolicy(
        PauseWindow comma,
        PauseWindow sentence,
        PauseWindow paragraph,
        double headOfChapter,
        double postChapterRead,
        double tail,
        double kneeWidth = 0.08,
        double ratioInside = 1.25,
        double ratioOutside = 3.0,
        double preserveTopQuantile = 0.95)
    {
        Comma = comma ?? throw new ArgumentNullException(nameof(comma));
        Sentence = sentence ?? throw new ArgumentNullException(nameof(sentence));
        Paragraph = paragraph ?? throw new ArgumentNullException(nameof(paragraph));
        HeadOfChapter = headOfChapter;
        PostChapterRead = postChapterRead;
        Tail = tail;
        KneeWidth = kneeWidth;
        RatioInside = ratioInside;
        RatioOutside = ratioOutside;
        PreserveTopQuantile = preserveTopQuantile;
    }

    public PausePolicy() : this(
        new PauseWindow(0.20, 0.50),
        new PauseWindow(0.60, 1.00),
        new PauseWindow(1.10, 1.40),
        headOfChapter: 0.75,
        postChapterRead: 1.50,
        tail: 3.00)
    {
    }

    public PauseWindow Comma { get; }
    public PauseWindow Sentence { get; }
    public PauseWindow Paragraph { get; }
    public double HeadOfChapter { get; }
    public double PostChapterRead { get; }
    public double Tail { get; }
    public double KneeWidth { get; }
    public double RatioInside { get; }
    public double RatioOutside { get; }
    public double PreserveTopQuantile { get; }
}

/// <summary>
/// Configuration for breath detection and gating during pause processing.
/// </summary>
public sealed record BreathGateConfig
{
    public double FrameMs { get; init; } = 10.0;
    public double HopMs { get; init; } = 5.0;
    public double HighbandHz { get; init; } = 4000.0;
    public double HighLowRatioThreshold { get; init; } = 0.85;
    public double FlatnessThreshold { get; init; } = 0.6;
    public double ZeroCrossingThreshold { get; init; } = 0.12;
    public double MinBreathMs { get; init; } = 60.0;
    public double FadeMs { get; init; } = 6.0;
    public double GuardTailMs { get; init; } = 20.0;
    public double GuardHeadMs { get; init; } = 20.0;
}

/// <summary>
/// Indicates the source/origin of a detected pause span.
/// </summary>
public enum PauseProvenance
{
    ScriptPunctuation,
    TextGridSilence,
    ScriptAndTextGrid,
    TimelineGap,
    Unknown
}

/// <summary>
/// Represents a detected pause interval between sentences or within a sentence.
/// </summary>
public sealed record PauseSpan(
    int LeftSentenceId,
    int RightSentenceId,
    double StartSec,
    double EndSec,
    double DurationSec,
    PauseClass Class,
    bool HasGapHint,
    bool CrossesParagraph,
    bool CrossesChapterHead,
    PauseProvenance Provenance = PauseProvenance.Unknown);

/// <summary>
/// Base type for pause transformation operations.
/// </summary>
public abstract record PauseTransform;

/// <summary>
/// Represents a breath sound to be cut from the audio.
/// </summary>
public sealed record BreathCut(double StartSec, double EndSec) : PauseTransform;

/// <summary>
/// Represents a pause duration adjustment operation.
/// </summary>
public sealed record PauseAdjust(
    int LeftSentenceId,
    int RightSentenceId,
    PauseClass Class,
    double OriginalDurationSec,
    double TargetDurationSec,
    double StartSec,
    double EndSec,
    bool HasGapHint) : PauseTransform
{
    public bool IsIntraSentence => LeftSentenceId >= 0 && LeftSentenceId == RightSentenceId;
}

/// <summary>
/// Collection of planned pause transformations for a chapter.
/// </summary>
public sealed record PauseTransformSet(
    IReadOnlyList<BreathCut> BreathCuts,
    IReadOnlyList<PauseAdjust> PauseAdjusts)
{
    public static readonly PauseTransformSet Empty = new(Array.Empty<BreathCut>(), Array.Empty<PauseAdjust>());
}

/// <summary>
/// Represents an intra-sentence gap adjustment with source and target timing.
/// </summary>
public sealed record PauseIntraGap(
    int SentenceId,
    double SourceStartSec,
    double SourceEndSec,
    double TargetStartSec,
    double TargetEndSec);

/// <summary>
/// Provides standard pause policy presets.
/// </summary>
public static class PausePolicyPresets
{
    public static PausePolicy House() => new();
}