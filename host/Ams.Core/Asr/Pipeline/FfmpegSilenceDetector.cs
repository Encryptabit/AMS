using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core
{
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
            await using (var fs = File.OpenRead(audioPath))
            {
                var h = SHA256.Create();
                var hash = await h.ComputeHashAsync(fs, ct).ConfigureAwait(false);
                sha = Convert.ToHexString(hash);
            }

            // --- optional adaptive floor ---
            double dbFloor = p.DbFloor;
            var baseline = await EstimateNoiseFloorAsync(audioPath, runner, ct);
            if (baseline is double meanDb)
            {
                dbFloor = Math.Min(dbFloor, meanDb - 6.0); // 6 dB under baseline mean
            }

            // --- prefilter chain ---
            var af = BuildSilenceFilter(dbFloor, p.MinSilenceDur);
            var args = $"-hide_banner -nostats -i \"{audioPath}\" -af \"{af}\" -f null -";
            var result = await runner.RunAsync("ffmpeg", args, ct).ConfigureAwait(false);
            if (result.ExitCode != 0)
            {
                var msg = result.StdErr?.Trim().Length > 0 ? result.StdErr : result.StdOut;
                throw new InvalidOperationException($"ffmpeg silencedetect failed (code {result.ExitCode}). Ensure ffmpeg is installed and accessible. Output:\n{msg}");
            }

            var raw = ParseSilenceEvents(result.StdErr + "\n" + result.StdOut);
            var ffmpegVer = ParseVersion(result.StdErr + "\n" + result.StdOut) ?? "unknown";

            // --- post refine ---
            var refined = RefineSilenceWindows(raw);

            return new SilenceTimeline(sha, ffmpegVer, p, refined);
        }

        private static string BuildSilenceFilter(double dbFloor, double minDur)
        {
            return
                "pan=mono|c0=0.5*c0+0.5*c1," +   // safe for mono or stereo
                "highpass=f=60," +               // cut rumble
                "lowpass=f=9000," +              // suppress hiss
                $"silencedetect=noise={dbFloor}dB:d={minDur}";
        }

        private static async Task<double?> EstimateNoiseFloorAsync(string audioPath, IProcessRunner runner, CancellationToken ct)
        {
            var args = $"-hide_banner -nostats -i \"{audioPath}\" -af " +
                       "\"aformat=sample_fmts=flt:channel_layouts=mono,volumedetect\" -f null -";
            var res = await runner.RunAsync("ffmpeg", args, ct);
            var text = (res.StdErr ?? "") + "\n" + (res.StdOut ?? "");
            
            Console.WriteLine($"Estimated Noise Floor: {text}");

            var mv = Regex.Match(text, @"mean_volume:\s*(-?\d+(?:\.\d+)?)\s*dB");
            if (mv.Success && double.TryParse(mv.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var mean))
                return mean;

            var mx = Regex.Match(text, @"max_volume:\s*(-?\d+(?:\.\d+)?)\s*dB");
            if (mx.Success && double.TryParse(mx.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var max))
                return max;

            return null;
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

        private static List<SilenceEvent> RefineSilenceWindows(List<SilenceEvent> raw)
        {
            var merged = new List<SilenceEvent>();
            foreach (var e in raw.OrderBy(e => e.Start))
            {
                if (merged.Count == 0) { merged.Add(e); continue; }
                var last = merged[^1];
                if ((e.Start - last.End) * 1000.0 <= 80) // mergeWithinMs
                {
                    var start = last.Start;
                    var end = Math.Max(last.End, e.End);
                    merged[^1] = new SilenceEvent(start, end, end - start, (start + end) * 0.5);
                }
                else
                {
                    merged.Add(e);
                }
            }

            var refined = new List<SilenceEvent>();
            foreach (var e in merged)
            {
                double s = e.Start + 0.010; // guardL
                double t = e.End - 0.015;   // guardR
                if (t <= s) continue;
                if ((t - s) * 1000.0 < 120) continue; // minKeepMs

                refined.Add(new SilenceEvent(s, t, t - s, (s + t) * 0.5));
            }
            return refined;
        }
    }
}
