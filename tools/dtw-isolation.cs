#:package Whisper.net@1.9.0
#:package Whisper.net.Runtime@1.9.0
#:package Whisper.net.Runtime.Cuda@1.9.0

using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.LibraryLoader;
using Whisper.net.Logger;
using Whisper.net.Wave;

try
{
    if (args.Any(a => string.Equals(a, "--help", StringComparison.OrdinalIgnoreCase)))
    {
        PrintUsage();
        return;
    }

    var options = Options.Parse(args);
    using var whisperLog = LogProvider.AddConsoleLogging(options.Verbose ? WhisperLogLevel.Debug : WhisperLogLevel.Info);

    ConfigureRuntimeOrder(options.RuntimeMode);

    var cacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".cache",
        "whisper-isolation");
    Directory.CreateDirectory(cacheDir);

    var (modelPath, inferredHeadsPreset) = await ResolveModelPathAsync(options, cacheDir).ConfigureAwait(false);
    var audioPath = await ResolveAudioPathAsync(options, cacheDir).ConfigureAwait(false);
    var headsPreset = ResolveHeadsPresetOption(options.HeadsPresetMode, inferredHeadsPreset);
    var dtwEnabled = options.Dtw && options.WordTimestamps && headsPreset.HasValue;

    if (options.Dtw && options.WordTimestamps && !headsPreset.HasValue)
    {
        Console.WriteLine(
            "WARNING: DTW requested, but no alignment-head preset is configured or inferable. DTW will be disabled to avoid native crashes.");
    }

    Console.WriteLine("=== Whisper DTW Isolation Runner ===");
    Console.WriteLine($"Model:            {modelPath}");
    Console.WriteLine($"Audio:            {audioPath}");
    Console.WriteLine($"Iterations:       {options.Iterations}");
    Console.WriteLine($"Runtime mode:     {options.RuntimeMode}");
    Console.WriteLine($"Runtime order:    {string.Join(", ", RuntimeOptions.RuntimeLibraryOrder)}");
    Console.WriteLine($"UseGpu:           {options.UseGpu}");
    Console.WriteLine($"GpuDevice:        {options.GpuDevice}");
    Console.WriteLine($"WordTimestamps:   {options.WordTimestamps}");
    Console.WriteLine($"SplitOnWord:      {options.SplitOnWord}");
    Console.WriteLine($"Dtw requested:    {options.Dtw}");
    Console.WriteLine($"Heads preset:     {(headsPreset?.ToString() ?? "None")}");
    Console.WriteLine($"Dtw effective:    {dtwEnabled}");
    Console.WriteLine($"BeamSize:         {options.BeamSize}");
    Console.WriteLine($"BestOf:           {options.BestOf}");
    Console.WriteLine($"Temperature:      {options.Temperature.ToString(CultureInfo.InvariantCulture)}");
    Console.WriteLine($"Language:         {options.Language}");
    Console.WriteLine($"InputMode:        {options.InputMode}");
    Console.WriteLine($"NoContext:        {options.NoContext}");
    Console.WriteLine($"NoSpeechThr:      {FormatOptionalFloat(options.NoSpeechThreshold)}");
    Console.WriteLine($"MaxLastTextToks:  {options.MaxLastTextTokens}");
    Console.WriteLine($"OffsetSec:        {FormatOptionalFloat(options.OffsetSeconds)}");
    Console.WriteLine($"DurationSec:      {FormatOptionalFloat(options.DurationSeconds)}");
    Console.WriteLine($"ChunkSec:         {FormatOptionalFloat(options.ChunkSeconds)}");
    Console.WriteLine($"ChunkOverlapSec:  {FormatOptionalFloat(options.ChunkOverlapSeconds)}");
    Console.WriteLine($"OutJson:          {options.OutJsonPath ?? "none"}");
    Console.WriteLine();

    var factoryOptions = new WhisperFactoryOptions
    {
        UseGpu = options.UseGpu,
        GpuDevice = options.GpuDevice,
        UseFlashAttention = options.FlashAttention,
        UseDtwTimeStamps = dtwEnabled,
        HeadsPreset = headsPreset ?? WhisperAlignmentHeadsPreset.None
    };

    var totalStopwatch = Stopwatch.StartNew();
    for (var i = 1; i <= options.Iterations; i++)
    {
        Console.WriteLine($"--- Iteration {i}/{options.Iterations} ---");
        var sw = Stopwatch.StartNew();

        try
        {
            using var factory = WhisperFactory.FromPath(modelPath, factoryOptions);
            Console.WriteLine($"Loaded runtime:    {RuntimeOptions.LoadedLibrary?.ToString() ?? "unknown"}");

            var segments = await ProcessSegmentsAsync(factory, options, audioPath, CancellationToken.None)
                .ConfigureAwait(false);

            var segmentCount = segments.Count;
            var tokenCount = segments.Sum(s => s.Tokens?.Length ?? 0);
            TimeSpan? firstStart = null;
            TimeSpan? lastEnd = null;
            foreach (var segment in segments)
            {
                firstStart ??= segment.Start;
                lastEnd = segment.End;

                if (!options.QuietSegments)
                {
                    Console.WriteLine(
                        $"{segment.Start:hh\\:mm\\:ss\\.fff} -> {segment.End:hh\\:mm\\:ss\\.fff} | {segment.Text?.Trim()}");
                }
            }

            sw.Stop();
            var firstStartText = firstStart?.ToString("hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture) ?? "n/a";
            var lastEndText = lastEnd?.ToString("hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture) ?? "n/a";
            Console.WriteLine(
                $"Result:            OK ({segmentCount} segments, {tokenCount} tokens, first={firstStartText}, last={lastEndText}, {sw.Elapsed})");

            if (!string.IsNullOrWhiteSpace(options.OutJsonPath))
            {
                var output = BuildAsrOutput(Path.GetFileName(modelPath) ?? "whisper", segments);
                var outputPath = Path.GetFullPath(options.OutJsonPath);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                WriteAsrOutputJson(outputPath, output);
                Console.WriteLine($"Output JSON:       {outputPath}");
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"Result:            FAIL after {sw.Elapsed}");
            Console.WriteLine(ex.ToString());
            Environment.ExitCode = 1;
            break;
        }

        Console.WriteLine();
    }

    totalStopwatch.Stop();
    Console.WriteLine($"Total elapsed:     {totalStopwatch.Elapsed}");
}
catch (Exception ex)
{
    Console.Error.WriteLine("FATAL: dtw-isolation terminated due to an unhandled exception.");
    Console.Error.WriteLine(ex.ToString());
    if (args.Length > 0)
    {
        Console.Error.WriteLine("Args:");
        foreach (var arg in args)
        {
            Console.Error.WriteLine($"  {arg}");
        }
    }

    Environment.ExitCode = 1;
}

static async Task<List<SegmentData>> ProcessSegmentsAsync(
    WhisperFactory factory,
    Options options,
    string audioPath,
    CancellationToken cancellationToken)
{
    var chunkSeconds = !float.IsNaN(options.ChunkSeconds) ? options.ChunkSeconds : 0f;
    if (chunkSeconds <= 0f)
    {
        var offsetSeconds = !float.IsNaN(options.OffsetSeconds) && options.OffsetSeconds > 0f ? options.OffsetSeconds : (float?)null;
        var durationSeconds = !float.IsNaN(options.DurationSeconds) && options.DurationSeconds > 0f
            ? options.DurationSeconds
            : (float?)null;
        return await ProcessWindowAsync(factory, options, audioPath, offsetSeconds, durationSeconds, cancellationToken)
            .ConfigureAwait(false);
    }

    var overlapSeconds = !float.IsNaN(options.ChunkOverlapSeconds) ? Math.Max(0f, options.ChunkOverlapSeconds) : 0f;
    overlapSeconds = Math.Min(overlapSeconds, Math.Max(0f, chunkSeconds - 0.01f));
    var stepSeconds = Math.Max(0.01f, chunkSeconds - overlapSeconds);

    double decodeStart = !float.IsNaN(options.OffsetSeconds) && options.OffsetSeconds > 0f ? options.OffsetSeconds : 0d;
    var totalDuration = GetAudioDurationSeconds(audioPath);
    double decodeEnd = totalDuration;
    if (!float.IsNaN(options.DurationSeconds) && options.DurationSeconds > 0f)
    {
        decodeEnd = Math.Min(totalDuration, decodeStart + options.DurationSeconds);
    }

    if (decodeEnd <= decodeStart)
    {
        throw new ArgumentException(
            $"Invalid chunk decode bounds. offset={decodeStart:F3}s, end={decodeEnd:F3}s, audio={totalDuration:F3}s.");
    }

    var result = new List<SegmentData>();
    var chunkIndex = 0;
    for (double chunkStart = decodeStart; chunkStart < decodeEnd - 1e-6; chunkStart += stepSeconds)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var chunkDuration = Math.Min(chunkSeconds, decodeEnd - chunkStart);
        if (chunkDuration <= 0.01)
        {
            break;
        }

        var chunkSegments = await ProcessWindowAsync(
                factory,
                options,
                audioPath,
                (float)chunkStart,
                (float)chunkDuration,
                cancellationToken)
            .ConfigureAwait(false);

        // Remove overlap duplicates by trimming early overlap region on every chunk except the first.
        var keepAfter = chunkIndex == 0 ? decodeStart : chunkStart + overlapSeconds * 0.5;
        foreach (var segment in chunkSegments)
        {
            var start = segment.Start.TotalSeconds;
            var end = segment.End.TotalSeconds;
            if (end <= keepAfter + 1e-6)
            {
                continue;
            }

            if (start >= decodeEnd + 1e-6)
            {
                continue;
            }

            result.Add(segment);
        }

        chunkIndex++;
        if (chunkStart + chunkDuration >= decodeEnd - 1e-6)
        {
            break;
        }
    }

    return result
        .OrderBy(s => s.Start)
        .ThenBy(s => s.End)
        .ToList();
}

