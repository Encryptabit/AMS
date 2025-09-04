using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core;

public interface ISilenceDetector
{
    Task<SilenceTimeline> DetectAsync(string audioPath, SilenceParams p, IProcessRunner runner, CancellationToken ct = default);
}

public sealed class FfmpegSilenceDetector : ISilenceDetector
{
    private static readonly Regex StartRx = new(@"silence_start:\s*([0-9]+\.?[0-9]*)", RegexOptions.Compiled);
    private static readonly Regex EndRx = new(@"silence_end:\s*([0-9]+\.?[0-9]*)\s*\|\s*silence_duration:\s*([0-9]+\.?[0-9]*)", RegexOptions.Compiled);
    private static readonly Regex VersionRx = new(@"ffmpeg version\s+([^\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async Task<SilenceTimeline> DetectAsync(string audioPath, SilenceParams p, IProcessRunner runner, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(audioPath)) throw new ArgumentNullException(nameof(audioPath));
        if (runner is null) throw new ArgumentNullException(nameof(runner));

        // Compute hash for determinism
        string sha;
        await using (var fs = System.IO.File.OpenRead(audioPath))
        {
            var h = SHA256.Create();
            var hash = await h.ComputeHashAsync(fs, ct).ConfigureAwait(false);
            sha = Convert.ToHexString(hash);
        }

        var args = $"-hide_banner -nostats -i \"{audioPath}\" -af silencedetect=noise={p.DbFloor}dB:d={p.MinSilenceDur} -f null -";
        var result = await runner.RunAsync("ffmpeg", args, ct).ConfigureAwait(false);
        if (result.ExitCode != 0)
        {
            var msg = result.StdErr?.Trim().Length > 0 ? result.StdErr : result.StdOut;
            throw new InvalidOperationException($"ffmpeg silencedetect failed (code {result.ExitCode}). Ensure ffmpeg is installed and accessible. Output:\n{msg}");
        }

        var events = ParseSilenceEvents(result.StdErr + "\n" + result.StdOut);
        var ffmpegVer = ParseVersion(result.StdErr + "\n" + result.StdOut) ?? "unknown";
        return new SilenceTimeline(sha, ffmpegVer, p, events);
    }

    public static List<SilenceEvent> ParseSilenceEvents(string text)
    {
        var evts = new List<SilenceEvent>();
        double? currentStart = null;
        var lines = text.Split('\n');
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            var mS = StartRx.Match(line);
            if (mS.Success)
            {
                currentStart = double.Parse(mS.Groups[1].Value, CultureInfo.InvariantCulture);
                continue;
            }
            var mE = EndRx.Match(line);
            if (mE.Success && currentStart.HasValue)
            {
                var end = double.Parse(mE.Groups[1].Value, CultureInfo.InvariantCulture);
                var dur = double.Parse(mE.Groups[2].Value, CultureInfo.InvariantCulture);
                var start = currentStart.Value;
                evts.Add(new SilenceEvent(start, end, dur, (start + end) * 0.5));
                currentStart = null;
            }
        }
        return evts;
    }

    public static string? ParseVersion(string text)
    {
        var m = VersionRx.Match(text);
        return m.Success ? m.Groups[1].Value : null;
    }
}
