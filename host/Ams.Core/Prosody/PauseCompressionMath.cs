using System;
using System.Collections.Generic;
using System.Linq;

namespace Ams.Core.Prosody;

public static class PauseCompressionMath
{
    public sealed record PauseCompressionProfile(PauseBounds Bounds, double? PreserveThreshold);

    public readonly struct PauseBounds
    {
        public PauseBounds(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public double Min { get; }
        public double Max { get; }
    }

    public static IReadOnlyDictionary<PauseClass, PauseCompressionProfile> BuildProfiles(IEnumerable<PauseSpan> spans, PausePolicy policy)
    {
        if (spans is null) throw new ArgumentNullException(nameof(spans));
        if (policy is null) throw new ArgumentNullException(nameof(policy));

        var durations = new Dictionary<PauseClass, List<double>>();
        foreach (var span in spans)
        {
            if (!double.IsFinite(span.DurationSec) || span.DurationSec <= 0d)
            {
                continue;
            }

            if (!durations.TryGetValue(span.Class, out var list))
            {
                list = new List<double>();
                durations[span.Class] = list;
            }

            list.Add(span.DurationSec);
        }

        return BuildProfiles(durations, policy);
    }

    public static IReadOnlyDictionary<PauseClass, PauseCompressionProfile> BuildProfiles(Dictionary<PauseClass, List<double>> durations, PausePolicy policy)
    {
        if (durations is null) throw new ArgumentNullException(nameof(durations));
        if (policy is null) throw new ArgumentNullException(nameof(policy));

        var profiles = new Dictionary<PauseClass, PauseCompressionProfile>();

        foreach (var kvp in durations)
        {
            if (!TryGetBounds(kvp.Key, policy, out var bounds))
            {
                continue;
            }

            var threshold = ComputePreserveThreshold(kvp.Value, policy.PreserveTopQuantile);
            profiles[kvp.Key] = new PauseCompressionProfile(bounds, threshold);
        }

        return profiles;
    }

    public static bool ShouldPreserve(double duration, PauseClass @class, IReadOnlyDictionary<PauseClass, PauseCompressionProfile> profiles)
    {
        if (profiles is null) throw new ArgumentNullException(nameof(profiles));

        if (!profiles.TryGetValue(@class, out var profile))
        {
            return false;
        }

        if (!profile.PreserveThreshold.HasValue)
        {
            return false;
        }

        return duration >= profile.PreserveThreshold.Value;
    }

    public static double ComputeTargetDuration(
        double duration,
        PauseClass @class,
        PausePolicy policy,
        IReadOnlyDictionary<PauseClass, PauseCompressionProfile> profiles)
    {
        if (policy is null) throw new ArgumentNullException(nameof(policy));
        if (profiles is null) throw new ArgumentNullException(nameof(profiles));

        if (!profiles.TryGetValue(@class, out var profile))
        {
            return duration;
        }

        return ComputeTargetDuration(duration, profile.Bounds, policy);
    }

    private static bool TryGetBounds(PauseClass @class, PausePolicy policy, out PauseBounds bounds)
    {
        switch (@class)
        {
            case PauseClass.Comma:
                bounds = new PauseBounds(policy.Comma.Min, policy.Comma.Max);
                return true;
            case PauseClass.Sentence:
                bounds = new PauseBounds(policy.Sentence.Min, policy.Sentence.Max);
                return true;
            case PauseClass.Paragraph:
                bounds = new PauseBounds(policy.Paragraph.Min, policy.Paragraph.Max);
                return true;
            case PauseClass.ChapterHead:
                bounds = new PauseBounds(policy.HeadOfChapter, policy.HeadOfChapter);
                return true;
            case PauseClass.PostChapterRead:
                bounds = new PauseBounds(policy.PostChapterRead, policy.PostChapterRead);
                return true;
            case PauseClass.Tail:
                bounds = new PauseBounds(policy.Tail, policy.Tail);
                return true;
            default:
                bounds = default;
                return false;
        }
    }

    private static double? ComputePreserveThreshold(IReadOnlyList<double> durations, double preserveTopQuantile)
    {
        if (durations is null || durations.Count < 2)
        {
            return double.PositiveInfinity;
        }

        var ordered = durations
            .Where(value => value > 0d && double.IsFinite(value))
            .OrderBy(value => value)
            .ToList();

        if (ordered.Count < 2)
        {
            return double.PositiveInfinity;
        }

        double quantile = Math.Clamp(preserveTopQuantile, 0d, 1d);
        if (quantile <= 0d)
        {
            return double.PositiveInfinity;
        }

        if (quantile >= 1d)
        {
            return ordered[^1];
        }

        double position = quantile * (ordered.Count - 1);
        int lowerIndex = (int)Math.Floor(position);
        int upperIndex = (int)Math.Ceiling(position);

        if (lowerIndex == upperIndex)
        {
            return ordered[lowerIndex];
        }

        double lower = ordered[lowerIndex];
        double upper = ordered[upperIndex];
        double fraction = position - lowerIndex;
        return lower + (upper - lower) * fraction;
    }

    private static double ComputeTargetDuration(double duration, PauseBounds bounds, PausePolicy policy)
    {
        if (!double.IsFinite(duration))
        {
            return duration;
        }

        double knee = Math.Max(0d, policy.KneeWidth);
        double ratioInside = Math.Max(1d, policy.RatioInside);
        double ratioOutside = Math.Max(1d, policy.RatioOutside);

        double min = bounds.Min;
        double max = bounds.Max;

        if (min > max)
        {
            (min, max) = (max, min);
        }

        if (duration < min)
        {
            double inside = CompressToward(duration, min, ratioInside);
            double outside = CompressToward(duration, min, ratioOutside);
            if (knee <= 0d)
            {
                return Math.Max(outside, min - knee);
            }

            double distance = Math.Clamp((min - duration) / knee, 0d, 1d);
            double candidate = Lerp(inside, outside, distance);
            return Math.Max(candidate, min - knee);
        }

        if (duration > max)
        {
            double inside = CompressToward(duration, max, ratioInside);
            double outside = CompressToward(duration, max, ratioOutside);
            if (knee <= 0d)
            {
                return Math.Min(outside, max + knee);
            }

            double distance = Math.Clamp((duration - max) / knee, 0d, 1d);
            double candidate = Lerp(inside, outside, distance);
            return Math.Min(candidate, max + knee);
        }

        double center = (min + max) * 0.5d;
        double target = CompressToward(duration, center, ratioInside);
        if (target < min)
        {
            return min;
        }

        if (target > max)
        {
            return max;
        }

        return target;
    }

    private static double Lerp(double a, double b, double t) => a + (b - a) * Math.Clamp(t, 0d, 1d);

    private static double CompressToward(double value, double target, double ratio)
    {
        double safeRatio = Math.Max(1d, ratio);
        return target + (value - target) / safeRatio;
    }
}
