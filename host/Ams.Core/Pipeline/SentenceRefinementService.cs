using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Ams.Core.Alignment.Tx;
using Ams.Core.Artifacts;
using SentenceTiming = Ams.Core.Artifacts.SentenceTiming;

namespace Ams.Core.Pipeline;

public sealed record SentenceRefined(int SentenceId, SentenceTiming Timing, int StartWordIdx, int EndWordIdx);

public sealed record SilenceInfo(double Start, double End, double Duration, double Confidence);

public sealed class SentenceRefinementService
{
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(10);

    public async Task<IReadOnlyList<SentenceRefined>> RefineAsync(
        string audioPath,
        TranscriptIndex tx,
        AsrResponse asr,
        string language = "eng",
        bool useSilence = true,
        double silenceThresholdDb = -30.0,
        double silenceMinDurationSec = 0.1)
    {
        if (!File.Exists(audioPath)) throw new FileNotFoundException(audioPath);

        // 1) Build per-sentence script text (from ASR tokens per ScriptRange)
        var lines = new List<string>(tx.Sentences.Count);
        var indexMap = new List<(int sentenceId, int startIdx, int endIdx)>(tx.Sentences.Count);
        foreach (var s in tx.Sentences)
        {
            if (s.ScriptRange is null || !s.ScriptRange.Start.HasValue || !s.ScriptRange.End.HasValue)
            {
                lines.Add(string.Empty);
                indexMap.Add((s.Id, -1, -1));
                continue;
            }

            int si = Math.Clamp(s.ScriptRange.Start!.Value, 0, asr.Tokens.Length - 1);
            int ei = Math.Clamp(s.ScriptRange.End!.Value, 0, asr.Tokens.Length - 1);
            var text = string.Join(' ', asr.Tokens.Skip(si).Take(ei - si + 1).Select(t => t.Word));
            lines.Add(text);
            indexMap.Add((s.Id, si, ei));
        }

        // 2) Call Aeneas with one line per sentence to get begin/end per sentence
        var fragments = await RunAeneasAsync(audioPath, lines, language);

        // 3) Detect silences with FFmpeg once (optional)
        var silences = useSilence ? await DetectSilencesAsync(audioPath, silenceThresholdDb, silenceMinDurationSec) : Array.Empty<SilenceInfo>();

        // 4) Compose refined sentences: start from Aeneas begin; end from nearest silence_end after fragment end and before next begin
        var results = new List<SentenceRefined>(tx.Sentences.Count);
        for (int i = 0; i < tx.Sentences.Count; i++)
        {
            var (sentenceId, si, ei) = indexMap[i];
            bool hasFragment = si >= 0 && ei >= 0 && i < fragments.Count;

            if (!hasFragment)
            {
                var emptyTiming = new SentenceTiming(0d, 0d, fragmentBacked: false);
                results.Add(new SentenceRefined(sentenceId, emptyTiming, Math.Max(0, si), Math.Max(0, ei)));
                continue;
            }

            var frag = fragments[i];
            double start = Math.Max(0, frag.begin);
            double nextBegin = i + 1 < fragments.Count ? fragments[i + 1].begin : double.MaxValue;

            double end = frag.end;
            if (useSilence && silences.Length > 0)
            {
                foreach (var s in silences)
                {
                    if (s.End >= frag.end - 1e-6 && s.End <= nextBegin)
                    {
                        end = s.End;
                        break;
                    }
                }
            }

            end = Math.Max(end, start + 0.05);
            end = Math.Min(end, nextBegin);

            var timing = new SentenceTiming(start, end, fragmentBacked: true);
            results.Add(new SentenceRefined(sentenceId, timing, si, ei));
        }

        // 5) Ensure monotonic non-overlap
        for (int i = 1; i < results.Count; i++)
        {
            var prev = results[i - 1];
            var curr = results[i];
            if (curr.Timing.StartSec < prev.Timing.EndSec)
            {
                var adjusted = prev.Timing.WithEnd(curr.Timing.StartSec);
                results[i - 1] = prev with { Timing = adjusted };
            }
        }

        return results;
    }