static async Task<List<SegmentData>> ProcessWindowAsync(
    WhisperFactory factory,
    Options options,
    string audioPath,
    float? offsetSeconds,
    float? durationSeconds,
    CancellationToken cancellationToken)
{
    var builder = CreateConfiguredBuilder(factory, options);

    if (offsetSeconds is > 0f)
    {
        builder.WithOffset(TimeSpan.FromSeconds(offsetSeconds.Value));
    }

    if (durationSeconds is > 0f)
    {
        builder.WithDuration(TimeSpan.FromSeconds(durationSeconds.Value));
    }

    await using var processor = builder.Build();
    await using var stream = File.OpenRead(audioPath);

    var inputMode = (options.InputMode ?? "stream").Trim().ToLowerInvariant();
    if (inputMode is not ("stream" or "samples"))
    {
        throw new ArgumentException($"Unknown input mode '{options.InputMode}'. Use stream|samples.");
    }

    var segments = new List<SegmentData>();
    if (inputMode == "samples")
    {
        var parser = new WaveParser(stream);
        var samples = parser.GetAvgSamples();
        await foreach (var segment in processor.ProcessAsync(samples, cancellationToken).ConfigureAwait(false))
        {
            segments.Add(segment);
        }
    }
    else
    {
        await foreach (var segment in processor.ProcessAsync(stream, cancellationToken).ConfigureAwait(false))
        {
            segments.Add(segment);
        }
    }

    return segments;
}

