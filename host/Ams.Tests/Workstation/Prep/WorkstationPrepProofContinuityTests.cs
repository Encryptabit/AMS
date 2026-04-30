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
using Ams.Core.Services;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Documents;
using Ams.Core.Services.Interfaces;
using Ams.Workstation.Server.Components.Layout;
using Ams.Workstation.Server.Components.Navigation;
using Ams.Workstation.Server.Services;
using Ams.Workstation.Server.Services.Prep;

namespace Ams.Tests.Workstation.Prep;

public sealed class WorkstationPrepProofContinuityTests : IDisposable
{
    private readonly List<string> _tempDirectories = [];

    [Fact]
    public async Task RunChapterPrepAsync_SuccessPath_PreservesPrepToProofAndPolishTraversalContinuity()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var chapterDisplayTitle = Assert.Single(workspace.AvailableChapters);
        var session = CreateSession(workspace);

        var success = await session.RunChapterPrepAsync(chapterDisplayTitle, PipelineStage.Hydrate);

        Assert.True(success);
        Assert.Equal(PrepRunPhase.Completed, session.CurrentPhase);
        Assert.NotNull(session.LastPipelineResult);
        Assert.Empty(session.LastPipelineOptionWarnings);
        Assert.True(session.LastChapterHandleReloaded);
        Assert.Equal("prep-pipeline", workspace.LastChapterHandleRefreshReason, ignoreCase: true);

        var expectedChapterId = workspace.GetStemForChapter(chapterDisplayTitle);
        Assert.Equal(expectedChapterId, session.LastPipelineResult!.ChapterId, ignoreCase: true);

        var artifactNames = session.LastArtifacts
            .Select(artifact => artifact.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("book-index", artifactNames);
        Assert.Contains("asr", artifactNames);
        Assert.Contains("anchors", artifactNames);
        Assert.Contains("transcript", artifactNames);
        Assert.Contains("hydrate", artifactNames);

        var proofPath = StageRouteCatalog.BuildProofChapterCompatibilityPath(chapterDisplayTitle);
        var proofMatch = StageRouteCatalog.Resolve(proofPath);
        Assert.True(
            proofMatch is not null
            && string.Equals(proofMatch.Stage.Id, StageRouteCatalog.StageIds.Proof, StringComparison.OrdinalIgnoreCase)
            && string.Equals(proofMatch.Module.Id, StageRouteCatalog.ModuleIds.ProofEditing, StringComparison.OrdinalIgnoreCase)
            && proofMatch.IsCompatibilityAlias,
            $"Expected prep handoff proof path '{proofPath}' to resolve to proof-editing compatibility route, but got '{proofMatch?.DiagnosticContext ?? "(null)"}'.");

        var proofState = StageModuleRail.ResolveStateForPath(proofPath);
        AssertVisibleShellState(
            proofState,
            proofPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing);

        var polishState = StageModuleRail.ResolveStateForPath("/polish");
        AssertVisibleShellState(
            polishState,
            "/polish",
            StageRouteCatalog.StageIds.Polish,
            StageRouteCatalog.ModuleIds.PolishScaffold);
    }

    [Fact]
    public async Task RunChapterPrepAsync_NormalizationWarnings_StillPreservesProofHandoffCompatibility()
    {
        var root = CreateTempDirectory();
        var bookFile = await WriteBookAsync(root, "book.md", "# Test Book\n\n## Chapter 1\n\nHello world.");
        WriteAudioStub(root, "1.wav");
        await CreateBookIndexAsync(bookFile, new FileInfo(Path.Combine(root, "book-index.json")));

        using var workspace = CreateWorkspace(root);
        var chapterDisplayTitle = Assert.Single(workspace.AvailableChapters);
        var session = CreateSession(workspace);

        var request = new PrepPipelineRunRequest
        {
            EndStage = PipelineStage.Hydrate,
            Asr = new PrepPipelineAsrRequest
            {
                Engine = AsrEngine.WhisperX,
                EnableDtwTimestamps = true,
                EnableFlashAttention = true,
                Language = "   "
            },
            Mfa = new PrepPipelineMfaRequest
            {
                Beam = -3,
                RetryBeam = 0
            },
            Chunking = new PrepPipelineChunkRequest
            {
                DisableChunkedMfa = true,
                RequireAsrChunkAudio = true
            }
        };

        var success = await session.RunChapterPrepAsync(chapterDisplayTitle, request);

        Assert.True(success);
        Assert.NotEmpty(session.LastPipelineOptionWarnings);
        Assert.Contains(
            session.LastPipelineOptionWarnings,
            warning => warning.Contains("WhisperX", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            session.LastPipelineOptionWarnings,
            warning => warning.Contains("chunk", StringComparison.OrdinalIgnoreCase));

        var compatibilityPath = StageRouteCatalog.BuildProofChapterCompatibilityPath(chapterDisplayTitle);
        var canonicalPath = StageRouteCatalog.BuildProofChapterCanonicalPath(chapterDisplayTitle);

        AssertProofEditingRoute(compatibilityPath, expectedCompatibilityAlias: true);
        AssertProofEditingRoute(canonicalPath, expectedCompatibilityAlias: true);
    }

    [Fact]
    public async Task RunChapterPrepAsync_MissingWorkspacePath_FailsBeforeProofHandoff()
    {
        using var workspace = new BlazorWorkspace(Path.Combine(CreateTempDirectory(), ".workstation-state.json"), loadPersistedState: false);
        var session = CreateSession(workspace);

        var success = await session.RunChapterPrepAsync("Chapter 1", PipelineStage.Hydrate);

        Assert.False(success);
        Assert.Equal(PrepRunPhase.Failed, session.CurrentPhase);
        Assert.Equal(RunFailureKind.Validation, session.LastFailureKind);
        Assert.Equal("pipeline", session.LastFailureStage);
        Assert.Contains("working directory", session.LastFailure!.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(session.LastRuntimeReadiness);
        Assert.Null(session.LastPipelineResult);
    }

    [Fact]
    public async Task RunChapterPrepAsync_InvalidChapterDisplayTitle_FailsWithValidationAndNoProofRoute()
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
        Assert.Equal("pipeline", session.LastFailureStage);
        Assert.Contains("Unknown chapter display title", session.LastFailure!.Message);
        Assert.Null(session.LastRuntimeReadiness);
    }

    [Fact]
    public void BuildProofChapterPath_MalformedDeepLinkSlug_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => StageRouteCatalog.BuildProofChapterCompatibilityPath("   "));
        Assert.Throws<ArgumentException>(() => StageRouteCatalog.BuildProofChapterCanonicalPath("   "));
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

