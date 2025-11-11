using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Asr;
using Whisper.net;
using Ams.Core.Services.Integrations.FFmpeg;

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

    private static async Task<AsrResponse> TranscribeBufferInternalAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var factoryOptions = CreateFactoryOptions(options);

        using var factory = WhisperFactory.FromPath(options.ModelPath, factoryOptions);
        var builder = ConfigureBuilder(factory, options, enableTokenTimestamps: options.EnableWordTimestamps);

        await using var processor = builder.Build();
        using var wavStream = new MemoryStream();
        FfFilterGraph.FromBuffer(buffer)
            .StreamToWave(wavStream, new AudioEncodeOptions
            {
                TargetSampleRate = AudioProcessor.DefaultAsrSampleRate,
                TargetBitDepth = 16
            });
        wavStream.Position = 0;

        var tokens = new List<AsrToken>();
        await foreach (var segment in processor.ProcessAsync(wavStream, cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            AppendTokens(tokens, segment);
        }

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

    private static void AppendTokens(List<AsrToken> tokens, SegmentData segment)
    {
        var aggregated = segment.Tokens is { Length: > 0 }
            ? AggregateTokens(segment.Tokens)
            : null;

        if (aggregated is { Count: > 0 })
        {
            foreach (var token in aggregated)
            {
                tokens.Add(token);
            }
            return;
        }

        /*var text = segment.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var words = text
            .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return;
        }

        var start = segment.Start.TotalSeconds;
        var end = Math.Max(start, segment.End.TotalSeconds);
        var durationPerWord = Math.Max(0.05, (end - start) / words.Length);

        for (int i = 0; i < words.Length; i++)
        {
            var wordStart = start + i * durationPerWord;
            tokens.Add(new AsrToken(segment.Words, durationPerWord, words[i]));
        }*/
    }

    private static List<AsrToken> AggregateTokens(WhisperToken[] rawTokens)
    {
        var result = new List<AsrToken>();
        var builder = new StringBuilder();
        double wordStart = 0;
        double wordEnd = 0;

        foreach (var token in rawTokens)
        {
            if (token.Start < 0 || token.End < 0)
            {
                continue;
            }

            var raw = token.Text;
            if (string.IsNullOrEmpty(raw))
            {
                continue;
            }

            if (IsSpecialToken(raw))
            {
                continue;
            }

            var normalized = NormalizeTokenText(raw);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            var leadingSpace = char.IsWhiteSpace(raw[0]);
            var tokenStart = token.Start / 100.0;
            var tokenEnd = Math.Max(tokenStart, token.End / 100.0);

            if (builder.Length == 0 || leadingSpace)
            {
                Flush();
                wordStart = tokenStart;
            }

            builder.Append(normalized);
            wordEnd = Math.Max(wordEnd, tokenEnd);
        }

        Flush();
        return result;

        void Flush()
        {
            if (builder.Length == 0)
            {
                return;
            }

            var duration = Math.Max(0.05, wordEnd - wordStart);
            result.Add(new AsrToken(wordStart, duration, builder.ToString()));

            builder.Clear();
            wordStart = 0;
            wordEnd = 0;
        }
    }

    private static bool IsSpecialToken(string text) =>
        text.StartsWith('[') && text.EndsWith(']');

    private static string NormalizeTokenText(string text)
    {
        var normalized = text
            .Replace("▁", " ")
            .Replace("Ġ", " ")
            .Replace("Ċ", " ")
            .Trim();

        return normalized;
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
    bool UseFlashAttention = true,
    bool UseDtwTimestamps = true);
