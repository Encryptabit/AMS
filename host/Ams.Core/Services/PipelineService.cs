using Ams.Core.Application.Commands;
using Ams.Core.Application.Mfa;
using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Processes;
using Ams.Core.Application.Runs;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;
using Ams.Core.Services.Alignment;

namespace Ams.Core.Services;

public sealed class PipelineService
{
    private readonly BuildBookIndexCommand _buildBookIndex;
    private readonly GenerateTranscriptCommand _generateTranscript;
    private readonly ComputeAnchorsCommand _computeAnchors;
    private readonly BuildTranscriptIndexCommand _buildTranscriptIndex;
    private readonly HydrateTranscriptCommand _hydrateTranscript;
    private readonly RunMfaCommand _runMfa;
    private readonly MergeTimingsCommand _mergeTimings;

    public PipelineService(
        BuildBookIndexCommand buildBookIndex,
        GenerateTranscriptCommand generateTranscript,
        ComputeAnchorsCommand computeAnchors,
        BuildTranscriptIndexCommand buildTranscriptIndex,
        HydrateTranscriptCommand hydrateTranscript,
        RunMfaCommand runMfa,
        MergeTimingsCommand mergeTimings)
    {
        _buildBookIndex = buildBookIndex ?? throw new ArgumentNullException(nameof(buildBookIndex));
        _generateTranscript = generateTranscript ?? throw new ArgumentNullException(nameof(generateTranscript));
        _computeAnchors = computeAnchors ?? throw new ArgumentNullException(nameof(computeAnchors));
        _buildTranscriptIndex = buildTranscriptIndex ?? throw new ArgumentNullException(nameof(buildTranscriptIndex));
        _hydrateTranscript = hydrateTranscript ?? throw new ArgumentNullException(nameof(hydrateTranscript));
        _runMfa = runMfa ?? throw new ArgumentNullException(nameof(runMfa));
        _mergeTimings = mergeTimings ?? throw new ArgumentNullException(nameof(mergeTimings));
    }

    public async Task<PipelineChapterResult> RunChapterAsync(
        IWorkspace workspace,
        PipelineRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);

        using var mfaInvocationScope = MfaInvocationContext.BeginScope($"ch={options.ChapterId}");

        var moduleId = options.ModuleId;
        var itemId = options.ChapterId;
        var progressUpdates = new List<RunProgressUpdate>();
        var stageResults = new List<PipelineStageResult>();
        var aggregateArtifacts = new Dictionary<string, RunArtifact>(StringComparer.OrdinalIgnoreCase);
        var paths = ResolveArtifactPaths(options);

        BuildBookIndexResult? bookIndexResult = null;
        var bookIndexBuilt = false;
        var asrRan = false;
        var anchorsRan = false;
        var transcriptRan = false;
        var hydrateRan = false;
        var mfaRan = false;
        var problematicChunkIndices = (IReadOnlyList<int>)Array.Empty<int>();

        void TrackArtifacts(IEnumerable<RunArtifact>? artifacts)
        {
            if (artifacts is null)
            {
                return;
            }

            foreach (var artifact in artifacts)
            {
                aggregateArtifacts[artifact.Path] = artifact;
            }
        }

        void Emit(RunProgressUpdate update)
        {
            progressUpdates.Add(update);
            TrackArtifacts(update.Artifacts);
            options.Progress?.Report(update);
        }