static WhisperProcessorBuilder CreateConfiguredBuilder(WhisperFactory factory, Options options)
{
    var builder = factory.CreateBuilder();
    builder.WithThreads(options.Threads > 0 ? options.Threads : Environment.ProcessorCount);

    if (options.WordTimestamps)
    {
        builder.WithTokenTimestamps();
        if (options.SplitOnWord)
        {
            builder.SplitOnWord();
        }
    }

    if (string.Equals(options.Language, "auto", StringComparison.OrdinalIgnoreCase))
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

    if (options.NoContext)
    {
        builder.WithNoContext();
    }

    if (!float.IsNaN(options.NoSpeechThreshold))
    {
        builder.WithNoSpeechThreshold(options.NoSpeechThreshold);
    }

    if (options.MaxLastTextTokens >= 0)
    {
        builder.WithMaxLastTextTokens(options.MaxLastTextTokens);
    }

    if (options.BeamSize > 1)
    {
        if (builder.WithBeamSearchSamplingStrategy() is BeamSearchSamplingStrategyBuilder beam)
        {
            beam.WithBeamSize(options.BeamSize);
        }
    }
    else if (options.BestOf > 1)
    {
        if (builder.WithGreedySamplingStrategy() is GreedySamplingStrategyBuilder greedy)
        {
            greedy.WithBestOf(options.BestOf);
        }
    }

    return builder;
}

