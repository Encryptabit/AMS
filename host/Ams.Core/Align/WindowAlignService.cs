using System;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ams.Core;
using System.Text;
using System.Text.Json;
using Ams.Core.Io;
using Ams.Core.Models;
using Ams.Core.Util;

namespace Ams.Core.Align;

public sealed class WindowAlignService
{
    private readonly HttpClient _httpClient;
    private readonly IProcessRunner _processRunner;

    public WindowAlignService(HttpClient httpClient, IProcessRunner processRunner)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    }

    public async Task<WindowAlignResult> AlignAsync(WindowAlignRequest request, CancellationToken ct = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        Directory.CreateDirectory(request.AudioDirectory);
        var normalizedInput = PathNormalizer.NormalizePath(request.InputAudioPath);

        var alignments = new List<WindowAlignment>(request.Windows.Windows.Count);

        foreach (var window in request.Windows.Windows)
        {
            var (startSec, endSec, okRange) = ComputeWindowTimeRange(request.TranscriptsMerged, window);
            var adjusted = AdjustBounds(startSec, endSec, okRange, request.Windows.Params, request.InputDurationSec);
            var audioPath = Path.Combine(request.AudioDirectory, $"{window.Id}.wav");
            var normalizedOutput = PathNormalizer.NormalizePath(audioPath);
            await ExtractSliceAsync(normalizedInput, normalizedOutput, adjusted.Start, adjusted.Duration, ct);

            var lines = BuildLinesFromAsrTokens(request.TranscriptsMerged, adjusted.Start, adjusted.End, 12, 400);
            if (lines.Count == 0)
            {
                lines = BuildLinesFromAsrTokens(request.TranscriptsMerged, adjusted.Start, adjusted.End, 8, 400);
            }

            var alignment = await RequestAlignmentAsync(window.Id, adjusted.Start, lines, request.Params, normalizedOutput, ct);
            alignments.Add(alignment);
        }

        var serviceTools = await GetToolVersionsAsync(request.Params.ServiceUrl, ct);
        return new WindowAlignResult(alignments, serviceTools);
    }

    private async Task<WindowAlignment> RequestAlignmentAsync(string windowId, double offsetSec, IReadOnlyList<string> lines, WindowAlignParams parameters, string audioPath, CancellationToken ct)
    {
        var digestInput = string.Join('\n', lines);
        var body = new
        {
            chunk_id = windowId,
            audio_path = PathNormalizer.NormalizePath(audioPath),
            lines,
            language = parameters.Language,
            timeout_sec = parameters.TimeoutSec
        };

        var payload = JsonSerializer.Serialize(body);
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync($"{parameters.ServiceUrl}/v1/align-chunk", content, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"window-align failed {response.StatusCode}: {err}");
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        var root = JsonSerializer.Deserialize<JsonElement>(json);

        var fragments = new List<WindowAlignFragment>();
        if (root.TryGetProperty("fragments", out var fragmentArray))
        {
            foreach (var fragment in fragmentArray.EnumerateArray())
            {
                var begin = Precision.RoundToMicroseconds(fragment.GetProperty("begin").GetDouble());
                var end = Precision.RoundToMicroseconds(fragment.GetProperty("end").GetDouble());
                fragments.Add(new WindowAlignFragment(begin, end));
            }
        }

        var toolVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("tool", out var toolInfo))
        {
            if (toolInfo.TryGetProperty("python", out var py))
            {
                var val = py.GetString();
                if (!string.IsNullOrWhiteSpace(val)) toolVersions["python"] = val;
            }
            if (toolInfo.TryGetProperty("aeneas", out var ae))
            {
                var val = ae.GetString();
                if (!string.IsNullOrWhiteSpace(val)) toolVersions["aeneas"] = val;
            }
        }

        return new WindowAlignment(
            windowId,
            Precision.RoundToMicroseconds(offsetSec),
            digestInput,
            fragments,
            toolVersions,
            DateTime.UtcNow
        );
    }

    private static (double Start, double End, double Duration) AdjustBounds(double start, double end, bool okRange, WindowsParams @params, double inputDuration)
    {
        var pre = @params.PrePadSec;
        var post = @params.PadSec;
        var duration = inputDuration > 0 ? inputDuration : end;

        var startSec = okRange ? Math.Max(0.0, start - pre) : Math.Max(0.0, (duration / 2.0) - WindowConstants.MaxSliceSec / 2.0);
        var endSec = okRange ? Math.Min(duration, end + post) : Math.Min(duration, startSec + WindowConstants.MaxSliceSec);

        var width = endSec - startSec;
        if (width < WindowConstants.MinSliceSec)
        {
            var center = (startSec + endSec) * 0.5;
            startSec = Math.Max(0.0, center - WindowConstants.MinSliceSec / 2.0);
            endSec = Math.Min(duration, center + WindowConstants.MinSliceSec / 2.0);
        }
        else if (width > WindowConstants.MaxSliceSec)
        {
            var center = (startSec + endSec) * 0.5;
            startSec = Math.Max(0.0, center - WindowConstants.MaxSliceSec / 2.0);
            endSec = Math.Min(duration, center + WindowConstants.MaxSliceSec / 2.0);
        }

        startSec = Precision.RoundToMicroseconds(startSec);
        endSec = Precision.RoundToMicroseconds(endSec);

        return (startSec, endSec, Precision.RoundToMicroseconds(Math.Max(0.05, endSec - startSec)));
    }

    private static (double start, double end, bool ok) ComputeWindowTimeRange(JsonElement merged, AnchorWindow window)
    {
        if (!merged.TryGetProperty("Words", out var words))
        {
            return (0.0, 0.0, false);
        }

        var hasBounds = window.AsrStart.HasValue && window.AsrEnd.HasValue && window.AsrStart.Value < window.AsrEnd.Value;
        var total = words.GetArrayLength();
        var aLo = hasBounds ? Math.Clamp(window.AsrStart!.Value, 0, Math.Max(0, total - 1)) : 0;
        var aHi = hasBounds ? Math.Clamp(window.AsrEnd!.Value - 1, 0, Math.Max(0, total - 1)) : Math.Max(0, total - 1);
        if (aLo > aHi)
        {
            (aLo, aHi) = (aHi, aLo);
        }

        double start = double.MaxValue;
        double end = 0.0;
        var index = 0;

        foreach (var word in words.EnumerateArray())
        {
            if (index >= aLo && index <= aHi)
            {
                var ws = word.GetProperty("Start").GetDouble();
                var we = word.GetProperty("End").GetDouble();
                if (ws < start) start = ws;
                if (we > end) end = we;
            }
            index++;
        }

        if (start == double.MaxValue)
        {
            foreach (var word in words.EnumerateArray())
            {
                start = word.GetProperty("Start").GetDouble();
                break;
            }
            foreach (var word in words.EnumerateArray().Reverse())
            {
                end = word.GetProperty("End").GetDouble();
                break;
            }
        }

        var ok = end > start;
        start = Precision.RoundToMicroseconds(start == double.MaxValue ? 0.0 : start);
        end = Precision.RoundToMicroseconds(Math.Max(end, start + 0.05));
        return (start, end, ok);
    }

    private static List<string> BuildLinesFromAsrTokens(JsonElement merged, double startSec, double endSec, int groupWords, int maxLines)
    {
        var lines = new List<string>();
        if (!merged.TryGetProperty("Words", out var words)) return lines;

        var bucket = new List<string>(groupWords);
        var total = words.GetArrayLength();
        for (var i = 0; i < total; i++)
        {
            var word = words[i];
            var ws = word.GetProperty("Start").GetDouble();
            var we = word.GetProperty("End").GetDouble();
            if (we < startSec || ws > endSec) continue;
            var text = word.GetProperty("Word").GetString();
            if (string.IsNullOrWhiteSpace(text)) continue;
            bucket.Add(text);
            if (bucket.Count >= groupWords)
            {
                lines.Add(string.Join(' ', bucket));
                bucket.Clear();
                if (lines.Count >= maxLines) break;
            }
        }

        if (bucket.Count > 0 && lines.Count < maxLines)
        {
            lines.Add(string.Join(' ', bucket));
        }

        return lines;
    }

    private async Task ExtractSliceAsync(string inputPath, string outputPath, double startSec, double durationSec, CancellationToken ct)
    {
        var ffmpeg = Environment.GetEnvironmentVariable("FFMPEG_EXE") ?? "ffmpeg";
        var args = $"-y -i \"{inputPath}\" -ss {Precision.ToInvariantMicroseconds(startSec)} -t {Precision.ToInvariantMicroseconds(durationSec)} -c copy \"{outputPath}\"";
        var result = await _processRunner.RunAsync(ffmpeg, args, ct);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"FFmpeg slice failed: {result.StdErr}");
        }
    }

    private async Task<Dictionary<string, string>> GetToolVersionsAsync(string serviceUrl, CancellationToken ct)
    {
        try
        {
            using var resp = await _httpClient.GetAsync($"{serviceUrl}/v1/health", ct);
            if (!resp.IsSuccessStatusCode) return new Dictionary<string, string> { ["python"] = "unknown", ["aeneas"] = "unknown" };
            var json = await resp.Content.ReadAsStringAsync(ct);
            var el = JsonSerializer.Deserialize<JsonElement>(json);
            var tool = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (el.TryGetProperty("python_version", out var py)) tool["python"] = py.GetString() ?? "unknown";
            if (el.TryGetProperty("aeneas_version", out var ae)) tool["aeneas"] = ae.GetString() ?? "unknown";
            return tool;
        }
        catch
        {
            return new Dictionary<string, string> { ["python"] = "unknown", ["aeneas"] = "unknown" };
        }
    }

    private static class WindowConstants
    {
        public const double MinSliceSec = 10.0;
        public const double MaxSliceSec = 90.0;
    }
}

public sealed record WindowAlignRequest(
    string InputAudioPath,
    double InputDurationSec,
    WindowsArtifact Windows,
    JsonElement TranscriptsMerged,
    WindowAlignParams Params,
    string AudioDirectory
);

public sealed record WindowAlignment(
    string WindowId,
    double OffsetSec,
    string DigestInput,
    IReadOnlyList<WindowAlignFragment> Fragments,
    IReadOnlyDictionary<string, string> ToolVersions,
    DateTime GeneratedAt
);

public sealed record WindowAlignResult(
    IReadOnlyList<WindowAlignment> Alignments,
    IReadOnlyDictionary<string, string> ServiceToolVersions
);
