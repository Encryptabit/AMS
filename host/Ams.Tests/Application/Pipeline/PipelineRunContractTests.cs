using Ams.Core.Application.Commands;
using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Runs;
using Ams.Core.Asr;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Processors;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;
using Ams.Core.Services;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Documents;
using Ams.Core.Services.Interfaces;

namespace Ams.Tests.Application.Pipeline;

public sealed class PipelineRunContractTests : IDisposable
{
    private readonly List<string> _tempDirectories = new();

    [Fact]
    public async Task RunChapterAsync_ExecutedStages_ReportOrderedTypedProgressAndArtifacts()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.txt", "Title\n\nHello world.");
        var audioFile = WriteAudioStub(root, "chapter-01.wav");
        var chapterId = "chapter-01";
        var chapterDirectory = new DirectoryInfo(Path.Combine(root, chapterId));
        var bookIndexFile = new FileInfo(Path.Combine(root, "book-index.json"));

        var asr = new RecordingAsrService();
        var alignment = new RecordingAlignmentService();
        var service = CreateService(asr, alignment);
        using var workspace = new TestWorkspace(root);

        var reported = new List<RunProgressUpdate>();
        var result = await service.RunChapterAsync(
            workspace,
            new PipelineRunOptions
            {
                BookFile = bookFile,
                BookIndexFile = bookIndexFile,
                AudioFile = audioFile,
                ChapterDirectory = chapterDirectory,
                ChapterId = chapterId,
                EndStage = PipelineStage.Hydrate,
                SkipTreatedCopy = false,
                Progress = new RecordingProgress(reported)
            });

        Assert.Equal(ModuleIds.PipelineRun.Value, result.ModuleId.Value);
        Assert.Equal(RunState.Completed, result.State);
        Assert.Null(result.Failure);
        Assert.True(result.BookIndexBuilt);
        Assert.True(result.AsrRan);
        Assert.True(result.AnchorsRan);
        Assert.True(result.TranscriptRan);
        Assert.True(result.HydrateRan);
        Assert.False(result.MfaRan);
        Assert.Equal(reported.Select(update => update.Message), result.ProgressUpdates.Select(update => update.Message));

        Assert.Collection(
            result.StageResults,
            stage => AssertStage(stage, PipelineStage.BookIndex, executed: true, RunState.Completed, "Index built"),
            stage => AssertStage(stage, PipelineStage.Asr, executed: true, RunState.Completed, "ASR complete"),
            stage => AssertStage(stage, PipelineStage.Anchors, executed: true, RunState.Completed, "Anchors generated"),
            stage => AssertStage(stage, PipelineStage.Transcript, executed: true, RunState.Completed, "Transcript indexed"),
            stage => AssertStage(stage, PipelineStage.Hydrate, executed: true, RunState.Completed, "Hydrate complete"));

        Assert.Equal(chapterId, result.ProgressUpdates[0].ItemId);
        Assert.Equal(RunState.Pending, result.ProgressUpdates[0].State);
        Assert.Equal(PipelineRunContract.PipelineStageName, result.ProgressUpdates[0].Stage);

        var completedStages = result.ProgressUpdates
            .Where(update => update.State == RunState.Completed
                             && !string.Equals(update.Stage, PipelineRunContract.PipelineStageName,
                                 StringComparison.OrdinalIgnoreCase))
            .Select(update => update.Stage)
            .ToArray();

        Assert.Equal(new[] { "book_index", "asr", "anchors", "transcript", "hydrate" }, completedStages);