static double GetAudioDurationSeconds(string audioPath)
{
    using var stream = File.OpenRead(audioPath);
    var parser = new WaveParser(stream);
    var samples = parser.GetAvgSamples();
    if (parser.SampleRate == 0 || samples.Length == 0)
    {
        return 0d;
    }

    return samples.Length / (double)parser.SampleRate;
}

static void WriteAsrOutputJson(string outputPath, AsrOutputDocument output)
{
    using var stream = File.Create(outputPath);
    using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
    {
        Indented = true
    });

    writer.WriteStartObject();
    writer.WriteString("modelVersion", output.modelVersion);

    writer.WritePropertyName("tokens");
    writer.WriteStartArray();
    foreach (var token in output.tokens)
    {
        writer.WriteStartObject();
        writer.WriteNumber("t", token.t);
        writer.WriteNumber("d", token.d);
        writer.WriteString("w", token.w);
        writer.WriteEndObject();
    }

    writer.WriteEndArray();

    writer.WritePropertyName("segments");
    writer.WriteStartArray();
    foreach (var segment in output.segments)
    {
        writer.WriteStartObject();
        writer.WriteNumber("startSec", segment.startSec);
        writer.WriteNumber("endSec", segment.endSec);
        writer.WriteString("text", segment.text);
        writer.WriteEndObject();
    }

    writer.WriteEndArray();
    writer.WriteEndObject();
    writer.Flush();
}

static AsrOutputDocument BuildAsrOutput(string modelVersion, List<SegmentData> segments)
{
    var outputSegments = segments
        .Select(segment => new AsrOutputSegment(
            segment.Start.TotalSeconds,
            segment.End.TotalSeconds,
            segment.Text?.Trim() ?? string.Empty))
        .ToArray();

    var outputTokens = AggregateTokens(segments)
        .Select(token => new AsrOutputToken(token.Start, token.Duration, token.Word))
        .ToArray();

    return new AsrOutputDocument(modelVersion, outputTokens, outputSegments);
}

static List<WordToken> AggregateTokens(IEnumerable<SegmentData> segments)
{
    var allRawTokens = segments
        .Where(segment => segment.Tokens is { Length: > 0 })
        .SelectMany(segment => segment.Tokens!)
        .OrderBy(token => token.Start)
        .ThenBy(token => token.End)
        .ToArray();

    var result = new List<WordToken>();
    var builder = new StringBuilder();
    double wordStart = 0;
    double wordEnd = 0;

    foreach (var token in allRawTokens)
    {
        var raw = token.Text;
        if (string.IsNullOrEmpty(raw))
        {
            continue;
        }

        if (raw.StartsWith('[') && raw.EndsWith(']'))
        {
            continue;
        }

        var normalized = raw
            .Replace("▁", " ")
            .Replace("Ġ", " ")
            .Replace("Ċ", " ")
            .Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            continue;
        }

        var hasBoundary = char.IsWhiteSpace(raw[0]) || raw[0] == '▁' || raw[0] == 'Ġ' || raw[0] == 'Ċ';
        var tokenStart = token.Start / 100.0;
        var tokenEnd = Math.Max(tokenStart, token.End / 100.0);

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
        result.Add(new WordToken(wordStart, duration, builder.ToString()));
        builder.Clear();
        wordStart = 0;
        wordEnd = 0;
    }
}

