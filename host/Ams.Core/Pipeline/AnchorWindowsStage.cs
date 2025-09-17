using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Ams.Align.Anchors;
using Ams.Core;
using Ams.Core.Align;
using Ams.Core.Align.Anchors;

namespace Ams.Core.Pipeline;

public class AnchorWindowsStage : StageRunner
{
    private readonly AnchorWindowParams _params;

    public AnchorWindowsStage(string workDir, AnchorWindowParams parameters) : base(workDir, "anchor-windows")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.json");
        if (!File.Exists(anchorsPath)) throw new InvalidOperationException("Missing anchors/anchors.json");
        var mergedPath = Path.Combine(WorkDir, "transcripts", "merged.json");
        if (!File.Exists(mergedPath)) throw new InvalidOperationException("Missing transcripts/merged.json");

        var anchorsJson = await File.ReadAllTextAsync(anchorsPath, ct);
        var mergedJson = await File.ReadAllTextAsync(mergedPath, ct);
        var anchors = JsonSerializer.Deserialize<AnchorsArtifact>(anchorsJson) ?? throw new InvalidOperationException("Invalid anchors.json");
        var merged = JsonSerializer.Deserialize<JsonElement>(mergedJson);

        // Reconstruct ASR response from merged Words and build normalized view for filtered->original mapping
        var tokens = new List<AsrToken>();
        if (merged.TryGetProperty("Words", out var wordsArr))
        {
            foreach (var w in wordsArr.EnumerateArray())
            {
                var txt = w.GetProperty("Word").GetString() ?? string.Empty;
                var start = w.GetProperty("Start").GetDouble();
                var end = w.GetProperty("End").GetDouble();
                tokens.Add(new AsrToken(start, Math.Max(0.0, end - start), txt));
            }
        }
        var asr = new AsrResponse("merged/derived", tokens.ToArray());
        var asrView = AnchorPreprocessor.BuildAsrView(asr);
        var wordTimings = ExtractWordTimings(merged);

        // Build half-open windows [BookStart, BookEnd) based on selected anchors
        var selected = anchors.Selected.OrderBy(a => a.Bp).ToList();
        int bookStart = 0, bookEnd = anchors.BookTokenCount; // filtered token count proxy
        var core = selected.Select(a => new Anchor(a.Bp, a.Ap)).ToList();
        var ranges = AnchorDiscovery.BuildWindows(core, bookStart, Math.Max(bookStart, bookEnd - 1), 0, Math.Max(0, asrView.Tokens.Count - 1));

        var rawWindows = new List<AnchorWindow>();
        for (int i = 0; i < ranges.Count; i++)
        {
            var (bLo, bHi, aLo, aHi) = ranges[i];
            int b0 = Math.Max(bookStart, bLo);
            int b1 = Math.Min(bookEnd, bHi);
            int? a0 = null, a1 = null;
            if (aLo < aHi && asrView.FilteredToOriginalToken.Count > 0)
            {
                int origStart = asrView.FilteredToOriginalToken[Math.Clamp(aLo, 0, asrView.FilteredToOriginalToken.Count - 1)];
                int origLast = asrView.FilteredToOriginalToken[Math.Clamp(aHi - 1, 0, asrView.FilteredToOriginalToken.Count - 1)];
                a0 = origStart;
                a1 = origLast + 1; // exclusive
            }
            rawWindows.Add(new AnchorWindow($"win_{i:D4}", b0, b1, a0, a1, i > 0 ? core[i - 1].Bp : null, i < core.Count ? core[i].Bp : null));
        }

        var mergedWindows = MergeWindows(rawWindows, wordTimings);

        // Coverage: fraction of [bookStart,bookEnd) covered by windows
        var covered = mergedWindows.Sum(w => Math.Max(0, w.BookEnd - w.BookStart));
        var coverage = Math.Min(1.0, (double)covered / Math.Max(1.0, (bookEnd - bookStart)));

        var artifact = new AnchorWindowsArtifact(
            Windows: mergedWindows,
            Params: _params,
            Coverage: coverage,
            LargestGapSec: 0.0,
            ToolVersions: new Dictionary<string, string>()
        );

