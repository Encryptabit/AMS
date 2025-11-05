using System;
using Ams.Core.Artifacts;

namespace Ams.Core.Processors;

/// <summary>
/// Central place for FFmpeg-backed audio primitives.
/// </summary>
public static class AudioProcessor
{
    public static AudioInfo Probe(string path) =>
        throw new NotImplementedException("Pending FFmpeg integration.");

    public static AudioBuffer Decode(string path, AudioDecodeOptions? options = null) =>
        throw new NotImplementedException("Pending FFmpeg integration.");

    public static void EncodeWav(string path, AudioBuffer buffer, AudioEncodeOptions? options = null) =>
        throw new NotImplementedException("Pending FFmpeg integration.");
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