static async Task<(string ModelPath, WhisperAlignmentHeadsPreset? HeadsPreset)> ResolveModelPathAsync(
    Options options,
    string cacheDir)
{
    if (!string.IsNullOrWhiteSpace(options.ModelPath))
    {
        var full = Path.GetFullPath(options.ModelPath);
        if (!File.Exists(full))
        {
            throw new FileNotFoundException($"Model path does not exist: {full}", full);
        }

        return (full, InferHeadsPresetFromModelPath(full));
    }

    var modelType = ParseModelAlias(options.ModelAlias);
    var modelFile = GetDefaultModelFileName(modelType);
    var target = Path.Combine(cacheDir, "models", modelFile);
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);

    if (File.Exists(target))
    {
        return (target, HeadsPresetFromModelType(modelType));
    }

    Console.WriteLine($"Downloading model {modelType} to {target}");
    await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(modelType).ConfigureAwait(false);
    await using var output = File.Create(target);
    await modelStream.CopyToAsync(output).ConfigureAwait(false);
    return (target, HeadsPresetFromModelType(modelType));
}

static async Task<string> ResolveAudioPathAsync(Options options, string cacheDir)
{
    if (!string.IsNullOrWhiteSpace(options.AudioPath))
    {
        var full = Path.GetFullPath(options.AudioPath);
        if (!File.Exists(full))
        {
            throw new FileNotFoundException($"Audio path does not exist: {full}", full);
        }

        return full;
    }

    var target = Path.Combine(cacheDir, "audio", "kennedy.wav");
    if (File.Exists(target))
    {
        return target;
    }

    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
    const string sampleUrl = "https://raw.githubusercontent.com/sandrohanea/whisper.net/1.9.0/examples/TestData/kennedy.wav";
    Console.WriteLine($"Downloading sample audio to {target}");
    using var client = new HttpClient();
    await using var source = await client.GetStreamAsync(sampleUrl).ConfigureAwait(false);
    await using var output = File.Create(target);
    await source.CopyToAsync(output).ConfigureAwait(false);
    return target;
}

static void ConfigureRuntimeOrder(string mode)
{
    switch (mode.ToLowerInvariant())
    {
        case "auto":
            break;
        case "cuda":
            RuntimeOptions.RuntimeLibraryOrder = [RuntimeLibrary.Cuda, RuntimeLibrary.Cpu, RuntimeLibrary.CpuNoAvx];
            break;
        case "cpu":
            RuntimeOptions.RuntimeLibraryOrder = [RuntimeLibrary.Cpu, RuntimeLibrary.CpuNoAvx];
            break;
        default:
            throw new ArgumentException($"Unknown runtime mode '{mode}'. Use auto|cuda|cpu.");
    }
}

static WhisperAlignmentHeadsPreset? ResolveHeadsPresetOption(
    string mode,
    WhisperAlignmentHeadsPreset? inferredPreset)
{
    var normalized = mode.Trim().ToLowerInvariant();
    if (normalized == "auto")
    {
        return inferredPreset;
    }

    if (normalized == "none")
    {
        return null;
    }

    return normalized switch
    {
        "tinyen" or "tiny.en" => WhisperAlignmentHeadsPreset.TinyEn,
        "tiny" => WhisperAlignmentHeadsPreset.Tiny,
        "baseen" or "base.en" => WhisperAlignmentHeadsPreset.BaseEn,
        "base" => WhisperAlignmentHeadsPreset.Base,
        "smallen" or "small.en" => WhisperAlignmentHeadsPreset.SmallEn,
        "small" => WhisperAlignmentHeadsPreset.Small,
        "mediumen" or "medium.en" => WhisperAlignmentHeadsPreset.MediumEn,
        "medium" => WhisperAlignmentHeadsPreset.Medium,
        "largev1" or "large-v1" => WhisperAlignmentHeadsPreset.LargeV1,
        "largev2" or "large-v2" => WhisperAlignmentHeadsPreset.LargeV2,
        "largev3" or "large-v3" => WhisperAlignmentHeadsPreset.LargeV3,
        "largev3turbo" or "large-v3-turbo" => WhisperAlignmentHeadsPreset.LargeV3Turbo,
        "ntopmost" => WhisperAlignmentHeadsPreset.NTopMost,
        _ => throw new ArgumentException(
            $"Unknown heads preset '{mode}'. Use auto|none|tiny.en|tiny|base.en|base|small.en|small|medium.en|medium|large-v1|large-v2|large-v3|large-v3-turbo|ntopmost.")
    };
}

