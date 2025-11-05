using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Asr;
using Whisper.net;

namespace Ams.Core.Processors;

/// <summary>
/// Whisper.NET backed ASR primitives exposed as static helpers.
/// </summary>
public static class AsrProcessor
{
    public static async Task<AsrResponse> TranscribeFileAsync(
        string audioPath,
        AsrOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);
        if (!File.Exists(audioPath))
        {
            throw new FileNotFoundException($"Audio file not found: {audioPath}", audioPath);
        }

        options = options ?? throw new ArgumentNullException(nameof(options));
        EnsureModelPath(options.ModelPath);
        cancellationToken.ThrowIfCancellationRequested();

        var decodeOptions = new AudioDecodeOptions(
            TargetSampleRate: AudioProcessor.DefaultAsrSampleRate,
            TargetChannels: 1);

        var buffer = AudioProcessor.Decode(audioPath, decodeOptions);
        return await TranscribeBufferInternalAsync(buffer, options, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<AsrResponse> TranscribeBufferAsync(
        ReadOnlyMemory<float> monoAudio,
        AsrOptions options,
        CancellationToken cancellationToken = default)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        EnsureModelPath(options.ModelPath);
        cancellationToken.ThrowIfCancellationRequested();

        var samples = monoAudio.ToArray();
        var buffer = new AudioBuffer(1, AudioProcessor.DefaultAsrSampleRate, samples.Length);
        Array.Copy(samples, buffer.Planar[0], samples.Length);

        return await TranscribeBufferInternalAsync(buffer, options, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<string> DetectLanguageAsync(
        string audioPath,
        AsrOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);
        if (!File.Exists(audioPath))
        {
            throw new FileNotFoundException($"Audio file not found: {audioPath}", audioPath);
        }

        options = options ?? throw new ArgumentNullException(nameof(options));
        EnsureModelPath(options.ModelPath);
        cancellationToken.ThrowIfCancellationRequested();

        var decodeOptions = new AudioDecodeOptions(
            TargetSampleRate: AudioProcessor.DefaultAsrSampleRate,
            TargetChannels: 1);

        var buffer = AudioProcessor.Decode(audioPath, decodeOptions);
        return await DetectLanguageInternalAsync(buffer, options, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<AsrResponse> TranscribeBufferInternalAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var factoryOptions = CreateFactoryOptions(options);

        using var factory = WhisperFactory.FromPath(options.ModelPath, factoryOptions);
        var builder = ConfigureBuilder(factory, options, enableTokenTimestamps: true);

        await using var processor = builder.Build();
        using var wavStream = AudioProcessor.EncodeWavToStream(buffer);
        wavStream.Position = 0;

        var tokens = new List<AsrToken>(buffer.Length);
        await foreach (var segment in processor.ProcessAsync(wavStream, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            AppendTokens(tokens, segment);
        }

        tokens.Sort(static (a, b) => a.StartTime.CompareTo(b.StartTime));
        var modelVersion = Path.GetFileName(options.ModelPath) ?? "whisper";
        return new AsrResponse(modelVersion, tokens.ToArray());
    }

    private static async Task<string> DetectLanguageInternalAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var factory = WhisperFactory.FromPath(options.ModelPath, CreateFactoryOptions(options));
        var builder = ConfigureBuilder(factory, options, enableTokenTimestamps: false);
        await using var processor = builder.Build();

        var samples = ExtractMonoSamples(buffer);
        var (language, _) = processor.DetectLanguageWithProbability(samples);
        return string.IsNullOrWhiteSpace(language) ? options.Language : language!;
    }
    private static void EnsureModelPath(string modelPath)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            throw new ArgumentException("Model path must be provided.", nameof(modelPath));
        }

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Whisper model not found: {modelPath}", modelPath);
        }
    }

    private static WhisperFactoryOptions CreateFactoryOptions(AsrOptions options) =>
        new()
        {
            UseGpu = options.UseGpu,
            GpuDevice = options.GpuDevice,
            UseFlashAttention = options.UseFlashAttention,
            UseDtwTimeStamps = options.UseDtwTimestamps && options.EnableWordTimestamps
        };

    private static WhisperProcessorBuilder ConfigureBuilder(
        WhisperFactory factory,
        AsrOptions options,
        bool enableTokenTimestamps)
    {
        var builder = factory.CreateBuilder();

        var threadCount = options.Threads > 0 ? options.Threads : Environment.ProcessorCount;
        builder.WithThreads(threadCount);

        if (enableTokenTimestamps || options.EnableWordTimestamps)
        {
            builder.WithTokenTimestamps();
            builder.SplitOnWord();
        }

        if (string.IsNullOrWhiteSpace(options.Language) || options.Language.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            builder.WithLanguageDetection();
        }
        else
        {
            builder.WithLanguage(options.Language);
        }

        if (options.Temperature > 0f)
        {
            builder.WithTemperature(options.Temperature);
        }

        builder.WithProbabilities();

        if (options.BeamSize > 1)
        {
            if (builder.WithBeamSearchSamplingStrategy() is BeamSearchSamplingStrategyBuilder beamBuilder)
            {
                beamBuilder.WithBeamSize(options.BeamSize);
            }
        }
        else if (options.BestOf > 1)
        {
            if (builder.WithGreedySamplingStrategy() is GreedySamplingStrategyBuilder greedyBuilder)
            {
                greedyBuilder.WithBestOf(options.BestOf);
            }
        }

        return builder;
    }

    private static void AppendTokens(ICollection<AsrToken> tokens, SegmentData segment)
    {
        if (segment.Tokens is { Length: > 0 })
        {
            foreach (var token in segment.Tokens)
            {
                var text = token.Text?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (token.Start < 0 || token.End < 0)
                {
                    continue;
                }

                var start = token.Start / 100.0;
                var duration = Math.Max(0, (token.End - token.Start) / 100.0);
                tokens.Add(new AsrToken(start, duration, text));
            }
        }
        else
        {
            var text = segment.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var start = segment.Start.TotalSeconds;
            var duration = Math.Max(0, (segment.End - segment.Start).TotalSeconds);
            tokens.Add(new AsrToken(start, duration, text));
        }
    }

    private static float[] ExtractMonoSamples(AudioBuffer buffer)
    {
        if (buffer.Channels <= 0 || buffer.Length == 0)
        {
            return Array.Empty<float>();
        }

        if (buffer.Channels == 1)
        {
            var mono = new float[buffer.Length];
            Array.Copy(buffer.Planar[0], mono, buffer.Length);
            return mono;
        }

        var samples = new float[buffer.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            double sum = 0;
            for (int ch = 0; ch < buffer.Channels; ch++)
            {
                sum += buffer.Planar[ch][i];
            }
            samples[i] = (float)(sum / buffer.Channels);
        }

        return samples;
    }
}

public sealed record AsrOptions(
    string ModelPath,
    string Language = "auto",
    int Threads = 0,
    bool UseGpu = true,
    bool EnableWordTimestamps = true,
    int BeamSize = 5,
    int BestOf = 1,
    float Temperature = 0.0f,
    bool NoSpeechBoost = true,
    int GpuDevice = 0,
    bool UseFlashAttention = false,
    bool UseDtwTimestamps = false);
