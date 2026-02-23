using Ams.Core.Artifacts;
using Ams.Core.Application.Processes;
using Ams.Core.Processors;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Interfaces;

namespace Ams.Core.Application.Commands;

public sealed class GenerateTranscriptCommand
{
    private readonly IAsrService _asrService;

    public GenerateTranscriptCommand(IAsrService asrService)
    {
        _asrService = asrService ?? throw new ArgumentNullException(nameof(asrService));
    }

    public async Task ExecuteAsync(
        ChapterContext chapter,
        GenerateTranscriptOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);

        var effectiveOptions = options ?? GenerateTranscriptOptions.Default;
        var engine = effectiveOptions.Engine ?? AsrEngineConfig.Resolve();

        switch (engine)
        {
            case AsrEngine.Nemo:
                await RunNemoAsync(chapter, effectiveOptions, cancellationToken).ConfigureAwait(false);
                break;

            case AsrEngine.Whisper:
            default:
                await RunWhisperAsync(chapter, effectiveOptions, cancellationToken).ConfigureAwait(false);
                break;
        }

        chapter.Save();
    }

    private async Task RunWhisperAsync(
        ChapterContext chapter,
        GenerateTranscriptOptions options,
        CancellationToken cancellationToken)
    {
        var (modelPath, modelType) = await AsrEngineConfig.ResolveModelPathAsync(options.Model, options.ModelPath)
            .ConfigureAwait(false);

        Log.Debug("Whisper model resolved: Path={ModelPath}, Type={ModelType}", modelPath, modelType);

        var asrOptions = new AsrOptions(
            ModelPath: modelPath,
            Language: options.Language,
            Threads: Math.Max(0, options.Threads),
            UseGpu: options.UseGpu,
            EnableWordTimestamps: options.EnableWordTimestamps,
            BeamSize: Math.Max(1, options.BeamSize),
            BestOf: Math.Max(1, options.BestOf),
            Temperature: (float)Math.Clamp(options.Temperature, 0.0, 1.0),
            NoSpeechBoost: true,
            GpuDevice: options.GpuDevice,
            UseFlashAttention: options.EnableFlashAttention,
            UseDtwTimestamps: options.EnableDtwTimestamps);

        Log.Debug(
            "Submitting audio to Whisper.NET (threads={Threads}, gpu={UseGpu}, beam={BeamSize}, bestOf={BestOf})",
            asrOptions.Threads,
            asrOptions.UseGpu,
            asrOptions.BeamSize,
            asrOptions.BestOf);

        var response = await _asrService.TranscribeAsync(chapter, asrOptions, cancellationToken)
            .ConfigureAwait(false);

        PersistResponse(chapter, response);
    }

    private async Task RunNemoAsync(
        ChapterContext chapter,
        GenerateTranscriptOptions options,
        CancellationToken cancellationToken)
    {
        var serviceUrl = string.IsNullOrWhiteSpace(options.ServiceUrl)
            ? GenerateTranscriptOptions.DefaultServiceUrl
            : options.ServiceUrl!;

        Log.Debug("Nemo service URL: {ServiceUrl}", serviceUrl);
        await AsrProcessSupervisor.EnsureServiceReadyAsync(serviceUrl, cancellationToken).ConfigureAwait(false);

        var buffer = _asrService.ResolveAsrReadyBuffer(chapter);
        var tempFile = ExportBufferToTempFile(buffer);

        try
        {
            using var client = new AsrClient(serviceUrl);

            Log.Debug("Checking ASR service health at {ServiceUrl}", serviceUrl);
            var isHealthy = await client.IsHealthyAsync(cancellationToken).ConfigureAwait(false);
            if (!isHealthy)
            {
                throw new InvalidOperationException($"ASR service at {serviceUrl} is not healthy or unreachable");
            }

            Log.Debug("Submitting audio for transcription");
            var response = await client
                .TranscribeAsync(tempFile.FullName, options.Model, options.Language, cancellationToken)
                .ConfigureAwait(false);

            PersistResponse(chapter, response);
        }
        finally
        {
            TryDelete(tempFile);
        }
    }

    private static void PersistResponse(ChapterContext chapter, AsrResponse response)
    {
        chapter.Documents.Asr = response;
        var corpusText = AsrTranscriptBuilder.BuildCorpusText(response);
        chapter.Documents.AsrTranscriptText = corpusText;
        Log.Debug("ASR summary: ModelVersion={ModelVersion}, Tokens={TokenCount}", response.ModelVersion,
            response.Tokens.Length);
    }

    private static FileInfo ExportBufferToTempFile(AudioBuffer buffer)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"ams-asr-{Guid.NewGuid():N}.wav");
        using var wavStream = buffer.ToWavStream(new AudioEncodeOptions(TargetSampleRate: buffer.SampleRate));

        using var file = File.Create(tempPath);
        wavStream.CopyTo(file);
        return new FileInfo(tempPath);
    }

    private static void TryDelete(FileInfo file)
    {
        if (file.Exists)
        {
            try
            {
                file.Delete();
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to delete temporary ASR file {File}: {Message}", file.FullName, ex.Message);
            }
        }
    }

}

public sealed record GenerateTranscriptOptions
{
    public static string DefaultServiceUrl => "http://127.0.0.1:5000";
    public static GenerateTranscriptOptions Default { get; } = new();

    public AsrEngine? Engine { get; init; }
    public string ServiceUrl { get; init; } = DefaultServiceUrl;
    public string? Model { get; init; }
    public FileInfo? ModelPath { get; init; }
    public string Language { get; init; } = "en";
    public int Threads { get; init; }
    public bool UseGpu { get; init; } = true;
    public int GpuDevice { get; init; }
    public int BeamSize { get; init; } = 8;
    public int BestOf { get; init; } = 1;
    public double Temperature { get; init; }
    public bool EnableWordTimestamps { get; init; } = true;
    public bool EnableFlashAttention { get; init; }
    public bool EnableDtwTimestamps { get; init; } = false;
}