static WhisperAlignmentHeadsPreset? HeadsPresetFromModelType(GgmlType modelType)
{
    return modelType switch
    {
        GgmlType.Tiny => WhisperAlignmentHeadsPreset.Tiny,
        GgmlType.TinyEn => WhisperAlignmentHeadsPreset.TinyEn,
        GgmlType.Base => WhisperAlignmentHeadsPreset.Base,
        GgmlType.BaseEn => WhisperAlignmentHeadsPreset.BaseEn,
        GgmlType.Small => WhisperAlignmentHeadsPreset.Small,
        GgmlType.SmallEn => WhisperAlignmentHeadsPreset.SmallEn,
        GgmlType.Medium => WhisperAlignmentHeadsPreset.Medium,
        GgmlType.MediumEn => WhisperAlignmentHeadsPreset.MediumEn,
        GgmlType.LargeV1 => WhisperAlignmentHeadsPreset.LargeV1,
        GgmlType.LargeV2 => WhisperAlignmentHeadsPreset.LargeV2,
        GgmlType.LargeV3 => WhisperAlignmentHeadsPreset.LargeV3,
        GgmlType.LargeV3Turbo => WhisperAlignmentHeadsPreset.LargeV3Turbo,
        _ => null
    };
}

static WhisperAlignmentHeadsPreset? InferHeadsPresetFromModelPath(string modelPath)
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

    if (name.Contains("medium.en", StringComparison.Ordinal) || name.Contains("medium-en", StringComparison.Ordinal))
    {
        return WhisperAlignmentHeadsPreset.MediumEn;
    }

    if (name.Contains("medium", StringComparison.Ordinal))
    {
        return WhisperAlignmentHeadsPreset.Medium;
    }

    if (name.Contains("small.en", StringComparison.Ordinal) || name.Contains("small-en", StringComparison.Ordinal))
    {
        return WhisperAlignmentHeadsPreset.SmallEn;
    }

    if (name.Contains("small", StringComparison.Ordinal))
    {
        return WhisperAlignmentHeadsPreset.Small;
    }

    if (name.Contains("base.en", StringComparison.Ordinal) || name.Contains("base-en", StringComparison.Ordinal))
    {
        return WhisperAlignmentHeadsPreset.BaseEn;
    }

    if (name.Contains("base", StringComparison.Ordinal))
    {
        return WhisperAlignmentHeadsPreset.Base;
    }

    if (name.Contains("tiny.en", StringComparison.Ordinal) || name.Contains("tiny-en", StringComparison.Ordinal))
    {
        return WhisperAlignmentHeadsPreset.TinyEn;
    }

    if (name.Contains("tiny", StringComparison.Ordinal))
    {
        return WhisperAlignmentHeadsPreset.Tiny;
    }

    return null;
}

static GgmlType ParseModelAlias(string alias)
{
    return alias.Trim().ToLowerInvariant() switch
    {
        "tiny" => GgmlType.Tiny,
        "tiny.en" => GgmlType.TinyEn,
        "base" => GgmlType.Base,
        "base.en" => GgmlType.BaseEn,
        "small" => GgmlType.Small,
        "small.en" => GgmlType.SmallEn,
        "medium" => GgmlType.Medium,
        "medium.en" => GgmlType.MediumEn,
        "large" or "large-v1" => GgmlType.LargeV1,
        "large-v2" => GgmlType.LargeV2,
        "large-v3" => GgmlType.LargeV3,
        "large-v3-turbo" => GgmlType.LargeV3Turbo,
        _ => throw new ArgumentException($"Unsupported model alias '{alias}'.")
    };
}

