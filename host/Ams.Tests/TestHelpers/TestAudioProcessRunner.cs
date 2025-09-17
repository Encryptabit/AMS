using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core;

namespace Ams.Tests.TestHelpers;

internal sealed class TestAudioProcessRunner : IProcessRunner
{
    private readonly bool _simulateHot;
    private readonly double _duration;

    public TestAudioProcessRunner(bool simulateHot = false, double duration = 10.0)
    {
        _simulateHot = simulateHot;
        _duration = duration;
    }

    public Task<ProcessResult> RunAsync(string fileName, string arguments, CancellationToken ct = default)
    {
        if (fileName.Equals("ffprobe", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ProcessResult(0, _duration.ToString(CultureInfo.InvariantCulture), string.Empty));
        }

        if (!fileName.Equals("ffmpeg", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty));
        }

        if (arguments.Contains("-version", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ProcessResult(0, "ffmpeg version 6.1-test", string.Empty));
        }

        if (arguments.Contains("-c copy", StringComparison.OrdinalIgnoreCase))
        {
            var outputPath = ExtractOutputPath(arguments);
            if (!string.IsNullOrEmpty(outputPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                if (!File.Exists(outputPath))
                {
                    File.WriteAllBytes(outputPath, Array.Empty<byte>());
                }
            }
            return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty));
        }

        if (arguments.Contains("volumedetect", StringComparison.OrdinalIgnoreCase))
        {
            var start = ParseStart(arguments);
            bool hot = _simulateHot && start < 0.01;
            var isBand = arguments.Contains("lowpass", StringComparison.OrdinalIgnoreCase);
            var isHigh = arguments.Contains("highpass=f=1500", StringComparison.OrdinalIgnoreCase);

            double value = -60.0;
            if (hot)
            {
                value = (isBand || isHigh) ? -20.0 : -30.0;
            }

            var stderr = $"mean_volume: {value.ToString("F1", CultureInfo.InvariantCulture)} dB\nmax_volume: {(value + 2).ToString("F1", CultureInfo.InvariantCulture)} dB\n";
            return Task.FromResult(new ProcessResult(0, string.Empty, stderr));
        }

        if (arguments.Contains("astats", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ProcessResult(0, string.Empty, "Overall.RMS_level: -60.0"));
        }

        return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty));
    }

    private static double ParseStart(string arguments)
    {
        var match = Regex.Match(arguments, "-ss\\s+(?<val>[-0-9.]+)");
        if (match.Success && double.TryParse(match.Groups["val"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var start))
        {
            return start;
        }
        return 0.0;
    }

    private static string? ExtractOutputPath(string arguments)
    {
        var match = Regex.Match(arguments, "-y\\s+\"(?<out>[^\"]+)\"");
        return match.Success ? match.Groups["out"].Value : null;
    }
}
