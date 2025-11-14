using System.Text.Json;
using Ams.Core.Application.Commands;
using Ams.Core.Application.Contexts;
using Ams.Core.Application.Pipeline;
using Ams.Core.Processors.DocumentProcessor;
using Ams.Core.Services.Alignment;

namespace Ams.Core.Services;

public sealed class PipelineService
{
    private readonly IChapterContextFactory _contextFactory;
    private readonly GenerateTranscriptCommand _generateTranscript;
    private readonly ComputeAnchorsCommand _computeAnchors;
    private readonly BuildTranscriptIndexCommand _buildTranscriptIndex;
    private readonly HydrateTranscriptCommand _hydrateTranscript;
    private readonly RunMfaCommand _runMfa;
    private readonly MergeTimingsCommand _mergeTimings;

    public PipelineService(
        IChapterContextFactory contextFactory,
        GenerateTranscriptCommand generateTranscript,
        ComputeAnchorsCommand computeAnchors,
        BuildTranscriptIndexCommand buildTranscriptIndex,
        HydrateTranscriptCommand hydrateTranscript,
        RunMfaCommand runMfa,
        MergeTimingsCommand mergeTimings)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _generateTranscript = generateTranscript ?? throw new ArgumentNullException(nameof(generateTranscript));
        _computeAnchors = computeAnchors ?? throw new ArgumentNullException(nameof(computeAnchors));
        _buildTranscriptIndex = buildTranscriptIndex ?? throw new ArgumentNullException(nameof(buildTranscriptIndex));
        _hydrateTranscript = hydrateTranscript ?? throw new ArgumentNullException(nameof(hydrateTranscript));
        _runMfa = runMfa ?? throw new ArgumentNullException(nameof(runMfa));
        _mergeTimings = mergeTimings ?? throw new ArgumentNullException(nameof(mergeTimings));
    }

    public async Task<PipelineChapterResult> RunChapterAsync(
        PipelineRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);

        Directory.CreateDirectory(options.BookIndexFile.Directory?.FullName ?? options.BookIndexFile.DirectoryName ?? ".");

        var bookIndexBuilt = await EnsureBookIndexAsync(options, cancellationToken).ConfigureAwait(false);

        using var handle = _contextFactory.Create(
            bookIndexFile: options.BookIndexFile,
            audioFile: options.AudioFile,
            chapterDirectory: options.ChapterDirectory,
            chapterId: options.ChapterId);

        var chapter = handle.Chapter;
        var chapterRoot = chapter.Descriptor.RootPath ?? throw new InvalidOperationException("Chapter root path is not configured.");

        bool HasAsrDocument() => chapter.Documents.Asr is not null;
        bool HasAnchorDocument() => chapter.Documents.Anchors is not null;
        bool HasTranscriptDocument() => chapter.Documents.Transcript is not null;
        bool HasHydrateDocument() => chapter.Documents.HydratedTranscript is not null;
        bool HasTextGridDocument()
        {
            if (options.MfaOptions?.TextGridFile is { } explicitGrid)
            {
                explicitGrid.Refresh();
                return explicitGrid.Exists;
            }

            var doc = chapter.Documents.TextGrid;
            return doc?.Intervals?.Count > 0;
        }

        FileInfo ResolveAsrFile()
            => chapter.Documents.GetAsrFile()
               ?? throw new InvalidOperationException("ASR artifact path is not available.");

        FileInfo ResolveAnchorsFile()
            => chapter.Documents.GetAnchorsFile()
               ?? throw new InvalidOperationException("Anchor artifact path is not available.");

        FileInfo ResolveTranscriptFile()
            => chapter.Documents.GetTranscriptFile()
               ?? throw new InvalidOperationException("Transcript artifact path is not available.");

        FileInfo ResolveHydrateFile()
            => chapter.Documents.GetHydratedTranscriptFile()
               ?? throw new InvalidOperationException("Hydrate artifact path is not available.");

        FileInfo ResolveTreatedFile() => options.TreatedCopyFile ?? chapter.ResolveArtifactFile("treated.wav");
        FileInfo TextGridFile()
        {
            if (options.MfaOptions?.TextGridFile is { } explicitGrid)
            {
                return explicitGrid;
            }

            return chapter.Documents.GetTextGridFile()
                   ?? ResolveTextGridFile(chapterRoot, chapter.Descriptor.ChapterId);
        }

        var hasAsr = HasAsrDocument();
        var hasAnchors = HasAnchorDocument();
        var hasTranscript = HasTranscriptDocument();
        var hasHydrate = HasHydrateDocument();
        var hasTextGrid = HasTextGridDocument();

        bool asrRan = false;
        bool anchorsRan = false;
        bool transcriptRan = false;
        bool hydrateRan = false;
        bool mfaRan = false;

        if (IsStageEnabled(PipelineStage.Asr, options) && (options.Force || !hasAsr))
        {
            await WaitAsync(options.Concurrency?.AsrSemaphore, cancellationToken).ConfigureAwait(false);
            try
            {
                await _generateTranscript.ExecuteAsync(chapter, options.TranscriptOptions, cancellationToken).ConfigureAwait(false);
                asrRan = true;
                hasAsr = true;
            }
            finally
            {
                Release(options.Concurrency?.AsrSemaphore);
            }
        }

        if (IsStageEnabled(PipelineStage.Anchors, options) && (options.Force || !hasAnchors))
        {
            var anchorOptions = (options.AnchorOptions ?? BuildDefaultAnchorOptions()) with { EmitWindows = false };
            await _computeAnchors.ExecuteAsync(chapter, anchorOptions, cancellationToken).ConfigureAwait(false);
            anchorsRan = true;
            hasAnchors = true;
        }

        if (IsStageEnabled(PipelineStage.Transcript, options) && (options.Force || !hasTranscript))
        {
            var transcriptOptions = options.TranscriptIndexOptions ?? new BuildTranscriptIndexOptions();
            transcriptOptions = transcriptOptions with
            {
                AudioFile = options.AudioFile,
                AsrFile = options.TranscriptIndexOptions?.AsrFile,
                BookIndexFile = options.BookIndexFile,
                AnchorOptions = (options.AnchorOptions ?? BuildDefaultAnchorOptions()) with { EmitWindows = true }
            };

            await _buildTranscriptIndex.ExecuteAsync(chapter, transcriptOptions, cancellationToken).ConfigureAwait(false);
            transcriptRan = true;
            hasTranscript = true;
        }

        if (IsStageEnabled(PipelineStage.Hydrate, options) && (options.Force || !hasHydrate))
        {
            await _hydrateTranscript.ExecuteAsync(chapter, options.HydrationOptions, cancellationToken).ConfigureAwait(false);
            hydrateRan = true;
            hasHydrate = true;
        }

        if (IsStageEnabled(PipelineStage.Mfa, options))
        {
            var textGridExists = hasTextGrid;
            if (options.Force || !textGridExists)
            {
                await WaitAsync(options.Concurrency?.MfaSemaphore, cancellationToken).ConfigureAwait(false);
                try
                {
                    var mfaOptions = (options.MfaOptions ?? new RunMfaOptions()) with
                    {
                        AudioFile = options.AudioFile,
                        HydrateFile = ResolveHydrateFile(),
                        TextGridFile = TextGridFile()
                    };

                    var result = await _runMfa.ExecuteAsync(chapter, mfaOptions, cancellationToken).ConfigureAwait(false);
                    mfaRan = true;
                    hasTextGrid = HasTextGridDocument();
                    textGridExists = hasTextGrid;
                }
                finally
                {
                    Release(options.Concurrency?.MfaSemaphore);
                }
            }

            if (hasTextGrid)
            {
                var mergeOptions = options.MergeOptions ?? new MergeTimingsOptions();
                mergeOptions = mergeOptions with
                {
                    TextGridFile = TextGridFile()
                };

                await _mergeTimings.ExecuteAsync(chapter, mergeOptions, cancellationToken).ConfigureAwait(false);
                hasHydrate = true;
                hasTranscript = true;
                hasTextGrid = true;
            }
        }

        if (!options.SkipTreatedCopy)
        {
            CopyTreatedAudio(options.AudioFile, ResolveTreatedFile(), options.Force);
        }

        handle.Save();

        return new PipelineChapterResult(
            chapter.Descriptor.ChapterId,
            bookIndexBuilt,
            asrRan,
            anchorsRan,
            transcriptRan,
            hydrateRan,
            mfaRan,
            options.BookIndexFile,
            ResolveAsrFile(),
            ResolveAnchorsFile(),
            ResolveTranscriptFile(),
            ResolveHydrateFile(),
            TextGridFile(),
            ResolveTreatedFile());
    }

    private static bool IsStageEnabled(PipelineStage stage, PipelineRunOptions options)
    {
        var start = Math.Max((int)PipelineStage.BookIndex, (int)options.StartStage);
        var end = Math.Min((int)PipelineStage.Mfa, (int)options.EndStage);
        var value = (int)stage;
        return value >= start && value <= end;
    }

    private static AnchorComputationOptions BuildDefaultAnchorOptions()
    {
        return new AnchorComputationOptions
        {
            DetectSection = true,
            AsrPrefixTokens = 8,
            NGram = 3,
            TargetPerTokens = 50,
            MinSeparation = 100,
            AllowBoundaryCross = false,
            UseDomainStopwords = true
        };
    }

    private async Task<bool> EnsureBookIndexAsync(PipelineRunOptions options, CancellationToken cancellationToken)
    {
        options.BookIndexFile.Refresh();
        var requestRebuild = options.ForceIndex || options.Force;
        var exists = options.BookIndexFile.Exists;
        var concurrency = options.Concurrency;

        if (requestRebuild && concurrency is not null && !concurrency.TryClaimBookIndexForce())
        {
            return false;
        }

        if (!requestRebuild && exists)
        {
            return false;
        }

        await WaitAsync(concurrency?.BookIndexSemaphore, cancellationToken).ConfigureAwait(false);
        try
        {
            options.BookIndexFile.Refresh();
            if (!requestRebuild && options.BookIndexFile.Exists)
            {
                return false;
            }

            await BuildBookIndexAsync(options, cancellationToken).ConfigureAwait(false);
            return true;
        }
        finally
        {
            Release(concurrency?.BookIndexSemaphore);
        }
    }

    private static async Task BuildBookIndexAsync(PipelineRunOptions options, CancellationToken cancellationToken)
    {
        var bookFile = options.BookFile;
        if (bookFile is null)
        {
            throw new InvalidOperationException("Book file must be specified.");
        }

        if (!bookFile.Exists)
        {
            throw new FileNotFoundException("Book file not found.", bookFile.FullName);
        }

        Log.Debug("Building book index for {Book} -> {Output}", bookFile.FullName, options.BookIndexFile.FullName);

        var cache = DocumentProcessor.CreateBookCache();
        BookIndex bookIndex;

        if (!options.ForceIndex && cache is not null)
        {
            var cachedIndex = await cache.GetAsync(bookFile.FullName, cancellationToken).ConfigureAwait(false);
            if (cachedIndex is not null)
            {
                Log.Debug("Book index cache hit for {Book}", bookFile.FullName);
                bookIndex = cachedIndex;
            }
            else
            {
                Log.Debug("Book index cache miss for {Book}", bookFile.FullName);
                bookIndex = await BuildBookIndexInternal(bookFile, options.AverageWordsPerMinute, cancellationToken).ConfigureAwait(false);
                await cache.SetAsync(bookIndex, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            if (options.ForceIndex)
            {
                Log.Debug("Force rebuilding book index for {Book}", bookFile.FullName);
            }

            bookIndex = await BuildBookIndexInternal(bookFile, options.AverageWordsPerMinute, cancellationToken).ConfigureAwait(false);
            if (cache is not null)
            {
                await cache.SetAsync(bookIndex, cancellationToken).ConfigureAwait(false);
            }
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(bookIndex, jsonOptions);
        await File.WriteAllTextAsync(options.BookIndexFile.FullName, json, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<BookIndex> BuildBookIndexInternal(FileInfo bookFile, double averageWpm, CancellationToken cancellationToken)
    {
        var parseResult = await DocumentProcessor.ParseBookAsync(bookFile.FullName, cancellationToken).ConfigureAwait(false);
        return await DocumentProcessor.BuildBookIndexAsync(
            parseResult,
            bookFile.FullName,
            new BookIndexOptions { AverageWpm = averageWpm },
            pronunciationProvider: null,
            cancellationToken).ConfigureAwait(false);
    }

    private static FileInfo ResolveTextGridFile(string chapterRoot, string chapterId)
    {
        var alignmentDir = Path.Combine(chapterRoot, "alignment", "mfa");
        Directory.CreateDirectory(alignmentDir);
        return new FileInfo(Path.Combine(alignmentDir, $"{chapterId}.TextGrid"));
    }

    private static void CopyTreatedAudio(FileInfo source, FileInfo destination, bool overwrite)
    {
        if (!source.Exists)
        {
            return;
        }

        if (!overwrite && destination.Exists)
        {
            return;
        }

        Directory.CreateDirectory(destination.Directory?.FullName ?? destination.DirectoryName ?? ".");
        File.Copy(source.FullName, destination.FullName, overwrite: true);
    }

    private static async Task WaitAsync(SemaphoreSlim? semaphore, CancellationToken cancellationToken)
    {
        if (semaphore is not null)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static void Release(SemaphoreSlim? semaphore)
    {
        semaphore?.Release();
    }

    private static void ValidateOptions(PipelineRunOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ChapterId))
        {
            throw new ArgumentException("ChapterId must be provided.", nameof(options.ChapterId));
        }

        if (options.BookFile is null)
        {
            throw new ArgumentException("BookFile must be provided.", nameof(options.BookFile));
        }

        if (options.BookIndexFile is null)
        {
            throw new ArgumentException("BookIndexFile must be provided.", nameof(options.BookIndexFile));
        }

        if (options.AudioFile is null)
        {
            throw new ArgumentException("AudioFile must be provided.", nameof(options.AudioFile));
        }
    }
}