static string GetDefaultModelFileName(GgmlType type)
{
    var suffix = type switch
    {
        GgmlType.Tiny => "tiny",
        GgmlType.TinyEn => "tiny.en",
        GgmlType.Base => "base",
        GgmlType.BaseEn => "base.en",
        GgmlType.Small => "small",
        GgmlType.SmallEn => "small.en",
        GgmlType.Medium => "medium",
        GgmlType.MediumEn => "medium.en",
        GgmlType.LargeV1 => "large-v1",
        GgmlType.LargeV2 => "large-v2",
        GgmlType.LargeV3 => "large-v3",
        GgmlType.LargeV3Turbo => "large-v3-turbo",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported model type")
    };

    return $"ggml-{suffix}.bin";
}

static void PrintUsage()
{
    Console.WriteLine("""
Usage:
  dotnet10 run tools/dtw-isolation.cs -- [options]

Options:
  --audio <path>            Optional WAV/audio path. Defaults to downloaded Kennedy sample.
  --model-path <path>       Optional local model path.
  --model <alias>           Model alias when --model-path is omitted. Default: tiny.en
  --iterations <n>          Number of sequential transcriptions. Default: 1
  --runtime <auto|cuda|cpu> Runtime order preset. Default: cuda
  --use-gpu <true|false>    Use GPU in factory options. Default: true
  --gpu-device <n>          GPU device index. Default: 0
  --dtw <true|false>        Enable DTW timestamps. Default: true
  --heads <mode>            DTW alignment heads preset. Default: auto
                            Modes: auto|none|tiny.en|tiny|base.en|base|small.en|small|medium.en|medium|large-v1|large-v2|large-v3|large-v3-turbo|ntopmost
  --word-ts <true|false>    Enable token/word timestamps. Default: true
  --split-on-word <bool>    Split decoded tokens on word boundaries. Default: true
  --flash <true|false>      Enable flash attention. Default: false
  --threads <n>             Worker threads (0=auto). Default: 0
  --beam <n>                Beam size. Default: 8
  --best-of <n>             Best-of for greedy mode. Default: 1
  --temperature <float>     Sampling temperature. Default: 0
  --language <code|auto>    Default: en
  --input-mode <mode>       Audio input mode. stream|samples. Default: stream
  --no-context <bool>       Disable prompt carry-over between windows. Default: false
  --no-speech-thr <float>   Override no-speech threshold. Default: unset
  --max-last-text <n>       Override max last text tokens (-1 = unset). Default: -1
  --offset-sec <float>      Override decode start offset seconds. Default: unset
  --duration-sec <float>    Override decode duration seconds. Default: unset
  --chunk-sec <float>       Decode in fixed windows (seconds). Default: unset
  --chunk-overlap-sec <f>   Overlap between chunks (seconds). Default: 2
  --out-json <path>         Optional output ASR JSON path.
  --quiet-segments <bool>   Suppress per-segment text output. Default: false
  --verbose <true|false>    Whisper debug logging. Default: true
""");
}

static string FormatOptionalFloat(float value)
    => float.IsNaN(value) ? "unset" : value.ToString(CultureInfo.InvariantCulture);

internal sealed record AsrOutputDocument(
    string modelVersion,
    AsrOutputToken[] tokens,
    AsrOutputSegment[] segments);

internal sealed record AsrOutputToken(
    double t,
    double d,
    string w);

internal sealed record AsrOutputSegment(
    double startSec,
    double endSec,
    string text);

internal sealed record WordToken(
    double Start,
    double Duration,
    string Word);