    private static void AssertProofEditingRoute(string path, bool expectedCompatibilityAlias)
    {
        var match = StageRouteCatalog.Resolve(path);

        Assert.True(
            match is not null
            && string.Equals(match.Stage.Id, StageRouteCatalog.StageIds.Proof, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.Module.Id, StageRouteCatalog.ModuleIds.ProofEditing, StringComparison.OrdinalIgnoreCase)
            && match.IsCompatibilityAlias == expectedCompatibilityAlias,
            $"Expected proof-editing route for path '{path}' with compatibility='{expectedCompatibilityAlias}', but resolved '{match?.DiagnosticContext ?? "(null)"}'.");
    }

    private static void AssertVisibleShellState(
        StageModuleRail.StageModuleRailState state,
        string path,
        string expectedStageId,
        string expectedModuleId)
    {
        Assert.True(state.IsVisible, $"Expected shell state for '{path}' to be visible.");
        Assert.True(
            string.Equals(state.ActiveStageId, expectedStageId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(state.ActiveModuleId, expectedModuleId, StringComparison.OrdinalIgnoreCase),
            $"Shell state mismatch for path '{path}'. Expected stage='{expectedStageId}', module='{expectedModuleId}'. Actual stage='{state.ActiveStageId}', module='{state.ActiveModuleId}', marker='{state.DiagnosticMarker}'.");
        Assert.True(
            string.Equals(state.DiagnosticMarker, $"{expectedStageId}:{expectedModuleId}", StringComparison.OrdinalIgnoreCase),
            $"Unexpected diagnostic marker for path '{path}': '{state.DiagnosticMarker}'.");
    }

    private PrepRunSession CreateSession(BlazorWorkspace workspace)
    {
        var documentService = new DocumentService(pronunciationProvider: null, cache: new RecordingBookCache());
        var buildBookIndex = new BuildBookIndexCommand(documentService);
        var alignment = new RecordingAlignmentService();
        var pipelineService = new PipelineService(
            new BuildBookIndexCommand(documentService),
            new GenerateTranscriptCommand(new RecordingAsrService()),
            new ComputeAnchorsCommand(alignment),
            new BuildTranscriptIndexCommand(alignment),
            new HydrateTranscriptCommand(alignment),
            new RunMfaCommand(),
            new MergeTimingsCommand());

        return new PrepRunSession(
            workspace,
            buildBookIndex,
            pipelineService,
            new StubRuntimeReadinessProbe());
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
        var root = Path.Combine(Path.GetTempPath(), "ams-workstation-prep-proof-continuity-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        _tempDirectories.Add(root);
        return root;
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
        public Task<AsrResponse> TranscribeAsync(
            ChapterContext chapter,
            AsrOptions options,
            CancellationToken cancellationToken = default)
        {
            _ = chapter;
            _ = options;
            _ = cancellationToken;

            return Task.FromResult(new AsrResponse(
                "test-model",
                [
                    new AsrToken(0.0, 0.5, "Hello"),
                    new AsrToken(0.5, 0.5, "world")
                ]));
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
            throw new NotSupportedException($"{nameof(TranscribeChunksAsync)} is not used in these tests.");
        }
    }

    private sealed class RecordingAlignmentService : IAlignmentService
    {
        public Task<AnchorDocument> ComputeAnchorsAsync(
            ChapterContext context,
            AnchorComputationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _ = context;
            _ = options;
            _ = cancellationToken;

            return Task.FromResult(new AnchorDocument(
                SectionDetected: true,
                Section: new AnchorDocumentSection(1, "Chapter 1", 1, "chapter", 0, 1),
                Policy: new AnchorDocumentPolicy(3, 50, 100, true, string.Empty),
                Tokens: new AnchorDocumentTokenStats(2, 2, 2, 2),
                Window: new AnchorDocumentWindow(0, 1),
                Anchors: [new AnchorDocumentAnchor(0, 0, 0)],
                Windows: null));
        }

        public Task<TranscriptIndex> BuildTranscriptIndexAsync(
            ChapterContext context,
            TranscriptBuildOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _ = context;
            _ = cancellationToken;

            var effective = options ?? throw new InvalidOperationException("Transcript build options are required for the test.");
            return Task.FromResult(new TranscriptIndex(
                effective.AudioPath ?? string.Empty,
                effective.ScriptPath ?? string.Empty,
                effective.BookIndexPath ?? string.Empty,
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
                ]));
        }

        public Task<HydratedTranscript> HydrateTranscriptAsync(
            ChapterContext context,
            HydrationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _ = options;
            _ = cancellationToken;

            return Task.FromResult(new HydratedTranscript(
                context.Descriptor.AudioBuffers.FirstOrDefault()?.Path ?? string.Empty,
                context.Documents.GetAsrFile()?.FullName ?? string.Empty,
                context.Book.Documents.GetBookIndexFile()?.FullName ?? string.Empty,
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
                ]));
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
