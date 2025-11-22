using System.Text.Json.Serialization;

namespace Ams.Core.Artifacts;

/// <summary>
/// Canonical timing payload for refined sentences with provenance metadata.
/// </summary>
public sealed record SentenceTiming : TimingRange
{
    [JsonConstructor]
    public SentenceTiming(double startSec, double endSec, bool fragmentBacked = false, double? confidence = null)
        : base(startSec, endSec)
    {
        FragmentBacked = fragmentBacked;
        Confidence = confidence;
    }

    public SentenceTiming(TimingRange range, bool fragmentBacked = false, double? confidence = null)
        : this(range.StartSec, range.EndSec, fragmentBacked, confidence)
    {
    }

    public bool FragmentBacked { get; init; }

    /// <summary>
    /// Optional downstream confidence or QA score. Null when unavailable.
    /// </summary>
    public double? Confidence { get; init; }

    public SentenceTiming WithFragmentBacked(bool fragmentBacked) => this with { FragmentBacked = fragmentBacked };

    public SentenceTiming WithConfidence(double? confidence) => this with { Confidence = confidence };

    public new SentenceTiming WithStart(double startSec) => new(startSec, EndSec, FragmentBacked, Confidence);

    public new SentenceTiming WithEnd(double endSec) => new(StartSec, endSec, FragmentBacked, Confidence);
}