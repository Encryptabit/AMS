using System.CommandLine;
using System.IO;
using System.Text.Json;
using Ams.Core.Asr;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Core.Services;
using Ams.Cli.Utilities;
using Whisper.net;
using Whisper.net.Ggml;

namespace Ams.Cli.Commands;

public static class AsrCommand
{
    internal const string DefaultServiceUrl = "http://localhost:8000";

    public static Command Create()
    {
        var asrCommand = new Command("asr", "ASR (Automatic Speech Recognition) operations");

        var runCommand = new Command("run", "Run ASR on an audio file");

        var audioOption = new Option<FileInfo?>("--audio", "Path to the audio file (any FFmpeg-readable format)");
        audioOption.AddAlias("-a");

        var outputOption = new Option<FileInfo?>("--out", "Output ASR JSON file");
        outputOption.AddAlias("-o");

        var engineOption = new Option<string>("--engine", () => "whisper", "ASR engine to use (whisper or nemo)");
        engineOption.AddAlias("-e");

        var serviceUrlOption = new Option<string>("--service", () => DefaultServiceUrl, "ASR service URL (Nemo engine)");
        serviceUrlOption.AddAlias("-s");

        var modelOption = new Option<string>("--model", "ASR model identifier (Nemo) or fallback model path (Whisper)");
        modelOption.AddAlias("-m");

        var modelPathOption = new Option<FileInfo?>("--model-path", "Path to Whisper model file (.gguf/.bin); falls back to --model or AMS_WHISPER_MODEL_PATH");

        var languageOption = new Option<string>("--language", () => "en", "Language code (use \"auto\" for detection)");
        languageOption.AddAlias("-l");

        var threadsOption = new Option<int>("--threads", () => 0, "Whisper thread count (0 = auto)");
        var useGpuOption = new Option<bool>("--use-gpu", () => true, "Enable GPU acceleration when supported");
        var gpuDeviceOption = new Option<int>("--gpu-device", () => 0, "GPU device index for Whisper");
        var beamSizeOption = new Option<int>("--beam-size", () => 5, "Beam size for Whisper beam search");
        var bestOfOption = new Option<int>("--best-of", () => 1, "Best-of sampling count for Whisper greedy search");
        var temperatureOption = new Option<double>("--temperature", () => 0.0, "Sampling temperature (0-1) for Whisper");
        var wordTimestampsOption = new Option<bool>("--word-timestamps", () => true, "Emit word-level timestamps (Whisper)");
        var flashAttentionOption = new Option<bool>("--flash-attention", () => false, "Enable FlashAttention kernels when building with support");
        var dtwOption = new Option<bool>("--dtw-timestamps", () => false, "Enable DTW timestamp refinement (Whisper)");

        runCommand.AddOption(audioOption);
        runCommand.AddOption(outputOption);
        runCommand.AddOption(engineOption);
        runCommand.AddOption(serviceUrlOption);
        runCommand.AddOption(modelOption);
        runCommand.AddOption(modelPathOption);
        runCommand.AddOption(languageOption);
        runCommand.AddOption(threadsOption);
        runCommand.AddOption(useGpuOption);
        runCommand.AddOption(gpuDeviceOption);
        runCommand.AddOption(beamSizeOption);
        runCommand.AddOption(bestOfOption);
        runCommand.AddOption(temperatureOption);
        runCommand.AddOption(wordTimestampsOption);
        runCommand.AddOption(flashAttentionOption);
        runCommand.AddOption(dtwOption);

        runCommand.SetHandler(async context =>
        {
            try
            {
                var parse = context.ParseResult;

                var audio = parse.GetValueForOption(audioOption);
                var output = parse.GetValueForOption(outputOption);
                var engineText = parse.GetValueForOption(engineOption);
                var serviceUrl = parse.GetValueForOption(serviceUrlOption) ?? DefaultServiceUrl;
                var model = parse.GetValueForOption(modelOption);
                var modelPath = parse.GetValueForOption(modelPathOption);
                var language = parse.GetValueForOption(languageOption) ?? "en";
                var threads = parse.GetValueForOption(threadsOption);
                var useGpu = parse.GetValueForOption(useGpuOption);
                var gpuDevice = parse.GetValueForOption(gpuDeviceOption);
                var beamSize = parse.GetValueForOption(beamSizeOption);
                var bestOf = parse.GetValueForOption(bestOfOption);
                var temperature = parse.GetValueForOption(temperatureOption);
                var wordTimestamps = parse.GetValueForOption(wordTimestampsOption);
                var flashAttention = parse.GetValueForOption(flashAttentionOption);
                var dtwTimestamps = parse.GetValueForOption(dtwOption);

                var audioFile = CommandInputResolver.RequireAudio(audio);
                var outputFile = CommandInputResolver.ResolveOutput(output, "asr.json");
                var engine = AsrEngineConfig.Resolve(engineText);

                await RunAsrAsync(
                    audioFile,
                    outputFile,
                    engine,
                    serviceUrl,
                    model,
                    modelPath,
                    language,
                    threads,
                    useGpu,
                    gpuDevice,
                    beamSize,
                    bestOf,
                    temperature,
                    wordTimestamps,
                    flashAttention,
                    dtwTimestamps);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "asr run command failed");
                context.ExitCode = 1;
            }
        });

