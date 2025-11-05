using System;
using System.IO;
using System.Text;
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
        if (effective.TargetSampleRate.HasValue && effective.TargetSampleRate.Value != buffer.SampleRate)
        {
            throw new NotSupportedException("Sample rate conversion during encode is not implemented yet.");
        }

        var bitDepth = effective.TargetBitDepth ?? 32;
        if (bitDepth != 32)
        {
            throw new NotSupportedException("Only 32-bit float WAV encoding is supported currently.");
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = File.Create(path);
        WriteFloatWave(stream, buffer);
    }

    public static MemoryStream EncodeWavToStream(AudioBuffer buffer, AudioEncodeOptions? options = null)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        var effective = options ?? new AudioEncodeOptions();
        if (effective.TargetSampleRate.HasValue && effective.TargetSampleRate.Value != buffer.SampleRate)
        {
            throw new NotSupportedException("Sample rate conversion during encode is not implemented yet.");
        }

        if (effective.TargetBitDepth.HasValue && effective.TargetBitDepth.Value != 32)
        {
            throw new NotSupportedException("Only 32-bit float WAV encoding is supported currently.");
        }

        var ms = new MemoryStream();
        WriteFloatWave(ms, buffer);
        ms.Position = 0;
        return ms;
    }

    private static void WriteFloatWave(Stream destination, AudioBuffer buffer)
    {
        const ushort audioFormat = 3; // IEEE float
        const ushort bitsPerSample = 32;
        var channels = (ushort)buffer.Channels;
        var sampleRate = (uint)buffer.SampleRate;
        var blockAlign = (ushort)(channels * (bitsPerSample / 8));
        var byteRate = sampleRate * blockAlign;
        var dataSize = (uint)(buffer.Length * blockAlign);
        var riffSize = 4 + (8 + 16) + (8 + dataSize);

        using var writer = new BinaryWriter(destination, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(riffSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));

        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16u);
        writer.Write(audioFormat);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);

        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize);

        for (int sample = 0; sample < buffer.Length; sample++)
        {
            for (int ch = 0; ch < buffer.Channels; ch++)
            {
                var value = Math.Clamp(buffer.Planar[ch][sample], -1f, 1f);
                writer.Write(value);
            }
        }

        writer.Flush();
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
