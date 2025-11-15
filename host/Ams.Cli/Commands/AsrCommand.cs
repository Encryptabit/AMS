using System.CommandLine;
using Ams.Cli.Repl;
using Ams.Cli.Utilities;

namespace Ams.Cli.Commands;

public static class AsrCommand
{
    internal const string DefaultServiceUrl = "http://localhost:8000";

    public static Command Create(IChapterContextFactory chapterFactory, GenerateTranscriptCommand transcriptCommand)
    {
        ArgumentNullException.ThrowIfNull(chapterFactory);
        ArgumentNullException.ThrowIfNull(transcriptCommand);

        var asrCommand = new Command("asr", "ASR (Automatic Speech Recognition) operations");

        var runCommand = new Command("run", "Run ASR on an audio file");

        var audioOption = new Option<FileInfo?>("--audio", "Path to the audio file (any FFmpeg-readable format)");
        audioOption.AddAlias("-a");

        var outputOption = new Option<FileInfo?>("--out", "Optional output ASR JSON file");
        outputOption.AddAlias("-o");

        var engineOption = new Option<string>("--engine", () => "whisper", "ASR engine to use (whisper or nemo)");
        engineOption.AddAlias("-e");

        var serviceUrlOption = new Option<string>("--service", () => DefaultServiceUrl, "ASR service URL (Nemo engine)");
        serviceUrlOption.AddAlias("-s");

        var modelOption = new Option<string>("--model",() => "ASR model identifier (Nemo) or fallback model path (Whisper)");
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
        var wordTimestampsOption = new Option<bool>("--word-timestamps", () => false, "Emit word-level timestamps (Whisper)");
        var flashAttentionOption = new Option<bool>("--flash-attention", () => false, "Enable FlashAttention kernels when building with support");
        var dtwOption = new Option<bool>("--dtw-timestamps", () => false, "Enable DTW timestamp refinement (Whisper)");
        var parallelOption = new Option<int>(
            "--parallel",
            () => 0,
            "Process multiple chapters in parallel when using 'mode all' (0 = auto based on CPU cores).");
        parallelOption.AddAlias("-p");

        var bookIndexOption = new Option<FileInfo?>("--book-index", "Path to book-index.json (required for context-aware ASR)");
        var chapterIdOption = new Option<string?>("--chapter-id", "Override chapter identifier (defaults to audio stem or active chapter)");

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
        runCommand.AddOption(bookIndexOption);
        runCommand.AddOption(chapterIdOption);
        runCommand.AddOption(parallelOption);

        runCommand.SetHandler(async context =>
        {
            try
            {
                var parse = context.ParseResult;
                var audio = CommandInputResolver.RequireAudio(parse.GetValueForOption(audioOption));
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
                var requestedParallelism = parse.GetValueForOption(parallelOption);

                if (requestedParallelism > 1 && ReplContext.Current?.RunAllChapters != true)
                {
                    Log.Debug("--parallel ignored because CLI is not running in mode ALL");
                }

                var bookIndexFile = CommandInputResolver.ResolveBookIndex(parse.GetValueForOption(bookIndexOption), mustExist: true);
                var chapterId = parse.GetValueForOption(chapterIdOption) ?? Path.GetFileNameWithoutExtension(audio.Name);

                using var handle = chapterFactory.Create(
                    bookIndexFile: bookIndexFile,
                    audioFile: audio,
                    chapterId: chapterId);

                var engine = AsrEngineConfig.Resolve(engineText);
                var transcriptOptions = new GenerateTranscriptOptions
                {
                    Engine = engine,
                    ServiceUrl = serviceUrl,
                    Model = model,
                    ModelPath = modelPath,
                    Language = language,
                    Threads = threads,
                    UseGpu = useGpu,
                    GpuDevice = gpuDevice,
                    BeamSize = beamSize,
                    BestOf = bestOf,
                    Temperature = temperature,
                    EnableWordTimestamps = wordTimestamps,
                    EnableFlashAttention = flashAttention,
                    EnableDtwTimestamps = dtwTimestamps
                };

                await transcriptCommand.ExecuteAsync(handle.Chapter, transcriptOptions, context.GetCancellationToken()).ConfigureAwait(false);
                handle.Save();

                if (output is not null)
                {
                    var artifact = handle.Chapter.Documents.GetAsrFile()
                                   ?? throw new InvalidOperationException("ASR artifact is not available.");
                    Directory.CreateDirectory(output.Directory?.FullName ?? output.DirectoryName ?? ".");
                    artifact.Refresh();
                    if (!artifact.Exists)
                    {
                        throw new FileNotFoundException($"Artifact not found: {artifact.FullName}");
                    }

                    File.Copy(artifact.FullName, output.FullName, overwrite: true);
                }
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
}