internal sealed record Options(
    string? AudioPath,
    string? ModelPath,
    string ModelAlias,
    int Iterations,
    string RuntimeMode,
    bool UseGpu,
    int GpuDevice,
    bool Dtw,
    string HeadsPresetMode,
    bool WordTimestamps,
    bool SplitOnWord,
    bool FlashAttention,
    int Threads,
    int BeamSize,
    int BestOf,
    float Temperature,
    string Language,
    string InputMode,
    bool NoContext,
    float NoSpeechThreshold,
    int MaxLastTextTokens,
    float OffsetSeconds,
    float DurationSeconds,
    float ChunkSeconds,
    float ChunkOverlapSeconds,
    string? OutJsonPath,
    bool QuietSegments,
    bool Verbose)
{
    public static Options Parse(string[] args)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            var key = args[i];
            if (!key.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unexpected argument '{key}'. Use --help for usage.");
            }

            key = key[2..];
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                var valueParts = new List<string>();
                while (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    valueParts.Add(args[++i]);
                }

                map[key] = string.Join(" ", valueParts);
            }
            else
            {
                map[key] = "true";
            }
        }

        return new Options(
            AudioPath: GetString(map, "audio"),
            ModelPath: GetString(map, "model-path"),
            ModelAlias: GetString(map, "model") ?? "tiny.en",
            Iterations: GetInt(map, "iterations", 1),
            RuntimeMode: GetString(map, "runtime") ?? "cuda",
            UseGpu: GetBool(map, "use-gpu", true),
            GpuDevice: GetInt(map, "gpu-device", 0),
            Dtw: GetBool(map, "dtw", true),
            HeadsPresetMode: GetString(map, "heads") ?? "auto",
            WordTimestamps: GetBool(map, "word-ts", true),
            SplitOnWord: GetBool(map, "split-on-word", true),
            FlashAttention: GetBool(map, "flash", false),
            Threads: GetInt(map, "threads", 0),
            BeamSize: GetInt(map, "beam", 8),
            BestOf: GetInt(map, "best-of", 1),
            Temperature: GetFloat(map, "temperature", 0f),
            Language: GetString(map, "language") ?? "en",
            InputMode: GetString(map, "input-mode") ?? "stream",
            NoContext: GetBool(map, "no-context", false),
            NoSpeechThreshold: GetOptionalFloat(map, "no-speech-thr"),
            MaxLastTextTokens: GetInt(map, "max-last-text", -1),
            OffsetSeconds: GetOptionalFloat(map, "offset-sec"),
            DurationSeconds: GetOptionalFloat(map, "duration-sec"),
            ChunkSeconds: GetOptionalFloat(map, "chunk-sec"),
            ChunkOverlapSeconds: GetFloat(map, "chunk-overlap-sec", 2f),
            OutJsonPath: GetString(map, "out-json"),
            QuietSegments: GetBool(map, "quiet-segments", false),
            Verbose: GetBool(map, "verbose", true));
    }

    private static string? GetString(Dictionary<string, string> map, string key)
        => map.TryGetValue(key, out var value) ? value : null;

    private static bool GetBool(Dictionary<string, string> map, string key, bool defaultValue)
    {
        if (!map.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        return value.ToLowerInvariant() switch
        {
            "1" or "true" or "yes" or "y" => true,
            "0" or "false" or "no" or "n" => false,
            _ => throw new ArgumentException($"Option --{key} expects a boolean value, got '{value}'.")
        };
    }

    private static int GetInt(Dictionary<string, string> map, string key, int defaultValue)
    {
        if (!map.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new ArgumentException($"Option --{key} expects an integer value, got '{value}'.");
        }

        return parsed;
    }

    private static float GetFloat(Dictionary<string, string> map, string key, float defaultValue)
    {
        if (!map.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new ArgumentException($"Option --{key} expects a float value, got '{value}'.");
        }

        return parsed;
    }

    private static float GetOptionalFloat(Dictionary<string, string> map, string key)
    {
        if (!map.TryGetValue(key, out var value))
        {
            return float.NaN;
        }

        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new ArgumentException($"Option --{key} expects a float value, got '{value}'.");
        }

        return parsed;
    }
}
