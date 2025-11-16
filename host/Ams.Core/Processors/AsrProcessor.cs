using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Ams.Core.Artifacts;
using EchoSharp.Abstractions.Audio;
using EchoSharp.Abstractions.SpeechTranscription;
using EchoSharp.Abstractions.VoiceActivityDetection;
using EchoSharp.Audio;
using EchoSharp.Onnx.SileroVad;
using EchoSharp.SpeechTranscription;
using EchoSharp.Whisper.net;
using Whisper.net;
using Whisper.net.Logger;

namespace Ams.Core.Processors;

internal static class WhisperFactoryPool
{
    private readonly record struct FactoryKey(string ModelPath, bool UseGpu, int GpuDevice, bool UseFlashAttention, bool UseDtw);

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
    private const string SileroVadDirectoryEnvVar = "AMS_SILERO_VAD_DIR";
    private static readonly SemaphoreSlim VadFactoryLock = new(1, 1);
    private static IVadDetectorFactory? _sileroVadFactory;

    private const string SileroVadModelFileName = "silero_vad.onnx";

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

        var echoResponse = await TranscribeWithEchoSharpRealtimeAsync(buffer, options, cancellationToken)
            .ConfigureAwait(false);
        if (echoResponse is not null)
        {
            return echoResponse;
        }

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

    private static async Task<AsrResponse?> TranscribeWithEchoSharpRealtimeAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            using var waveSource = BuildAwaitableWaveSource(buffer);
           // using var whisperLogger = LogProvider.AddConsoleLogging(WhisperLogLevel.Debug);
            using var whisperFactory = WhisperFactory.FromPath(options.ModelPath, CreateFactoryOptions(options));
            using var speechFactory = new WhisperSpeechTranscriptorFactory(whisperFactory, dispose: false);
            var vadFactory = await GetSileroVadFactoryAsync(cancellationToken).ConfigureAwait(false);

            var realtimeFactory = new EchoSharpRealtimeTranscriptorFactory(
                speechTranscriptorFactory: speechFactory,
                vadDetectorFactory: vadFactory,
                recognizingSpeechTranscriptorFactory: null,
                echoSharpOptions: BuildRealtimeOptions(),
                vadDetectorOptions: BuildVadOptions());

            var realtime = realtimeFactory.Create(BuildRealtimeSpeechOptions(options));
            var tokens = new List<AsrToken>();
            var segments = new List<AsrSegment>();

            try
            {
                await foreach (var evt in realtime.TranscribeAsync(waveSource, cancellationToken).ConfigureAwait(false))
                {
                    if (evt is RealtimeSegmentRecognized recognized)
                    {
                        AppendSegmentAndTokens(segments, tokens, recognized.Segment);
                    }
                }
            }
            finally
            {
                (realtime as IDisposable)?.Dispose();
            }

            if (segments.Count == 0)
            {
                return null;
            }

            tokens.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            segments.Sort((a, b) => a.StartSec.CompareTo(b.StartSec));