        var artifactsByName = result.Artifacts.ToDictionary(artifact => artifact.Name, artifact => artifact,
            StringComparer.OrdinalIgnoreCase);
        Assert.True(artifactsByName["book-index"].Exists);
        Assert.True(artifactsByName["asr"].Exists);
        Assert.True(artifactsByName["anchors"].Exists);
        Assert.True(artifactsByName["transcript"].Exists);
        Assert.True(artifactsByName["hydrate"].Exists);
        Assert.True(artifactsByName["treated-audio"].Exists);
        Assert.DoesNotContain(result.Artifacts, artifact => string.Equals(artifact.Name, "text-grid", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(1, asr.TranscribeCalls);
        Assert.Equal(1, alignment.ComputeAnchorsCalls);
        Assert.Equal(1, alignment.BuildTranscriptCalls);
        Assert.Equal(1, alignment.HydrateCalls);
    }

    [Fact]
    public async Task RunChapterAsync_CachedStagesRemainTypedWithoutInvokingCommands()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.txt", "Title\n\nHello cached world.");
        var audioFile = WriteAudioStub(root, "chapter-01.wav");
        var chapterId = "chapter-01";
        var chapterDirectory = new DirectoryInfo(Path.Combine(root, chapterId));
        var bookIndexFile = new FileInfo(Path.Combine(root, "book-index.json"));

        using var workspace = new TestWorkspace(root);
        await CreateBookIndexAsync(bookFile, bookIndexFile);
        SeedCachedArtifacts(workspace, bookIndexFile, audioFile, chapterDirectory, chapterId);

        var asr = new RecordingAsrService();
        var alignment = new RecordingAlignmentService();
        var service = CreateService(asr, alignment);

        var result = await service.RunChapterAsync(
            workspace,
            new PipelineRunOptions
            {
                BookFile = bookFile,
                BookIndexFile = bookIndexFile,
                AudioFile = audioFile,
                ChapterDirectory = chapterDirectory,
                ChapterId = chapterId,
                EndStage = PipelineStage.Hydrate,
                SkipTreatedCopy = false
            });

        Assert.Equal(RunState.Completed, result.State);
        Assert.False(result.BookIndexBuilt);
        Assert.False(result.AsrRan);
        Assert.False(result.AnchorsRan);
        Assert.False(result.TranscriptRan);
        Assert.False(result.HydrateRan);
        Assert.All(result.StageResults, stage => Assert.False(stage.Executed));

        Assert.Collection(
            result.StageResults,
            stage => AssertStage(stage, PipelineStage.BookIndex, executed: false, RunState.Completed, "Index ready"),
            stage => AssertStage(stage, PipelineStage.Asr, executed: false, RunState.Completed, "ASR cached"),
            stage => AssertStage(stage, PipelineStage.Anchors, executed: false, RunState.Completed, "Anchors cached"),
            stage => AssertStage(stage, PipelineStage.Transcript, executed: false, RunState.Completed, "Transcript cached"),
            stage => AssertStage(stage, PipelineStage.Hydrate, executed: false, RunState.Completed, "Hydrate cached"));

        Assert.Equal(0, asr.TranscribeCalls);
        Assert.Equal(0, alignment.ComputeAnchorsCalls);
        Assert.Equal(0, alignment.BuildTranscriptCalls);
        Assert.Equal(0, alignment.HydrateCalls);
        Assert.Contains(result.Artifacts, artifact => artifact.Name == "treated-audio" && artifact.Exists);
    }

    [Fact]
    public async Task RunChapterAsync_ForceRerunsChapterStagesWithoutRebuildingBookIndex()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.txt", "Title\n\nHello forced chapter world.");
        var audioFile = WriteAudioStub(root, "chapter-01.wav");
        var chapterId = "chapter-01";
        var chapterDirectory = new DirectoryInfo(Path.Combine(root, chapterId));
        var bookIndexFile = new FileInfo(Path.Combine(root, "book-index.json"));

        using var workspace = new TestWorkspace(root);
        await CreateBookIndexAsync(bookFile, bookIndexFile);
        SeedCachedArtifacts(workspace, bookIndexFile, audioFile, chapterDirectory, chapterId);

        var asr = new RecordingAsrService();
        var alignment = new RecordingAlignmentService();
        var service = CreateService(asr, alignment);

        var result = await service.RunChapterAsync(
            workspace,
            new PipelineRunOptions
            {
                BookFile = bookFile,
                BookIndexFile = bookIndexFile,
                AudioFile = audioFile,
                ChapterDirectory = chapterDirectory,
                ChapterId = chapterId,
                EndStage = PipelineStage.Hydrate,
                SkipTreatedCopy = false,
                Force = true,
                ForceIndex = false
            });