        PipelineChapterResult BuildResult(RunState state, RunFailure? failure)
        {
            var orderedArtifacts = aggregateArtifacts.Values
                .OrderBy(artifact => artifact.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new PipelineChapterResult(
                itemId,
                bookIndexBuilt,
                asrRan,
                anchorsRan,
                transcriptRan,
                hydrateRan,
                mfaRan,
                paths.BookIndexFile,
                paths.AsrFile,
                paths.AnchorFile,
                paths.TranscriptFile,
                paths.HydrateFile,
                paths.TextGridFile,
                paths.TreatedAudioFile,
                problematicChunkIndices,
                bookIndexResult,
                moduleId,
                state,
                failure,
                orderedArtifacts,
                progressUpdates,
                stageResults);
        }

        PipelineRunException FailStage(
            PipelineStage? stage,
            Exception exception,
            bool executed = false,
            IReadOnlyList<RunArtifact>? overrideArtifacts = null,
            RunFailure? overrideFailure = null)
        {
            if (exception is PipelineRunException pipelineRunException)
            {
                return pipelineRunException;
            }

            var failure = overrideFailure ?? MapFailure(stage, exception);
            var failureArtifacts = overrideArtifacts
                                   ?? exception switch
                                   {
                                       BuildBookIndexCommandException buildBookIndexException => buildBookIndexException.Artifacts,
                                       _ when stage is PipelineStage artifactStage => ResolveStageArtifacts(artifactStage, paths),
                                       _ => ResolveAllArtifacts(paths)
                                   };

            if (stage is PipelineStage resolvedStage)
            {
                var stageResult = new PipelineStageResult(
                    resolvedStage,
                    RunState.Failed,
                    executed,
                    failure.Message,
                    failureArtifacts,
                    failure);
                stageResults.Add(stageResult);
                Emit(RunProgressUpdate.CreateFailure(
                    moduleId,
                    failure,
                    progress: PipelineRunContract.StageProgressBefore(resolvedStage),
                    artifacts: failureArtifacts,
                    itemId: itemId));
            }
            else
            {
                Emit(RunProgressUpdate.CreateFailure(
                    moduleId,
                    failure,
                    artifacts: failureArtifacts,
                    itemId: itemId));
            }

            return new PipelineRunException(BuildResult(RunState.Failed, failure), exception);
        }

        async Task ExecuteStageAsync(
            PipelineStage stage,
            string runningMessage,
            Func<Task<(bool Executed, string Message, IReadOnlyList<RunArtifact> Artifacts)>> executeAsync)
        {
            if (!IsStageEnabled(stage, options))
            {
                return;
            }

            Emit(RunProgressUpdate.CreateStatus(
                moduleId,
                RunState.Running,
                runningMessage,
                PipelineRunContract.StageProgressBefore(stage),
                PipelineRunContract.ToStageName(stage),
                itemId: itemId));

            try
            {
                var outcome = await executeAsync().ConfigureAwait(false);
                var stageResult = new PipelineStageResult(
                    stage,
                    RunState.Completed,
                    outcome.Executed,
                    outcome.Message,
                    outcome.Artifacts);
                stageResults.Add(stageResult);
                Emit(RunProgressUpdate.CreateStatus(
                    moduleId,
                    RunState.Completed,
                    stageResult.Message,
                    PipelineRunContract.StageProgress(stage),
                    stageResult.StageName,
                    stageResult.Artifacts,
                    itemId));
            }
            catch (PipelineRunException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                Emit(RunProgressUpdate.CreateFailure(
                    moduleId,
                    new RunFailure(
                        RunFailureKind.Cancelled,
                        $"{PipelineRunContract.FormatStageLabel(stage)} cancelled.",
                        PipelineRunContract.ToStageName(stage)),
                    progress: PipelineRunContract.StageProgressBefore(stage),
                    artifacts: ResolveStageArtifacts(stage, paths),
                    itemId: itemId));
                throw;
            }
            catch (Exception ex)
            {
                throw FailStage(stage, ex);
            }
        }

        Emit(RunProgressUpdate.CreateStatus(
            moduleId,
            RunState.Pending,
            "Queued",
            0d,
            PipelineRunContract.PipelineStageName,
            itemId: itemId));

        Emit(RunProgressUpdate.CreateStatus(
            moduleId,
            RunState.Running,
            "Running pipeline",
            0d,
            PipelineRunContract.PipelineStageName,
            itemId: itemId));

        try
        {
            Directory.CreateDirectory(options.BookIndexFile.Directory?.FullName ??
                                      options.BookIndexFile.DirectoryName ?? ".");

            await ExecuteStageAsync(
                PipelineStage.BookIndex,
                "Preparing book index",
                async () =>
                {
                    bookIndexResult = await EnsureBookIndexAsync(options, cancellationToken).ConfigureAwait(false);
                    bookIndexBuilt = bookIndexResult?.WasRebuilt ?? false;
                    var artifacts = bookIndexResult?.Artifacts ?? [CreateArtifact("book-index", RunArtifactKind.Output, paths.BookIndexFile)];
                    return (bookIndexBuilt, bookIndexBuilt ? "Index built" : "Index ready", artifacts);
                }).ConfigureAwait(false);

            var openOptions = new ChapterOpenOptions
            {
                BookIndexFile = options.BookIndexFile,
                AudioFile = options.AudioFile,
                ChapterDirectory = options.ChapterDirectory,
                ChapterId = options.ChapterId,
                ReloadBookIndex = bookIndexBuilt
            };

            using var handle = workspace.OpenChapter(openOptions);
            var chapter = handle.Chapter;
            var chapterRoot = chapter.Descriptor.RootPath
                              ?? throw new InvalidOperationException("Chapter root path is not configured.");

            paths = ResolveArtifactPaths(options, chapter);

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

            var hasAsr = HasAsrDocument();
            var hasAnchors = HasAnchorDocument();
            var hasTranscript = HasTranscriptDocument();
            var hasHydrate = HasHydrateDocument();
            var hasTextGrid = HasTextGridDocument();

            // Scoped re-ASR rewrites asr.json mid-pipeline — anchors / transcript / hydrate /
            // MFA must re-run regardless of their cached state. Treat scoped requests as
            // implicit Force for ALL stages from ASR onward. Without this, a direct
            // PipelineRunOptions caller could patch asr.json while reusing stale downstream
            // outputs (the CLI orchestrator already sets Force on recovery passes, but the
            // contract should hold for any caller).
            var scopedIndices = options.ScopedReAsrChunkIndices;
            var wantsScoped = scopedIndices is { Count: > 0 };
            var effectiveForce = options.Force || wantsScoped;

            await ExecuteStageAsync(
                PipelineStage.Asr,
                "Running ASR",
                async () =>
                {
                    var shouldRun = effectiveForce || !hasAsr;

                    if (shouldRun)
                    {
                        await WaitAsync(options.Concurrency?.AsrSemaphore, cancellationToken).ConfigureAwait(false);
                        try
                        {
                            var didScoped = false;
                            if (wantsScoped)
                            {
                                try
                                {
                                    await _generateTranscript.ExecuteScopedAsync(
                                            chapter, scopedIndices!, options.TranscriptOptions, cancellationToken)
                                        .ConfigureAwait(false);
                                    didScoped = true;
                                    Log.Info(
                                        "Scoped re-ASR succeeded for {Chapter} ({Count} chunk(s) patched)",
                                        chapter.Descriptor.ChapterId, scopedIndices!.Count);
                                }
                                catch (InvalidOperationException ex)
                                {
                                    // Chunk plan invalid, no existing AsrResponse, or engine
                                    // not Whisper — fall back to full ExecuteAsync.
                                    Log.Warn(
                                        "Scoped re-ASR not feasible for {Chapter}: {Reason}; falling back to full chapter re-ASR",
                                        chapter.Descriptor.ChapterId, ex.Message);
                                }
                            }

                            if (!didScoped)
                            {
                                await _generateTranscript.ExecuteAsync(chapter, options.TranscriptOptions, cancellationToken)
                                    .ConfigureAwait(false);
                            }

                            asrRan = true;
                            hasAsr = true;
                        }
                        finally
                        {
                            Release(options.Concurrency?.AsrSemaphore);
                        }
                    }

                    paths = ResolveArtifactPaths(options, chapter);
                    var artifacts = ResolveStageArtifacts(PipelineStage.Asr, paths);
                    return (asrRan, asrRan ? "ASR complete" : "ASR cached", artifacts);
                }).ConfigureAwait(false);

            await ExecuteStageAsync(
                PipelineStage.Anchors,
                "Computing anchors",
                async () =>
                {
                    if (effectiveForce || !hasAnchors)
                    {
                        var anchorOptions = (options.AnchorOptions ?? BuildDefaultAnchorOptions()) with { EmitWindows = false };
                        await _computeAnchors.ExecuteAsync(chapter, anchorOptions, cancellationToken)
                            .ConfigureAwait(false);
                        anchorsRan = true;
                        hasAnchors = true;
                    }

                    paths = ResolveArtifactPaths(options, chapter);
                    var artifacts = ResolveStageArtifacts(PipelineStage.Anchors, paths);
                    return (anchorsRan, anchorsRan ? "Anchors generated" : "Anchors cached", artifacts);
                }).ConfigureAwait(false);

            await ExecuteStageAsync(
                PipelineStage.Transcript,
                "Building transcript index",
                async () =>
                {
                    if (effectiveForce || !hasTranscript)
                    {
                        var transcriptOptions = options.TranscriptIndexOptions ?? new BuildTranscriptIndexOptions();
                        transcriptOptions = transcriptOptions with
                        {
                            AudioFile = options.AudioFile,
                            AsrFile = options.TranscriptIndexOptions?.AsrFile,
                            BookIndexFile = options.BookIndexFile,
                            AnchorOptions = (options.AnchorOptions ?? BuildDefaultAnchorOptions()) with { EmitWindows = true }
                        };

                        await _buildTranscriptIndex.ExecuteAsync(chapter, transcriptOptions, cancellationToken)
                            .ConfigureAwait(false);
                        transcriptRan = true;
                        hasTranscript = true;
                    }

                    paths = ResolveArtifactPaths(options, chapter);
                    var artifacts = ResolveStageArtifacts(PipelineStage.Transcript, paths);
                    return (transcriptRan, transcriptRan ? "Transcript indexed" : "Transcript cached", artifacts);
                }).ConfigureAwait(false);

            await ExecuteStageAsync(
                PipelineStage.Hydrate,
                "Hydrating transcript",
                async () =>
                {
                    if (effectiveForce || !hasHydrate)
                    {
                        await _hydrateTranscript.ExecuteAsync(chapter, options.HydrationOptions, cancellationToken)
                            .ConfigureAwait(false);
                        hydrateRan = true;
                        hasHydrate = true;
                    }

                    paths = ResolveArtifactPaths(options, chapter);
                    var artifacts = ResolveStageArtifacts(PipelineStage.Hydrate, paths);
                    return (hydrateRan, hydrateRan ? "Hydrate complete" : "Hydrate cached", artifacts);
                }).ConfigureAwait(false);

            await ExecuteStageAsync(
                PipelineStage.Mfa,
                "Running MFA",
                async () =>
                {
                    var textGridExists = hasTextGrid;
                    if (effectiveForce || !textGridExists)
                    {
                        await WaitAsync(options.Concurrency?.MfaSemaphore, cancellationToken).ConfigureAwait(false);
                        string? workspaceRoot = null;
                        EnvironmentVariableScope? mfaRootScope = null;
                        try
                        {
                            workspaceRoot = options.Concurrency?.RentMfaWorkspace();
                            var useDedicatedProcess = options.MfaOptions?.UseDedicatedProcess ?? false;
                            var requiresWorkspaceBinding = !useDedicatedProcess && !string.IsNullOrWhiteSpace(workspaceRoot);
                            if (requiresWorkspaceBinding)
                            {
                                mfaRootScope = new EnvironmentVariableScope("MFA_ROOT_DIR", workspaceRoot!);
                                MfaProcessSupervisor.Shutdown();
                            }

                            var mfaOptions = (options.MfaOptions ?? new RunMfaOptions()) with
                            {
                                AudioFile = options.AudioFile,
                                HydrateFile = paths.HydrateFile,
                                TextGridFile = paths.TextGridFile,
                                WorkspaceRoot = workspaceRoot ?? options.MfaOptions?.WorkspaceRoot
                            };

                            var result = await _runMfa.ExecuteAsync(chapter, mfaOptions, cancellationToken)
                                .ConfigureAwait(false);
                            mfaRan = true;
                            hasTextGrid = HasTextGridDocument();
                            textGridExists = hasTextGrid;

                            if (result.ProblematicChunkIndices.Count > 0)
                            {
                                problematicChunkIndices = result.ProblematicChunkIndices;
                                Log.Info(
                                    "MFA flagged {Count} problematic chunk(s) for {Chapter}; deferring recovery so scheduler can requeue without hijacking ASR concurrency",
                                    problematicChunkIndices.Count,
                                    chapter.Descriptor.ChapterId);
                            }
                        }
                        finally
                        {
                            mfaRootScope?.Dispose();
                            options.Concurrency?.ReturnMfaWorkspace(workspaceRoot);
                            Release(options.Concurrency?.MfaSemaphore);
                        }
                    }

                    paths = ResolveArtifactPaths(options, chapter);
                    var textGridArtifact = CreateArtifact("text-grid", RunArtifactKind.Output, paths.TextGridFile);
                    if (!textGridArtifact.Exists)
                    {
                        var failure = PipelineRunContract.CreateMissingArtifactFailure(PipelineStage.Mfa, textGridArtifact);
                        throw FailStage(
                            PipelineStage.Mfa,
                            new InvalidOperationException(failure.Message),
                            executed: mfaRan,
                            overrideArtifacts: [textGridArtifact],
                            overrideFailure: failure);
                    }

                    if (hasTextGrid)
                    {
                        var mergeOptions = options.MergeOptions ?? new MergeTimingsOptions();
                        mergeOptions = mergeOptions with
                        {
                            TextGridFile = paths.TextGridFile
                        };

                        await _mergeTimings.ExecuteAsync(chapter, mergeOptions, cancellationToken).ConfigureAwait(false);
                        hasHydrate = true;
                        hasTranscript = true;
                        hasTextGrid = true;
                    }

                    return (mfaRan, mfaRan ? "MFA aligned" : "MFA cached", [textGridArtifact]);
                }).ConfigureAwait(false);

            if (!options.SkipTreatedCopy)
            {
                CopyTreatedAudio(options.AudioFile, paths.TreatedAudioFile, options.Force);
            }

            var treatedArtifact = CreateArtifact("treated-audio", RunArtifactKind.Output, paths.TreatedAudioFile);
            TrackArtifacts([treatedArtifact]);

            handle.Save();

            var overallArtifacts = aggregateArtifacts.Values
                .OrderBy(artifact => artifact.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (problematicChunkIndices.Count > 0)
            {
                Emit(RunProgressUpdate.CreateStatus(
                    moduleId,
                    RunState.Running,
                    $"Recovery requested for {problematicChunkIndices.Count} chunk(s)",
                    PipelineRunContract.StageProgress(PipelineStage.Mfa),
                    PipelineRunContract.PipelineStageName,
                    overallArtifacts,
                    itemId));

                return BuildResult(RunState.Running, failure: null);
            }

            Emit(RunProgressUpdate.CreateStatus(
                moduleId,
                RunState.Completed,
                "Pipeline complete",
                1d,
                PipelineRunContract.PipelineStageName,
                overallArtifacts,
                itemId));

            return BuildResult(RunState.Completed, failure: null);
        }
        catch (PipelineRunException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            Emit(RunProgressUpdate.CreateFailure(
                moduleId,
                new RunFailure(RunFailureKind.Cancelled, "Pipeline cancelled.", PipelineRunContract.PipelineStageName),
                artifacts: ResolveAllArtifacts(paths),
                itemId: itemId));
            throw;
        }
        catch (Exception ex)
        {
            throw FailStage(stage: null, ex, overrideArtifacts: ResolveAllArtifacts(paths));
        }
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

    private async Task<BuildBookIndexResult?> EnsureBookIndexAsync(
        PipelineRunOptions options,
        CancellationToken cancellationToken)
    {
        options.BookIndexFile.Refresh();
        var requestRebuild = options.ForceIndex;
        var exists = options.BookIndexFile.Exists;
        var concurrency = options.Concurrency;

        if (requestRebuild && concurrency is not null && !concurrency.TryClaimBookIndexForce())
        {
            return null;
        }

        if (!requestRebuild && exists)
        {
            return null;
        }

        await WaitAsync(concurrency?.BookIndexSemaphore, cancellationToken).ConfigureAwait(false);
        try
        {
            options.BookIndexFile.Refresh();
            if (!requestRebuild && options.BookIndexFile.Exists)
            {
                return null;
            }

            var result = await _buildBookIndex.ExecuteAsync(
                    BuildBookIndexRequest.FromPipelineOptions(options),
                    cancellationToken)
                .ConfigureAwait(false);

            options.BookIndexFile.Refresh();

            Log.Debug(
                "Book index prepared for {Book}: {CacheDisposition} (Rebuilt={WasRebuilt}, PhonemesBackfilled={PhonemesBackfilled})",
                options.BookFile.FullName,
                result.CacheDisposition,
                result.WasRebuilt,
                result.PhonemesBackfilled);

            return result;
        }
        finally
        {
            Release(concurrency?.BookIndexSemaphore);
        }
    }

    private static PipelineArtifactPaths ResolveArtifactPaths(PipelineRunOptions options, ChapterContext? chapter = null)
    {
        var chapterDirectory = chapter?.Descriptor.RootPath is { Length: > 0 } rootPath
            ? new DirectoryInfo(rootPath)
            : options.ChapterDirectory
              ?? new DirectoryInfo(Path.Combine(
                  options.BookIndexFile.Directory?.FullName
                  ?? options.BookFile.Directory?.FullName
                  ?? Directory.GetCurrentDirectory(),
                  options.ChapterId));

        var chapterId = chapter?.Descriptor.ChapterId ?? options.ChapterId;
        var bookIndexFile = new FileInfo(Path.GetFullPath(options.BookIndexFile.FullName));
        var asrFile = chapter?.Documents.GetAsrFile()
                      ?? new FileInfo(Path.Combine(chapterDirectory.FullName, $"{chapterId}.asr.json"));
        var anchorFile = chapter?.Documents.GetAnchorsFile()
                         ?? new FileInfo(Path.Combine(chapterDirectory.FullName, $"{chapterId}.align.anchors.json"));
        var transcriptFile = chapter?.Documents.GetTranscriptFile()
                             ?? new FileInfo(Path.Combine(chapterDirectory.FullName, $"{chapterId}.align.tx.json"));
        var hydrateFile = chapter?.Documents.GetHydratedTranscriptFile()
                          ?? new FileInfo(Path.Combine(chapterDirectory.FullName, $"{chapterId}.align.hydrate.json"));
        var textGridFile = options.MfaOptions?.TextGridFile
                           ?? chapter?.Documents.GetTextGridFile()
                           ?? new FileInfo(Path.Combine(chapterDirectory.FullName, "alignment", "mfa", $"{chapterId}.TextGrid"));
        var treatedAudioFile = options.TreatedCopyFile
                               ?? new FileInfo(Path.Combine(chapterDirectory.FullName, $"{chapterId}.treated.wav"));

        return new PipelineArtifactPaths(
            bookIndexFile,
            asrFile,
            anchorFile,
            transcriptFile,
            hydrateFile,
            textGridFile,
            treatedAudioFile);
    }

    private static IReadOnlyList<RunArtifact> ResolveStageArtifacts(PipelineStage stage, PipelineArtifactPaths paths)
        => stage switch
        {
            PipelineStage.BookIndex => [CreateArtifact("book-index", RunArtifactKind.Output, paths.BookIndexFile)],
            PipelineStage.Asr => [CreateArtifact("asr", RunArtifactKind.Output, paths.AsrFile)],
            PipelineStage.Anchors => [CreateArtifact("anchors", RunArtifactKind.Output, paths.AnchorFile)],
            PipelineStage.Transcript => [CreateArtifact("transcript", RunArtifactKind.Output, paths.TranscriptFile)],
            PipelineStage.Hydrate => [CreateArtifact("hydrate", RunArtifactKind.Output, paths.HydrateFile)],
            PipelineStage.Mfa => [CreateArtifact("text-grid", RunArtifactKind.Output, paths.TextGridFile)],
            _ => ResolveAllArtifacts(paths)
        };

    private static IReadOnlyList<RunArtifact> ResolveAllArtifacts(PipelineArtifactPaths paths)
    {
        return
        [
            CreateArtifact("book-index", RunArtifactKind.Output, paths.BookIndexFile),
            CreateArtifact("asr", RunArtifactKind.Output, paths.AsrFile),
            CreateArtifact("anchors", RunArtifactKind.Output, paths.AnchorFile),
            CreateArtifact("transcript", RunArtifactKind.Output, paths.TranscriptFile),
            CreateArtifact("hydrate", RunArtifactKind.Output, paths.HydrateFile),
            CreateArtifact("text-grid", RunArtifactKind.Output, paths.TextGridFile),
            CreateArtifact("treated-audio", RunArtifactKind.Output, paths.TreatedAudioFile)
        ];
    }

    private static RunFailure MapFailure(PipelineStage? stage, Exception exception)
    {
        var stageName = stage is PipelineStage pipelineStage
            ? PipelineRunContract.ToStageName(pipelineStage)
            : PipelineRunContract.PipelineStageName;

        return exception switch
        {
            BuildBookIndexCommandException buildBookIndexException => buildBookIndexException.Failure,
            TimeoutException => new RunFailure(RunFailureKind.Timeout, exception.Message, stageName),
            FileNotFoundException => new RunFailure(RunFailureKind.Validation, exception.Message, stageName),
            DirectoryNotFoundException => new RunFailure(RunFailureKind.Validation, exception.Message, stageName),
            ArgumentException => new RunFailure(RunFailureKind.Validation, exception.Message, stageName),
            IOException => new RunFailure(RunFailureKind.Dependency, exception.Message, stageName),
            InvalidOperationException => new RunFailure(RunFailureKind.Execution, exception.Message, stageName),
            _ => new RunFailure(RunFailureKind.Execution, exception.Message, stageName)
        };
    }

    private static RunArtifact CreateArtifact(string name, RunArtifactKind kind, FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);
        file.Refresh();
        return new RunArtifact(name, kind, file.FullName, file.Exists);
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

    private sealed record PipelineArtifactPaths(
        FileInfo BookIndexFile,
        FileInfo AsrFile,
        FileInfo AnchorFile,
        FileInfo TranscriptFile,
        FileInfo HydrateFile,
        FileInfo TextGridFile,
        FileInfo TreatedAudioFile);

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly string _name;
        private readonly string? _previousValue;
        private readonly bool _changed;

        public EnvironmentVariableScope(string name, string value)
        {
            _name = name;
            _previousValue = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
            _changed = true;
        }

        public void Dispose()
        {
            if (!_changed)
            {
                return;
            }

            Environment.SetEnvironmentVariable(_name, _previousValue);
        }
    }
}
