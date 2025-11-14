using System.Diagnostics;
using System.Globalization;
using Ams.Web.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ams.Web.Services;

public sealed class FfmpegCliSegmentExporter : IAudioSegmentExporter
{
    private readonly string _ffmpegPath;
    private readonly ILogger<FfmpegCliSegmentExporter> _logger;

    public FfmpegCliSegmentExporter(IOptions<AmsOptions> options, ILogger<FfmpegCliSegmentExporter> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ffmpegPath = FfmpegPathResolver.Resolve(options.Value.Ffmpeg?.ExecutablePath);
    }

    public async Task<FileInfo> ExportAsync(
        string sourceAudioPath,
        string destinationDirectory,
        string fileName,
        double startSeconds,
        double endSeconds,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceAudioPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (!File.Exists(sourceAudioPath))
        {
            throw new FileNotFoundException("Source audio file was not found.", sourceAudioPath);
        }

        if (endSeconds < startSeconds)
        {
            (startSeconds, endSeconds) = (endSeconds, startSeconds);
        }

        Directory.CreateDirectory(destinationDirectory);
        var targetPath = Path.Combine(destinationDirectory, fileName);

        var arguments =
            $"-hide_banner -y -ss {FormatSeconds(startSeconds)} -to {FormatSeconds(endSeconds)} -i \"{sourceAudioPath}\" -c copy \"{targetPath}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _logger.LogDebug("Invoking ffmpeg: {Path} {Args}", _ffmpegPath, arguments);
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start ffmpeg process.");

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException($"ffmpeg exited with code {process.ExitCode}: {error}");
        }

        return new FileInfo(targetPath);
    }

    private static string FormatSeconds(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);
}
