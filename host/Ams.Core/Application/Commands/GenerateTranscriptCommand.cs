using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Runtime.Book;
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
            case AsrEngine.WhisperX:
                await RunWhisperXAsync(chapter, effectiveOptions, cancellationToken).ConfigureAwait(false);
                break;

            case AsrEngine.Whisper:
            default:
                await RunWhisperAsync(chapter, effectiveOptions, cancellationToken).ConfigureAwait(false);
                break;
        }

        chapter.Save();
    }

    // Re-transcribes only the specified chunk indices via IAsrService.TranscribeChunksAsync
    // (which splices new tokens into the existing AsrResponse). Whisper engine only — WhisperX
    // model loading is opaque to the in-process cache, so scoped recovery is not supported
    // there (the orchestrator skips the AlternateModel tier for WhisperX anyway).
    //
    // Throws InvalidOperationException when the chunk plan is no longer valid for the current
    // audio (caller should fall back to full ExecuteAsync).
    public async Task ExecuteScopedAsync(
        ChapterContext chapter,
        IReadOnlyList<int> chunkIndices,
        GenerateTranscriptOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        ArgumentNullException.ThrowIfNull(chunkIndices);

        if (chunkIndices.Count == 0)
        {
            throw new ArgumentException(
                "chunkIndices must contain at least one chunk index.", nameof(chunkIndices));
        }

        var effectiveOptions = options ?? GenerateTranscriptOptions.Default;
        var engine = effectiveOptions.Engine ?? AsrEngineConfig.Resolve();

        if (engine != AsrEngine.Whisper)
        {
            throw new InvalidOperationException(
                "Scoped re-ASR is only supported for the Whisper engine. " +
                $"Engine '{engine}' must use full chapter re-transcription.");
        }

        var (modelPath, _) = await AsrEngineConfig.ResolveModelPathAsync(effectiveOptions.Model, effectiveOptions.ModelPath)
            .ConfigureAwait(false);

        var prompt = effectiveOptions.DisablePrompt ? null : BuildAsrPrompt(chapter);

        var asrOptions = new AsrOptions(
            ModelPath: modelPath,
            Engine: AsrEngine.Whisper,
            Language: effectiveOptions.Language,
            Threads: Math.Max(0, effectiveOptions.Threads),
            UseGpu: effectiveOptions.UseGpu,
            EnableWordTimestamps: effectiveOptions.EnableWordTimestamps,
            BeamSize: Math.Max(1, effectiveOptions.BeamSize),
            BestOf: Math.Max(1, effectiveOptions.BestOf),
            Temperature: (float)Math.Clamp(effectiveOptions.Temperature, 0.0, 1.0),
            NoSpeechBoost: true,
            GpuDevice: effectiveOptions.GpuDevice,
            UseFlashAttention: effectiveOptions.EnableFlashAttention,
            UseDtwTimestamps: effectiveOptions.EnableDtwTimestamps,
            Prompt: prompt,
            DisableChunkPlan: effectiveOptions.DisableChunkPlan);

        Log.Info(
            "Scoped re-ASR: {Count} chunk(s) at indices [{Indices}] (model={Model}, prompt={PromptStatus})",
            chunkIndices.Count, string.Join(",", chunkIndices), effectiveOptions.Model ?? "default",
            prompt is null ? "disabled" : "preserved");

        var splicedResponse = await _asrService.TranscribeChunksAsync(chapter, chunkIndices, asrOptions, cancellationToken)
            .ConfigureAwait(false);

        PersistResponse(chapter, splicedResponse);
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

        var prompt = options.DisablePrompt ? null : BuildAsrPrompt(chapter);

        var asrOptions = new AsrOptions(
            ModelPath: modelPath,
            Engine: AsrEngine.Whisper,
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
            UseDtwTimestamps: options.EnableDtwTimestamps,
            Prompt: prompt,
            DisableChunkPlan: options.DisableChunkPlan);

        if (prompt is not null)
        {
            var section = chapter.Book.Documents.BookIndex?.Sections?
                .FirstOrDefault(s => s.StartWord == chapter.Descriptor.BookStartWord);
            Log.Debug("ASR prompt from BookIndex proper nouns ({Count} terms): {Prompt}",
                section?.ProperNouns?.Length ?? 0,
                prompt.Length > 200 ? prompt[..200] + "..." : prompt);
        }

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

    private async Task RunWhisperXAsync(
        ChapterContext chapter,
        GenerateTranscriptOptions options,
        CancellationToken cancellationToken)
    {
        var model = string.IsNullOrWhiteSpace(options.Model)
            ? AsrEngineConfig.DefaultWhisperXModel
            : options.Model!.Trim();
        if (options.ModelPath is not null)
        {
            model = Path.GetFullPath(options.ModelPath.FullName);
        }

        Log.Debug("WhisperX model resolved: {Model}", model);

        var prompt = options.DisablePrompt ? null : BuildAsrPrompt(chapter);
        var asrOptions = new AsrOptions(
            ModelPath: string.Empty,
            Engine: AsrEngine.WhisperX,
            ModelName: model,
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
            UseDtwTimestamps: false,
            Prompt: prompt,
            DisableChunkPlan: options.DisableChunkPlan);

        Log.Debug(
            "Submitting audio to WhisperX (gpu={UseGpu}, beam={BeamSize}, bestOf={BestOf})",
            asrOptions.UseGpu,
            asrOptions.BeamSize,
            asrOptions.BestOf);

        var response = await _asrService.TranscribeAsync(chapter, asrOptions, cancellationToken)
            .ConfigureAwait(false);

        PersistResponse(chapter, response);
    }

    private static void PersistResponse(ChapterContext chapter, AsrResponse response)
    {
        chapter.Documents.Asr = response;
        var corpusText = AsrTranscriptBuilder.BuildCorpusText(response);
        chapter.Documents.AsrTranscriptText = corpusText;
        Log.Debug("ASR summary: ModelVersion={ModelVersion}, Tokens={TokenCount}", response.ModelVersion,
            response.Tokens.Length);
    }

    private static string? BuildAsrPrompt(ChapterContext chapter)
    {
        var bookIndex = chapter.Book.Documents.BookIndex;
        if (bookIndex?.Sections is not { Length: > 0 })
            return null;

        // Find the section matching this chapter via BookStartWord on the descriptor
        var startWord = chapter.Descriptor.BookStartWord;
        if (startWord is null)
            return null;

        SectionRange? section = null;
        foreach (var s in bookIndex.Sections)
        {
            if (s.StartWord == startWord.Value)
            {
                section = s;
                break;
            }
        }

        if (section?.ProperNouns is not { Length: > 0 })
            return null;

        var promptTerms = ProperNounPromptFilter.Filter(section.ProperNouns);
        if (promptTerms.Length == 0)
        {
            return null;
        }

        // Join proper nouns with commas -- Whisper treats the prompt as prior text context.
        // Comma-separated names prime the decoder vocabulary without forming sentences.
        var prompt = string.Join(", ", promptTerms);
        return prompt.Length > 0 ? prompt : null;
    }

}

public sealed record GenerateTranscriptOptions
{
    public static GenerateTranscriptOptions Default { get; } = new();

    public AsrEngine? Engine { get; init; }
    public string? Model { get; init; }
    public FileInfo? ModelPath { get; init; }
    public string Language { get; init; } = "en";
    public int Threads { get; init; }
    public bool UseGpu { get; init; } = true;
    public int GpuDevice { get; init; }
    public int BeamSize { get; init; } = 3;
    public int BestOf { get; init; } = 1;
    public double Temperature { get; init; }
    public bool EnableWordTimestamps { get; init; } = true;
    public bool EnableFlashAttention { get; init; }
    public bool EnableDtwTimestamps { get; init; } = false;
    public bool DisablePrompt { get; init; }

    /// <summary>
    /// When true, ASR processes the full audio buffer as a single pass without
    /// chunk plan generation. Reverts to pre-chunking behavior for rollout control.
    /// </summary>
    public bool DisableChunkPlan { get; init; }
}
