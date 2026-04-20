using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Ams.Cli.Utilities;
using Ams.Core.Application.Benchmark;
using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Processes;
using Ams.Core.Application.Runs;
using Ams.Core.Asr;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Core.Runtime.Workspace;
using Ams.Core.Services.Integrations.FFmpeg;

namespace Ams.Cli.Commands;

public static class BenchmarkCommand
{
    private const int ExitSuccess = 0;
    private const int ExitUsageError = 2;
    private const int ExitDeterministicRejected = 3;
    private const int ExitCancelled = 4;
    private const int ExitExecutionFailure = 5;

    private static readonly BenchmarkRunManifestValidator ManifestValidator = new();

    public static Command Create(
        BenchmarkRunService benchmarkRunService,
        BenchmarkCompareService benchmarkCompareService,
        Func<IWorkspace, BenchmarkRunRequest, CancellationToken, Task<BenchmarkRunResult>>? executeRunAsync = null,
        Func<BenchmarkCompareRequest, CancellationToken, Task<BenchmarkCompareResult>>? executeCompareAsync = null)
    {
        ArgumentNullException.ThrowIfNull(benchmarkRunService);
        ArgumentNullException.ThrowIfNull(benchmarkCompareService);

        executeRunAsync ??= benchmarkRunService.ExecuteAsync;
        executeCompareAsync ??= benchmarkCompareService.ExecuteAsync;

        var benchmark = new Command("benchmark", "Run benchmark workflows with deterministic contract reporting.");
        benchmark.AddCommand(CreateRunCommand(executeRunAsync));
        benchmark.AddCommand(CreateCompareCommand(executeCompareAsync));
        benchmark.AddCommand(CreateListCommand());
        return benchmark;
    }