        Assert.Equal(RunState.Completed, result.State);
        Assert.False(result.BookIndexBuilt);
        Assert.True(result.AsrRan);
        Assert.True(result.AnchorsRan);
        Assert.True(result.TranscriptRan);
        Assert.True(result.HydrateRan);

        Assert.Collection(
            result.StageResults,
            stage => AssertStage(stage, PipelineStage.BookIndex, executed: false, RunState.Completed, "Index ready"),
            stage => AssertStage(stage, PipelineStage.Asr, executed: true, RunState.Completed, "ASR complete"),
            stage => AssertStage(stage, PipelineStage.Anchors, executed: true, RunState.Completed, "Anchors generated"),
            stage => AssertStage(stage, PipelineStage.Transcript, executed: true, RunState.Completed, "Transcript indexed"),
            stage => AssertStage(stage, PipelineStage.Hydrate, executed: true, RunState.Completed, "Hydrate complete"));

        Assert.Equal(1, asr.TranscribeCalls);
        Assert.Equal(1, alignment.ComputeAnchorsCalls);
        Assert.Equal(1, alignment.BuildTranscriptCalls);
        Assert.Equal(1, alignment.HydrateCalls);
    }

    [Fact]
    public async Task RunChapterAsync_ScopedReAsr_ForcesAllDownstreamStages()
    {
        // C4 contract: when PipelineRunOptions.ScopedReAsrChunkIndices is non-empty, the ASR
        // stage rewrites asr.json (via scoped re-transcribe) — anchors / transcript / hydrate /
        // MFA must therefore re-run regardless of their cached state and regardless of
        // options.Force. Without this, a direct caller patching asr.json would leave stale
        // downstream outputs (CLI orchestrator already sets Force, but the contract should hold
        // for any caller).
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.txt", "Title\n\nHello scoped world.");
        var audioFile = WriteAudioStub(root, "chapter-01.wav");
        var chapterId = "chapter-01";
        var chapterDirectory = new DirectoryInfo(Path.Combine(root, chapterId));
        var bookIndexFile = new FileInfo(Path.Combine(root, "book-index.json"));

        using var workspace = new TestWorkspace(root);
        await CreateBookIndexAsync(bookFile, bookIndexFile);
        SeedCachedArtifacts(workspace, bookIndexFile, audioFile, chapterDirectory, chapterId);

        var asr = new RecordingAsrService();
        var alignment = new RecordingAlignmentService();
        var service = CreateService(asr, alignment);

        var result = await service.RunChapterAsync(
            workspace,
            new PipelineRunOptions
            {
                BookFile = bookFile,
                BookIndexFile = bookIndexFile,
                AudioFile = audioFile,
                ChapterDirectory = chapterDirectory,
                ChapterId = chapterId,
                EndStage = PipelineStage.Hydrate,
                SkipTreatedCopy = false,
                Force = false,                              // explicitly NOT forcing
                ScopedReAsrChunkIndices = new[] { 0, 1 }    // but scoped re-ASR is requested
            });

        Assert.Equal(RunState.Completed, result.State);
        // ASR ran (scoped path tried first; falls back to full when chunk plan absent in test).
        Assert.True(result.AsrRan, "ASR stage must run when ScopedReAsrChunkIndices is set");
        // Downstream stages cascade despite Force=false because effectiveForce includes scoped.
        Assert.True(result.AnchorsRan, "Anchors must re-run after scoped ASR rewrites asr.json");
        Assert.True(result.TranscriptRan, "Transcript must re-run after scoped ASR rewrites asr.json");
        Assert.True(result.HydrateRan, "Hydrate must re-run after scoped ASR rewrites asr.json");

        Assert.Equal(1, alignment.ComputeAnchorsCalls);
        Assert.Equal(1, alignment.BuildTranscriptCalls);
        Assert.Equal(1, alignment.HydrateCalls);
    }

    [Fact]
    public async Task RunChapterAsync_FailureSurfacesTypedFailureProgressAndArtifacts()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.txt", "Title\n\nHello failure world.");
        var audioFile = WriteAudioStub(root, "chapter-01.wav");
        var chapterId = "chapter-01";
        var chapterDirectory = new DirectoryInfo(Path.Combine(root, chapterId));
        var bookIndexFile = new FileInfo(Path.Combine(root, "book-index.json"));

        var asr = new RecordingAsrService();
        var alignment = new RecordingAlignmentService
        {
            HydrateException = new InvalidOperationException("Hydrate service exploded")
        };
        var service = CreateService(asr, alignment);
        using var workspace = new TestWorkspace(root);

        var exception = await Assert.ThrowsAsync<PipelineRunException>(() => service.RunChapterAsync(
            workspace,
            new PipelineRunOptions
            {
                BookFile = bookFile,
                BookIndexFile = bookIndexFile,
                AudioFile = audioFile,
                ChapterDirectory = chapterDirectory,
                ChapterId = chapterId,
                EndStage = PipelineStage.Hydrate,
                SkipTreatedCopy = false
            }));

        var result = exception.Result;
        Assert.Equal(RunState.Failed, result.State);
        Assert.NotNull(result.Failure);
        Assert.Equal(RunFailureKind.Execution, result.Failure!.Kind);
        Assert.Equal("hydrate", result.Failure.Stage);

        var failedUpdate = result.ProgressUpdates.Last();
        Assert.Equal(chapterId, failedUpdate.ItemId);
        Assert.Equal(RunState.Failed, failedUpdate.State);
        Assert.Equal("hydrate", failedUpdate.Stage);
        Assert.NotNull(failedUpdate.Failure);
        Assert.Equal(RunFailureKind.Execution, failedUpdate.Failure!.Kind);
        Assert.Equal("hydrate", failedUpdate.Failure.Stage);
        Assert.Contains("exploded", failedUpdate.Failure.Message, StringComparison.OrdinalIgnoreCase);

        var failedStage = Assert.Single(result.StageResults, stage => stage.State == RunState.Failed);
        Assert.Equal(PipelineStage.Hydrate, failedStage.Stage);
        Assert.False(failedStage.Executed);
        Assert.Equal(RunFailureKind.Execution, failedStage.Failure!.Kind);
        Assert.Contains(failedStage.Artifacts, artifact => artifact.Name == "hydrate" && !artifact.Exists);
        Assert.True(result.AsrRan);
        Assert.True(result.AnchorsRan);
        Assert.True(result.TranscriptRan);
        Assert.False(result.HydrateRan);
    }

    [Fact]
    public void PipelineResultContracts_RejectInconsistentFailureShapes_AndExposeMissingArtifactFailures()
    {
        var root = CreateTempDirectory();
        var bookIndexFile = new FileInfo(Path.Combine(root, "book-index.json"));
        var asrFile = new FileInfo(Path.Combine(root, "chapter-01", "chapter-01.asr.json"));
        var anchorFile = new FileInfo(Path.Combine(root, "chapter-01", "chapter-01.align.anchors.json"));
        var transcriptFile = new FileInfo(Path.Combine(root, "chapter-01", "chapter-01.align.tx.json"));
        var hydrateFile = new FileInfo(Path.Combine(root, "chapter-01", "chapter-01.align.hydrate.json"));
        var textGridFile = new FileInfo(Path.Combine(root, "chapter-01", "alignment", "mfa", "chapter-01.TextGrid"));
        var treatedFile = new FileInfo(Path.Combine(root, "chapter-01", "chapter-01.treated.wav"));

        var stageFailure = new RunFailure(RunFailureKind.Execution, "MFA output is missing", "mfa");
        Assert.Throws<ArgumentException>(() => new PipelineStageResult(
            PipelineStage.Mfa,
            RunState.Completed,
            executed: false,
            message: "Invalid",
            failure: stageFailure));

        Assert.Throws<ArgumentException>(() => new PipelineChapterResult(
            "chapter-01",
            false,
            false,
            false,
            false,
            false,
            false,
            bookIndexFile,
            asrFile,
            anchorFile,
            transcriptFile,
            hydrateFile,
            textGridFile,
            treatedFile,
            state: RunState.Failed,
            failure: null));

        var missingTextGrid = new RunArtifact("text-grid", RunArtifactKind.Output, textGridFile.FullName, exists: false);
        var missingFailure = PipelineRunContract.CreateMissingArtifactFailure(PipelineStage.Mfa, missingTextGrid);
        Assert.Equal(RunFailureKind.Execution, missingFailure.Kind);
        Assert.Equal("mfa", missingFailure.Stage);
        Assert.Contains(textGridFile.FullName, missingFailure.Message);
    }

    [Theory]
    [InlineData("book_index", PipelineStage.BookIndex)]
    [InlineData(" ASR ", PipelineStage.Asr)]
    [InlineData("anchors", PipelineStage.Anchors)]
    [InlineData("transcript", PipelineStage.Transcript)]
    [InlineData("HYDRATE", PipelineStage.Hydrate)]
    [InlineData("mfa", PipelineStage.Mfa)]
    public void PipelineRunContract_TryParseStage_ParsesKnownStagesCaseInsensitively(string stageName, PipelineStage expectedStage)
    {
        var parsed = PipelineRunContract.TryParseStage(stageName, out var stage);

        Assert.True(parsed);
        Assert.Equal(expectedStage, stage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("hydrate-now")]
    [InlineData("mfa/final")]
    public void PipelineRunContract_TryParseStage_RejectsMalformedStageNames(string? stageName)
    {
        var parsed = PipelineRunContract.TryParseStage(stageName, out var stage);

        Assert.False(parsed);
        Assert.Equal(PipelineStage.Pending, stage);
    }

    public void Dispose()
    {
        foreach (var directory in _tempDirectories)
        {
            try
            {
                Directory.Delete(directory, recursive: true);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }
    }

    private static void AssertStage(
        PipelineStageResult stage,
        PipelineStage expectedStage,
        bool executed,
        RunState expectedState,
        string expectedMessage)
    {
        Assert.Equal(expectedStage, stage.Stage);
        Assert.Equal(executed, stage.Executed);
        Assert.Equal(expectedState, stage.State);
        Assert.Equal(expectedMessage, stage.Message);
        Assert.Null(stage.Failure);
        Assert.NotEmpty(stage.Artifacts);
    }

    private static PipelineService CreateService(RecordingAsrService asr, RecordingAlignmentService alignment)
    {
        var documentService = new DocumentService(pronunciationProvider: null, cache: new RecordingBookCache());
        return new PipelineService(
            new BuildBookIndexCommand(documentService),
            new GenerateTranscriptCommand(asr),
            new ComputeAnchorsCommand(alignment),
            new BuildTranscriptIndexCommand(alignment),
            new HydrateTranscriptCommand(alignment),
            new RunMfaCommand(),
            new MergeTimingsCommand());
    }

    private static async Task CreateBookIndexAsync(FileInfo bookFile, FileInfo outputFile)
    {
        var documentService = new DocumentService(pronunciationProvider: null, cache: new RecordingBookCache());
        var command = new BuildBookIndexCommand(documentService);
        await command.ExecuteAsync(new BuildBookIndexRequest(bookFile, outputFile));
    }

    private static void SeedCachedArtifacts(
        TestWorkspace workspace,
        FileInfo bookIndexFile,
        FileInfo audioFile,
        DirectoryInfo chapterDirectory,
        string chapterId)
    {
        using var handle = workspace.OpenChapter(new ChapterOpenOptions
        {
            BookIndexFile = bookIndexFile,
            AudioFile = audioFile,
            ChapterDirectory = chapterDirectory,
            ChapterId = chapterId
        });

        var asr = CreateAsrResponse();
        handle.Chapter.Documents.Asr = asr;
        handle.Chapter.Documents.Anchors = CreateAnchorDocument();
        handle.Chapter.Documents.Transcript = CreateTranscriptIndex(
            audioFile.FullName,
            handle.Chapter.Documents.GetAsrFile()!.FullName,
            bookIndexFile.FullName);
        handle.Chapter.Documents.HydratedTranscript = CreateHydratedTranscript(
            audioFile.FullName,
            handle.Chapter.Documents.GetAsrFile()!.FullName,
            bookIndexFile.FullName);
        handle.Save();
    }

    private static AsrResponse CreateAsrResponse()
    {
        return new AsrResponse(
            "test-model",
            new[]
            {
                new AsrToken(0.0, 0.5, "Hello"),
                new AsrToken(0.5, 0.5, "world")
            });
    }

    private static AnchorDocument CreateAnchorDocument()
    {
        return new AnchorDocument(
            SectionDetected: true,
            Section: new AnchorDocumentSection(1, "Chapter 01", 1, "chapter", 0, 1),
            Policy: new AnchorDocumentPolicy(3, 50, 100, true, string.Empty),
            Tokens: new AnchorDocumentTokenStats(2, 2, 2, 2),
            Window: new AnchorDocumentWindow(0, 1),
            Anchors: new[] { new AnchorDocumentAnchor(0, 0, 0) },
            Windows: null);
    }

    private static TranscriptIndex CreateTranscriptIndex(string audioPath, string scriptPath, string bookIndexPath)
    {
        return new TranscriptIndex(
            audioPath,
            scriptPath,
            bookIndexPath,
            DateTime.UtcNow,
            "test-normalization",
            new[] { new WordAlign(0, 0, AlignOp.Match, string.Empty, 1.0) },
            new[]
            {
                new SentenceAlign(
                    1,
                    new IntRange(0, 1),
                    new ScriptRange(0, 1),
                    new TimingRange(0.0, 1.0),
                    new SentenceMetrics(0.0, 0.0, 0.0, 0, 0),
                    "match")
            },
            new[]
            {
                new ParagraphAlign(
                    1,
                    new IntRange(0, 1),
                    new[] { 1 },
                    new ParagraphMetrics(0.0, 0.0, 1.0),
                    "match")
            });
    }

    private static HydratedTranscript CreateHydratedTranscript(string audioPath, string scriptPath, string bookIndexPath)
    {
        return new HydratedTranscript(
            audioPath,
            scriptPath,
            bookIndexPath,
            DateTime.UtcNow,
            "test-normalization",
            new[]
            {
                new HydratedWord(0, 0, "Hello", "Hello", "match", string.Empty, 1.0)
                {
                    StartSec = 0.0,
                    EndSec = 1.0,
                    DurationSec = 1.0
                }
            },
            new[]
            {
                new HydratedSentence(
                    1,
                    new HydratedRange(0, 1),
                    new HydratedScriptRange(0, 1),
                    "Hello world",
                    "Hello world",
                    new SentenceMetrics(0.0, 0.0, 0.0, 0, 0),
                    "match",
                    new TimingRange(0.0, 1.0),
                    null)
            },
            new[]
            {
                new HydratedParagraph(
                    1,
                    new HydratedRange(0, 1),
                    new[] { 1 },
                    "Hello world",
                    new ParagraphMetrics(0.0, 0.0, 1.0),
                    "match",
                    null)
            });
    }

    private async Task<FileInfo> WriteBookAsync(string root, string fileName, string content)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, fileName);
        await File.WriteAllTextAsync(path, content);
        return new FileInfo(path);
    }

    private static FileInfo WriteAudioStub(string root, string fileName)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, fileName);
        File.WriteAllBytes(path, new byte[] { 0x52, 0x49, 0x46, 0x46 });
        return new FileInfo(path);
    }

    private string CreateTempDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "ams-pipeline-run-contract-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        _tempDirectories.Add(root);
        return root;
    }

    private sealed class RecordingProgress(List<RunProgressUpdate> updates) : IProgress<RunProgressUpdate>
    {
        private readonly List<RunProgressUpdate> _updates = updates ?? throw new ArgumentNullException(nameof(updates));

        public void Report(RunProgressUpdate value)
        {
            _updates.Add(value);
        }
    }

    private sealed class RecordingAsrService : IAsrService
    {
        public int TranscribeCalls { get; private set; }
        public int TranscribeChunksCalls { get; private set; }
        public IReadOnlyList<int> LastScopedIndices { get; private set; } = Array.Empty<int>();

        public Task<AsrResponse> TranscribeAsync(
            ChapterContext chapter,
            AsrOptions options,
            CancellationToken cancellationToken = default)
        {
            _ = chapter;
            _ = options;
            TranscribeCalls++;
            return Task.FromResult(CreateAsrResponse());
        }

        public AudioBuffer ResolveAsrReadyBuffer(ChapterContext chapter)
        {
            throw new NotSupportedException($"{nameof(ResolveAsrReadyBuffer)} is not used in these tests.");
        }

        public Task<AsrResponse> TranscribeChunksAsync(
            ChapterContext chapter,
            IReadOnlyList<int> chunkIndices,
            AsrOptions options,
            CancellationToken cancellationToken = default)
        {
            _ = chapter;
            _ = options;
            TranscribeChunksCalls++;
            LastScopedIndices = chunkIndices.ToArray();
            return Task.FromResult(CreateAsrResponse());
        }
    }

    private sealed class RecordingAlignmentService : IAlignmentService
    {
        public int ComputeAnchorsCalls { get; private set; }
        public int BuildTranscriptCalls { get; private set; }
        public int HydrateCalls { get; private set; }
        public Exception? HydrateException { get; init; }

        public Task<AnchorDocument> ComputeAnchorsAsync(
            ChapterContext context,
            AnchorComputationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _ = context;
            _ = options;
            _ = cancellationToken;
            ComputeAnchorsCalls++;
            return Task.FromResult(CreateAnchorDocument());
        }

        public Task<TranscriptIndex> BuildTranscriptIndexAsync(
            ChapterContext context,
            TranscriptBuildOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _ = context;
            _ = cancellationToken;
            BuildTranscriptCalls++;
            var effective = options ?? throw new InvalidOperationException("Transcript build options are required for the test.");
            return Task.FromResult(CreateTranscriptIndex(
                effective.AudioPath ?? string.Empty,
                effective.ScriptPath ?? string.Empty,
                effective.BookIndexPath ?? string.Empty));
        }

        public Task<HydratedTranscript> HydrateTranscriptAsync(
            ChapterContext context,
            HydrationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _ = options;
            _ = cancellationToken;
            HydrateCalls++;
            if (HydrateException is not null)
            {
                throw HydrateException;
            }

            return Task.FromResult(CreateHydratedTranscript(
                context.Descriptor.AudioBuffers.FirstOrDefault()?.Path ?? string.Empty,
                context.Documents.GetAsrFile()?.FullName ?? string.Empty,
                context.Book.Documents.GetBookIndexFile()?.FullName ?? string.Empty));
        }
    }

    private sealed class RecordingBookCache : IBookCache
    {
        public Task<BookIndex?> GetAsync(string sourceFile, CancellationToken cancellationToken = default)
        {
            _ = sourceFile;
            _ = cancellationToken;
            return Task.FromResult<BookIndex?>(null);
        }

        public Task<bool> SetAsync(BookIndex bookIndex, CancellationToken cancellationToken = default)
        {
            _ = bookIndex;
            _ = cancellationToken;
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAsync(string sourceFile, CancellationToken cancellationToken = default)
        {
            _ = sourceFile;
            _ = cancellationToken;
            return Task.FromResult(true);
        }

        public Task<bool> IsValidAsync(BookIndex bookIndex, CancellationToken cancellationToken = default)
        {
            _ = bookIndex;
            _ = cancellationToken;
            return Task.FromResult(true);
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class TestWorkspace : IWorkspace, IDisposable
    {
        private readonly BookManager _manager;

        public TestWorkspace(string rootPath)
        {
            RootPath = rootPath;
            _manager = new BookManager(
                new[] { new BookDescriptor("test-book", rootPath, Array.Empty<ChapterDescriptor>()) },
                FileArtifactResolver.Instance);
        }

        public string RootPath { get; }

        public BookContext Book => _manager.Current;

        public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var bookIndexFile = options.BookIndexFile ?? new FileInfo(Path.Combine(RootPath, "book-index.json"));
            var chapterDirectory = options.ChapterDirectory
                                   ?? new DirectoryInfo(Path.Combine(RootPath, options.ChapterId ?? "chapter-01"));

            return Book.Chapters.CreateContext(ChapterOpenRequest.FromTrusted(
                bookIndexFile,
                options.AsrFile,
                options.TranscriptFile,
                options.HydrateFile,
                options.AudioFile,
                chapterDirectory,
                options.ChapterId,
                options.ReloadBookIndex));
        }

        public void Dispose()
        {
            _manager.DeallocateAll();
        }
    }
}