            var modelVersion = Path.GetFileName(options.ModelPath) ?? "whisper";
            return new AsrResponse(modelVersion, tokens.ToArray(), segments.ToArray());
        }
        catch
        {
            return null;
        }
    }


    private static async Task<IVadDetectorFactory> GetSileroVadFactoryAsync(CancellationToken cancellationToken)
    {
        var cached = Volatile.Read(ref _sileroVadFactory);
        if (cached is not null)
        {
            return cached;
        }

        await VadFactoryLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_sileroVadFactory is not null)
            {
                return _sileroVadFactory;
            }

            var modelPath = await EnsureSileroModelAsync(cancellationToken).ConfigureAwait(false);
            var options = new SileroVadOptions(modelPath);
            _sileroVadFactory = new SileroVadDetectorFactory(options);
            return _sileroVadFactory;
        }
        finally
        {
            VadFactoryLock.Release();
        }
    }

    private static Task<string> EnsureSileroModelAsync(CancellationToken cancellationToken)
    {
        var directory = ResolveSileroModelDirectory();
        var modelPath = Path.Combine(directory, SileroVadModelFileName);

        return Task.FromResult(modelPath);
    }
    

    private static string ResolveSileroModelDirectory()
    {
        var env = Environment.GetEnvironmentVariable(SileroVadDirectoryEnvVar);
        if (!string.IsNullOrWhiteSpace(env))
        {
            return Path.GetFullPath(env);
        }

        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "models");
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

    private static EchoSharpRealtimeOptions BuildRealtimeOptions() => new()
    {
        
    };

    private static VadDetectorOptions BuildVadOptions() => new()
    {
    };

    private static RealtimeSpeechTranscriptorOptions BuildRealtimeSpeechOptions(AsrOptions options)
    {
        var autoDetect = string.IsNullOrWhiteSpace(options.Language) ||
                         options.Language.Equals("auto", StringComparison.OrdinalIgnoreCase);
        return new RealtimeSpeechTranscriptorOptions
        {
            Language = ResolveCulture(options.Language),
            RetrieveTokenDetails = true,
            IncludeSpeechRecogizingEvents = true 
        };
    }

    private static CultureInfo ResolveCulture(string? language)
    {
        if (string.IsNullOrWhiteSpace(language) || language.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            return CultureInfo.GetCultureInfo("en-US");
        }

        try
        {
            return CultureInfo.GetCultureInfo(language);
        }
        catch (CultureNotFoundException)
        {
            if (language.Length > 2)
            {
                var shortCode = language[..2];
                try
                {
                    return CultureInfo.GetCultureInfo(shortCode);
                }
                catch (CultureNotFoundException)
                {
                    // ignored
                }
            }

            return CultureInfo.GetCultureInfo("en-US");
        }
    }

    private static AwaitableWaveFileSource BuildAwaitableWaveSource(AudioBuffer buffer)
    {
        var source = new AwaitableWaveFileSource();
        using var wavStream = buffer.ToWavStream(new AudioEncodeOptions
        {
            TargetSampleRate = buffer.SampleRate,
            TargetBitDepth = 16
        });
        wavStream.Position = 0;

        var rented = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            int read;
            while ((read = wavStream.Read(rented, 0, rented.Length)) > 0)
            {
                source.WriteData(rented.AsMemory(0, read));
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }

        source.Flush();
        return source;
    }

    private static void AppendSegmentAndTokens(List<AsrSegment> segments, List<AsrToken> tokens,
        TranscriptSegment segment)
    {
        if (segment is null)
        {
            return;
        }

        var text = segment.Text?.Trim() ?? string.Empty;
        var start = segment.StartTime.TotalSeconds;
        var end = start + segment.Duration.TotalSeconds;
        if (end < start)
        {
            end = start;
        }

        segments.Add(new AsrSegment(start, end, text));

        if (segment.Tokens is not { Count: > 0 })
        {
            return;
        }

        var normalizedWords = PronunciationHelper.ExtractPronunciationParts(segment.Text ?? string.Empty);
        if (normalizedWords.Count == 0)
        {
            return;
        }

        var pieces = BuildTokenPieces(start, segment.Tokens);
        if (pieces.Count == 0)
        {
            return;
        }

        var pieceIndex = 0;
        foreach (var normalizedWord in normalizedWords)
        {
            if (pieceIndex >= pieces.Count)
            {
                break;
            }

            double? wordStart = null;
            double wordEnd = start;
            var builder = new StringBuilder();
            var consumed = 0;
            var normalizedTarget = normalizedWord ?? string.Empty;

            while (pieceIndex + consumed < pieces.Count)
            {
                var piece = pieces[pieceIndex + consumed];
                if (string.IsNullOrWhiteSpace(piece.Text))
                {
                    consumed++;
                    continue;
                }

                wordStart ??= piece.Start;
                wordEnd = Math.Max(wordEnd, piece.End);
                builder.Append(piece.Text);
                consumed++;

                if (builder.ToString().Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase))
                {
                    var duration = Math.Max(0.001, wordEnd - wordStart.Value);
                    tokens.Add(new AsrToken(wordStart.Value, duration, normalizedTarget));
                    pieceIndex += consumed;
                    break;
                }
            }

            if (builder.Length == 0)
            {
                continue;
            }

            if (!builder.ToString().Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase) && wordStart.HasValue)
            {
                var duration = Math.Max(0.001, wordEnd - wordStart.Value);
                tokens.Add(new AsrToken(wordStart.Value, duration, builder.ToString()));
                pieceIndex += consumed;
            }
        }

        for (; pieceIndex < pieces.Count; pieceIndex++)
        {
            var piece = pieces[pieceIndex];
            var duration = Math.Max(0.001, piece.End - piece.Start);
            tokens.Add(new AsrToken(piece.Start, duration, piece.Text));
        }
    }

    private sealed record TokenPiece(string Text, double Start, double End);

    private static List<TokenPiece> BuildTokenPieces(double segmentStart, IList<TranscriptToken> transcriptTokens)
    {
        var pieces = new List<TokenPiece>();
        foreach (var token in transcriptTokens)
        {
            if (token is null || string.IsNullOrWhiteSpace(token.Text) || IsSpecialToken(token.Text))
            {
                continue;
            }

            var parts = PronunciationHelper.ExtractPronunciationParts(token.Text);
            if (parts.Count == 0)
            {
                continue;
            }

            var tokenStart = segmentStart + token.StartTime.TotalSeconds;
            var tokenDuration = Math.Max(0.001, token.Duration.TotalSeconds);
            var tokenEnd = tokenStart + tokenDuration;
            var sliceDuration = tokenDuration / parts.Count;
            var pieceStart = tokenStart;

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                var pieceEnd = i == parts.Count - 1 ? tokenEnd : pieceStart + sliceDuration;
                pieces.Add(new TokenPiece(part, pieceStart, pieceEnd));
                pieceStart = pieceEnd;
            }
        }

        pieces.Sort((a, b) => a.Start.CompareTo(b.Start));
        return pieces;
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