    private static Command CreateRunCommand(
        Func<IWorkspace, BenchmarkRunRequest, CancellationToken, Task<BenchmarkRunResult>> executeRunAsync)
    {
        var run = new Command("run", "Run benchmark execution and emit manifest/invalid-run artifact summary.");

        var bookOption = new Option<FileInfo?>(
            "--book",
            "Path to manuscript source used for book indexing (defaults to discovered manuscript in current workspace)." );
        bookOption.AddAlias("-b");

        var audioOption = new Option<FileInfo[]>(
            "--audio",
            "One or more chapter WAV files to benchmark (defaults to active REPL chapter when omitted).")
        {
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true
        };
        audioOption.AddAlias("-a");

        var chapterIdOption = new Option<string?>(
            "--chapter-id",
            () => null,
            "Override chapter identifier for single-audio invocations (defaults to audio stem)");

        var workDirOption = new Option<DirectoryInfo?>(
            "--work-dir",
            () => null,
            "Working directory used for chapter folders and default output roots");

        var outputRootOption = new Option<DirectoryInfo?>(
            "--output-root",
            () => null,
            "Benchmark artifact output directory (defaults to <work-dir>/benchmark-runs)");

        var bookIndexOption = new Option<FileInfo?>(
            "--book-index",
            () => null,
            "Book index path (defaults to <work-dir>/book-index.json)");

        var deterministicOption = new Option<bool>(
            "--deterministic",
            () => false,
            "Enable deterministic benchmark gate (fail-closed)");

        var asrModelOption = new Option<string?>(
            "--asr-model",
            () => null,
            "ASR model path for benchmark provenance; deterministic mode requires existing pinned file path");
        asrModelOption.AddAlias("-m");

        var asrEngineOption = new Option<string>(
            "--asr-engine",
            () => AsrEngineConfig.Resolve().ToString().ToLowerInvariant(),
            "ASR engine (whisper or whisperx)");

        var languageOption = new Option<string>("--language", () => "en", "ASR language code");
        languageOption.AddAlias("-l");

        var dtwTimestampsOption = new Option<bool>(
            "--dtw-timestamps",
            () => false,
            "Enable DTW timestamp refinement for Whisper ASR");

        var flashAttentionOption = new Option<bool>(
            "--flash-attention",
            () => false,
            "Enable FlashAttention for Whisper ASR");

        var forceOption = new Option<bool>(
            "--force",
            () => false,
            "Force pipeline artifact regeneration during benchmark execution");

        var forceIndexOption = new Option<bool>(
            "--force-index",
            () => false,
            "Force book-index rebuild before benchmark execution");

        var noChunkPlanOption = new Option<bool>(
            "--no-chunk-plan",
            () => false,
            "Disable ASR chunk planning for benchmark run");

        var noChunkedMfaOption = new Option<bool>(
            "--no-chunked-mfa",
            () => false,
            "Disable chunked MFA path for benchmark run");

        var runIdOption = new Option<string?>(
            "--run-id",
            () => null,
            "Optional explicit run identifier for manifest naming");

        run.AddOption(bookOption);
        run.AddOption(audioOption);
        run.AddOption(chapterIdOption);
        run.AddOption(workDirOption);
        run.AddOption(outputRootOption);
        run.AddOption(bookIndexOption);
        run.AddOption(deterministicOption);
        run.AddOption(asrModelOption);
        run.AddOption(asrEngineOption);
        run.AddOption(languageOption);
        run.AddOption(dtwTimestampsOption);
        run.AddOption(flashAttentionOption);
        run.AddOption(forceOption);
        run.AddOption(forceIndexOption);
        run.AddOption(noChunkPlanOption);
        run.AddOption(noChunkedMfaOption);
        run.AddOption(runIdOption);

        run.SetHandler(async context =>
        {
            var cancellationToken = context.GetCancellationToken();

            BenchmarkRunRequest request;
            IWorkspace workspace;

            try
            {
                var parse = context.ParseResult;

                var resolvedBook = CommandInputResolver.ResolveBookSource(parse.GetValueForOption(bookOption));
                var bookFile = NormalizeRequiredExistingFile(resolvedBook, "--book");

                var providedAudioFiles = parse.GetValueForOption(audioOption) ?? [];
                FileInfo[] normalizedAudioFiles;
                if (providedAudioFiles.Length == 0)
                {
                    var activeChapterAudio = CommandInputResolver.RequireAudio(null);
                    normalizedAudioFiles =
                    [
                        NormalizeRequiredExistingFile(activeChapterAudio, "--audio")
                    ];
                }
                else
                {
                    normalizedAudioFiles = providedAudioFiles
                        .Select((file, index) => NormalizeRequiredExistingFile(file, $"--audio[{index}]"))
                        .ToArray();
                }

                var chapterIdOverride = NormalizeOptionalText(parse.GetValueForOption(chapterIdOption));
                if (chapterIdOverride is not null && normalizedAudioFiles.Length != 1)
                {
                    throw new InvalidOperationException("--chapter-id is only valid when exactly one --audio source is provided.");
                }

                var inferredIds = normalizedAudioFiles
                    .Select(file => BuildChapterId(file.Name))
                    .ToArray();

                var duplicateChapterIds = inferredIds
                    .GroupBy(id => id, StringComparer.OrdinalIgnoreCase)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToArray();

                if (duplicateChapterIds.Length > 0)
                {
                    throw new InvalidOperationException(
                        $"Duplicate chapter selectors detected from audio stems: {string.Join(", ", duplicateChapterIds)}. Use unique audio names or run chapters separately.");
                }

                var deterministic = parse.GetValueForOption(deterministicOption);
                var requestedModel = NormalizeOptionalText(parse.GetValueForOption(asrModelOption));

                if (deterministic)
                {
                    if (requestedModel is null)
                    {
                        throw new InvalidOperationException("--deterministic requires --asr-model with a pinned existing model file path.");
                    }

                    if (AsrEngineConfig.ParseModelAlias(requestedModel).HasValue && !LooksLikePath(requestedModel))
                    {
                        throw new InvalidOperationException(
                            "--deterministic rejects alias-only --asr-model values. Provide a concrete model file path.");
                    }

                    string normalizedModelPath;
                    try
                    {
                        normalizedModelPath = AmsPathResolver.NormalizePath(requestedModel);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"--deterministic model path is invalid: {ex.Message}", ex);
                    }

                    if (!File.Exists(normalizedModelPath))
                    {
                        throw new InvalidOperationException(
                            $"--deterministic requires an existing --asr-model file path. Not found: {normalizedModelPath}");
                    }
                }

                var asrEngine = AsrEngineConfig.Resolve(parse.GetValueForOption(asrEngineOption));
                var language = parse.GetValueForOption(languageOption) ?? "en";
                var dtwTimestamps = parse.GetValueForOption(dtwTimestampsOption);
                var flashAttention = parse.GetValueForOption(flashAttentionOption);
                var force = parse.GetValueForOption(forceOption);
                var forceIndex = parse.GetValueForOption(forceIndexOption);
                var disableChunkPlan = parse.GetValueForOption(noChunkPlanOption);
                var disableChunkedMfa = parse.GetValueForOption(noChunkedMfaOption);

                var workDir = ResolveWorkDir(parse.GetValueForOption(workDirOption), normalizedAudioFiles[0]);
                Directory.CreateDirectory(workDir.FullName);

                var bookIndexFile = parse.GetValueForOption(bookIndexOption) is { } explicitBookIndex
                    ? AmsPathResolver.NormalizeFile(explicitBookIndex)
                    : new FileInfo(Path.Combine(workDir.FullName, "book-index.json"));

                var outputRoot = parse.GetValueForOption(outputRootOption) is { } explicitOutputRoot
                    ? AmsPathResolver.NormalizeDirectory(explicitOutputRoot)
                    : new DirectoryInfo(Path.Combine(workDir.FullName, "benchmark-runs"));

                var chapterRequests = BuildChapterRequests(normalizedAudioFiles, chapterIdOverride, workDir);
                var pipelineOptions = BuildPipelineOptions(
                    bookFile,
                    bookIndexFile,
                    chapterRequests[0],
                    requestedModel,
                    asrEngine,
                    language,
                    dtwTimestamps,
                    flashAttention,
                    force,
                    forceIndex,
                    disableChunkPlan,
                    disableChunkedMfa);

                request = new BenchmarkRunRequest(
                    deterministic,
                    requestedModel,
                    pipelineOptions,
                    chapterRequests,
                    outputRoot,
                    runId: NormalizeOptionalText(parse.GetValueForOption(runIdOption)),
                    moduleId: ModuleIds.BenchmarkRun);

                workspace = CommandInputResolver.ResolveWorkspace(bookIndexFile);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or FileNotFoundException or DirectoryNotFoundException)
            {
                WriteLine(context.Console.Error, $"Benchmark run argument validation failed: {ex.Message}");
                context.ExitCode = ExitUsageError;
                return;
            }

            BenchmarkRunResult result;
            var capturedProcessingActivities = new List<AudioProcessorActivity>();
            using var processingActivityCapture = AudioProcessor.BeginActivityCapture(activity =>
                capturedProcessingActivities.Add(activity));

            try
            {
                result = await executeRunAsync(workspace, request, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                WriteLine(context.Console.Error, "Benchmark run cancelled.");
                context.ExitCode = ExitCancelled;
                return;
            }
            catch (Exception ex)
            {
                WriteLine(context.Console.Error, $"Benchmark run execution failed: {ex.Message}");
                context.ExitCode = ExitExecutionFailure;
                return;
            }

            if (!TryValidateResultContract(result, out var contractError))
            {
                WriteLine(context.Console.Error, $"Benchmark run contract error: {contractError}");
                context.ExitCode = ExitUsageError;
                return;
            }

            WriteSummary(context.Console.Out, workspace.RootPath, result, capturedProcessingActivities);
            context.ExitCode = MapExitCode(result);
        });

        return run;
    }