    private async Task<List<(double begin, double end)>> RunAeneasAsync(string audioPath, List<string> lines, string language)
    {
        var temp = Path.Combine(Path.GetTempPath(), $"aeneas_sent_{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);
        try
        {
            var txt = Path.Combine(temp, "sentences.txt");
            await File.WriteAllLinesAsync(txt, lines, new UTF8Encoding(false));
            var outJson = Path.Combine(temp, "alignment.json");

            var pythonExe = Environment.GetEnvironmentVariable("AENEAS_PYTHON");
            if (string.IsNullOrWhiteSpace(pythonExe)) pythonExe = "python";

            var psi = new ProcessStartInfo
            {
                FileName = pythonExe!,
                Arguments = $"-m aeneas.tools.execute_task \"{audioPath}\" \"{txt}\" \"task_language={language}|is_text_type=plain|os_task_file_format=json\" \"{outJson}\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start aeneas");
            using var cts = new CancellationTokenSource(_timeout);
            await proc.WaitForExitAsync(cts.Token);
            var stderr = await proc.StandardError.ReadToEndAsync();
            if (proc.ExitCode != 0) throw new InvalidOperationException($"aeneas failed: {stderr}");
            var json = await File.ReadAllTextAsync(outJson);
            using var doc = JsonDocument.Parse(json);
            var arr = doc.RootElement.GetProperty("fragments").EnumerateArray();
            var frags = new List<(double, double)>();
            foreach (var f in arr)
            {
                double b = ParseDouble(f.GetProperty("begin"));
                double e = ParseDouble(f.GetProperty("end"));
                frags.Add((b, e));
            }
            return frags;
        }
        finally
        {
            try { Directory.Delete(temp, true); } catch { }
        }

        static double ParseDouble(JsonElement el) => el.ValueKind == JsonValueKind.String ? double.Parse(el.GetString()!) : el.GetDouble();
    }

    private async Task<SilenceInfo[]> DetectSilencesAsync(string audioPath, double thresholdDb, double minDurationSec)
    {
        var ffmpegExe = Environment.GetEnvironmentVariable("FFMPEG_EXE");
        if (string.IsNullOrWhiteSpace(ffmpegExe)) ffmpegExe = "ffmpeg";

        var psi = new ProcessStartInfo
        {
            FileName = ffmpegExe!,
            Arguments = $"-i \"{audioPath}\" -af silencedetect=noise={thresholdDb}dB:duration={minDurationSec} -f null -",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var p = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start ffmpeg");
        using var cts = new CancellationTokenSource(_timeout);
        await p.WaitForExitAsync(cts.Token);
        var stderr = await p.StandardError.ReadToEndAsync();
        return ParseSilenceOutput(stderr);
    }

    private SilenceInfo[] ParseSilenceOutput(string ffmpegOutput)
    {
        var silences = new List<SilenceInfo>();
        double? cur = null;
        foreach (var line in ffmpegOutput.Split('\n'))
        {
            if (line.Contains("silence_start:"))
            {
                var m = System.Text.RegularExpressions.Regex.Match(line, @"silence_start:\s*([\d.]+)");
                if (m.Success) cur = double.Parse(m.Groups[1].Value);
            }
            else if (line.Contains("silence_end:") && cur.HasValue)
            {
                var m = System.Text.RegularExpressions.Regex.Match(line, @"silence_end:\s*([\d.]+)");
                if (m.Success)
                {
                    var end = double.Parse(m.Groups[1].Value);
                    silences.Add(new SilenceInfo(cur.Value, end, end - cur.Value, 1.0));
                    cur = null;
                }
            }
        }
        return silences.ToArray();
    }
}

