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

        var bitDepth = effective.TargetBitDepth ?? 16;
        if (bitDepth != 16)
        {
            throw new NotSupportedException("Only 16-bit PCM WAV encoding is supported currently.");
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = File.Create(path);
        WritePcm16Wave(stream, buffer);
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

        if (effective.TargetBitDepth.HasValue && effective.TargetBitDepth.Value != 16)
        {
            throw new NotSupportedException("Only 16-bit PCM WAV encoding is supported currently.");
        }

        var ms = new MemoryStream();
        FfEncoder.AudioBufferToWavStream(buffer, ms, PcmEncoding.Pcm16);
        ms.Position = 0;
        return ms;
    }

    private static void WritePcm16Wave(Stream destination, AudioBuffer buffer)
    {
        //FfEncoder.Encode(new AudioEncodeOptions { TargetBitDepth = 16, TargetSampleRate = DefaultAsrSampleRate}, buffer, destination);
        
        const ushort audioFormat = 1; // PCM
        const ushort bitsPerSample = 16;
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
                writer.Write((short)Math.Round(value * short.MaxValue, MidpointRounding.AwayFromZero));
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
