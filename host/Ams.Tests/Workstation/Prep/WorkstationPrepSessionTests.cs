using Ams.Core.Application.Commands;
using Ams.Core.Application.Mfa.Models;
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
using Ams.Core.Services;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Documents;
using Ams.Core.Services.Interfaces;
using Ams.Workstation.Server.Services;
using Ams.Workstation.Server.Services.Prep;

namespace Ams.Tests.Workstation.Prep;

public sealed class WorkstationPrepSessionTests : IDisposable
{
    private readonly List<string> _tempDirectories = [];

    [Fact]
    public async Task BuildBookIndexAsync_MissingWorkspaceInitialization_SetsValidationFailure()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");

        using var workspace = new BlazorWorkspace(Path.Combine(root, ".workstation-state.json"), loadPersistedState: false);
        var session = CreateSession(workspace);

        var success = await session.BuildBookIndexAsync(bookFile.FullName);

        Assert.False(success);
        Assert.Equal(PrepRunPhase.Failed, session.CurrentPhase);
        Assert.Equal(RunFailureKind.Validation, session.LastFailureKind);
        Assert.Equal(PrepRunSession.BookIndexStage, session.LastFailureStage);
        Assert.Contains("working directory", session.LastFailure!.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(session.LastBuildRequest);
    }

    [Fact]
    public async Task BuildBookIndexAsync_NotifiesStateChangesForPageBindings()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");

        using var workspace = CreateWorkspace(root);
        var session = CreateSession(workspace);
        var notifications = 0;
        session.StateChanged += () => Interlocked.Increment(ref notifications);

        var success = await session.BuildBookIndexAsync(bookFile.FullName);