        Directory.CreateDirectory(stageDir);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "anchor-windows.json"), JsonSerializer.Serialize(artifact, new JsonSerializerOptions { WriteIndented = true }), ct);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "params.snapshot.json"), SerializeParams(_params), ct);

        return new Dictionary<string, string>
        {
            ["anchor-windows"] = "anchor-windows.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));
        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.json");
        var hash = File.Exists(anchorsPath) ? ComputeHash(await File.ReadAllTextAsync(anchorsPath, ct)) : string.Empty;
        return new StageFingerprint(hash, paramsHash, new Dictionary<string, string>());
    }

    private static List<WordInfo> ExtractWordTimings(JsonElement merged)
    {
        var list = new List<WordInfo>();
        if (merged.TryGetProperty("Words", out var words))
        {
            foreach (var word in words.EnumerateArray())
            {
                var start = word.TryGetProperty("Start", out var s) ? s.GetDouble() : 0.0;
                var end = word.TryGetProperty("End", out var e) ? e.GetDouble() : start;
                list.Add(new WordInfo(Math.Max(0.0, start), Math.Max(start, end)));
            }
        }
        return list;
    }

    private List<AnchorWindow> MergeWindows(List<AnchorWindow> windows, IReadOnlyList<WordInfo> wordTimings)
    {
        if (windows.Count == 0)
        {
            return windows;
        }

        var metrics = windows.Select(w => ComputeMetrics(w, wordTimings)).ToList();
        var merged = new List<AnchorWindow>();
        var acc = new MergeAccumulator();

        void Flush()
        {
            if (acc.IsEmpty)
            {
                return;
            }

            merged.Add(acc.Build(merged.Count));
            acc = new MergeAccumulator();
        }

        for (int i = 0; i < metrics.Count; i++)
        {
            var metric = metrics[i];
            bool metricTooLarge = metric.Duration >= _params.MaxDurationSec || metric.WordCount >= _params.MaxWordCount;
            if (metricTooLarge)
            {
                Flush();
                acc.Add(metric);
                Flush();
                continue;
            }

            double projectedStart = acc.IsEmpty ? metric.Start : Math.Min(acc.Start, metric.Start);
            double projectedEnd = acc.IsEmpty ? metric.End : Math.Max(acc.End, metric.End);
            double projectedDuration = Math.Max(0.05, projectedEnd - projectedStart);
            int projectedWords = (acc.IsEmpty ? 0 : acc.WordCount) + Math.Max(metric.WordCount, 1);

            if (!acc.IsEmpty && (projectedDuration > _params.MaxDurationSec || projectedWords > _params.MaxWordCount))
            {
                Flush();
            }

            acc.Add(metric);

            double currentDuration = acc.Duration;
            int currentWords = acc.WordCount;
            bool meetsTarget = currentDuration >= _params.TargetDurationSec || currentWords >= _params.MinWordCount;

            bool isLast = i == metrics.Count - 1;
            bool nextWouldExceed = false;
            if (!isLast)
            {
                var next = metrics[i + 1];
                double nextStart = Math.Min(acc.Start, next.Start);
                double nextEnd = Math.Max(acc.End, next.End);
                double nextDuration = Math.Max(0.05, nextEnd - nextStart);
                int nextWords = currentWords + Math.Max(next.WordCount, 1);
                nextWouldExceed = nextDuration > _params.MaxDurationSec || nextWords > _params.MaxWordCount;
            }

            if (isLast || (meetsTarget && nextWouldExceed))
            {
                Flush();
            }
        }

        Flush();
        return merged;
    }

    private WindowMetrics ComputeMetrics(AnchorWindow window, IReadOnlyList<WordInfo> words)
    {
        if (words.Count == 0)
        {
            return new WindowMetrics(window, 0.0, 0.05, 0.05, 0, window.AsrStart, window.AsrEnd);
        }

        int spanStart = window.AsrStart.HasValue ? Math.Clamp(window.AsrStart.Value, 0, words.Count - 1) : 0;
        int spanEnd = window.AsrEnd.HasValue ? Math.Clamp(window.AsrEnd.Value, spanStart + 1, words.Count) : words.Count;
        if (spanEnd <= spanStart)
        {
            spanEnd = Math.Min(words.Count, spanStart + 1);
        }

        double start = double.PositiveInfinity;
        double end = 0.0;
        for (int i = spanStart; i < spanEnd && i < words.Count; i++)
        {
            var w = words[i];
            if (w.Start < start) start = w.Start;
            if (w.End > end) end = w.End;
        }

        if (double.IsPositiveInfinity(start))
        {
            start = 0.0;
        }
        if (end <= start)
        {
            end = start + 0.05;
        }

        var duration = Math.Max(0.05, end - start);
        var wordCount = Math.Max(0, spanEnd - spanStart);
        return new WindowMetrics(window, start, end, duration, wordCount, window.AsrStart, window.AsrEnd);
    }

    private readonly record struct WordInfo(double Start, double End);

    private sealed class MergeAccumulator
    {
        private readonly List<WindowMetrics> _items = new();

        public double Start { get; private set; } = double.PositiveInfinity;
        public double End { get; private set; }
        public int WordCount { get; private set; }
        public int? AsrStart { get; private set; }
        public int? AsrEnd { get; private set; }

        public bool IsEmpty => _items.Count == 0;
        public double Duration => IsEmpty ? 0.0 : Math.Max(0.05, End - Start);

        public void Add(WindowMetrics metrics)
        {
            _items.Add(metrics);
            if (_items.Count == 1)
            {
                Start = metrics.Start;
                End = metrics.End;
            }
            else
            {
                Start = Math.Min(Start, metrics.Start);
                End = Math.Max(End, metrics.End);
            }

            WordCount += Math.Max(metrics.WordCount, 1);
            if (metrics.AsrStart.HasValue)
            {
                AsrStart = AsrStart.HasValue ? Math.Min(AsrStart.Value, metrics.AsrStart.Value) : metrics.AsrStart;
            }
            if (metrics.AsrEnd.HasValue)
            {
                AsrEnd = AsrEnd.HasValue ? Math.Max(AsrEnd.Value, metrics.AsrEnd.Value) : metrics.AsrEnd;
            }
        }

        public AnchorWindow Build(int index)
        {
            if (_items.Count == 0)
            {
                throw new InvalidOperationException("Cannot build a window from an empty accumulator.");
            }

            var first = _items[0].Window;
            var last = _items[^1].Window;
            return new AnchorWindow(
                $"win_{index:D4}",
                first.BookStart,
                last.BookEnd,
                AsrStart,
                AsrEnd,
                first.PrevAnchorBp,
                last.NextAnchorBp
            );
        }
    }

    private readonly record struct WindowMetrics(
        AnchorWindow Window,
        double Start,
        double End,
        double Duration,
        int WordCount,
        int? AsrStart,
        int? AsrEnd
    );
}
