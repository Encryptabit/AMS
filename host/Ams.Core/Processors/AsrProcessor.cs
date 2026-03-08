using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using Ams.Core.Artifacts;
using Ams.Core.Asr;
using Ams.Core.Audio;
using Ams.Core.Common;
using Whisper.net;

namespace Ams.Core.Processors;

internal static class WhisperFactoryPool
{
    private readonly record struct FactoryKey(
        string ModelPath,
        bool UseGpu,
        int GpuDevice,
        bool UseFlashAttention,
        bool UseDtw,
        WhisperAlignmentHeadsPreset DtwPreset);

    private sealed class FactoryEntry
    {
        public FactoryEntry(FactoryKey key)
        {
            Factory = WhisperFactory.FromPath(key.ModelPath, new WhisperFactoryOptions
            {
                UseGpu = key.UseGpu,
                GpuDevice = key.GpuDevice,
                UseFlashAttention = key.UseFlashAttention,
                UseDtwTimeStamps = key.UseDtw,
                HeadsPreset = key.DtwPreset
            });
            LastUsedUtcTicks = DateTime.UtcNow.Ticks;
        }

        public WhisperFactory Factory { get; }
        public object SyncRoot { get; } = new();
        public int RefCount;
        public long LastUsedUtcTicks;
        public bool IsDisposed;
    }

    private sealed class FactoryHandle : IDisposable
    {
        private FactoryEntry? _entry;
        private int _disposed;

        public FactoryHandle(FactoryEntry entry)
        {
            _entry = entry;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            var entry = Interlocked.Exchange(ref _entry, null);
            if (entry is null)
            {
                return;
            }

            Release(entry);
        }
    }

    private static readonly TimeSpan IdleTtl =
        TimeSpan.FromSeconds(ReadPositiveIntEnv("AMS_ASR_FACTORY_IDLE_SECONDS", 300));
    private static readonly TimeSpan EvictionSweepInterval =
        TimeSpan.FromSeconds(ReadPositiveIntEnv("AMS_ASR_FACTORY_SWEEP_SECONDS", 15));
    private static readonly int MaxCachedFactories = ReadPositiveIntEnv("AMS_ASR_FACTORY_CACHE_SIZE", 3);

    private static readonly ConcurrentDictionary<FactoryKey, Lazy<FactoryEntry>> Entries = new();
    private static readonly object EvictionSync = new();
    private static long _lastEvictionUtcTicks;

    static WhisperFactoryPool()
    {
        Log.Debug(
            "Whisper factory pool configured: cache_size={CacheSize} idle_ttl_s={IdleTtlSeconds} sweep_interval_s={SweepSeconds}",
            MaxCachedFactories,
            (int)IdleTtl.TotalSeconds,
            (int)EvictionSweepInterval.TotalSeconds);
    }

    public static IDisposable Acquire(string modelPath, WhisperFactoryOptions options, out WhisperFactory factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelPath);

        var key = new FactoryKey(
            ModelPath: NormalizeModelPath(modelPath),
            UseGpu: options.UseGpu,
            GpuDevice: options.GpuDevice,
            UseFlashAttention: options.UseFlashAttention,
            UseDtw: options.UseDtwTimeStamps,
            DtwPreset: options.HeadsPreset);

