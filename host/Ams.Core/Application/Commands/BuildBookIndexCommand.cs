using System.Text.Json;
using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Runs;
using Ams.Core.Runtime.Book;
using Ams.Core.Services.Interfaces;

namespace Ams.Core.Application.Commands;

public sealed class BuildBookIndexCommand
{
    private const string Stage = "book_index";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDocumentService _documentService;

    public BuildBookIndexCommand(IDocumentService documentService)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
    }

    public async Task<BuildBookIndexResult> ExecuteAsync(
        BuildBookIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            if (!request.BookFile.Exists)
            {
                throw new FileNotFoundException($"Book file not found: {request.BookFile.FullName}",
                    request.BookFile.FullName);
            }

            var buildResult = await _documentService.BuildIndexAsync(
                    new DocumentBuildIndexRequest(
                        request.BookFile.FullName,
                        request.IndexOptions,
                        request.CacheMode),
                    cancellationToken)
                .ConfigureAwait(false);

            var outputDirectory = request.OutputFile.Directory?.FullName
                                  ?? request.OutputFile.DirectoryName
                                  ?? ".";
            Directory.CreateDirectory(outputDirectory);

            var json = JsonSerializer.Serialize(buildResult.Index, JsonOptions);
            await File.WriteAllTextAsync(request.OutputFile.FullName, json, cancellationToken).ConfigureAwait(false);
            request.OutputFile.Refresh();

            var outputArtifact = CreateOutputArtifact(request.OutputFile);

            return new BuildBookIndexResult(
                buildResult.Index,
                buildResult.CacheDisposition,
                buildResult.PhonemesBackfilled,
                [outputArtifact]);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new BuildBookIndexCommandException(MapFailure(ex), [CreateOutputArtifact(request.OutputFile)], ex);
        }
    }

    private static RunFailure MapFailure(Exception exception)
    {
        return exception switch
        {
            FileNotFoundException => new RunFailure(RunFailureKind.Validation, exception.Message, Stage),
            ArgumentException => new RunFailure(RunFailureKind.Validation, exception.Message, Stage),
            InvalidOperationException => new RunFailure(RunFailureKind.Validation, exception.Message, Stage),
            IOException => new RunFailure(RunFailureKind.Dependency, exception.Message, Stage),
            BookCacheException => new RunFailure(RunFailureKind.Dependency, exception.Message, Stage),
            BookParseException => new RunFailure(RunFailureKind.Execution, exception.Message, Stage),
            BookIndexException => new RunFailure(RunFailureKind.Execution, exception.Message, Stage),
            _ => new RunFailure(RunFailureKind.Execution, exception.Message, Stage)
        };
    }

    private static RunArtifact CreateOutputArtifact(FileInfo outputFile)
    {
        return new RunArtifact(
            name: "book-index",
            kind: RunArtifactKind.Output,
            path: outputFile.FullName,
            exists: outputFile.Exists);
    }
}

public sealed record BuildBookIndexRequest
{
    public BuildBookIndexRequest(
        FileInfo bookFile,
        FileInfo outputFile,
        BookIndexOptions? indexOptions = null,
        BookIndexCacheMode cacheMode = BookIndexCacheMode.PreferCache)
    {
        ArgumentNullException.ThrowIfNull(bookFile);
        ArgumentNullException.ThrowIfNull(outputFile);

        BookFile = new FileInfo(Path.GetFullPath(bookFile.FullName));
        OutputFile = new FileInfo(Path.GetFullPath(outputFile.FullName));
        IndexOptions = indexOptions;
        CacheMode = cacheMode;
    }

    public FileInfo BookFile { get; }

    public FileInfo OutputFile { get; }

    public BookIndexOptions? IndexOptions { get; }

    public BookIndexCacheMode CacheMode { get; }

    public static BuildBookIndexRequest FromCliOptions(
        FileInfo bookFile,
        FileInfo outputFile,
        bool forceRefresh,
        bool noCache,
        double averageWordsPerMinute)
    {
        if (forceRefresh && noCache)
        {
            throw new ArgumentException("Cannot combine --force-refresh with --no-cache.");
        }

        var cacheMode = noCache
            ? BookIndexCacheMode.DisableCache
            : forceRefresh
                ? BookIndexCacheMode.Rebuild
                : BookIndexCacheMode.PreferCache;

        return new BuildBookIndexRequest(
            bookFile,
            outputFile,
            new BookIndexOptions
            {
                AverageWpm = averageWordsPerMinute
            },
            cacheMode);
    }

    public static BuildBookIndexRequest FromPipelineOptions(PipelineRunOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new BuildBookIndexRequest(
            options.BookFile,
            options.BookIndexFile,
            new BookIndexOptions
            {
                AverageWpm = options.AverageWordsPerMinute
            },
            options.ForceIndex
                ? BookIndexCacheMode.Rebuild
                : BookIndexCacheMode.PreferCache);
    }
}

public sealed record BuildBookIndexResult(
    BookIndex Index,
    BookIndexCacheDisposition CacheDisposition,
    bool PhonemesBackfilled,
    IReadOnlyList<RunArtifact> Artifacts)
{
    public ModuleId ModuleId => ModuleIds.BuildBookIndex;

    public RunState State => RunState.Completed;

    public bool WasRebuilt => CacheDisposition != BookIndexCacheDisposition.CacheHit;
}

public sealed class BuildBookIndexCommandException : Exception
{
    public BuildBookIndexCommandException(
        RunFailure failure,
        IReadOnlyList<RunArtifact>? artifacts = null,
        Exception? innerException = null)
        : base(failure?.Message, innerException)
    {
        Failure = failure ?? throw new ArgumentNullException(nameof(failure));
        Artifacts = artifacts?.ToArray() ?? [];
    }

    public ModuleId ModuleId => ModuleIds.BuildBookIndex;

    public RunFailure Failure { get; }

    public IReadOnlyList<RunArtifact> Artifacts { get; }
}
