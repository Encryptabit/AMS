using System;
using Ams.Core;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Io;

namespace Ams.Core.Asr.Pipeline;

public sealed record VolumeAnalysisParams(
    double DbFloor,
    double SpeechFloorDb,
    double MinProbeSec,
    double ProbeWindowSec,
    double HfBandLowHz,
    double HfBandHighHz,
    double HfMarginDb,
    double WeakMarginDb,
    double NudgeStepSec,
    int MaxLeftNudges,
    int MaxRightNudges,
    double GuardLeftSec,
    double GuardRightSec
);

public sealed record VolumeProbe(
    double ProbeStart,
    double ProbeDuration,
    double? FullbandDb,
    double? HighbandDb
);

public sealed record VolumeAnalysisResult(
    VolumeProbe Left,
    VolumeProbe Right,
    double SuggestedStart,
    double SuggestedEnd,
    bool LeftEdgeHot,
    bool RightEdgeHot,
    int LeftNudges,
    int RightNudges
)
{
    public double SuggestedDuration => Math.Max(0.0, SuggestedEnd - SuggestedStart);
}

public sealed class AudioAnalysisService
{
    private readonly IProcessRunner _processRunner;
    private readonly int _targetRate;
    private static readonly Regex MeanVolume = new(@"mean_volume:\s*(-?\d+(?:\.\d+)?)\s*dB", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex MaxVolume = new(@"max_volume:\s*(-?\d+(?:\.\d+)?)\s*dB", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex AstatsOverall = new(@"Overall\.RMS_level\s*:\s*(-?\d+(?:\.\d+)?)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex AstatsFrame = new(@"\bRMS_level\s*:\s*(-?\d+(?:\.\d+)?)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public AudioAnalysisService(IProcessRunner processRunner, int targetRate = 44100)
    {
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _targetRate = targetRate;
    }

    public Task<VolumeAnalysisResult> GetVolumeAnalysis(
        string audioPath,
        double start,
        double duration,
        VolumeAnalysisParams parameters) =>
        GetVolumeAnalysis(audioPath, start, duration, parameters, CancellationToken.None);

    public async Task<VolumeAnalysisResult> GetVolumeAnalysis(
        string audioPath,
        double start,
        double duration,
        VolumeAnalysisParams parameters,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(audioPath))
            throw new ArgumentException("Audio path is required", nameof(audioPath));
        if (!File.Exists(audioPath))
            throw new FileNotFoundException("Audio file not found", audioPath);
        if (duration <= 0)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive");

        ct.ThrowIfCancellationRequested();

        double totalDuration = await GetAudioDurationAsync(audioPath, ct).ConfigureAwait(false);
        double windowStart = Math.Max(0.0, start);
        double windowEnd = Math.Min(totalDuration, windowStart + duration);

        var (nudgedStart, nudgedEnd, leftHot, rightHot, leftNudges, rightNudges) =
            await NudgeBoundariesAsync(audioPath, windowStart, windowEnd, totalDuration, parameters, ct)
                .ConfigureAwait(false);

        var leftProbe = await CaptureProbeAsync(audioPath, nudgedStart, true, parameters, totalDuration, ct)
            .ConfigureAwait(false);
        var rightProbe = await CaptureProbeAsync(audioPath, nudgedEnd, false, parameters, totalDuration, ct)
            .ConfigureAwait(false);

        return new VolumeAnalysisResult(
            leftProbe,
            rightProbe,
            nudgedStart,
            nudgedEnd,
            leftHot,
            rightHot,
            leftNudges,
            rightNudges);
    }

    private async Task<(double Start, double End, bool LeftHot, bool RightHot, int LeftNudges, int RightNudges)> NudgeBoundariesAsync(
        string audioPath,
        double start,
        double end,
        double totalDuration,
        VolumeAnalysisParams parameters,
        CancellationToken ct)
    {
        double nudgedStart = Math.Max(0.0, start);
        double nudgedEnd = Math.Min(totalDuration, Math.Max(nudgedStart, end));

        bool leftHot = false;
        bool rightHot = false;
        int leftNudges = 0;
        int rightNudges = 0;

        double leftWindow = Math.Max(parameters.MinProbeSec, Math.Max(parameters.GuardLeftSec, parameters.ProbeWindowSec));
        double rightWindow = Math.Max(parameters.MinProbeSec, Math.Max(parameters.GuardRightSec, parameters.ProbeWindowSec));

        while (leftNudges < parameters.MaxLeftNudges && (nudgedStart + parameters.NudgeStepSec) < nudgedEnd)
        {
            double probeStart = Math.Max(0.0, nudgedStart - leftWindow);
            double probeDuration = Math.Min(leftWindow, nudgedEnd - probeStart);
            if (probeDuration <= 0.0)
                break;

            var band = await GetBandRmsAsync(audioPath, probeStart, probeDuration, parameters.HfBandLowHz,
                parameters.HfBandHighHz, ct).ConfigureAwait(false);
            var full = await GetFullbandRmsAsync(audioPath, probeStart, probeDuration, ct).ConfigureAwait(false);

            if (!IsHot(band, full, parameters, out var hot))
            {
                leftHot = hot;
                break;
            }

            leftHot = true;
            nudgedStart = Math.Min(nudgedEnd - 0.002, nudgedStart + parameters.NudgeStepSec);
            leftNudges++;
        }

        // Final check for left edge after nudging
        {
            double probeStart = Math.Max(0.0, nudgedStart - leftWindow);
            double probeDuration = Math.Min(leftWindow, nudgedEnd - probeStart);
            var band = await GetBandRmsAsync(audioPath, probeStart, probeDuration, parameters.HfBandLowHz,
                parameters.HfBandHighHz, ct).ConfigureAwait(false);
            var full = await GetFullbandRmsAsync(audioPath, probeStart, probeDuration, ct).ConfigureAwait(false);
            IsHot(band, full, parameters, out leftHot);
        }

        while (rightNudges < parameters.MaxRightNudges && nudgedEnd < totalDuration)
        {
            double probeStart = Math.Max(0.0, nudgedEnd);
            double available = Math.Max(0.0, totalDuration - probeStart);
            double probeDuration = Math.Min(rightWindow, Math.Max(parameters.MinProbeSec, available));
            if (probeDuration <= 0.0)
                break;

            var band = await GetBandRmsAsync(audioPath, probeStart, probeDuration, parameters.HfBandLowHz,
                parameters.HfBandHighHz, ct).ConfigureAwait(false);
            var full = await GetFullbandRmsAsync(audioPath, probeStart, probeDuration, ct).ConfigureAwait(false);

            if (!IsHot(band, full, parameters, out var hot))
            {
                rightHot = hot;
                break;
            }

            rightHot = true;
            nudgedEnd = Math.Min(totalDuration, nudgedEnd + parameters.NudgeStepSec);
            rightNudges++;
        }

        {
            double probeStart = Math.Max(0.0, nudgedEnd);
            double available = Math.Max(0.0, totalDuration - probeStart);
            double probeDuration = Math.Min(rightWindow, Math.Max(parameters.MinProbeSec, available));
            var band = await GetBandRmsAsync(audioPath, probeStart, probeDuration, parameters.HfBandLowHz,
                parameters.HfBandHighHz, ct).ConfigureAwait(false);
            var full = await GetFullbandRmsAsync(audioPath, probeStart, probeDuration, ct).ConfigureAwait(false);
            IsHot(band, full, parameters, out rightHot);
        }

        return (nudgedStart, nudgedEnd, leftHot, rightHot, leftNudges, rightNudges);
    }

    private async Task<VolumeProbe> CaptureProbeAsync(
        string audioPath,
        double boundary,
        bool isLeft,
        VolumeAnalysisParams parameters,
        double totalDuration,
        CancellationToken ct)
    {
        double baseWindow = Math.Max(parameters.MinProbeSec,
            Math.Max(isLeft ? parameters.GuardLeftSec : parameters.GuardRightSec, parameters.ProbeWindowSec));

        if (isLeft)
        {
            double length = Math.Min(baseWindow, boundary);
            if (length <= 0.0)
                return new VolumeProbe(boundary, 0.0, null, null);

            double start = Math.Max(0.0, boundary - length);
            var full = await GetFullbandRmsAsync(audioPath, start, length, ct).ConfigureAwait(false);
            var high = await GetHighbandRmsAsync(audioPath, start, length, ct).ConfigureAwait(false);
            return new VolumeProbe(start, length, full, high);
        }
        else
        {
            double start = Math.Max(0.0, boundary);
            double available = Math.Max(0.0, totalDuration - start);
            double length = Math.Min(baseWindow, Math.Max(parameters.MinProbeSec, available));
            if (length <= 0.0)
                return new VolumeProbe(boundary, 0.0, null, null);

            var full = await GetFullbandRmsAsync(audioPath, start, length, ct).ConfigureAwait(false);
            var high = await GetHighbandRmsAsync(audioPath, start, length, ct).ConfigureAwait(false);
            return new VolumeProbe(start, length, full, high);
        }
    }

    private async Task<double?> GetBandRmsAsync(
        string audioPath,
        double start,
        double duration,
        double lowHz,
        double highHz,
        CancellationToken ct)
    {
        var ci = CultureInfo.InvariantCulture;
        var normalized = PathNormalizer.NormalizePath(audioPath);
        string afilter =
            $"aformat=sample_fmts=flt:sample_rates={_targetRate}:channel_layouts=mono," +
            $"lowpass=f={highHz.ToString("F0", ci)}," +
            $"highpass=f={lowHz.ToString("F0", ci)},volumedetect";
        string args =
            $"-v info -ss {start.ToString("F6", ci)} -t {duration.ToString("F6", ci)} " +
            $"-i \"{normalized}\" -af \"{afilter}\" -f null -";

        var res = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct).ConfigureAwait(false);
        if (res.ExitCode != 0)
            return null;

        return ParseDb(res.StdErr);
    }

    private async Task<double?> GetFullbandRmsAsync(string audioPath, double start, double duration, CancellationToken ct)
    {
        var ci = CultureInfo.InvariantCulture;
        var normalized = PathNormalizer.NormalizePath(audioPath);
        string afilter =
            $"aformat=sample_fmts=flt:sample_rates={_targetRate}:channel_layouts=mono,volumedetect";
        string args =
            $"-v info -ss {start.ToString("F6", ci)} -t {duration.ToString("F6", ci)} " +
            $"-i \"{normalized}\" -af \"{afilter}\" -f null -";

        var res = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct).ConfigureAwait(false);
        if (res.ExitCode != 0)
            return null;

        return ParseDb(res.StdErr);
    }

