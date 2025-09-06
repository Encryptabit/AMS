using System.Net.Http;
using System.Text;
using System.Text.Json;
using Ams.Core.Io;
using Ams.Align.Anchors;

namespace Ams.Core.Pipeline;

public class WindowAlignStage : StageRunner
{
    private readonly HttpClient _http;
    private readonly WindowAlignParams _params;
    private readonly IProcessRunner _processRunner = new DefaultProcessRunner();
    public WindowAlignStage(string workDir, HttpClient httpClient, WindowAlignParams parameters) : base(workDir, "window-align")
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        var windowsPath = Path.Combine(WorkDir, "windows", "windows.json");
        if (!File.Exists(windowsPath)) throw new InvalidOperationException("Missing windows/windows.json");
        var chunkIndexPath = Path.Combine(WorkDir, "chunks", "index.json");
        if (!File.Exists(chunkIndexPath)) throw new InvalidOperationException("Missing chunks/index.json");
        var transcriptMerged = Path.Combine(WorkDir, "transcripts", "merged.json");
        if (!File.Exists(transcriptMerged)) throw new InvalidOperationException("Missing transcripts/merged.json");

        var windows = JsonSerializer.Deserialize<WindowsArtifact>(await File.ReadAllTextAsync(windowsPath, ct))!;
        var chunkIndex = JsonSerializer.Deserialize<ChunkIndex>(await File.ReadAllTextAsync(chunkIndexPath, ct))!;
        var merged = JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(transcriptMerged, ct));

        var audioPaths = chunkIndex.Chunks.OrderBy(c => c.Span.Start)
            .Select(c => PathNormalizer.NormalizePath(Path.Combine(WorkDir, "chunks", "wav", c.Filename)))
            .ToList();
        if (audioPaths.Count == 0) throw new InvalidOperationException("No chunk WAVs to align.");

        var winDir = Path.Combine(stageDir, "windows");
        Directory.CreateDirectory(winDir);
        var map = new Dictionary<string, string>();

        foreach (var win in windows.Windows)
        {
            // Compute time slice from ASR tokens range
            var (sliceStart, sliceEnd) = ComputeWindowTimeRange(merged, win, out bool okRange);
            double inputDuration = manifest.Input.DurationSec;
            double pre = windows.Params.PrePadSec;
            double post = windows.Params.PadSec;
            // initial proposal from token span
            double startSec = Math.Max(0.0, sliceStart - pre);
            double endSec = Math.Min(inputDuration > 0 ? inputDuration : sliceEnd + post, sliceEnd + post);
            // clamp to sane min/max window sizes
            const double MinSliceSec = 10.0;
            const double MaxSliceSec = 90.0; // align within ~chunk length
            if (!okRange)
            {
                // if no token bounds, derive a reasonable window near the middle
                double center = inputDuration > 0 ? inputDuration / 2.0 : 30.0;
                startSec = Math.Max(0.0, center - MaxSliceSec / 2.0);
                endSec = Math.Min(inputDuration > 0 ? inputDuration : center + MaxSliceSec / 2.0, (inputDuration > 0 ? inputDuration : center + MaxSliceSec / 2.0));
            }
            else
            {
                double width = endSec - startSec;
                if (width < MinSliceSec)
                {
                    double center = (startSec + endSec) * 0.5;
                    startSec = Math.Max(0.0, center - MinSliceSec / 2.0);
                    endSec = Math.Min(inputDuration > 0 ? inputDuration : center + MinSliceSec / 2.0, (inputDuration > 0 ? inputDuration : center + MinSliceSec / 2.0));
                }
                else if (width > MaxSliceSec)
                {
                    double center = (startSec + endSec) * 0.5;
                    startSec = Math.Max(0.0, center - MaxSliceSec / 2.0);
                    endSec = Math.Min(inputDuration > 0 ? inputDuration : center + MaxSliceSec / 2.0, (inputDuration > 0 ? inputDuration : center + MaxSliceSec / 2.0));
                }
            }

            var sliceDir = Path.Combine(winDir, "audio");
            Directory.CreateDirectory(sliceDir);
            var audioPath = Path.Combine(sliceDir, $"{win.Id}.wav");
            await ExtractSliceAsync(PathNormalizer.NormalizePath(manifest.Input.Path), PathNormalizer.NormalizePath(audioPath), startSec, Math.Max(0.05, endSec - startSec), ct);

            // Build window text lines from ASR tokens within [startSec, endSec]
            var lines = BuildLinesFromAsrTokens(merged, startSec, endSec, groupWords: 12, maxLines: 400);
            if (lines.Count == 0)
            {
                // Fallback: keep within the same time slice but lower grouping to coax more lines
                lines = BuildLinesFromAsrTokens(merged, startSec, endSec, groupWords: 8, maxLines: 400);
            }
            var text = string.Join("\n", lines);
            var digest = ComputeHash(text)[..16];

            var body = new
            {
                chunk_id = win.Id,
                audio_path = PathNormalizer.NormalizePath(audioPath),
                lines = lines,
                language = _params.Language,
                timeout_sec = _params.TimeoutSec
            };
            var req = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var resp = await _http.PostAsync($"{_params.ServiceUrl}/v1/align-chunk", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"window-align failed {resp.StatusCode}: {err}");
            }
            var json = await resp.Content.ReadAsStringAsync(ct);
            var root = JsonSerializer.Deserialize<JsonElement>(json);
            var frags = new List<WindowAlignFragment>();
            if (root.TryGetProperty("fragments", out var arr))
            {
                foreach (var f in arr.EnumerateArray())
                    frags.Add(new WindowAlignFragment(f.GetProperty("begin").GetDouble(), f.GetProperty("end").GetDouble()));
            }
            var tool = new Dictionary<string, string>();
            if (root.TryGetProperty("tool", out var t))
            {
                if (t.TryGetProperty("python", out var py)) tool["python"] = py.GetString() ?? "unknown";
                if (t.TryGetProperty("aeneas", out var ae)) tool["aeneas"] = ae.GetString() ?? "unknown";
            }
            var entry = new WindowAlignEntry(win.Id, startSec, digest, frags, tool, DateTime.UtcNow);
            var outPath = Path.Combine(winDir, $"{win.Id}.aeneas.json");
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true }), ct);
            map[win.Id] = $"{win.Id}.aeneas.json";
        }

        var idx = new WindowAlignIndex(windows.Windows.Select(w => w.Id).ToList(), map, _params, new Dictionary<string, string>());
        await File.WriteAllTextAsync(Path.Combine(stageDir, "index.json"), JsonSerializer.Serialize(idx, new JsonSerializerOptions { WriteIndented = true }), ct);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "params.snapshot.json"), SerializeParams(_params), ct);

        return new Dictionary<string, string> {
            ["index"] = "index.json",
            ["windows_dir"] = "windows",
            ["params"] = "params.snapshot.json"
        };
    }

    private static List<string> BuildLinesFromAsrTokens(JsonElement merged, double startSec, double endSec, int groupWords, int maxLines)
    {
        var lines = new List<string>();
        if (!merged.TryGetProperty("Words", out var wordsArr)) return lines;
        var bucket = new List<string>(groupWords);
        int total = wordsArr.GetArrayLength();
        for (int i = 0; i < total; i++)
        {
            var w = wordsArr[i];
            double ws = w.GetProperty("Start").GetDouble();
            double we = w.GetProperty("End").GetDouble();
            if (we < startSec || ws > endSec) continue;
            var txt = w.GetProperty("Word").GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(txt)) continue;
            bucket.Add(txt);
            if (bucket.Count >= groupWords)
            {
                lines.Add(string.Join(' ', bucket));
                bucket.Clear();
                if (lines.Count >= maxLines) break;
            }
        }
        if (bucket.Count > 0 && lines.Count < maxLines)
            lines.Add(string.Join(' ', bucket));
        return lines;
    }

    private static (double start, double end) ComputeWindowTimeRange(JsonElement merged, AnchorWindow win, out bool ok)
    {
        ok = false;
        if (!merged.TryGetProperty("Words", out var wordsArr)) return (0.0, 0.0);
        var hasBounds = win.AsrStart.HasValue && win.AsrEnd.HasValue && win.AsrStart!.Value < win.AsrEnd!.Value;
        int total = wordsArr.GetArrayLength();
        int aLo = hasBounds ? Math.Clamp(win.AsrStart!.Value, 0, Math.Max(0, total - 1)) : 0;
        int aHi = hasBounds ? Math.Clamp(win.AsrEnd!.Value - 1, 0, Math.Max(0, total - 1)) : Math.Max(0, total - 1);
        if (aLo > aHi) (aLo, aHi) = (aHi, aLo);

        double start = double.MaxValue, end = 0.0;
        int i = 0;
        foreach (var w in wordsArr.EnumerateArray())
        {
            if (i >= aLo && i <= aHi)
            {
                var ws = w.GetProperty("Start").GetDouble();
                var we = w.GetProperty("End").GetDouble();
                if (ws < start) start = ws;
                if (we > end) end = we;
            }
            i++;
        }
        if (start == double.MaxValue)
        {
            foreach (var w in wordsArr.EnumerateArray()) { start = w.GetProperty("Start").GetDouble(); break; }
            foreach (var w in wordsArr.EnumerateArray().Reverse()) { end = w.GetProperty("End").GetDouble(); break; }
        }
        ok = end > start;
        return (start, Math.Max(end, start + 0.05));
    }

    private async Task ExtractSliceAsync(string inputPath, string outputPath, double startSec, double durSec, CancellationToken ct)
    {
        var ffmpeg = Environment.GetEnvironmentVariable("FFMPEG_EXE") ?? "ffmpeg";
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var args = $"-y -i \"{inputPath}\" -ss {startSec.ToString("F6", ci)} -t {durSec.ToString("F6", ci)} -c copy \"{outputPath}\"";
        var res = await _processRunner.RunAsync(ffmpeg, args, ct);
        if (res.ExitCode != 0)
            throw new InvalidOperationException($"FFmpeg slice failed: {res.StdErr}");
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));
        string w = string.Empty, c = string.Empty, t = string.Empty;
        var wPath = Path.Combine(WorkDir, "windows", "windows.json");
        if (File.Exists(wPath)) w = ComputeHash(await File.ReadAllTextAsync(wPath, ct));
        var cPath = Path.Combine(WorkDir, "chunks", "index.json");
        if (File.Exists(cPath)) c = ComputeHash(await File.ReadAllTextAsync(cPath, ct));
        var tPath = Path.Combine(WorkDir, "transcripts", "merged.json");
        if (File.Exists(tPath)) t = ComputeHash(await File.ReadAllTextAsync(tPath, ct));
        var inputHash = ComputeHash(w + "\n" + c + "\n" + t);
        var toolVersions = await GetToolVersionsAsync(ct);
        return new StageFingerprint(inputHash, paramsHash, toolVersions);
    }

    private async Task<Dictionary<string, string>> GetToolVersionsAsync(CancellationToken ct)
    {
        try
        {
            using var resp = await _http.GetAsync($"{_params.ServiceUrl}/v1/health", ct);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                var el = JsonSerializer.Deserialize<JsonElement>(json);
                var tool = new Dictionary<string, string>();
                if (el.TryGetProperty("python_version", out var py)) tool["python"] = py.GetString() ?? "unknown";
                if (el.TryGetProperty("aeneas_version", out var ae)) tool["aeneas"] = ae.GetString() ?? "unknown";
                return tool;
            }
        }
        catch { /* ignore */ }
        return new Dictionary<string, string> { ["python"] = "unknown", ["aeneas"] = "unknown" };
    }
}