    private static Command CreateCompareCommand(
        Func<BenchmarkCompareRequest, CancellationToken, Task<BenchmarkCompareResult>> executeCompareAsync)
    {
        var compare = new Command("compare", "Compare baseline and candidate benchmark artifacts with fail-closed contract checks.");

        var selectorsArgument = new Argument<string[]>("selectors")
        {
            Arity = ArgumentArity.ZeroOrMore,
            Description = "Optional baseline/candidate selectors (file paths or zero-based indexes from benchmark list)."
        };

        var baselineOption = new Option<FileInfo?>(
            "--baseline",
            "Baseline benchmark artifact (.manifest.json or .invalid-run.json)");
        baselineOption.AddAlias("-b");

        var candidateOption = new Option<FileInfo?>(
            "--candidate",
            "Candidate benchmark artifact (.manifest.json or .invalid-run.json)");
        candidateOption.AddAlias("-c");

        var artifactsDirOption = new Option<DirectoryInfo?>(
            "--dir",
            () => null,
            "Artifact directory used for index selectors (defaults to ./benchmark-runs)." );
        artifactsDirOption.AddAlias("--artifacts-dir");

        var outputRootOption = new Option<DirectoryInfo?>(
            "--output-root",
            () => null,
            "Compare artifact output directory (defaults to baseline artifact directory)");

        var compareIdOption = new Option<string?>(
            "--compare-id",
            () => null,
            "Optional explicit compare identifier for artifact naming");

        compare.AddArgument(selectorsArgument);
        compare.AddOption(baselineOption);
        compare.AddOption(candidateOption);
        compare.AddOption(artifactsDirOption);
        compare.AddOption(outputRootOption);
        compare.AddOption(compareIdOption);

        compare.SetHandler(async context =>
        {
            var cancellationToken = context.GetCancellationToken();

            BenchmarkCompareRequest request;
            string displayRoot;

            try
            {
                var parse = context.ParseResult;
                var selectors = parse.GetValueForArgument(selectorsArgument) ?? [];
                if (selectors.Length > 2)
                {
                    throw new InvalidOperationException(
                        "Compare accepts at most two positional selectors: baseline and candidate.");
                }

                var artifactsDirectory = parse.GetValueForOption(artifactsDirOption) is { } explicitArtifactsDirectory
                    ? AmsPathResolver.NormalizeDirectory(explicitArtifactsDirectory)
                    : new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "benchmark-runs"));

                var baselineSelector = parse.GetValueForOption(baselineOption);
                var candidateSelector = parse.GetValueForOption(candidateOption);

                var pendingSelectors = new Queue<string>(selectors);

                if (baselineSelector is null && pendingSelectors.TryDequeue(out var baselineToken))
                {
                    baselineSelector = ResolveCompareSelectorToFile(baselineToken, artifactsDirectory, "--baseline");
                }

                if (candidateSelector is null && pendingSelectors.TryDequeue(out var candidateToken))
                {
                    candidateSelector = ResolveCompareSelectorToFile(candidateToken, artifactsDirectory, "--candidate");
                }

                if (pendingSelectors.Count > 0)
                {
                    throw new InvalidOperationException(
                        "Too many positional selectors were provided. Expected baseline and candidate only.");
                }

                if (baselineSelector is null || candidateSelector is null)
                {
                    throw new InvalidOperationException(
                        "Provide baseline and candidate via --baseline/--candidate or positional selectors (example: benchmark compare 0 1)."
                    );
                }

                var baselineArtifact = NormalizeRequiredExistingCompareArtifact(baselineSelector, "--baseline");
                var candidateArtifact = NormalizeRequiredExistingCompareArtifact(candidateSelector, "--candidate");

                var outputRoot = parse.GetValueForOption(outputRootOption) is { } explicitOutputRoot
                    ? AmsPathResolver.NormalizeDirectory(explicitOutputRoot)
                    : ResolveCompareOutputRoot(baselineArtifact);

                Directory.CreateDirectory(outputRoot.FullName);

                request = new BenchmarkCompareRequest(
                    baselineArtifact: baselineArtifact,
                    candidateArtifact: candidateArtifact,
                    outputRoot: outputRoot,
                    compareId: NormalizeOptionalText(parse.GetValueForOption(compareIdOption)),
                    moduleId: ModuleIds.BenchmarkCompare);

                displayRoot = outputRoot.FullName;
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or FileNotFoundException or DirectoryNotFoundException)
            {
                WriteLine(context.Console.Error, $"Benchmark compare argument validation failed: {ex.Message}");
                context.ExitCode = ExitUsageError;
                return;
            }

            BenchmarkCompareResult result;
            try
            {
                result = await executeCompareAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                WriteLine(context.Console.Error, "Benchmark compare cancelled.");
                context.ExitCode = ExitCancelled;
                return;
            }
            catch (Exception ex)
            {
                WriteLine(context.Console.Error, $"Benchmark compare execution failed: {ex.Message}");
                context.ExitCode = ExitExecutionFailure;
                return;
            }

            if (!TryValidateCompareResultContract(result, out var contractError))
            {
                WriteLine(context.Console.Error, $"Benchmark compare contract error: {contractError}");
                context.ExitCode = ExitUsageError;
                return;
            }

            WriteCompareSummary(context.Console.Out, displayRoot, result);
            context.ExitCode = MapCompareExitCode(result);
        });

        return compare;
    }

    private static Command CreateListCommand()
    {
        var list = new Command("list", "List benchmark compare artifacts with stable index selectors.");
        list.AddAlias("ls");

        var artifactsDirOption = new Option<DirectoryInfo?>(
            "--dir",
            () => null,
            "Artifact directory to inspect (defaults to ./benchmark-runs).");
        artifactsDirOption.AddAlias("--artifacts-dir");

        list.AddOption(artifactsDirOption);

        list.SetHandler(context =>
        {
            try
            {
                var parse = context.ParseResult;
                var artifactsDirectory = parse.GetValueForOption(artifactsDirOption) is { } explicitArtifactsDirectory
                    ? AmsPathResolver.NormalizeDirectory(explicitArtifactsDirectory)
                    : new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "benchmark-runs"));

                var entries = EnumerateCompareArtifacts(artifactsDirectory);

                WriteLine(context.Console.Out, $"Benchmark artifacts: {artifactsDirectory.FullName}");
                if (entries.Length == 0)
                {
                    WriteLine(context.Console.Out, "(no .manifest.json or .invalid-run.json artifacts found)");
                    context.ExitCode = ExitSuccess;
                    return;
                }

                foreach (var entry in entries)
                {
                    var deterministicText = entry.Deterministic switch
                    {
                        true => "yes",
                        false => "no",
                        _ => "unknown"
                    };

                    var chapterCountText = entry.ChapterCount?.ToString(CultureInfo.InvariantCulture) ?? "unknown";
                    var timestampText = (entry.RecordedAtUtc ?? entry.ArtifactFile.LastWriteTimeUtc).ToString(
                        "yyyy-MM-dd HH:mm:ss 'UTC'",
                        CultureInfo.InvariantCulture);

                    var malformedSuffix = entry.IsMalformed
                        ? " malformed"
                        : string.Empty;

                    WriteLine(
                        context.Console.Out,
                        $"[{entry.Index}] run={entry.RunId}, kind={entry.ArtifactKind}{malformedSuffix}, phase={ToKebabToken(entry.Phase)}, state={ToKebabToken(entry.State)}, chapters={chapterCountText}, deterministic={deterministicText}, at={timestampText}");
                    WriteLine(context.Console.Out, $"    file={entry.ArtifactFile.Name}");
                }

                context.ExitCode = ExitSuccess;
            }
            catch (Exception ex) when (ex is ArgumentException or DirectoryNotFoundException or IOException)
            {
                WriteLine(context.Console.Error, $"Benchmark list failed: {ex.Message}");
                context.ExitCode = ExitUsageError;
            }
        });

        return list;
    }

    private static PipelineRunOptions BuildPipelineOptions(
        FileInfo bookFile,
        FileInfo bookIndexFile,
        BenchmarkRunChapterRequest primaryChapter,
        string? requestedModel,
        AsrEngine asrEngine,
        string language,
        bool dtwTimestamps,
        bool flashAttention,
        bool force,
        bool forceIndex,
        bool disableChunkPlan,
        bool disableChunkedMfa)
    {
        return new PipelineRunOptions
        {
            BookFile = bookFile,
            BookIndexFile = bookIndexFile,
            AudioFile = primaryChapter.AudioFile,
            ChapterDirectory = primaryChapter.ChapterDirectory,
            ChapterId = primaryChapter.ChapterId,
            ModuleId = ModuleIds.BenchmarkRun,
            Force = force,
            ForceIndex = forceIndex,
            StartStage = PipelineStage.BookIndex,
            EndStage = PipelineStage.Mfa,
            TranscriptOptions = new GenerateTranscriptOptions
            {
                Engine = asrEngine,
                Model = requestedModel,
                Language = language,
                EnableWordTimestamps = true,
                EnableDtwTimestamps = dtwTimestamps,
                EnableFlashAttention = flashAttention,
                DisableChunkPlan = disableChunkPlan
            },
            DisableChunkPlan = disableChunkPlan,
            DisableChunkedMfa = disableChunkedMfa,
            MfaOptions = new RunMfaOptions
            {
                DisableChunkedMfa = disableChunkedMfa
            }
        };
    }

    private static BenchmarkRunChapterRequest[] BuildChapterRequests(
        IReadOnlyList<FileInfo> audioFiles,
        string? chapterIdOverride,
        DirectoryInfo workDir)
    {
        var chapterIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var requests = new List<BenchmarkRunChapterRequest>(audioFiles.Count);

        for (var i = 0; i < audioFiles.Count; i++)
        {
            var audioFile = audioFiles[i];
            var chapterId = i == 0 && chapterIdOverride is not null
                ? chapterIdOverride
                : BuildChapterId(audioFile.Name);

            if (!chapterIds.Add(chapterId))
            {
                throw new InvalidOperationException(
                    $"Duplicate chapter id '{chapterId}' resolved from --audio inputs. Use unique chapter selectors.");
            }

            var chapterDirectory = new DirectoryInfo(Path.Combine(workDir.FullName, chapterId));
            requests.Add(new BenchmarkRunChapterRequest(chapterId, audioFile, chapterDirectory));
        }

        return requests.ToArray();
    }

    private static DirectoryInfo ResolveWorkDir(DirectoryInfo? explicitWorkDir, FileInfo firstAudio)
    {
        if (explicitWorkDir is not null)
        {
            return AmsPathResolver.NormalizeDirectory(explicitWorkDir);
        }

        if (firstAudio.Directory is not null)
        {
            return firstAudio.Directory;
        }

        return new DirectoryInfo(Directory.GetCurrentDirectory());
    }

    private static DirectoryInfo ResolveCompareOutputRoot(FileInfo baselineArtifact)
    {
        return baselineArtifact.Directory
               ?? new DirectoryInfo(Directory.GetCurrentDirectory());
    }

    private static FileInfo ResolveCompareSelectorToFile(
        string selector,
        DirectoryInfo artifactsDirectory,
        string optionName)
    {
        var normalizedSelector = NormalizeOptionalText(selector);
        if (normalizedSelector is null)
        {
            throw new InvalidOperationException($"{optionName} selector was blank.");
        }

        if (int.TryParse(normalizedSelector, NumberStyles.Integer, CultureInfo.InvariantCulture, out var artifactIndex))
        {
            if (artifactIndex < 0)
            {
                throw new InvalidOperationException($"{optionName} index cannot be negative. Received: {artifactIndex}.");
            }

            var entries = EnumerateCompareArtifacts(artifactsDirectory);
            if (entries.Length == 0)
            {
                throw new FileNotFoundException(
                    $"No benchmark artifacts were found in {artifactsDirectory.FullName}. Expected .manifest.json or .invalid-run.json files.");
            }

            if (artifactIndex >= entries.Length)
            {
                throw new InvalidOperationException(
                    $"{optionName} index {artifactIndex} is out of range. Available index range: 0..{entries.Length - 1}.");
            }

            return entries[artifactIndex].ArtifactFile;
        }

        return new FileInfo(normalizedSelector);
    }

    private static BenchmarkArtifactListEntry[] EnumerateCompareArtifacts(DirectoryInfo artifactsDirectory)
    {
        var normalizedDirectory = AmsPathResolver.NormalizeDirectory(artifactsDirectory);
        normalizedDirectory.Refresh();

        if (!normalizedDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Benchmark artifact directory was not found: {normalizedDirectory.FullName}");
        }

        return normalizedDirectory
            .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
            .Where(file => IsSupportedCompareArtifactKind(file.Name))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ThenBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
            .Select((file, index) => BuildArtifactListEntry(file, index))
            .ToArray();
    }

    private static BenchmarkArtifactListEntry BuildArtifactListEntry(FileInfo artifactFile, int index)
    {
        var artifactKind = artifactFile.Name.EndsWith(".invalid-run.json", StringComparison.OrdinalIgnoreCase)
            ? "invalid-run"
            : "manifest";

        var fallbackRunId = DeriveArtifactStem(artifactFile.Name);

        try
        {
            var payload = File.ReadAllText(artifactFile.FullName);
            using var jsonDocument = JsonDocument.Parse(payload);
            var root = jsonDocument.RootElement;

            var runId = TryGetString(root, "runId") ?? fallbackRunId;
            var phase = TryGetString(root, "phase")
                        ?? (artifactKind == "invalid-run" ? BenchmarkRunPhase.Invalid.ToString() : "unknown");
            var state = TryGetString(root, "state")
                        ?? (artifactKind == "invalid-run" ? RunState.Completed.ToString() : "unknown");

            var deterministic = TryGetBool(root, "deterministic");
            var chapterCount = TryGetArrayCount(root, "chapterSet");
            var recordedAtUtc = TryGetDateTimeOffset(root, "completedAtUtc")
                                ?? TryGetDateTimeOffset(root, "rejectedAtUtc")
                                ?? TryGetDateTimeOffset(root, "startedAtUtc");

            return new BenchmarkArtifactListEntry(
                index,
                artifactFile,
                artifactKind,
                runId,
                phase,
                state,
                deterministic,
                chapterCount,
                recordedAtUtc,
                IsMalformed: false,
                MalformedReason: null);
        }
        catch (Exception ex)
        {
            return new BenchmarkArtifactListEntry(
                index,
                artifactFile,
                artifactKind,
                fallbackRunId,
                Phase: "malformed",
                State: "malformed",
                Deterministic: null,
                ChapterCount: null,
                RecordedAtUtc: null,
                IsMalformed: true,
                MalformedReason: ex.Message);
        }
    }

    private static string DeriveArtifactStem(string fileName)
    {
        if (fileName.EndsWith(".manifest.json", StringComparison.OrdinalIgnoreCase))
        {
            return fileName[..^".manifest.json".Length];
        }

        if (fileName.EndsWith(".invalid-run.json", StringComparison.OrdinalIgnoreCase))
        {
            return fileName[..^".invalid-run.json".Length];
        }

        return Path.GetFileNameWithoutExtension(fileName);
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var node) || node.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = node.GetString();
        return NormalizeOptionalText(value);
    }

    private static bool? TryGetBool(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var node))
        {
            return null;
        }

        return node.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static int? TryGetArrayCount(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var node) || node.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return node.GetArrayLength();
    }

    private static DateTimeOffset? TryGetDateTimeOffset(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var node) || node.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var raw = node.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed.ToUniversalTime()
            : null;
    }

    private static FileInfo NormalizeRequiredExistingCompareArtifact(FileInfo? value, string optionName)
    {
        var normalized = NormalizeRequiredExistingFile(value, optionName);

        if (!IsSupportedCompareArtifactKind(normalized.Name))
        {
            throw new ArgumentException(
                $"{optionName} must end with .manifest.json or .invalid-run.json. Received: {normalized.FullName}",
                optionName);
        }

        return normalized;
    }

    private static bool IsSupportedCompareArtifactKind(string fileName)
    {
        return fileName.EndsWith(".manifest.json", StringComparison.OrdinalIgnoreCase)
               || fileName.EndsWith(".invalid-run.json", StringComparison.OrdinalIgnoreCase);
    }

    private static FileInfo NormalizeRequiredExistingFile(FileInfo? value, string optionName)
    {
        if (value is null)
        {
            throw new InvalidOperationException($"{optionName} is required.");
        }

        var normalized = AmsPathResolver.NormalizeFile(value);
        normalized.Refresh();
        if (!normalized.Exists)
        {
            throw new FileNotFoundException($"{optionName} file was not found: {normalized.FullName}", normalized.FullName);
        }

        return normalized;
    }

    private static string BuildChapterId(string sourceFileName)
    {
        const string fallback = "chapter";

        var stem = NormalizeOptionalText(Path.GetFileNameWithoutExtension(sourceFileName));
        if (string.IsNullOrWhiteSpace(stem))
        {
            return fallback;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var normalized = new string(
                stem.Select(character =>
                    Array.IndexOf(invalidChars, character) >= 0
                        ? '_'
                        : character)
                    .ToArray())
            .Trim();

        return string.IsNullOrWhiteSpace(normalized)
            ? fallback
            : normalized;
    }

    private static bool TryValidateResultContract(BenchmarkRunResult? result, out string error)
    {
        if (result is null)
        {
            error = "Result payload was null.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(result.RunId))
        {
            error = "RunId was blank.";
            return false;
        }

        if (result.Deterministic && result.Determinism is null)
        {
            error = "Deterministic run result omitted determinism contract payload.";
            return false;
        }

        if (result.Phase == BenchmarkRunPhase.Invalid && result.InvalidRunFile is null)
        {
            error = "Invalid deterministic result omitted invalid-run artifact file.";
            return false;
        }

        if (result.State == RunState.Failed && result.Failure is null)
        {
            error = "Failed run state omitted failure metadata.";
            return false;
        }

        if (result.State != RunState.Failed && result.Failure is not null)
        {
            error = "Non-failed run state unexpectedly included failure metadata.";
            return false;
        }

        if (result.ArtifactFile is null)
        {
            error = "Run result omitted artifact file path.";
            return false;
        }

        var aggregateValidation = ManifestValidator.Validate(result);
        if (!aggregateValidation.IsValid)
        {
            error = $"Aggregate manifest counters were inconsistent. {aggregateValidation.ToSummary()}";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool TryValidateCompareResultContract(BenchmarkCompareResult? result, out string error)
    {
        if (result is null)
        {
            error = "Compare result payload was null.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(result.CompareId))
        {
            error = "CompareId was blank.";
            return false;
        }

        if (result.Compatibility is null)
        {
            error = "Compare result omitted compatibility payload.";
            return false;
        }

        if (result.ArtifactFile is null)
        {
            error = "Compare result omitted compare artifact path.";
            return false;
        }

        result.ArtifactFile.Refresh();
        if (!result.ArtifactFile.Exists)
        {
            error = $"Compare artifact file does not exist: {result.ArtifactFile.FullName}";
            return false;
        }

        if (result.Compatibility.IsCompatible)
        {
            if (result.Compatibility.Reasons.Count > 0)
            {
                error = "Compatible compare result unexpectedly included incompatibility reasons.";
                return false;
            }

            if (result.MetricVerdicts.Count == 0)
            {
                error = "Compatible compare result omitted metric verdicts.";
                return false;
            }
        }
        else
        {
            if (result.Compatibility.Reasons.Count == 0)
            {
                error = "Incompatible or malformed compare result omitted explicit reasons.";
                return false;
            }

            if (result.MetricVerdicts.Count > 0)
            {
                error = "Incompatible or malformed compare result unexpectedly included metric verdicts.";
                return false;
            }
        }

        foreach (var verdict in result.MetricVerdicts)
        {
            if (string.IsNullOrWhiteSpace(verdict.Metric))
            {
                error = "Compare metric verdict omitted metric identifier.";
                return false;
            }

            if (verdict.Threshold is null)
            {
                error = $"Compare metric verdict '{verdict.Metric}' omitted threshold payload.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(verdict.Rationale))
            {
                error = $"Compare metric verdict '{verdict.Metric}' omitted rationale.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(verdict.Threshold.Rationale))
            {
                error = $"Compare metric verdict '{verdict.Metric}' omitted threshold rationale.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    private static int MapExitCode(BenchmarkRunResult result)
    {
        if (result.Phase == BenchmarkRunPhase.Invalid)
        {
            return ExitDeterministicRejected;
        }

        if (result.State == RunState.Completed)
        {
            return ExitSuccess;
        }

        if (result.Failure?.Kind == RunFailureKind.Cancelled)
        {
            return ExitCancelled;
        }

        return ExitExecutionFailure;
    }

    private static int MapCompareExitCode(BenchmarkCompareResult result)
    {
        if (result.Failure?.Kind == RunFailureKind.Cancelled)
        {
            return ExitCancelled;
        }

        if (result.Compatibility.IsCompatible && result.Failure is null)
        {
            return ExitSuccess;
        }

        return ExitExecutionFailure;
    }

    private static void WriteSummary(
        IStandardStreamWriter output,
        string workspaceRoot,
        BenchmarkRunResult result,
        IReadOnlyList<AudioProcessorActivity>? capturedProcessingActivities = null)
    {
        var deterministicVerdict = result.Determinism is null
            ? (result.Deterministic ? "unknown" : "not-requested")
            : result.Determinism.IsValid
                ? "valid"
                : "invalid";

        WriteLine(output, $"Benchmark run ID: {result.RunId}");
        WriteLine(output, $"Deterministic verdict: {deterministicVerdict}");
        WriteLine(output, $"Run phase: {ToKebabToken(result.Phase.ToString())}");
        WriteLine(output, $"Run state: {ToKebabToken(result.State.ToString())}");
        WriteLine(
            output,
            $"Aggregate chapter-state counts: pending={result.AggregateMetrics.ChapterStates.Pending}, running={result.AggregateMetrics.ChapterStates.Running}, failed={result.AggregateMetrics.ChapterStates.Failed}, completed={result.AggregateMetrics.ChapterStates.Completed}, total={result.AggregateMetrics.ChapterStates.Total}");
        WriteLine(
            output,
            $"Aggregate metrics-state counts: not-run={result.AggregateMetrics.MetricsStates.NotRun}, completed={result.AggregateMetrics.MetricsStates.Completed}, partial={result.AggregateMetrics.MetricsStates.Partial}, failed={result.AggregateMetrics.MetricsStates.Failed}, total={result.AggregateMetrics.MetricsStates.Total}");
        WriteLine(
            output,
            $"Aggregate runtime-ms: pipeline={result.AggregateMetrics.TotalPipelineRuntimeMs}, analysis={result.AggregateMetrics.TotalAnalysisRuntimeMs}, total={result.AggregateMetrics.TotalPipelineRuntimeMs + result.AggregateMetrics.TotalAnalysisRuntimeMs}");
        WriteLine(
            output,
            $"Aggregate quality totals: mismatch-count={result.AggregateMetrics.TotalMismatchCount}, missing-speech-sec={result.AggregateMetrics.TotalMissingSpeechSec.ToString("0.###", CultureInfo.InvariantCulture)}, extra-speech-sec={result.AggregateMetrics.TotalExtraSpeechSec.ToString("0.###", CultureInfo.InvariantCulture)}, qc-flag-count={result.AggregateMetrics.TotalQcFlags}");

        var benchmarkAudioActivities = result.ChapterOutcomes
            .SelectMany(outcome => outcome.Metrics.AudioProcessingActivities)
            .ToArray();

        var emittedAudioActivities = capturedProcessingActivities is { Count: > 0 }
            ? capturedProcessingActivities
            : benchmarkAudioActivities
                .Select(activity => new AudioProcessorActivity(
                    activity.Function,
                    activity.StartedAtUtc,
                    activity.DurationMs,
                    activity.Succeeded,
                    activity.FailureKind,
                    activity.Detail,
                    activity.DurationUs))
                .ToArray();

        var audioActivityRuntimeUs = emittedAudioActivities.Sum(activity => activity.DurationUs);
        var audioActivityFailureCount = emittedAudioActivities.Count(activity => !activity.Succeeded);

        WriteLine(
            output,
            $"Audio processing activity totals: count={emittedAudioActivities.Count}, runtime-ms={FormatRuntimeMilliseconds(audioActivityRuntimeUs)}, failures={audioActivityFailureCount}");

        foreach (var functionGroup in emittedAudioActivities
                     .GroupBy(activity => activity.Function, StringComparer.Ordinal)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var callCount = functionGroup.Count();
            var failureCount = functionGroup.Count(activity => !activity.Succeeded);
            var totalDurationUs = functionGroup.Sum(activity => activity.DurationUs);

            WriteLine(
                output,
                $"Audio processing activity: function={functionGroup.Key}, calls={callCount}, runtime-ms={FormatRuntimeMilliseconds(totalDurationUs)}, failures={failureCount}");
        }

        if (result.Determinism is not null)
        {
            WriteLine(
                output,
                $"Dependency readiness: ffmpeg={ToKebabToken(result.Determinism.Ffmpeg.State.ToString())}, mfa={ToKebabToken(result.Determinism.Mfa.State.ToString())}");

            if (result.Determinism.ReasonCodes.Count > 0)
            {
                var reasons = string.Join(
                    ", ",
                    result.Determinism.ReasonCodes.Select(reason => ToKebabToken(reason.ToString())));
                WriteLine(output, $"Deterministic reason codes: {reasons}");
            }
        }

        if (result.ArtifactFile is not null)
        {
            WriteLine(output, $"Artifact path: {ToDisplayPath(workspaceRoot, result.ArtifactFile.FullName)}");
        }
        else
        {
            WriteLine(output, "Artifact path: (none)");
        }

        if (result.Failure is not null)
        {
            var failureStage = string.IsNullOrWhiteSpace(result.Failure.Stage)
                ? "n/a"
                : result.Failure.Stage;

            WriteLine(
                output,
                $"Failure: kind={ToKebabToken(result.Failure.Kind.ToString())}, stage={failureStage}, message={result.Failure.Message}");
        }
    }

    private static void WriteCompareSummary(IStandardStreamWriter output, string workspaceRoot, BenchmarkCompareResult result)
    {
        WriteLine(output, $"Benchmark compare ID: {result.CompareId}");
        WriteLine(output, $"Compare compatibility: {ToKebabToken(result.Compatibility.Status.ToString())}");

        if (result.Compatibility.Reasons.Count == 0)
        {
            WriteLine(output, "Compare compatibility reason: (none)");
        }
        else
        {
            foreach (var reason in result.Compatibility.Reasons)
            {
                var field = string.IsNullOrWhiteSpace(reason.Field)
                    ? "n/a"
                    : reason.Field;
                var expected = string.IsNullOrWhiteSpace(reason.Expected)
                    ? "n/a"
                    : reason.Expected;
                var actual = string.IsNullOrWhiteSpace(reason.Actual)
                    ? "n/a"
                    : reason.Actual;

                WriteLine(
                    output,
                    $"Compare compatibility reason: code={ToKebabToken(reason.Code.ToString())}, field={field}, expected={expected}, actual={actual}, message={reason.Message}");
            }
        }

        if (result.Compatibility.IsCompatible)
        {
            foreach (var metricVerdict in result.MetricVerdicts.OrderBy(verdict => verdict.Metric, StringComparer.Ordinal))
            {
                var thresholdUnitSuffix = string.IsNullOrWhiteSpace(metricVerdict.Threshold.Unit)
                    ? string.Empty
                    : $" {metricVerdict.Threshold.Unit}";

                WriteLine(
                    output,
                    $"Metric verdict: metric={metricVerdict.Metric}, verdict={ToKebabToken(metricVerdict.Verdict.ToString())}, baseline={FormatNumber(metricVerdict.Baseline)}, candidate={FormatNumber(metricVerdict.Candidate)}, delta={FormatNumber(metricVerdict.Delta)}, threshold={FormatNumber(metricVerdict.Threshold.Value)}{thresholdUnitSuffix}, rationale={metricVerdict.Rationale}");
            }
        }
        else
        {
            WriteLine(
                output,
                $"Metric verdict scoring skipped: compatibility={ToKebabToken(result.Compatibility.Status.ToString())}.");
        }

        WriteLine(output, $"Compare artifact path: {ToDisplayPath(workspaceRoot, result.ArtifactFile?.FullName ?? string.Empty)}");

        if (result.Failure is not null)
        {
            var failureStage = string.IsNullOrWhiteSpace(result.Failure.Stage)
                ? "n/a"
                : result.Failure.Stage;

            WriteLine(
                output,
                $"Compare failure: kind={ToKebabToken(result.Failure.Kind.ToString())}, stage={failureStage}, message={result.Failure.Message}");
        }
    }

    private static string ToDisplayPath(string workspaceRoot, string absolutePath)
    {
        if (string.IsNullOrWhiteSpace(absolutePath))
        {
            return "unknown";
        }

        try
        {
            var fullPath = Path.GetFullPath(absolutePath);
            if (string.IsNullOrWhiteSpace(workspaceRoot))
            {
                return Path.GetFileName(fullPath);
            }

            var root = Path.GetFullPath(workspaceRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (fullPath.Equals(root, StringComparison.OrdinalIgnoreCase))
            {
                return ".";
            }

            var rootedPrefix = root + Path.DirectorySeparatorChar;
            if (fullPath.StartsWith(rootedPrefix, StringComparison.OrdinalIgnoreCase)
                || fullPath.StartsWith(root + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetRelativePath(root, fullPath).Replace('\\', '/');
            }

            return Path.GetFileName(fullPath);
        }
        catch
        {
            return Path.GetFileName(absolutePath);
        }
    }

    private static string ToKebabToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var characters = new List<char>(value.Length + 8);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsUpper(character) && index > 0 && value[index - 1] != '-')
            {
                characters.Add('-');
            }

            characters.Add(char.ToLowerInvariant(character));
        }

        return new string(characters.ToArray());
    }

    private static string FormatRuntimeMilliseconds(long durationUs)
    {
        if (durationUs <= 0)
        {
            return "0";
        }

        var milliseconds = durationUs / 1000d;
        return milliseconds.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static bool LooksLikePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (Path.IsPathRooted(value))
        {
            return true;
        }

        if (value.Contains(Path.DirectorySeparatorChar)
            || value.Contains(Path.AltDirectorySeparatorChar)
            || value.StartsWith(".", StringComparison.Ordinal))
        {
            return true;
        }

        return value.EndsWith(".bin", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static void WriteLine(IStandardStreamWriter writer, string message)
    {
        writer.Write(message);
        writer.Write(Environment.NewLine);
    }

    private sealed record BenchmarkArtifactListEntry(
        int Index,
        FileInfo ArtifactFile,
        string ArtifactKind,
        string RunId,
        string Phase,
        string State,
        bool? Deterministic,
        int? ChapterCount,
        DateTimeOffset? RecordedAtUtc,
        bool IsMalformed,
        string? MalformedReason);
}

internal sealed class BenchmarkDependencyReadinessProbe : IBenchmarkDependencyReadinessProbe
{
    private static readonly TimeSpan DefaultMfaTimeout = TimeSpan.FromSeconds(30);

    private readonly TimeSpan _mfaTimeout;
    private readonly Func<DateTimeOffset> _utcNow;

    public BenchmarkDependencyReadinessProbe(
        TimeSpan? mfaTimeout = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _mfaTimeout = mfaTimeout ?? DefaultMfaTimeout;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<BenchmarkDependencyReadinessSnapshot> CaptureAsync(CancellationToken cancellationToken = default)
    {
        var ffmpeg = ProbeFfmpeg();
        var mfa = await ProbeMfaAsync(cancellationToken).ConfigureAwait(false);

        var notes = new List<string>();
        if (!ffmpeg.IsReady)
        {
            notes.Add(ffmpeg.Summary);
        }

        if (!mfa.IsReady)
        {
            notes.Add(mfa.Summary);
        }

        if (notes.Count == 0)
        {
            notes.Add("Runtime dependency readiness checks passed.");
        }

        return new BenchmarkDependencyReadinessSnapshot(
            capturedAtUtc: _utcNow(),
            ffmpeg,
            mfa,
            notes);
    }

    private static BenchmarkDependencyReadiness ProbeFfmpeg()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            FfSession.EnsureFiltersAvailable();
            return new BenchmarkDependencyReadiness(
                dependency: "FFmpeg",
                state: BenchmarkReadinessState.Ready,
                summary: "FFmpeg runtime is ready.",
                detail: "FFmpeg native bindings and filter graph support are available.",
                durationMs: stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return new BenchmarkDependencyReadiness(
                dependency: "FFmpeg",
                state: BenchmarkReadinessState.Failed,
                summary: "FFmpeg runtime is not ready.",
                detail: ex.Message,
                durationMs: stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task<BenchmarkDependencyReadiness> ProbeMfaAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_mfaTimeout);

        try
        {
            await MfaProcessSupervisor.EnsureReadyAsync(timeoutCts.Token).ConfigureAwait(false);
            return new BenchmarkDependencyReadiness(
                dependency: "MFA",
                state: BenchmarkReadinessState.Ready,
                summary: "MFA supervisor is ready.",
                detail: "Warm MFA environment responded successfully.",
                durationMs: stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return new BenchmarkDependencyReadiness(
                dependency: "MFA",
                state: BenchmarkReadinessState.Failed,
                summary: $"MFA readiness timed out after {_mfaTimeout.TotalSeconds:0}s.",
                detail: "Retry probe or restart MFA environment.",
                durationMs: stopwatch.ElapsedMilliseconds);
        }
        catch (TimeoutException ex)
        {
            return new BenchmarkDependencyReadiness(
                dependency: "MFA",
                state: BenchmarkReadinessState.Failed,
                summary: "MFA readiness probe timed out.",
                detail: ex.Message,
                durationMs: stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return new BenchmarkDependencyReadiness(
                dependency: "MFA",
                state: BenchmarkReadinessState.Failed,
                summary: "MFA readiness probe failed.",
                detail: ex.Message,
                durationMs: stopwatch.ElapsedMilliseconds);
        }
    }
}