    private async Task<double?> GetHighbandRmsAsync(string audioPath, double start, double duration, CancellationToken ct)
    {
        var ci = CultureInfo.InvariantCulture;
        var normalized = PathNormalizer.NormalizePath(audioPath);
        string afilter =
            $"aformat=sample_fmts=flt:sample_rates={_targetRate}:channel_layouts=mono,highpass=f=1500,highpass=f=1500,volumedetect";
        string args =
            $"-v info -ss {start.ToString("F6", ci)} -t {duration.ToString("F6", ci)} " +
            $"-i \"{normalized}\" -af \"{afilter}\" -f null -";

        var res = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct).ConfigureAwait(false);
        if (res.ExitCode == 0)
        {
            var parsed = ParseDb(res.StdErr);
            if (parsed is not null)
                return parsed;
        }

        string astats =
            $"aformat=sample_fmts=flt:sample_rates={_targetRate}:channel_layouts=mono,highpass=f=1500,highpass=f=1500,astats=metadata=1:reset=1:measure_overall=1:measure_perchannel=0:length=1";
        string argsAst =
            $"-v info -ss {start.ToString("F6", ci)} -t {duration.ToString("F6", ci)} " +
            $"-i \"{normalized}\" -af \"{astats}\" -f null -";
        var resAst = await _processRunner.RunAsync(GetFfmpegExecutable(), argsAst, ct).ConfigureAwait(false);
        var combined = (resAst.StdErr ?? string.Empty) + "\n" + (resAst.StdOut ?? string.Empty);
        var overall = AstatsOverall.Match(combined);
        if (overall.Success && double.TryParse(overall.Groups[1].Value, NumberStyles.Float, ci, out var db))
            return db;
        var frame = AstatsFrame.Match(combined);
        return frame.Success && double.TryParse(frame.Groups[1].Value, NumberStyles.Float, ci, out var frameDb)
            ? frameDb
            : (double?)null;
    }

    private async Task<double> GetAudioDurationAsync(string audioPath, CancellationToken ct)
    {
        var normalized = PathNormalizer.NormalizePath(audioPath);
        var args = $"-v quiet -show_entries format=duration -of csv=p=0 \"{normalized}\"";
        var result = await _processRunner.RunAsync(GetFfprobeExecutable(), args, ct).ConfigureAwait(false);
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"Failed to read audio duration: {result.StdErr}");

        if (double.TryParse(result.StdOut.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var duration))
            return duration;

        throw new InvalidOperationException($"Unable to parse duration from ffprobe output: {result.StdOut}");
    }

    private static bool IsHot(double? bandDb, double? fullDb, VolumeAnalysisParams parameters, out bool isHot)
    {
        if (bandDb is null || fullDb is null)
        {
            isHot = false;
            return false;
        }

        double delta = bandDb.Value - fullDb.Value;
        bool hotAbsolute = bandDb.Value > parameters.DbFloor;
        bool hotRelative = delta >= parameters.HfMarginDb;
        bool speechLevel = bandDb.Value > parameters.SpeechFloorDb;

        isHot = hotAbsolute && (speechLevel || hotRelative);
        if (!isHot && delta >= parameters.WeakMarginDb)
        {
            isHot = hotAbsolute;
        }

        return isHot;
    }

    private static double? ParseDb(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        var mean = MeanVolume.Match(text);
        if (mean.Success && double.TryParse(mean.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var meanDb))
            return meanDb;
        var max = MaxVolume.Match(text);
        if (max.Success && double.TryParse(max.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxDb))
            return maxDb;
        return null;
    }

    private static string GetFfmpegExecutable() =>
        Environment.GetEnvironmentVariable("FFMPEG_EXE") ?? "ffmpeg";

    private static string GetFfprobeExecutable() =>
        Environment.GetEnvironmentVariable("FFPROBE_EXE") ?? "ffprobe";
}
