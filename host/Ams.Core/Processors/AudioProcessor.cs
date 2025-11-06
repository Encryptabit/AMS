using System;
using System.IO;
using Ams.Core.Artifacts;
using Ams.Core.Services.Integrations.FFmpeg;

namespace Ams.Core.Processors;

/// <summary>
/// Central place for FFmpeg-backed audio primitives.
/// </summary>
public static class AudioProcessor
{
    public const int DefaultAsrSampleRate = 16_000;

    public static AudioInfo Probe(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return FfDecoder.Probe(path);
    }

    public static AudioBuffer Decode(string path, AudioDecodeOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var effective = options ?? new AudioDecodeOptions();
        return FfDecoder.Decode(path, effective);
    }

    public static void EncodeWav(string path, AudioBuffer buffer, AudioEncodeOptions? options = null)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        var effective = options ?? new AudioEncodeOptions();

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = File.Create(path);
        FfEncoder.EncodeToCustomStream(buffer, stream, effective);
        stream.Flush();
    }

    public static MemoryStream EncodeWavToStream(AudioBuffer buffer, AudioEncodeOptions? options = null)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        var effective = options ?? new AudioEncodeOptions();

        var ms = new MemoryStream();
        FfEncoder.EncodeToDynamicBuffer(buffer, ms, effective);
        ms.Position = 0;
        return ms;
    }
}

public readonly record struct AudioInfo(
    string Format,
    int SampleRate,
    int Channels,
    TimeSpan Duration);

public sealed record AudioDecodeOptions(
    TimeSpan? Start = null,
    TimeSpan? Duration = null,
    int? TargetSampleRate = null,
    int? TargetChannels = null);

public sealed record AudioEncodeOptions(
    int? TargetSampleRate = null,
    int? TargetBitDepth = null);