        Assert.True(success);
        Assert.True(Volatile.Read(ref notifications) >= 4);
    }

    [Fact]
    public async Task BuildBookIndexAsync_NoIndexBuild_RequiresExplicitManuscript_RefreshesWorkspaceAndCapturesState()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");

        using var workspace = CreateWorkspace(root);
        var session = CreateSession(workspace);

        Assert.False(workspace.HasBookIndex);
        Assert.False(workspace.IsInitialized);

        var success = await session.BuildBookIndexAsync(bookFile.FullName);

        Assert.True(success);
        Assert.Equal(PrepRunPhase.Completed, session.CurrentPhase);
        Assert.NotNull(session.LastBuildBookIndexResult);
        Assert.Equal(PrepRunSession.ExplicitAverageWordsPerMinute, session.LastBuildRequest?.IndexOptions?.AverageWpm);
        Assert.Equal(BookIndexCacheDisposition.CacheMiss, session.LastBuildBookIndexResult!.CacheDisposition);
        Assert.Equal(PrepRunSession.WorkspaceReloadStage, session.LastCompletedStage);
        Assert.True(workspace.HasBookIndex);
        Assert.True(workspace.IsInitialized);
        Assert.Equal("prep-build-index", workspace.LastWorkspaceRefreshReason);
        Assert.True(workspace.WorkspaceRefreshCount >= 1);
        Assert.NotEmpty(workspace.AvailableChapters);
        Assert.Contains(session.ProgressUpdates, update =>
            update.ModuleId.Value == ModuleIds.BuildBookIndex.Value
            && update.Stage == PrepRunSession.WorkspaceReloadStage
            && update.State == RunState.Completed);
    }

    [Fact]
    public async Task BuildBookIndexAsync_EmptyManuscriptPath_SetsValidationFailureWithoutReloadingWorkspace()
    {
        var root = CreateTempDirectory();
        WriteAudioStub(root, "1.wav");

        using var workspace = CreateWorkspace(root);
        var session = CreateSession(workspace);

        var success = await session.BuildBookIndexAsync("   ");

        Assert.False(success);
        Assert.Equal(PrepRunPhase.Failed, session.CurrentPhase);
        Assert.Equal(RunFailureKind.Validation, session.LastFailureKind);
        Assert.Equal(PrepRunSession.BookIndexStage, session.LastFailureStage);
        Assert.Contains("explicit manuscript path", session.LastFailure!.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(session.LastWorkspaceReloaded);
        Assert.False(workspace.HasBookIndex);
    }

    [Fact]
    public async Task RunChapterPrepAsync_MapsExplicitOptions_CapturesProgress_AndReloadsSelectedHandle()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var displayTitle = Assert.Single(workspace.AvailableChapters);
        Assert.True(workspace.SelectChapter(displayTitle));
        var originalHandle = workspace.CurrentChapterHandle;

        var session = CreateSession(workspace);
        var success = await session.RunChapterPrepAsync(displayTitle, PipelineStage.Hydrate);

        Assert.True(success);
        Assert.Equal(PrepRunPhase.Completed, session.CurrentPhase);
        Assert.NotNull(session.LastPipelineResult);
        Assert.NotNull(session.LastPipelineOptions);
        Assert.Equal(bookFile.FullName, session.LastPipelineOptions!.BookFile.FullName);
        Assert.Equal(Path.Combine(root, "book-index.json"), session.LastPipelineOptions.BookIndexFile.FullName);
        Assert.Equal(Path.Combine(root, "1.wav"), session.LastPipelineOptions.AudioFile.FullName);
        Assert.Equal(Path.Combine(root, "1"), session.LastPipelineOptions.ChapterDirectory!.FullName);
        Assert.Equal("1", session.LastPipelineOptions.ChapterId);
        Assert.Equal(PrepRunSession.ExplicitAverageWordsPerMinute, session.LastPipelineOptions.AverageWordsPerMinute);
        Assert.Equal(PrepRunSession.ChapterReloadStage, session.LastCompletedStage);
        Assert.NotNull(session.LastRuntimeReadiness);
        Assert.Equal("1", session.LastRuntimeReadiness!.ChapterId);
        Assert.Equal(PrepRuntimeReadinessState.Ready, session.LastRuntimeReadiness.Ffmpeg.State);
        Assert.Equal("prep-pipeline", workspace.LastChapterHandleRefreshReason);
        Assert.Equal("1", workspace.LastRefreshedChapterId);
        Assert.True(workspace.LastChapterHandleRefreshReopenedCurrentSelection);
        Assert.NotSame(originalHandle, workspace.CurrentChapterHandle);
        Assert.Contains(session.ProgressUpdates, update => update.Stage == "asr" && update.State == RunState.Completed);
        Assert.Contains(session.ProgressUpdates, update => update.Stage == "hydrate" && update.State == RunState.Completed);
        Assert.Contains(session.ProgressUpdates, update => update.Stage == PrepRunSession.ChapterReloadStage && update.State == RunState.Completed);
    }

    [Fact]
    public async Task RunChapterPrepAsync_TypedRequest_MapsCoreOptions_AndNormalizesWarnings()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var displayTitle = Assert.Single(workspace.AvailableChapters);

        var asr = new RecordingAsrService();
        var session = CreateSession(workspace, asr: asr);

        var request = new PrepPipelineRunRequest
        {
            EndStage = PipelineStage.Hydrate,
            Force = true,
            ForceIndex = true,
            Asr = new PrepPipelineAsrRequest
            {
                Engine = AsrEngine.WhisperX,
                Model = "   ",
                Language = "   ",
                EnableFlashAttention = true,
                EnableDtwTimestamps = true,
                DisablePrompt = true
            },
            Mfa = new PrepPipelineMfaRequest
            {
                BeamProfile = MfaBeamProfile.Strict,
                Beam = -4,
                RetryBeam = 0
            },
            Chunking = new PrepPipelineChunkRequest
            {
                DisableChunkPlan = true,
                DisableChunkedMfa = true,
                RequireAsrChunkAudio = true,
                Policy = new PrepPipelineChunkPolicyRequest
                {
                    MinChunkDurationSec = -2,
                    MaxChunkDurationSec = 0
                }
            }
        };

        var success = await session.RunChapterPrepAsync(displayTitle, request);

        Assert.True(success);
        Assert.NotNull(session.LastPipelineRequest);
        Assert.NotNull(session.LastPipelineOptions);

        var lastRequest = session.LastPipelineRequest!;
        Assert.Equal(PipelineStage.Hydrate, lastRequest.EndStage);
        Assert.Null(lastRequest.Asr.Model);
        Assert.Equal(GenerateTranscriptOptions.Default.Language, lastRequest.Asr.Language);
        Assert.False(lastRequest.Asr.EnableDtwTimestamps);
        Assert.False(lastRequest.Asr.EnableFlashAttention);
        Assert.Equal(1, lastRequest.Mfa.Beam);
        Assert.Equal(1, lastRequest.Mfa.RetryBeam);
        Assert.False(lastRequest.Chunking.RequireAsrChunkAudio);

        var options = session.LastPipelineOptions!;
        Assert.True(options.Force);
        Assert.True(options.ForceIndex);
        Assert.Equal(PipelineStage.Hydrate, options.EndStage);
        Assert.True(options.DisableChunkPlan);
        Assert.True(options.DisableChunkedMfa);
        Assert.Null(options.ChunkPlanningPolicy);
        Assert.NotNull(options.TranscriptOptions);
        Assert.NotNull(options.MfaOptions);

        Assert.Equal(AsrEngine.WhisperX, options.TranscriptOptions!.Engine);
        Assert.Null(options.TranscriptOptions.Model);
        Assert.Equal(GenerateTranscriptOptions.Default.Language, options.TranscriptOptions.Language);
        Assert.False(options.TranscriptOptions.EnableDtwTimestamps);
        Assert.False(options.TranscriptOptions.EnableFlashAttention);
        Assert.True(options.TranscriptOptions.DisablePrompt);
        Assert.True(options.TranscriptOptions.DisableChunkPlan);

        Assert.Equal(MfaBeamProfile.Strict, options.MfaOptions!.BeamProfile);
        Assert.Equal(1, options.MfaOptions.Beam);
        Assert.Equal(1, options.MfaOptions.RetryBeam);
        Assert.True(options.MfaOptions.DisableChunkedMfa);
        Assert.False(options.MfaOptions.RequireAsrChunkAudio);

        Assert.NotNull(asr.LastTranscribeOptions);
        Assert.Equal(AsrEngine.WhisperX, asr.LastTranscribeOptions!.Engine);
        Assert.False(asr.LastTranscribeOptions.UseDtwTimestamps);
        Assert.False(asr.LastTranscribeOptions.UseFlashAttention);
        Assert.True(asr.LastTranscribeOptions.DisableChunkPlan);

        Assert.Contains(session.LastPipelineOptionWarnings, warning => warning.Contains("WhisperX", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(session.LastPipelineOptionWarnings, warning => warning.Contains("beam", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(session.LastPipelineOptionWarnings, warning => warning.Contains("chunk", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RunChapterPrepAsync_RuntimeReadinessProbeCancellation_SetsCancelledStateWithoutPipelineDispatch()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var displayTitle = Assert.Single(workspace.AvailableChapters);
        var asr = new RecordingAsrService();
        var blockingProbe = new BlockingRuntimeReadinessProbe();
        var session = CreateSession(workspace, asr: asr, runtimeReadinessProbe: blockingProbe);

        var runTask = session.RunChapterPrepAsync(displayTitle, PipelineStage.Hydrate);
        await blockingProbe.WaitUntilStartedAsync();

        Assert.True(session.Cancel());

        var success = await runTask;

        Assert.False(success);
        Assert.Equal(PrepRunPhase.Cancelled, session.CurrentPhase);
        Assert.Equal(PrepRunSession.RuntimeReadinessStage, session.LastFailureStage);
        Assert.Null(session.LastRuntimeReadiness);
        Assert.Null(session.LastPipelineResult);
        Assert.Null(asr.LastTranscribeOptions);
    }

    [Fact]
    public async Task RunChapterPrepAsync_RuntimeReadinessProbeMalformedSnapshot_FailsBeforePipelineDispatch()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var displayTitle = Assert.Single(workspace.AvailableChapters);
        var asr = new RecordingAsrService();
        var malformedProbe = new MalformedRuntimeReadinessProbe();
        var session = CreateSession(workspace, asr: asr, runtimeReadinessProbe: malformedProbe);

        var success = await session.RunChapterPrepAsync(displayTitle, PipelineStage.Hydrate);

        Assert.False(success);
        Assert.Equal(PrepRunPhase.Failed, session.CurrentPhase);
        Assert.Equal(RunFailureKind.Validation, session.LastFailureKind);
        Assert.Equal(PrepRunSession.RuntimeReadinessStage, session.LastFailureStage);
        Assert.Null(session.LastRuntimeReadiness);
        Assert.Null(session.LastPipelineResult);
        Assert.Null(asr.LastTranscribeOptions);
    }

    [Fact]
    public async Task RunChapterPrepAsync_ValidationFailure_RetainsPreviousPipelineSnapshots()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var displayTitle = Assert.Single(workspace.AvailableChapters);
        var session = CreateSession(workspace);

        var firstSuccess = await session.RunChapterPrepAsync(displayTitle, PipelineStage.Hydrate);
        Assert.True(firstSuccess);

        var previousOptions = session.LastPipelineOptions;
        var previousRequest = session.LastPipelineRequest;
        var previousWarnings = session.LastPipelineOptionWarnings.ToArray();

        Assert.NotNull(previousOptions);
        Assert.NotNull(previousRequest);

        var failed = await session.RunChapterPrepAsync("missing chapter", PrepPipelineRunRequest.Default with
        {
            Asr = new PrepPipelineAsrRequest
            {
                Engine = AsrEngine.WhisperX,
                EnableDtwTimestamps = true
            }
        });

        Assert.False(failed);
        Assert.Equal(RunFailureKind.Validation, session.LastFailureKind);
        Assert.Same(previousOptions, session.LastPipelineOptions);
        Assert.Same(previousRequest, session.LastPipelineRequest);
        Assert.Equal(previousWarnings, session.LastPipelineOptionWarnings);
    }

    [Fact]
    public async Task RunChapterPrepAsync_WhitespaceChapterDisplayTitle_SetsValidationFailure()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var session = CreateSession(workspace);

        var success = await session.RunChapterPrepAsync("   ", PipelineStage.Hydrate);

        Assert.False(success);
        Assert.Equal(PrepRunPhase.Failed, session.CurrentPhase);
        Assert.Equal(RunFailureKind.Validation, session.LastFailureKind);
        Assert.Equal("pipeline", session.LastFailureStage);
        Assert.Contains("chapter display title is required", session.LastFailure!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunChapterPrepAsync_UnknownChapterDisplayTitle_SetsValidationFailure()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var session = CreateSession(workspace);

        var success = await session.RunChapterPrepAsync("Missing chapter", PipelineStage.Hydrate);

        Assert.False(success);
        Assert.Equal(PrepRunPhase.Failed, session.CurrentPhase);
        Assert.Equal(RunFailureKind.Validation, session.LastFailureKind);
        Assert.Contains("Unknown chapter display title", session.LastFailure!.Message);
    }

    [Fact]
    public async Task RunChapterPrepAsync_MissingBookSource_SetsValidationFailure()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));
        File.Delete(bookFile.FullName);

        using var workspace = CreateWorkspace(root);
        var displayTitle = Assert.Single(workspace.AvailableChapters);
        var session = CreateSession(workspace);

        var success = await session.RunChapterPrepAsync(displayTitle, PipelineStage.Hydrate);

        Assert.False(success);
        Assert.Equal(RunFailureKind.Validation, session.LastFailureKind);
        Assert.Contains("Book source file not found", session.LastFailure!.Message);
    }

    [Fact]
    public async Task RunChapterPrepAsync_MissingAudioFile_SetsValidationFailure()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        var audioFile = WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var displayTitle = Assert.Single(workspace.AvailableChapters);
        File.Delete(audioFile.FullName);
        var session = CreateSession(workspace);

        var success = await session.RunChapterPrepAsync(displayTitle, PipelineStage.Hydrate);

        Assert.False(success);
        Assert.Equal(RunFailureKind.Validation, session.LastFailureKind);
        Assert.Contains("Audio file not found", session.LastFailure!.Message);
    }

    [Fact]
    public async Task RunChapterPrepAsync_PipelineFailure_PreservesFailureState_AndReloadsSelectedHandle()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var displayTitle = Assert.Single(workspace.AvailableChapters);
        Assert.True(workspace.SelectChapter(displayTitle));
        var originalHandle = workspace.CurrentChapterHandle;

        var session = CreateSession(
            workspace,
            alignment: new RecordingAlignmentService
            {
                HydrateException = new InvalidOperationException("Hydrate service exploded")
            });

        var success = await session.RunChapterPrepAsync(displayTitle, new PrepPipelineRunRequest
        {
            EndStage = PipelineStage.Mfa
        });

        Assert.False(success);
        Assert.Equal(PipelineStage.Mfa, session.LastPipelineOptions?.EndStage);
        Assert.NotNull(session.LastRuntimeReadiness);
        Assert.Equal(PrepRuntimeReadinessState.Ready, session.LastRuntimeReadiness!.Mfa.State);
        Assert.Equal(PrepRunPhase.Failed, session.CurrentPhase);
        Assert.NotNull(session.LastPipelineResult);
        Assert.Equal(RunFailureKind.Execution, session.LastFailureKind);
        Assert.Equal("hydrate", session.LastFailureStage);
        Assert.Equal("transcript", session.LastCompletedStage);
        Assert.Equal("prep-pipeline-failed", workspace.LastChapterHandleRefreshReason);
        Assert.True(workspace.LastChapterHandleRefreshReopenedCurrentSelection);
        Assert.NotSame(originalHandle, workspace.CurrentChapterHandle);
    }

    [Fact]
    public async Task RunChapterPrepAsync_Cancelled_PreservesLastCompletedStage_AndCancellationState()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var displayTitle = Assert.Single(workspace.AvailableChapters);
        Assert.True(workspace.SelectChapter(displayTitle));

        var asr = new BlockingAsrService();
        var session = CreateSession(workspace, asr: asr);
        var notifications = 0;
        session.StateChanged += () => Interlocked.Increment(ref notifications);

        var runTask = session.RunChapterPrepAsync(displayTitle, PipelineStage.Hydrate);
        await asr.WaitUntilStartedAsync();
        Assert.True(Volatile.Read(ref notifications) >= 1);
        Assert.False(session.IsCancellationRequested);
        Assert.True(session.Cancel());
        Assert.True(session.IsCancellationRequested);

        var success = await runTask;

        Assert.False(success);
        Assert.NotNull(session.LastRuntimeReadiness);
        Assert.Equal(PrepRunPhase.Cancelled, session.CurrentPhase);
        Assert.True(session.WasCancelled);
        Assert.Equal(RunFailureKind.Cancelled, session.LastFailureKind);
        Assert.Equal("book_index", session.LastCompletedStage);
        Assert.Equal("prep-pipeline-cancelled", workspace.LastChapterHandleRefreshReason);
        Assert.True(workspace.LastChapterHandleRefreshReopenedCurrentSelection);
        Assert.False(session.IsCancellationRequested);
        Assert.True(Volatile.Read(ref notifications) >= 2);
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
                // Best-effort cleanup for temp fixtures.
            }
        }
    }

    private PrepRunSession CreateSession(
        BlazorWorkspace workspace,
        IAsrService? asr = null,
        IAlignmentService? alignment = null,
        IPrepRuntimeReadinessProbe? runtimeReadinessProbe = null)
    {
        var documentService = new DocumentService(pronunciationProvider: null, cache: new RecordingBookCache());
        var buildBookIndex = new BuildBookIndexCommand(documentService);
        var pipelineService = new PipelineService(
            new BuildBookIndexCommand(documentService),
            new GenerateTranscriptCommand(asr ?? new RecordingAsrService()),
            new ComputeAnchorsCommand(alignment ?? new RecordingAlignmentService()),
            new BuildTranscriptIndexCommand(alignment ?? new RecordingAlignmentService()),
            new HydrateTranscriptCommand(alignment ?? new RecordingAlignmentService()),
            new RunMfaCommand(),
            new MergeTimingsCommand());

        return new PrepRunSession(
            workspace,
            buildBookIndex,
            pipelineService,
            runtimeReadinessProbe ?? new StubRuntimeReadinessProbe());
    }

    private static BlazorWorkspace CreateWorkspace(string root)
    {
        var workspace = new BlazorWorkspace(Path.Combine(root, ".workstation-state.json"), loadPersistedState: false);
        Assert.True(workspace.SetWorkingDirectory(root));
        return workspace;
    }

    private static async Task CreateBookIndexAsync(FileInfo bookFile, FileInfo outputFile)
    {
        var documentService = new DocumentService(pronunciationProvider: null, cache: new RecordingBookCache());
        var command = new BuildBookIndexCommand(documentService);
        await command.ExecuteAsync(new BuildBookIndexRequest(bookFile, outputFile));
    }

    private static AsrResponse CreateAsrResponse()
    {
        return new AsrResponse(
            "test-model",
            [
                new AsrToken(0.0, 0.5, "Hello"),
                new AsrToken(0.5, 0.5, "world")
            ]);
    }

    private static AnchorDocument CreateAnchorDocument()
    {
        return new AnchorDocument(
            SectionDetected: true,
            Section: new AnchorDocumentSection(1, "Chapter 1", 1, "chapter", 0, 1),
            Policy: new AnchorDocumentPolicy(3, 50, 100, true, string.Empty),
            Tokens: new AnchorDocumentTokenStats(2, 2, 2, 2),
            Window: new AnchorDocumentWindow(0, 1),
            Anchors: [new AnchorDocumentAnchor(0, 0, 0)],
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
            [new WordAlign(0, 0, AlignOp.Match, string.Empty, 1.0)],
            [
                new SentenceAlign(
                    1,
                    new IntRange(0, 1),
                    new ScriptRange(0, 1),
                    new TimingRange(0.0, 1.0),
                    new SentenceMetrics(0.0, 0.0, 0.0, 0, 0),
                    "match")
            ],
            [
                new ParagraphAlign(
                    1,
                    new IntRange(0, 1),
                    [1],
                    new ParagraphMetrics(0.0, 0.0, 1.0),
                    "match")
            ]);
    }

    private static HydratedTranscript CreateHydratedTranscript(string audioPath, string scriptPath, string bookIndexPath)
    {
        return new HydratedTranscript(
            audioPath,
            scriptPath,
            bookIndexPath,
            DateTime.UtcNow,
            "test-normalization",
            [
                new HydratedWord(0, 0, "Hello", "Hello", "match", string.Empty, 1.0)
                {
                    StartSec = 0.0,
                    EndSec = 1.0,
                    DurationSec = 1.0
                }
            ],
            [
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
            ],
            [
                new HydratedParagraph(
                    1,
                    new HydratedRange(0, 1),
                    [1],
                    "Hello world",
                    new ParagraphMetrics(0.0, 0.0, 1.0),
                    "match",
                    null)
            ]);
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
        File.WriteAllBytes(path, [0x52, 0x49, 0x46, 0x46]);
        return new FileInfo(path);
    }

    private string CreateTempDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "ams-workstation-prep-session-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        _tempDirectories.Add(root);
        return root;
    }

    private sealed class BlockingRuntimeReadinessProbe : IPrepRuntimeReadinessProbe
    {
        private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<PrepRuntimeReadinessSnapshot> CaptureAsync(
            PrepPipelineRunRequest request,
            string? chapterDisplayTitle,
            string? chapterId,
            CancellationToken cancellationToken = default)
        {
            _ = request;
            _ = chapterDisplayTitle;
            _ = chapterId;
            _started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Cancellation should interrupt the runtime readiness probe.");
        }

        public Task WaitUntilStartedAsync()
            => _started.Task;
    }

    private sealed class MalformedRuntimeReadinessProbe : IPrepRuntimeReadinessProbe
    {
        public Task<PrepRuntimeReadinessSnapshot> CaptureAsync(
            PrepPipelineRunRequest request,
            string? chapterDisplayTitle,
            string? chapterId,
            CancellationToken cancellationToken = default)
        {
            _ = request;
            _ = chapterDisplayTitle;
            _ = chapterId;
            _ = cancellationToken;

            return Task.FromResult(new PrepRuntimeReadinessSnapshot
            {
                CapturedAtUtc = DateTimeOffset.UtcNow,
                ModelProvenance = new PrepRuntimeModelProvenance(
                    PrepRuntimeReadinessState.Unknown,
                    PrepModelProvenanceKind.InvalidModelInput,
                    RequestedModel: null,
                    NormalizedModelPath: null,
                    IsDeterministic: false,
                    Summary: string.Empty,
                    Guidance: string.Empty),
                Ffmpeg = new PrepRuntimeDependencyReadiness(
                    "FFmpeg",
                    PrepRuntimeReadinessState.Unknown,
                    string.Empty),
                Mfa = new PrepRuntimeDependencyReadiness(
                    "MFA",
                    PrepRuntimeReadinessState.Unknown,
                    string.Empty),
                Notes = []
            });
        }
    }

    private sealed class StubRuntimeReadinessProbe : IPrepRuntimeReadinessProbe
    {
        public Task<PrepRuntimeReadinessSnapshot> CaptureAsync(
            PrepPipelineRunRequest request,
            string? chapterDisplayTitle,
            string? chapterId,
            CancellationToken cancellationToken = default)
        {
            _ = request;
            _ = chapterDisplayTitle;
            _ = chapterId;
            _ = cancellationToken;

            return Task.FromResult(new PrepRuntimeReadinessSnapshot
            {
                CapturedAtUtc = DateTimeOffset.UtcNow,
                ChapterDisplayTitle = chapterDisplayTitle,
                ChapterId = chapterId,
                ModelProvenance = new PrepRuntimeModelProvenance(
                    PrepRuntimeReadinessState.Ready,
                    PrepModelProvenanceKind.PinnedPath,
                    RequestedModel: "stub-model.bin",
                    NormalizedModelPath: "stub-model.bin",
                    IsDeterministic: true,
                    Summary: "Stub model provenance is deterministic.",
                    Guidance: "Stub guidance."),
                Ffmpeg = new PrepRuntimeDependencyReadiness(
                    "FFmpeg",
                    PrepRuntimeReadinessState.Ready,
                    "Stub FFmpeg readiness passed."),
                Mfa = new PrepRuntimeDependencyReadiness(
                    "MFA",
                    PrepRuntimeReadinessState.Ready,
                    "Stub MFA readiness passed."),
                IsDeterministic = true,
                IsReady = true,
                Notes = ["Stub runtime readiness."]
            });
        }
    }

    private sealed class RecordingAsrService : IAsrService
    {
        public AsrOptions? LastTranscribeOptions { get; private set; }

        public Task<AsrResponse> TranscribeAsync(
            ChapterContext chapter,
            AsrOptions options,
            CancellationToken cancellationToken = default)
        {
            _ = chapter;
            LastTranscribeOptions = options;
            _ = cancellationToken;
            return Task.FromResult(CreateAsrResponse());
        }

        public AudioBuffer ResolveAsrReadyBuffer(ChapterContext chapter)
        {
            throw new NotSupportedException($"{nameof(ResolveAsrReadyBuffer)} is not used in these tests.");
        }
    }

    private sealed class BlockingAsrService : IAsrService
    {
        private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<AsrResponse> TranscribeAsync(
            ChapterContext chapter,
            AsrOptions options,
            CancellationToken cancellationToken = default)
        {
            _ = chapter;
            _ = options;
            _started.TrySetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return CreateAsrResponse();
        }

        public AudioBuffer ResolveAsrReadyBuffer(ChapterContext chapter)
        {
            throw new NotSupportedException($"{nameof(ResolveAsrReadyBuffer)} is not used in these tests.");
        }

        public Task WaitUntilStartedAsync()
            => _started.Task;
    }

    private sealed class RecordingAlignmentService : IAlignmentService
    {
        public Exception? HydrateException { get; init; }

        public Task<AnchorDocument> ComputeAnchorsAsync(
            ChapterContext context,
            AnchorComputationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _ = context;
            _ = options;
            _ = cancellationToken;
            return Task.FromResult(CreateAnchorDocument());
        }

        public Task<TranscriptIndex> BuildTranscriptIndexAsync(
            ChapterContext context,
            TranscriptBuildOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _ = context;
            _ = cancellationToken;
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
}
