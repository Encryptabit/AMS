using System;
using System.Collections.Generic;
using System.Linq;

namespace Ams.Core.Prosody;

public sealed record PauseClassSummary(
    int Count,
    double TotalDuration,
    double Minimum,
    double Maximum,
    double Mean,
    double Median)
{
    public static readonly PauseClassSummary Empty = new(0, 0.0, 0.0, 0.0, 0.0, 0.0);

    public static PauseClassSummary FromDurations(IEnumerable<double> durations)
    {
        if (durations is null) throw new ArgumentNullException(nameof(durations));

        var list = durations.Where(d => d >= 0.0 && double.IsFinite(d)).ToList();
        if (list.Count == 0)
        {
            return Empty;
        }

        list.Sort();
        double total = list.Sum();
        double min = list[0];
        double max = list[^1];
        double mean = total / list.Count;
        double median = list.Count % 2 == 1
            ? list[list.Count / 2]
            : (list[list.Count / 2 - 1] + list[list.Count / 2]) * 0.5;

        return new PauseClassSummary(list.Count, total, min, max, mean, median);
    }
}

public sealed record PauseAnalysisReport(
    IReadOnlyList<PauseSpan> Spans,
    IReadOnlyDictionary<PauseClass, PauseClassSummary> Classes)
{
    public static readonly PauseAnalysisReport Empty = new(Array.Empty<PauseSpan>(),
        new Dictionary<PauseClass, PauseClassSummary>());
}