        asrCommand.AddCommand(runCommand);
        return asrCommand;
    }

    internal static async Task RunAsrAsync(
        FileInfo audioFile,
        FileInfo outputFile,
        AsrEngine engine,
        string serviceUrl,
        string? model,
        FileInfo? modelPath,
        string language,
        int threads,
        bool useGpu,
        int gpuDevice,
        int beamSize,
        int bestOf,
        double temperature,
        bool wordTimestamps,
        bool flashAttention,
        bool dtwTimestamps)
    {
        if (!audioFile.Exists)
        {
            throw new FileNotFoundException($"Audio file not found: {audioFile.FullName}");
        }

        serviceUrl ??= DefaultServiceUrl;
        language ??= "en";

        Log.Debug("Running ASR for {AudioFile} -> {OutputFile} via engine {Engine}", audioFile.FullName, outputFile.FullName, engine);

        if (engine == AsrEngine.Nemo)
        {
            await RunNemoAsync(audioFile, outputFile, serviceUrl, model, language);
            return;
        }

        await RunWhisperAsync(
            audioFile,
            outputFile,
            model,
            modelPath,
            language,
            threads,
            useGpu,
            gpuDevice,
            beamSize,
            bestOf,
            temperature,
            wordTimestamps,
            flashAttention,
            dtwTimestamps);
    }

    internal static Task RunAsrAsync(FileInfo audioFile, FileInfo outputFile, string serviceUrl, string? model, string language)
    {
        var engine = AsrEngineConfig.Resolve();
        FileInfo? modelPath = null;
        if (!string.IsNullOrWhiteSpace(model) && File.Exists(model))
        {
            modelPath = new FileInfo(model);
        }

        return RunAsrAsync(
            audioFile,
            outputFile,
            engine,
            serviceUrl,
            model,
            modelPath,
            language,
            threads: 0,
            useGpu: true,
            gpuDevice: 0,
            beamSize: 5,
            bestOf: 1,
            temperature: 0.0,
            wordTimestamps: true,
            flashAttention: false,
            dtwTimestamps: false);
    }

    private static async Task RunWhisperAsync(
        FileInfo audioFile,
        FileInfo outputFile,
        string? model,
        FileInfo? modelPath,
        string language,
        int threads,
        bool useGpu,
        int gpuDevice,
        int beamSize,
        int bestOf,
        double temperature,
        bool wordTimestamps,
        bool flashAttention,
        bool dtwTimestamps)
    {
        var resolvedModelPath = await ResolveWhisperModelPathAsync(model, modelPath).ConfigureAwait(false);
        Log.Debug("Whisper model path resolved to {ModelPath}", resolvedModelPath);

        var options = new AsrOptions(
            ModelPath: resolvedModelPath,
            Language: language,
            Threads: Math.Max(0, threads),
            UseGpu: useGpu,
            EnableWordTimestamps: wordTimestamps,
            BeamSize: Math.Max(1, beamSize),
            BestOf: Math.Max(1, bestOf),
            Temperature: (float)Math.Clamp(temperature, 0.0, 1.0),
            NoSpeechBoost: true,
            GpuDevice: gpuDevice,
            UseFlashAttention: flashAttention,
            UseDtwTimestamps: dtwTimestamps);

        var service = new AsrService();

        Log.Debug("Submitting audio to Whisper.NET (threads={Threads}, gpu={UseGpu}, beam={BeamSize}, bestOf={BestOf})",
            options.Threads,
            options.UseGpu,
            options.BeamSize,
            options.BestOf);

        var response = await service.TranscribeFileAsync(audioFile.FullName, options, CancellationToken.None);

        await WriteResponseAsync(outputFile, response);
        Log.Debug("ASR summary: ModelVersion={ModelVersion}, Tokens={TokenCount}", response.ModelVersion, response.Tokens.Length);
    }

    private static async Task<string> ResolveWhisperModelPathAsync(string? modelOption, FileInfo? modelPath)
    {
        if (modelPath is not null)
        {
            var fullPath = Path.GetFullPath(modelPath.FullName);
            if (!File.Exists(fullPath))
            {
                var type = ParseModelAlias(modelOption) ?? ParseModelAlias(Path.GetFileName(fullPath)) ?? GgmlType.Base;
                await DownloadModelIfMissingAsync(fullPath, type).ConfigureAwait(false);
            }
            return fullPath;
        }

        if (!string.IsNullOrWhiteSpace(modelOption))
        {
            var trimmed = modelOption.Trim();
            if (File.Exists(trimmed))
            {
                return Path.GetFullPath(trimmed);
            }

            if (TryParseModelAlias(trimmed, out var aliasType))
            {
                return await DownloadModelIfMissingAsync(null, aliasType).ConfigureAwait(false);
            }
        }

        var envModel = Environment.GetEnvironmentVariable(AsrEngineConfig.WhisperModelPathEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(envModel))
        {
            var envPath = Path.GetFullPath(envModel);
            if (File.Exists(envPath))
            {
                return envPath;
            }

            var envType = ParseModelAlias(Path.GetFileName(envPath)) ?? ParseModelAlias(envModel) ?? GgmlType.Base;
            await DownloadModelIfMissingAsync(envPath, envType).ConfigureAwait(false);
            return envPath;
        }

        return await DownloadModelIfMissingAsync(null, GgmlType.Base).ConfigureAwait(false);
    }

    private static bool TryParseModelAlias(string? value, out GgmlType type)
    {
        type = GgmlType.Base;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.EndsWith(".bin", StringComparison.Ordinal))
        {
            normalized = normalized[..^4];
        }
        if (normalized.StartsWith("ggml-", StringComparison.Ordinal))
        {
            normalized = normalized[5..];
        }
        normalized = normalized.Replace('_', '-');

        switch (normalized)
        {
            case "tiny":
                type = GgmlType.Tiny;
                return true;
            case "tiny-en":
            case "tiny.en":
                type = GgmlType.TinyEn;
                return true;
            case "base":
                type = GgmlType.Base;
                return true;
            case "base-en":
            case "base.en":
                type = GgmlType.BaseEn;
                return true;
            case "small":
                type = GgmlType.Small;
                return true;
            case "small-en":
            case "small.en":
                type = GgmlType.SmallEn;
                return true;
            case "medium":
                type = GgmlType.Medium;
                return true;
            case "medium-en":
            case "medium.en":
                type = GgmlType.MediumEn;
                return true;
            case "large":
            case "large-v3":
            case "large-v3-q5_0": // common alias
                type = GgmlType.LargeV3;
                return true;
            case "large-v1":
                type = GgmlType.LargeV1;
                return true;
            case "large-v2":
                type = GgmlType.LargeV2;
                return true;
            case "large-v3-en":
            case "large-v3-en-q5_0":
                type = GgmlType.LargeV3;
                return true;
            default:
                return false;
        }
    }

    private static GgmlType? ParseModelAlias(string? value)
    {
        return TryParseModelAlias(value, out var type) ? type : null;
    }

    private static async Task<string> DownloadModelIfMissingAsync(string? destinationPath, GgmlType type)
    {
        var fileName = GetDefaultModelFileName(type);
        var targetPath = destinationPath ?? Path.Combine(AppContext.BaseDirectory, "models", fileName);
        targetPath = Path.GetFullPath(targetPath);

        if (File.Exists(targetPath))
        {
            Log.Debug("Using cached Whisper model at {ModelPath}", targetPath);
            return targetPath;
        }

        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Log.Info("Downloading Whisper model {ModelType} to {ModelPath}", type, targetPath);
        using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(type).ConfigureAwait(false);
        using var fileWriter = File.Open(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await modelStream.CopyToAsync(fileWriter).ConfigureAwait(false);
        Log.Info("Whisper model ready at {ModelPath}", targetPath);

        return targetPath;
    }

    private static string GetDefaultModelFileName(GgmlType type) =>
        type switch
        {
            GgmlType.Tiny => "ggml-tiny.bin",
            GgmlType.TinyEn => "ggml-tiny.en.bin",
            GgmlType.Base => "ggml-base.bin",
            GgmlType.BaseEn => "ggml-base.en.bin",
            GgmlType.Small => "ggml-small.bin",
            GgmlType.SmallEn => "ggml-small.en.bin",
            GgmlType.Medium => "ggml-medium.bin",
            GgmlType.MediumEn => "ggml-medium.en.bin",
            GgmlType.LargeV1 => "ggml-large-v1.bin",
            GgmlType.LargeV2 => "ggml-large-v2.bin",
            GgmlType.LargeV3 => "ggml-large-v3.bin",
            _ => "ggml-base.bin"
        };

    private static async Task RunNemoAsync(FileInfo audioFile, FileInfo outputFile, string serviceUrl, string? model, string language)
    {
        Log.Debug("Nemo service URL: {ServiceUrl}", serviceUrl);
        Log.Debug("ASR parameters: Language={Language}, Model={Model}", language, model ?? "(default)");

        await AsrProcessSupervisor.EnsureServiceReadyAsync(serviceUrl, CancellationToken.None);

        using var client = new AsrClient(serviceUrl);

        Log.Debug("Checking ASR service health at {ServiceUrl}", serviceUrl);
        var isHealthy = await client.IsHealthyAsync();
        if (!isHealthy)
        {
            throw new InvalidOperationException($"ASR service at {serviceUrl} is not healthy or unreachable");
        }

        Log.Debug("Submitting audio for transcription");
        var response = await client.TranscribeAsync(audioFile.FullName, model, language);
        Log.Debug("Transcription complete");

        await WriteResponseAsync(outputFile, response);
        Log.Debug("ASR summary: ModelVersion={ModelVersion}, Tokens={TokenCount}", response.ModelVersion, response.Tokens.Length);
    }

    private static async Task WriteResponseAsync(FileInfo outputFile, AsrResponse response)
    {
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await File.WriteAllTextAsync(outputFile.FullName, json);
        Log.Debug("ASR results written to {OutputFile}", outputFile.FullName);
    }
}