        while (true)
        {
            var lazyEntry = Entries.GetOrAdd(
                key,
                static factoryKey => new Lazy<FactoryEntry>(
                    () => CreateEntry(factoryKey),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            FactoryEntry entry;
            try
            {
                entry = lazyEntry.Value;
            }
            catch
            {
                Entries.TryRemove(new KeyValuePair<FactoryKey, Lazy<FactoryEntry>>(key, lazyEntry));
                throw;
            }

            var nowTicks = DateTime.UtcNow.Ticks;
            lock (entry.SyncRoot)
            {
                if (entry.IsDisposed)
                {
                    Entries.TryRemove(new KeyValuePair<FactoryKey, Lazy<FactoryEntry>>(key, lazyEntry));
                    continue;
                }

                entry.RefCount++;
                entry.LastUsedUtcTicks = nowTicks;
            }

            factory = entry.Factory;
            TrySweep(nowTicks);
            return new FactoryHandle(entry);
        }
    }

    private static FactoryEntry CreateEntry(FactoryKey key)
    {
        Log.Debug(
            "Whisper factory cache miss; loading model={Model} gpu={UseGpu} device={GpuDevice} flash={Flash} dtw={UseDtw}",
            key.ModelPath,
            key.UseGpu,
            key.GpuDevice,
            key.UseFlashAttention,
            key.UseDtw);

        return new FactoryEntry(key);
    }

    private static void Release(FactoryEntry entry)
    {
        var nowTicks = DateTime.UtcNow.Ticks;

        lock (entry.SyncRoot)
        {
            if (entry.IsDisposed)
            {
                return;
            }

            if (entry.RefCount > 0)
            {
                entry.RefCount--;
            }

            if (entry.RefCount == 0)
            {
                entry.LastUsedUtcTicks = nowTicks;
            }
        }

        TrySweep(nowTicks);
    }

    private static void TrySweep(long nowTicks)
    {
        if (nowTicks <= Interlocked.Read(ref _lastEvictionUtcTicks) + EvictionSweepInterval.Ticks)
        {
            return;
        }

        lock (EvictionSync)
        {
            if (nowTicks <= _lastEvictionUtcTicks + EvictionSweepInterval.Ticks)
            {
                return;
            }

            _lastEvictionUtcTicks = nowTicks;
            SweepIdleEntries(nowTicks);
            SweepLruEntries();
        }
    }

    private static void SweepIdleEntries(long nowTicks)
    {
        foreach (var pair in Entries)
        {
            if (!pair.Value.IsValueCreated)
            {
                continue;
            }

            if (!TryGetEntry(pair.Key, pair.Value, out var entry))
            {
                continue;
            }

            bool idleTooLong;
            lock (entry.SyncRoot)
            {
                if (entry.IsDisposed || entry.RefCount > 0)
                {
                    continue;
                }

                idleTooLong = nowTicks - entry.LastUsedUtcTicks >= IdleTtl.Ticks;
            }

            if (idleTooLong)
            {
                TryRemoveAndDispose(pair.Key, pair.Value, entry, "idle");
            }
        }
    }

    private static void SweepLruEntries()
    {
        if (Entries.Count <= MaxCachedFactories)
        {
            return;
        }

        var idleEntries = new List<(FactoryKey Key, Lazy<FactoryEntry> Lazy, FactoryEntry Entry, long LastUsedUtcTicks)>();

        foreach (var pair in Entries)
        {
            if (!pair.Value.IsValueCreated)
            {
                continue;
            }

            if (!TryGetEntry(pair.Key, pair.Value, out var entry))
            {
                continue;
            }

            lock (entry.SyncRoot)
            {
                if (entry.IsDisposed || entry.RefCount > 0)
                {
                    continue;
                }

                idleEntries.Add((pair.Key, pair.Value, entry, entry.LastUsedUtcTicks));
            }
        }

        if (idleEntries.Count == 0)
        {
            return;
        }

        idleEntries.Sort(static (a, b) => a.LastUsedUtcTicks.CompareTo(b.LastUsedUtcTicks));
        var excess = Entries.Count - MaxCachedFactories;

        for (var i = 0; i < idleEntries.Count && excess > 0; i++)
        {
            if (TryRemoveAndDispose(idleEntries[i].Key, idleEntries[i].Lazy, idleEntries[i].Entry, "lru"))
            {
                excess--;
            }
        }
    }

    private static bool TryGetEntry(FactoryKey key, Lazy<FactoryEntry> lazy, out FactoryEntry entry)
    {
        try
        {
            entry = lazy.Value;
            return true;
        }
        catch
        {
            Entries.TryRemove(new KeyValuePair<FactoryKey, Lazy<FactoryEntry>>(key, lazy));
            entry = null!;
            return false;
        }
    }

    private static bool TryRemoveAndDispose(
        FactoryKey key,
        Lazy<FactoryEntry> lazy,
        FactoryEntry entry,
        string reason)
    {
        lock (entry.SyncRoot)
        {
            if (entry.IsDisposed || entry.RefCount > 0)
            {
                return false;
            }

            if (!Entries.TryRemove(new KeyValuePair<FactoryKey, Lazy<FactoryEntry>>(key, lazy)))
            {
                return false;
            }

            entry.IsDisposed = true;
        }

        entry.Factory.Dispose();
        Log.Debug("Whisper factory cache evicted reason={Reason} model={Model}", reason, key.ModelPath);
        return true;
    }

    private static string NormalizeModelPath(string modelPath)
    {
        var fullPath = Path.GetFullPath(modelPath);
        return OperatingSystem.IsWindows() ? fullPath.ToUpperInvariant() : fullPath;
    }

    private static int ReadPositiveIntEnv(string name, int fallback)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return int.TryParse(raw, out var value) && value > 0 ? value : fallback;
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
        ValidateOptions(options);
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
        ValidateOptions(options);
        cancellationToken.ThrowIfCancellationRequested();

        var samples = monoAudio.ToArray();
        var buffer = new AudioBuffer(1, AudioProcessor.DefaultAsrSampleRate, samples.Length);
        samples.AsSpan().CopyTo(buffer.GetChannelSpan(0));

        return await TranscribeBufferInternalAsync(buffer, options, cancellationToken).ConfigureAwait(false);
    }

