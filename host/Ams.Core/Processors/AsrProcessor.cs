using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Ams.Core.Artifacts;
using Whisper.net;

namespace Ams.Core.Processors;

internal static class WhisperFactoryPool
{
    private readonly record struct FactoryKey(
        string ModelPath,
        bool UseGpu,
        int GpuDevice,
        bool UseFlashAttention,
        bool UseDtw);

    private sealed class FactoryEntry
    {
        public FactoryEntry(WhisperFactory factory)
        {
            Factory = factory;
        }

        public WhisperFactory Factory { get; }
        public int RefCount;
    }

    private sealed class FactoryHandle : IDisposable
    {
        private readonly FactoryKey _key;
        private FactoryEntry? _entry;

        public FactoryHandle(FactoryKey key, FactoryEntry entry)
        {
            _key = key;
            _entry = entry;
        }

        public WhisperFactory Factory => _entry!.Factory;

        public void Dispose()
        {
            if (_entry is null)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (--_entry.RefCount == 0)
                {
                    if (Entries.Remove(_key, out var removed))
                    {
                        removed.Factory.Dispose();
                    }
                }
            }

            _entry = null;
        }
    }

    private static readonly object SyncRoot = new();
    private static readonly Dictionary<FactoryKey, FactoryEntry> Entries = new();

    public static IDisposable Acquire(string modelPath, WhisperFactoryOptions options, out WhisperFactory factory)
    {
        var key = new FactoryKey(
            ModelPath: Path.GetFullPath(modelPath),
            UseGpu: options.UseGpu,
            GpuDevice: options.GpuDevice,
            UseFlashAttention: options.UseFlashAttention,
            UseDtw: options.UseDtwTimeStamps);

        FactoryEntry entry;
        lock (SyncRoot)
        {
            if (!Entries.TryGetValue(key, out var cached))
            {
                cached = new FactoryEntry(WhisperFactory.FromPath(key.ModelPath, options));
                Entries[key] = cached;
            }

            cached.RefCount++;
            entry = cached;
        }

        factory = entry.Factory;
        return new FactoryHandle(key, entry);
    }
}

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

    public static Task<AsrResponse> TranscribeBufferAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken = default)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        EnsureModelPath(options.ModelPath);
        cancellationToken.ThrowIfCancellationRequested();
        return TranscribeBufferInternalAsync(buffer, options, cancellationToken);
    }

    private static async Task<AsrResponse> TranscribeBufferInternalAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        buffer = NormalizeBuffer(buffer);

        return await TranscribeWithWhisperNetAsync(buffer, options, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> DetectLanguageInternalAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var factoryOptions = CreateFactoryOptions(options);
        using var handle = WhisperFactoryPool.Acquire(options.ModelPath, factoryOptions, out var factory);
        var builder = ConfigureBuilder(factory, options, enableTokenTimestamps: false);
        await using var processor = builder.Build();

        var samples = ExtractMonoSamples(buffer);
        var (language, _) = processor.DetectLanguageWithProbability(samples);
        return string.IsNullOrWhiteSpace(language) ? options.Language : language!;
    }

    private static async Task<AsrResponse> TranscribeWithWhisperNetAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        var factoryOptions = CreateFactoryOptions(options);

        using var factoryHandle = WhisperFactoryPool.Acquire(options.ModelPath, factoryOptions, out var factory);
        var active = Interlocked.Increment(ref _whisperInflight);
        Log.Debug("Whisper.NET inflight={Active} model={Model}", active, options.ModelPath);
        try
        {
            var builder = ConfigureBuilder(factory, options, enableTokenTimestamps: options.EnableWordTimestamps);

            await using var processor = builder.Build();
            await using var wavStream = buffer.ToWavStream(new AudioEncodeOptions
            {
                TargetSampleRate = AudioProcessor.DefaultAsrSampleRate,
                TargetBitDepth = 16
            });
            wavStream.Position = 0;

            var tokens = new List<AsrToken>();
            var segments = new List<AsrSegment>();
            await foreach (var segment in processor.ProcessAsync(wavStream, cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                segments.Add(new AsrSegment(
                    segment.Start.TotalSeconds,
                    segment.End.TotalSeconds,
                    segment.Text?.Trim() ?? string.Empty));
                AppendTokens(tokens, segment);
            }

            var modelVersion = Path.GetFileName(options.ModelPath) ?? "whisper";
            return new AsrResponse(modelVersion, tokens.ToArray(), segments.ToArray());
        }
        finally
        {
            var remaining = Interlocked.Decrement(ref _whisperInflight);
            Log.Debug("Whisper.NET completed inflight={Active} model={Model}", remaining, options.ModelPath);
        }
    }

    private static int _whisperInflight;

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
        return ConfigureBuilder(builder, options, enableTokenTimestamps);
    }

    private static WhisperProcessorBuilder ConfigureBuilder(
        WhisperProcessorBuilder builder,
        AsrOptions options,
        bool enableTokenTimestamps)
    {
        var threadCount = options.Threads > 0 ? options.Threads : Environment.ProcessorCount;
        builder.WithThreads(threadCount);

        if (enableTokenTimestamps || options.EnableWordTimestamps)
        {
            builder.WithTokenTimestamps();
            builder.SplitOnWord();
        }

        if (string.IsNullOrWhiteSpace(options.Language) ||
            options.Language.Equals("auto", StringComparison.OrdinalIgnoreCase))
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
        }
    }

    private static List<AsrToken> AggregateTokens(WhisperToken[] rawTokens) =>
        AggregateTokens(
            rawTokens,
            token => token.Start / 100.0,
            token => Math.Max(token.Start / 100.0, token.End / 100.0),
            token => token.Text);

    private static List<AsrToken> AggregateTokens<TToken>(
        IEnumerable<TToken> rawTokens,
        Func<TToken, double> startSelector,
        Func<TToken, double> endSelector,
        Func<TToken, string?> textSelector)
    {
        var result = new List<AsrToken>();
        var builder = new StringBuilder();
        double wordStart = 0;
        double wordEnd = 0;

        foreach (var token in rawTokens)
        {
            var raw = textSelector(token);
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

            var hasBoundary = HasExplicitWordBoundary(raw);
            var tokenStart = startSelector(token);
            var tokenEnd = Math.Max(tokenStart, endSelector(token));

            if (builder.Length > 0 && hasBoundary)
            {
                Flush();
            }

            if (builder.Length == 0)
            {
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

    private static bool HasExplicitWordBoundary(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var first = text[0];
        return char.IsWhiteSpace(first) || first == '▁' || first == 'Ġ' || first == 'Ċ';
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

    private static AudioBuffer NormalizeBuffer(AudioBuffer buffer)
    {
        var working = buffer;
        if (working.Channels != 1)
        {
            working = DownmixToMono(working);
        }

        if (working.SampleRate != AudioProcessor.DefaultAsrSampleRate)
        {
            working = AudioProcessor.Resample(working, AudioProcessor.DefaultAsrSampleRate);
        }

        return working;
    }

    private static AudioBuffer DownmixToMono(AudioBuffer buffer)
    {
        if (buffer.Channels == 1)
        {
            return buffer;
        }

        var mono = new AudioBuffer(1, buffer.SampleRate, buffer.Length);
        for (var i = 0; i < buffer.Length; i++)
        {
            double sum = 0;
            for (var ch = 0; ch < buffer.Channels; ch++)
            {
                sum += buffer.Planar[ch][i];
            }

            mono.Planar[0][i] = (float)(sum / buffer.Channels);
        }

        return mono;
    }
}

public sealed record AsrOptions(
    string ModelPath,
    string Language = "auto",
    int Threads = 8,
    bool UseGpu = true,
    bool EnableWordTimestamps = true,
    bool SplitOnWord = true,
    int BeamSize = 5,
    int BestOf = 1,
    float Temperature = 0.0f,
    bool NoSpeechBoost = true,
    int GpuDevice = 0,
    bool UseFlashAttention = true,
    bool UseDtwTimestamps = true);