    public static Task<AsrResponse> TranscribeBufferAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken = default)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        ValidateOptions(options);
        cancellationToken.ThrowIfCancellationRequested();
        return TranscribeBufferInternalAsync(buffer, options, cancellationToken);
    }

    private static void ValidateOptions(AsrOptions options)
    {
        switch (options.Engine)
        {
            case AsrEngine.WhisperX:
                if (string.IsNullOrWhiteSpace(options.ModelName))
                {
                    throw new ArgumentException("WhisperX model name must be provided.", nameof(options));
                }

                break;

            case AsrEngine.Whisper:
            default:
                EnsureModelPath(options.ModelPath);
                break;
        }
    }

    private static async Task<AsrResponse> TranscribeBufferInternalAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        buffer = AsrAudioPreparer.PrepareForAsr(buffer);

        return options.Engine switch
        {
            AsrEngine.WhisperX => await RunWhisperXPassAsync(buffer, options, cancellationToken).ConfigureAwait(false),
            _ => await TranscribeWithWhisperNetAsync(buffer, options, cancellationToken).ConfigureAwait(false)
        };
    }

    private static async Task<AsrResponse> TranscribeWithWhisperNetAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        var response = await RunWhisperPassAsync(buffer, options, cancellationToken).ConfigureAwait(false);

        if (!ShouldRetryWithoutDtw(options, buffer, response, out var audioDurationSec, out var transcriptEndSec,
                out var coverage))
        {
            return response;
        }

        Log.Warn(
            "DTW timestamps appear truncated for model '{Model}' (end={TranscriptEnd:F2}s, audio={AudioDuration:F2}s, coverage={Coverage:P1}). Retrying once with DTW disabled.",
            options.ModelPath,
            transcriptEndSec,
            audioDurationSec,
            coverage);

        var fallbackOptions = options with { UseDtwTimestamps = false };
        return await RunWhisperPassAsync(buffer, fallbackOptions, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<AsrResponse> RunWhisperPassAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        var factoryOptions = CreateFactoryOptions(options);

        using var factoryHandle = WhisperFactoryPool.Acquire(options.ModelPath, factoryOptions, out var factory);
        var modelName = Path.GetFileName(options.ModelPath) ?? options.ModelPath;
        var active = Interlocked.Increment(ref _whisperInflight);
        var timer = Stopwatch.StartNew();
        if (active > 1)
        {
            Log.Debug("Whisper.NET contention inflight={Active} model={Model}", active, modelName);
        }

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
                var aggregatedTokens = segment.Tokens is { Length: > 0 }
                    ? AggregateTokens(segment.Tokens)
                    : null;
                segments.Add(BuildSegment(
                    segment.Start.TotalSeconds,
                    segment.End.TotalSeconds,
                    segment.Text?.Trim() ?? string.Empty,
                    aggregatedTokens));

                if (aggregatedTokens is { Count: > 0 })
                {
                    tokens.AddRange(aggregatedTokens);
                }
            }

            var modelVersion = Path.GetFileName(options.ModelPath) ?? "whisper";
            return new AsrResponse(modelVersion, tokens.ToArray(), segments.ToArray());
        }
        finally
        {
            timer.Stop();
            var remaining = Interlocked.Decrement(ref _whisperInflight);
            if (remaining > 0)
            {
                Log.Debug(
                    "Whisper.NET pass completed elapsed_ms={ElapsedMs} inflight={Active} model={Model}",
                    timer.ElapsedMilliseconds,
                    remaining,
                    modelName);
            }
            else
            {
                Log.Trace(
                    "Whisper.NET pass completed elapsed_ms={ElapsedMs} model={Model}",
                    timer.ElapsedMilliseconds,
                    modelName);
            }
        }
    }

    private static async Task<AsrResponse> RunWhisperXPassAsync(
        AudioBuffer buffer,
        AsrOptions options,
        CancellationToken cancellationToken)
    {
        var executable = ResolveWhisperXExecutable();
        var model = ResolveWhisperXModel(options);
        var tempDir = Path.Combine(Path.GetTempPath(), $"ams-whisperx-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var audioPath = Path.Combine(tempDir, "input.wav");
            AudioProcessor.EncodeWav(audioPath, buffer);

            using var process = new Process
            {
                StartInfo = CreateWhisperXStartInfo(executable, model, audioPath, tempDir, options)
            };

            Log.Debug(
                "Starting WhisperX process executable={Executable} model={Model} gpu={UseGpu} align={Align}",
                executable,
                model,
                options.UseGpu,
                options.EnableWordTimestamps);

            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to start whisperx process.");
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            using var registration = cancellationToken.Register(static state =>
            {
                var proc = (Process)state!;
                try
                {
                    if (!proc.HasExited)
                    {
                        proc.Kill(entireProcessTree: true);
                    }
                }
                catch
                {
                }
            }, process);

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var stdout = await stdoutTask.ConfigureAwait(false);
            var stderr = await stderrTask.ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"whisperx exited with code {process.ExitCode}.{FormatWhisperXFailure(stdout, stderr)}");
            }

            var jsonPath = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(audioPath) + ".json");
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"WhisperX output JSON not found: {jsonPath}", jsonPath);
            }

            var json = await File.ReadAllTextAsync(jsonPath, cancellationToken).ConfigureAwait(false);
            return ParseWhisperXJson(json, $"whisperx:{model}");
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    private static bool ShouldRetryWithoutDtw(
        AsrOptions options,
        AudioBuffer buffer,
        AsrResponse response,
        out double audioDurationSec,
        out double transcriptEndSec,
        out double coverage)
    {
        audioDurationSec = ComputeAudioDurationSeconds(buffer);
        transcriptEndSec = ComputeTranscriptEndSeconds(response);
        coverage = audioDurationSec > 0 ? transcriptEndSec / audioDurationSec : 0;

        if (!IsDtwEffectivelyEnabled(options))
        {
            return false;
        }

        if (audioDurationSec < DtwFallbackMinAudioSeconds)
        {
            return false;
        }

        if (transcriptEndSec <= 0)
        {
            return true;
        }

        return coverage < DtwFallbackCoverageThreshold &&
               (audioDurationSec - transcriptEndSec) >= DtwFallbackMinimumShortfallSeconds;
    }

    private static bool IsDtwEffectivelyEnabled(AsrOptions options) =>
        options.UseDtwTimestamps &&
        options.EnableWordTimestamps &&
        ResolveDtwPreset(options.ModelPath).HasValue;

    private static double ComputeAudioDurationSeconds(AudioBuffer buffer) =>
        buffer.SampleRate > 0 ? buffer.Length / (double)buffer.SampleRate : 0;

    private static double ComputeTranscriptEndSeconds(AsrResponse response)
    {
        var maxEnd = 0d;
        if (response.Segments is { Length: > 0 })
        {
            foreach (var segment in response.Segments)
            {
                if (segment.EndSec > maxEnd)
                {
                    maxEnd = segment.EndSec;
                }
            }
        }

        if (response.Tokens is { Length: > 0 })
        {
            foreach (var token in response.Tokens)
            {
                var tokenEnd = token.StartTime + Math.Max(0, token.Duration);
                if (tokenEnd > maxEnd)
                {
                    maxEnd = tokenEnd;
                }
            }
        }

        return maxEnd;
    }

    private const double DtwFallbackMinAudioSeconds = 90d;
    private const double DtwFallbackCoverageThreshold = 0.70d;
    private const double DtwFallbackMinimumShortfallSeconds = 30d;

    private static int _whisperInflight;

    private static string ResolveWhisperXExecutable()
    {
        var configured = Environment.GetEnvironmentVariable(AsrEngineConfig.WhisperXExecutableEnvironmentVariable);
        return string.IsNullOrWhiteSpace(configured) ? "whisperx" : configured.Trim();
    }

    private static string ResolveWhisperXModel(AsrOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ModelName))
        {
            return options.ModelName.Trim();
        }

        return AsrEngineConfig.DefaultWhisperXModel;
    }

    private static ProcessStartInfo CreateWhisperXStartInfo(
        string executable,
        string model,
        string audioPath,
        string outputDirectory,
        AsrOptions options)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(audioPath);
        startInfo.ArgumentList.Add("--model");
        startInfo.ArgumentList.Add(model);
        startInfo.ArgumentList.Add("--output_dir");
        startInfo.ArgumentList.Add(outputDirectory);
        startInfo.ArgumentList.Add("--output_format");
        startInfo.ArgumentList.Add("json");
        startInfo.ArgumentList.Add("--threads");
        startInfo.ArgumentList.Add(Math.Max(0, options.Threads).ToString());
        startInfo.ArgumentList.Add("--log-level");
        startInfo.ArgumentList.Add("error");

        if (options.UseGpu)
        {
            startInfo.ArgumentList.Add("--device");
            startInfo.ArgumentList.Add("cuda");
            startInfo.ArgumentList.Add("--device_index");
            startInfo.ArgumentList.Add(options.GpuDevice.ToString());
            startInfo.ArgumentList.Add("--compute_type");
            startInfo.ArgumentList.Add("float16");
        }
        else
        {
            startInfo.ArgumentList.Add("--device");
            startInfo.ArgumentList.Add("cpu");
            startInfo.ArgumentList.Add("--compute_type");
            startInfo.ArgumentList.Add("int8");
        }

        if (!string.IsNullOrWhiteSpace(options.Language) &&
            !options.Language.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            startInfo.ArgumentList.Add("--language");
            startInfo.ArgumentList.Add(options.Language);
        }

        if (!options.EnableWordTimestamps)
        {
            startInfo.ArgumentList.Add("--no_align");
        }

        if (options.BeamSize > 0)
        {
            startInfo.ArgumentList.Add("--beam_size");
            startInfo.ArgumentList.Add(options.BeamSize.ToString());
        }

        if (options.BestOf > 0)
        {
            startInfo.ArgumentList.Add("--best_of");
            startInfo.ArgumentList.Add(options.BestOf.ToString());
        }

        if (options.Temperature > 0f)
        {
            startInfo.ArgumentList.Add("--temperature");
            startInfo.ArgumentList.Add(options.Temperature.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrWhiteSpace(options.Prompt))
        {
            var normalizedPrompt = NormalizePrompt(options.Prompt);
            if (!string.IsNullOrWhiteSpace(normalizedPrompt))
            {
                startInfo.ArgumentList.Add("--initial_prompt");
                startInfo.ArgumentList.Add(normalizedPrompt);
            }
        }

        return startInfo;
    }

    private static string FormatWhisperXFailure(string stdout, string stderr)
    {
        var details = new List<string>(2);
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            details.Add($" stderr: {stderr.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(stdout))
        {
            details.Add($" stdout: {stdout.Trim()}");
        }

        return details.Count == 0 ? string.Empty : string.Concat(details);
    }

    private static void TryDeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to delete temporary directory {Directory}: {Message}", path, ex.Message);
        }
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

    private static WhisperFactoryOptions CreateFactoryOptions(AsrOptions options)
    {
        var dtwRequested = options.UseDtwTimestamps && options.EnableWordTimestamps;
        var dtwPreset = dtwRequested ? ResolveDtwPreset(options.ModelPath) : null;
        var dtwEnabled = dtwRequested && dtwPreset.HasValue;

        if (dtwRequested && OperatingSystem.IsWindows())
        {
            Log.Warn(
                "DTW timestamps requested on Windows for model '{Model}', but the Windows DTW path is currently disabled.",
                options.ModelPath);
            dtwEnabled = false;
        }

        if (dtwRequested && !dtwPreset.HasValue)
        {
            Log.Warn(
                "DTW timestamps requested but no alignment-head preset could be inferred for model '{Model}'. DTW is disabled to avoid native runtime errors.",
                options.ModelPath);
        }

        var useFlashAttention = options.UseFlashAttention;
        if (dtwEnabled && useFlashAttention)
        {
            Log.Warn(
                "DTW timestamps requested with FlashAttention enabled for model '{Model}'. Disabling FlashAttention because whisper.cpp does not support DTW with flash_attn.",
                options.ModelPath);
            useFlashAttention = false;
        }

        return new WhisperFactoryOptions
        {
            UseGpu = options.UseGpu,
            GpuDevice = options.GpuDevice,
            UseFlashAttention = useFlashAttention,
            UseDtwTimeStamps = dtwEnabled,
            HeadsPreset = dtwPreset ?? WhisperAlignmentHeadsPreset.None
        };
    }

    private static WhisperAlignmentHeadsPreset? ResolveDtwPreset(string modelPath)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            return null;
        }

        var name = Path.GetFileName(modelPath).ToLowerInvariant().Replace('_', '-');

        if (name.Contains("large-v3-turbo", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.LargeV3Turbo;
        }

        if (name.Contains("large-v3", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.LargeV3;
        }

        if (name.Contains("large-v2", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.LargeV2;
        }

        if (name.Contains("large-v1", StringComparison.Ordinal) ||
            name.Contains("ggml-large.bin", StringComparison.Ordinal) ||
            name.Equals("large.bin", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.LargeV1;
        }

        if (name.Contains("medium.en", StringComparison.Ordinal) ||
            name.Contains("medium-en", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.MediumEn;
        }

        if (name.Contains("medium", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.Medium;
        }

        if (name.Contains("small.en", StringComparison.Ordinal) ||
            name.Contains("small-en", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.SmallEn;
        }

        if (name.Contains("small", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.Small;
        }

        if (name.Contains("base.en", StringComparison.Ordinal) ||
            name.Contains("base-en", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.BaseEn;
        }

        if (name.Contains("base", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.Base;
        }

        if (name.Contains("tiny.en", StringComparison.Ordinal) ||
            name.Contains("tiny-en", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.TinyEn;
        }

        if (name.Contains("tiny", StringComparison.Ordinal))
        {
            return WhisperAlignmentHeadsPreset.Tiny;
        }

        return null;
    }

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

        if (!string.IsNullOrWhiteSpace(options.Prompt))
        {
            var normalizedPrompt = NormalizePrompt(options.Prompt);
            if (!string.IsNullOrWhiteSpace(normalizedPrompt))
            {
                builder.WithPrompt(normalizedPrompt);
            }
        }

        return builder;
    }

    private static string NormalizePrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return string.Empty;
        }

        var normalized = TextNormalizer.NormalizeTypography(prompt);

        // Guard against replacement-char artifacts leaking into decoder context.
        normalized = normalized.Replace('\uFFFD', '\'');

        return CollapseWhitespace(normalized);
    }

    private static string CollapseWhitespace(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        var pendingSpace = false;

        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(ch);
        }

        return builder.ToString().Trim();
    }

    internal static AsrSegment BuildSegment(
        double rawStartSec,
        double rawEndSec,
        string text,
        IReadOnlyList<AsrToken>? tokens)
    {
        if (tokens is { Count: > 0 })
        {
            var start = tokens[0].StartTime;
            var end = tokens[^1].StartTime + Math.Max(0, tokens[^1].Duration);
            return new AsrSegment(start, Math.Max(start, end), text);
        }

        return new AsrSegment(rawStartSec, rawEndSec, text);
    }

    internal static List<AsrToken> AggregateTokens(WhisperToken[] rawTokens)
    {
        if (rawTokens.Length == 0)
        {
            return new List<AsrToken>();
        }

        if (!rawTokens.Any(token => token.DtwTimestamp >= 0))
        {
            return AggregateTokens(
                rawTokens,
                token => token.Start / 100.0,
                token => Math.Max(token.Start / 100.0, token.End / 100.0),
                token => token.Text);
        }

        var timedTokens = BuildDtwTimedTokens(rawTokens);
        return AggregateTokens(
            timedTokens,
            token => token.StartSec,
            token => token.EndSec,
            token => token.Text);
    }

    private static DtwTimedToken[] BuildDtwTimedTokens(WhisperToken[] rawTokens)
    {
        var nextDtwStartSec = new double?[rawTokens.Length];
        double? nextAssignedSec = null;
        for (int i = rawTokens.Length - 1; i >= 0; i--)
        {
            nextDtwStartSec[i] = nextAssignedSec;
            if (rawTokens[i].DtwTimestamp >= 0)
            {
                nextAssignedSec = rawTokens[i].DtwTimestamp / 100.0;
            }
        }

        var timedTokens = new DtwTimedToken[rawTokens.Length];
        for (int i = 0; i < rawTokens.Length; i++)
        {
            var token = rawTokens[i];
            var rawStartSec = token.Start / 100.0;
            var rawEndSec = Math.Max(rawStartSec, token.End / 100.0);
            var startSec = token.DtwTimestamp >= 0 ? token.DtwTimestamp / 100.0 : rawStartSec;
            var endSec = token.DtwTimestamp >= 0
                ? Math.Max(startSec, nextDtwStartSec[i] ?? rawEndSec)
                : rawEndSec;
            timedTokens[i] = new DtwTimedToken(startSec, endSec, token.Text);
        }

        return timedTokens;
    }

    internal static AsrResponse ParseWhisperXJson(string json, string modelVersion)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var segments = ParseWhisperXSegments(root);
        var tokens = ParseWhisperXTokens(root);
        return new AsrResponse(modelVersion, tokens.ToArray(), segments.ToArray());
    }

    private static List<AsrSegment> ParseWhisperXSegments(JsonElement root)
    {
        var segments = new List<AsrSegment>();
        if (!root.TryGetProperty("segments", out var segmentArray) || segmentArray.ValueKind != JsonValueKind.Array)
        {
            return segments;
        }

        foreach (var segment in segmentArray.EnumerateArray())
        {
            if (!TryGetDouble(segment, "start", out var start))
            {
                continue;
            }

            var end = TryGetDouble(segment, "end", out var parsedEnd) ? Math.Max(start, parsedEnd) : start;
            var text = segment.TryGetProperty("text", out var textElement)
                ? textElement.GetString()?.Trim() ?? string.Empty
                : string.Empty;
            segments.Add(new AsrSegment(start, end, text));
        }

        return segments;
    }

    private static List<AsrToken> ParseWhisperXTokens(JsonElement root)
    {
        if (root.TryGetProperty("word_segments", out var wordSegments) && wordSegments.ValueKind == JsonValueKind.Array)
        {
            var direct = ParseWhisperXWordArray(wordSegments);
            if (direct.Count > 0)
            {
                return direct;
            }
        }

        if (!root.TryGetProperty("segments", out var segmentArray) || segmentArray.ValueKind != JsonValueKind.Array)
        {
            return new List<AsrToken>();
        }

        var tokens = new List<AsrToken>();
        foreach (var segment in segmentArray.EnumerateArray())
        {
            if (!segment.TryGetProperty("words", out var words) || words.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            tokens.AddRange(ParseWhisperXWordArray(words));
        }

        return tokens;
    }

    private static List<AsrToken> ParseWhisperXWordArray(JsonElement words)
    {
        var tokens = new List<AsrToken>();
        foreach (var word in words.EnumerateArray())
        {
            if (!TryGetDouble(word, "start", out var start))
            {
                continue;
            }

            var end = TryGetDouble(word, "end", out var parsedEnd) ? Math.Max(start, parsedEnd) : start;
            var text = GetWhisperXWordText(word);
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            tokens.Add(new AsrToken(start, Math.Max(0.05, end - start), text));
        }

        return tokens;
    }

    private static string GetWhisperXWordText(JsonElement word)
    {
        if (word.TryGetProperty("word", out var value))
        {
            return value.GetString()?.Trim() ?? string.Empty;
        }

        if (word.TryGetProperty("text", out value))
        {
            return value.GetString()?.Trim() ?? string.Empty;
        }

        return string.Empty;
    }

    private static bool TryGetDouble(JsonElement element, string propertyName, out double value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number => property.TryGetDouble(out value),
            JsonValueKind.String => double.TryParse(
                property.GetString(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out value),
            _ => false
        };
    }

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
            return buffer.GetChannel(0).ToArray();
        }

        var samples = new float[buffer.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            double sum = 0;
            for (int ch = 0; ch < buffer.Channels; ch++)
            {
                sum += buffer.GetChannel(ch).Span[i];
            }

            samples[i] = (float)(sum / buffer.Channels);
        }

        return samples;
    }
}

public sealed record AsrOptions(
    string ModelPath,
    AsrEngine Engine = AsrEngine.Whisper,
    string? ModelName = null,
    string Language = "auto",
    int Threads = 8,
    bool UseGpu = true,
    bool EnableWordTimestamps = true,
    bool SplitOnWord = true,
    int BeamSize = 3,
    int BestOf = 1,
    float Temperature = 0.0f,
    bool NoSpeechBoost = true,
    int GpuDevice = 0,
    bool UseFlashAttention = true,
    bool UseDtwTimestamps = false,
    string? Prompt = null,
    bool DisableChunkPlan = false);

internal readonly record struct DtwTimedToken(double StartSec, double EndSec, string? Text);
