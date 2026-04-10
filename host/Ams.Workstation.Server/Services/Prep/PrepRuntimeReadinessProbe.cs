using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Ams.Core.Application.Processes;
using Ams.Core.Asr;
using Ams.Core.Common;

namespace Ams.Workstation.Server.Services.Prep;

public interface IPrepRuntimeReadinessProbe
{
    Task<PrepRuntimeReadinessSnapshot> CaptureAsync(
        PrepPipelineRunRequest request,
        string? chapterDisplayTitle,
        string? chapterId,
        CancellationToken cancellationToken = default);
}

public sealed class PrepRuntimeReadinessProbe : IPrepRuntimeReadinessProbe
{
    private const string FfmpegDetectedToken = "FFmpeg binaries detected:";

    private static readonly TimeSpan DefaultFfmpegTimeout = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan DefaultMfaTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromSeconds(20);

    private readonly Func<CancellationToken, Task<PrepRuntimeDependencyReadiness>> _ffmpegProbe;
    private readonly Func<CancellationToken, Task<PrepRuntimeDependencyReadiness>> _mfaProbe;
    private readonly Func<string, bool> _fileExists;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly string? _ffmpegScriptPath;
    private readonly TimeSpan _ffmpegTimeout;
    private readonly TimeSpan _mfaTimeout;
    private readonly TimeSpan _cacheTtl;

    private readonly object _cacheGate = new();
    private PrepRuntimeReadinessSnapshot? _cachedSnapshot;
    private string? _cachedKey;

    public PrepRuntimeReadinessProbe(
        Func<CancellationToken, Task<PrepRuntimeDependencyReadiness>>? ffmpegProbe = null,
        Func<CancellationToken, Task<PrepRuntimeDependencyReadiness>>? mfaProbe = null,
        Func<string, bool>? fileExists = null,
        Func<DateTimeOffset>? utcNow = null,
        string? ffmpegScriptPath = null,
        TimeSpan? ffmpegTimeout = null,
        TimeSpan? mfaTimeout = null,
        TimeSpan? cacheTtl = null)
    {
        _fileExists = fileExists ?? File.Exists;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _ffmpegTimeout = ffmpegTimeout ?? DefaultFfmpegTimeout;
        _mfaTimeout = mfaTimeout ?? DefaultMfaTimeout;
        _cacheTtl = cacheTtl ?? DefaultCacheTtl;
        _ffmpegScriptPath = ResolveFfmpegScriptPath(ffmpegScriptPath);

        _ffmpegProbe = ffmpegProbe ?? ProbeFfmpegReadinessAsync;
        _mfaProbe = mfaProbe ?? ProbeMfaReadinessAsync;
    }

    public async Task<PrepRuntimeReadinessSnapshot> CaptureAsync(
        PrepPipelineRunRequest request,
        string? chapterDisplayTitle,
        string? chapterId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var modelProvenance = EvaluateModelProvenance(request.Asr?.Model);
        EnsureModelShape(modelProvenance);

        var normalizedChapterDisplayTitle = NormalizeOptionalText(chapterDisplayTitle);
        var normalizedChapterId = NormalizeOptionalText(chapterId);
        var cacheKey = BuildCacheKey(request, modelProvenance, normalizedChapterDisplayTitle, normalizedChapterId);

        if (TryGetCached(cacheKey, out var cached))
        {
            var reuseNote = $"Probe reused cached readiness result captured at {cached.CapturedAtUtc:O}.";
            var notes = cached.Notes.Contains(reuseNote, StringComparer.Ordinal)
                ? cached.Notes
                : cached.Notes.Concat([reuseNote]).ToArray();

            return cached with
            {
                CapturedAtUtc = _utcNow(),
                ChapterDisplayTitle = normalizedChapterDisplayTitle,
                ChapterId = normalizedChapterId,
                ReusedCachedProbe = true,
                Notes = notes
            };
        }

        var ffmpeg = await ExecuteProbeSafelyAsync("FFmpeg", _ffmpegProbe, cancellationToken).ConfigureAwait(false);
        var mfa = await ExecuteProbeSafelyAsync("MFA", _mfaProbe, cancellationToken).ConfigureAwait(false);

        EnsureDependencyShape(ffmpeg, "FFmpeg");
        EnsureDependencyShape(mfa, "MFA");

        var deterministic = modelProvenance.IsDeterministic
            && ffmpeg.State == PrepRuntimeReadinessState.Ready
            && mfa.State == PrepRuntimeReadinessState.Ready;

        var snapshot = new PrepRuntimeReadinessSnapshot
        {
            CapturedAtUtc = _utcNow(),
            ChapterDisplayTitle = normalizedChapterDisplayTitle,
            ChapterId = normalizedChapterId,
            ModelProvenance = modelProvenance,
            Ffmpeg = ffmpeg,
            Mfa = mfa,
            IsDeterministic = deterministic,
            IsReady = deterministic,
            ReusedCachedProbe = false,
            Notes = BuildSnapshotNotes(modelProvenance, ffmpeg, mfa)
        };

        StoreCache(cacheKey, snapshot);
        return snapshot;
    }

    private async Task<PrepRuntimeDependencyReadiness> ExecuteProbeSafelyAsync(
        string dependencyName,
        Func<CancellationToken, Task<PrepRuntimeDependencyReadiness>> probe,
        CancellationToken cancellationToken)
    {
        try
        {
            return await probe(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new PrepRuntimeDependencyReadiness(
                dependencyName,
                PrepRuntimeReadinessState.Failed,
                $"{dependencyName} readiness probe threw an exception.",
                Detail: ex.Message);
        }
    }

    private PrepRuntimeModelProvenance EvaluateModelProvenance(string? requestedModel)
    {
        var normalizedInput = NormalizeOptionalText(requestedModel);
        if (string.IsNullOrWhiteSpace(normalizedInput))
        {
            return new PrepRuntimeModelProvenance(
                PrepRuntimeReadinessState.Warning,
                PrepModelProvenanceKind.MissingExplicitModel,
                RequestedModel: null,
                NormalizedModelPath: null,
                IsDeterministic: false,
                Summary: "ASR model is not explicitly pinned.",
                Guidance: "Provide an explicit ASR model file path to make Prep runtime inputs deterministic.");
        }

        if (HasMalformedPathInput(normalizedInput))
        {
            return new PrepRuntimeModelProvenance(
                PrepRuntimeReadinessState.Warning,
                PrepModelProvenanceKind.InvalidModelInput,
                RequestedModel: normalizedInput,
                NormalizedModelPath: null,
                IsDeterministic: false,
                Summary: "ASR model input contains invalid path characters.",
                Guidance: "Use a valid filesystem path to a pinned Whisper model file.");
        }

        string normalizedPath;
        try
        {
            normalizedPath = AmsPathResolver.NormalizePath(normalizedInput);
        }
        catch (Exception ex)
        {
            return new PrepRuntimeModelProvenance(
                PrepRuntimeReadinessState.Warning,
                PrepModelProvenanceKind.InvalidModelInput,
                RequestedModel: normalizedInput,
                NormalizedModelPath: null,
                IsDeterministic: false,
                Summary: "ASR model input could not be normalized as a path.",
                Guidance: $"Model path normalization failed: {ex.Message}");
        }

        if (_fileExists(normalizedPath))
        {
            return new PrepRuntimeModelProvenance(
                PrepRuntimeReadinessState.Ready,
                PrepModelProvenanceKind.PinnedPath,
                RequestedModel: normalizedInput,
                NormalizedModelPath: normalizedPath,
                IsDeterministic: true,
                Summary: "ASR model is pinned to an existing file path.",
                Guidance: "Pinned model path verified.");
        }

        if (LooksLikePath(normalizedInput))
        {
            return new PrepRuntimeModelProvenance(
                PrepRuntimeReadinessState.Failed,
                PrepModelProvenanceKind.MissingModelFile,
                RequestedModel: normalizedInput,
                NormalizedModelPath: normalizedPath,
                IsDeterministic: false,
                Summary: "Pinned ASR model path does not exist.",
                Guidance: "Verify the model path exists or update the Prep request before running.");
        }

        if (AsrEngineConfig.ParseModelAlias(normalizedInput).HasValue)
        {
            return new PrepRuntimeModelProvenance(
                PrepRuntimeReadinessState.Warning,
                PrepModelProvenanceKind.AliasOnly,
                RequestedModel: normalizedInput,
                NormalizedModelPath: null,
                IsDeterministic: false,
                Summary: "ASR model uses an alias instead of a pinned file path.",
                Guidance: "Aliases can resolve differently across machines; provide a concrete model file path for deterministic runs.");
        }

        return new PrepRuntimeModelProvenance(
            PrepRuntimeReadinessState.Warning,
            PrepModelProvenanceKind.InvalidModelInput,
            RequestedModel: normalizedInput,
            NormalizedModelPath: normalizedPath,
            IsDeterministic: false,
            Summary: "ASR model input is neither a known alias nor an existing file path.",
            Guidance: "Use a valid alias or, preferably, a pinned model file path.");
    }

    private async Task<PrepRuntimeDependencyReadiness> ProbeFfmpegReadinessAsync(CancellationToken cancellationToken)
    {
        var scriptPath = _ffmpegScriptPath;
        if (string.IsNullOrWhiteSpace(scriptPath) || !_fileExists(scriptPath))
        {
            return new PrepRuntimeDependencyReadiness(
                "FFmpeg",
                PrepRuntimeReadinessState.Failed,
                "FFmpeg readiness script was not found.",
                Detail: "Expected scripts/setup_ffmpeg.py to be available from the AMS repository root.");
        }

        var launchErrors = new List<string>();
        foreach (var pythonCommand in ResolvePythonCommands())
        {
            var result = await TryRunFfmpegCheckAsync(pythonCommand, scriptPath, cancellationToken).ConfigureAwait(false);
            if (!result.Started)
            {
                if (!string.IsNullOrWhiteSpace(result.StartFailure))
                {
                    launchErrors.Add($"{pythonCommand}: {result.StartFailure}");
                }

                continue;
            }

            return MapFfmpegResult(pythonCommand, result);
        }

        return new PrepRuntimeDependencyReadiness(
            "FFmpeg",
            PrepRuntimeReadinessState.Failed,
            "Unable to start FFmpeg readiness probe.",
            Detail: launchErrors.Count == 0
                ? "No Python interpreter command could be launched."
                : string.Join(" | ", launchErrors));
    }

    private async Task<ProcessExecutionResult> TryRunFfmpegCheckAsync(
        string pythonCommand,
        string scriptPath,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        using var process = new Process
        {
            StartInfo =
            {
                FileName = pythonCommand,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.StartInfo.ArgumentList.Add(scriptPath);
        process.StartInfo.ArgumentList.Add("--check-only");

        try
        {
            if (!process.Start())
            {
                return ProcessExecutionResult.StartFailed("Process start returned false.", stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Win32Exception ex)
        {
            return ProcessExecutionResult.StartFailed(ex.Message, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return ProcessExecutionResult.StartFailed(ex.Message, stopwatch.ElapsedMilliseconds);
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_ffmpegTimeout);

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            TryKillProcess(process);
            var timeoutStdOut = await ReadProcessOutputSafelyAsync(stdoutTask).ConfigureAwait(false);
            var timeoutStdErr = await ReadProcessOutputSafelyAsync(stderrTask).ConfigureAwait(false);
            return ProcessExecutionResult.TimeoutResult(timeoutStdOut, timeoutStdErr, stopwatch.ElapsedMilliseconds);
        }

        var stdout = await ReadProcessOutputSafelyAsync(stdoutTask).ConfigureAwait(false);
        var stderr = await ReadProcessOutputSafelyAsync(stderrTask).ConfigureAwait(false);

        return ProcessExecutionResult.Completed(process.ExitCode, stdout, stderr, stopwatch.ElapsedMilliseconds);
    }

    private PrepRuntimeDependencyReadiness MapFfmpegResult(string pythonCommand, ProcessExecutionResult result)
    {
        if (result.TimedOut)
        {
            return new PrepRuntimeDependencyReadiness(
                "FFmpeg",
                PrepRuntimeReadinessState.Failed,
                $"FFmpeg readiness probe timed out after {_ffmpegTimeout.TotalSeconds:0}s.",
                Detail: "Retry readiness probing or run scripts/setup_ffmpeg.py manually to verify installation.",
                DurationMs: result.DurationMs);
        }

        if (result.ExitCode != 0)
        {
            var summary = BuildOutputSummary(result.StdErr, result.StdOut, fallback: "FFmpeg readiness check failed.");
            return new PrepRuntimeDependencyReadiness(
                "FFmpeg",
                PrepRuntimeReadinessState.Failed,
                $"FFmpeg readiness check failed (exit {result.ExitCode}).",
                Detail: $"{summary} Interpreter: {pythonCommand}. Run scripts/setup_ffmpeg.py to install required binaries.",
                ExitCode: result.ExitCode,
                DurationMs: result.DurationMs);
        }

        if (TryExtractFfmpegDetectionPath(result.StdOut, out var detectedPath))
        {
            return new PrepRuntimeDependencyReadiness(
                "FFmpeg",
                PrepRuntimeReadinessState.Ready,
                "FFmpeg shared-library precheck passed.",
                Detail: string.IsNullOrWhiteSpace(detectedPath)
                    ? $"Interpreter: {pythonCommand}."
                    : $"Detected at {detectedPath} via {pythonCommand}.",
                ExitCode: result.ExitCode,
                DurationMs: result.DurationMs);
        }

        var unparseableSummary = BuildOutputSummary(result.StdOut, result.StdErr, fallback: "No probe output was captured.");
        return new PrepRuntimeDependencyReadiness(
            "FFmpeg",
            PrepRuntimeReadinessState.Unknown,
            "FFmpeg readiness probe returned unparseable probe output.",
            Detail: $"Exit code: {result.ExitCode}. Output preview: {unparseableSummary}",
            ExitCode: result.ExitCode,
            DurationMs: result.DurationMs);
    }

    private async Task<PrepRuntimeDependencyReadiness> ProbeMfaReadinessAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_mfaTimeout);

        try
        {
            await MfaProcessSupervisor.EnsureReadyAsync(timeoutCts.Token).ConfigureAwait(false);
            return new PrepRuntimeDependencyReadiness(
                "MFA",
                PrepRuntimeReadinessState.Ready,
                "MFA supervisor is ready for forced-alignment commands.",
                Detail: "Warm MFA environment responded successfully.",
                DurationMs: stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return new PrepRuntimeDependencyReadiness(
                "MFA",
                PrepRuntimeReadinessState.Failed,
                $"MFA readiness timed out after {_mfaTimeout.TotalSeconds:0}s.",
                Detail: "Retry readiness probing; if it persists, restart the workstation host and MFA environment.",
                DurationMs: stopwatch.ElapsedMilliseconds);
        }
        catch (TimeoutException ex)
        {
            return new PrepRuntimeDependencyReadiness(
                "MFA",
                PrepRuntimeReadinessState.Failed,
                "MFA readiness probe timed out.",
                Detail: ex.Message,
                DurationMs: stopwatch.ElapsedMilliseconds);
        }
        catch (InvalidOperationException ex)
        {
            return new PrepRuntimeDependencyReadiness(
                "MFA",
                PrepRuntimeReadinessState.Unknown,
                "MFA readiness returned an inconsistent state.",
                Detail: ex.Message,
                DurationMs: stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return new PrepRuntimeDependencyReadiness(
                "MFA",
                PrepRuntimeReadinessState.Failed,
                "MFA readiness probe failed.",
                Detail: ex.Message,
                DurationMs: stopwatch.ElapsedMilliseconds);
        }
    }

    private static IReadOnlyList<string> BuildSnapshotNotes(
        PrepRuntimeModelProvenance model,
        PrepRuntimeDependencyReadiness ffmpeg,
        PrepRuntimeDependencyReadiness mfa)
    {
        var notes = new List<string>();

        if (!model.IsDeterministic)
        {
            notes.Add(model.Summary);
            notes.Add(model.Guidance);
        }

        if (ffmpeg.State != PrepRuntimeReadinessState.Ready)
        {
            notes.Add(ffmpeg.Summary);
            if (!string.IsNullOrWhiteSpace(ffmpeg.Detail))
            {
                notes.Add(ffmpeg.Detail);
            }
        }

        if (mfa.State != PrepRuntimeReadinessState.Ready)
        {
            notes.Add(mfa.Summary);
            if (!string.IsNullOrWhiteSpace(mfa.Detail))
            {
                notes.Add(mfa.Detail);
            }
        }

        if (notes.Count == 0)
        {
            notes.Add("Runtime readiness checks passed with pinned model provenance.");
        }

        return notes
            .Where(note => !string.IsNullOrWhiteSpace(note))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private bool TryGetCached(string cacheKey, out PrepRuntimeReadinessSnapshot snapshot)
    {
        lock (_cacheGate)
        {
            if (_cachedSnapshot is null
                || !string.Equals(_cachedKey, cacheKey, StringComparison.Ordinal))
            {
                snapshot = null!;
                return false;
            }

            var age = _utcNow() - _cachedSnapshot.CapturedAtUtc;
            if (age > _cacheTtl)
            {
                snapshot = null!;
                return false;
            }

            snapshot = _cachedSnapshot;
            return true;
        }
    }

    private void StoreCache(string cacheKey, PrepRuntimeReadinessSnapshot snapshot)
    {
        lock (_cacheGate)
        {
            _cachedKey = cacheKey;
            _cachedSnapshot = snapshot;
        }
    }

    private static void EnsureModelShape(PrepRuntimeModelProvenance model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (string.IsNullOrWhiteSpace(model.Summary) || string.IsNullOrWhiteSpace(model.Guidance))
        {
            throw new InvalidOperationException("Runtime readiness probe returned malformed model provenance data.");
        }
    }

    private static void EnsureDependencyShape(PrepRuntimeDependencyReadiness dependency, string dependencyName)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        if (string.IsNullOrWhiteSpace(dependency.Dependency)
            || string.IsNullOrWhiteSpace(dependency.Summary))
        {
            throw new InvalidOperationException($"Runtime readiness probe returned malformed {dependencyName} dependency data.");
        }
    }

    private static bool TryExtractFfmpegDetectionPath(string stdout, out string? detectedPath)
    {
        detectedPath = null;
        var lines = stdout.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            if (!line.StartsWith(FfmpegDetectedToken, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            detectedPath = line[FfmpegDetectedToken.Length..].Trim();
            return true;
        }

        return false;
    }

    private static string BuildOutputSummary(string primary, string secondary, string fallback)
    {
        var candidate = ExtractFirstNonEmptyLine(primary)
                        ?? ExtractFirstNonEmptyLine(secondary)
                        ?? fallback;

        return candidate.Length <= 220
            ? candidate
            : candidate[..220] + "...";
    }

    private static string? ExtractFirstNonEmptyLine(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var lines = value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return lines.FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));
    }

    private static void TryKillProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Best-effort process cleanup after timeout.
        }
    }

    private static async Task<string> ReadProcessOutputSafelyAsync(Task<string> outputTask)
    {
        try
        {
            return await outputTask.ConfigureAwait(false);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool HasMalformedPathInput(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return true;
        }

        return value.Any(char.IsControl);
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

    private static IReadOnlyList<string> ResolvePythonCommands()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ["py", "python", "python3"];
        }

        return ["python3", "python"];
    }

    private static string BuildCacheKey(
        PrepPipelineRunRequest request,
        PrepRuntimeModelProvenance model,
        string? chapterDisplayTitle,
        string? chapterId)
    {
        var engine = request.Asr?.Engine?.ToString() ?? "auto";
        var modelMarker = model.NormalizedModelPath ?? model.RequestedModel ?? "<default>";
        return string.Join(
            "|",
            NormalizeOptionalText(chapterDisplayTitle) ?? "<none>",
            NormalizeOptionalText(chapterId) ?? "<none>",
            engine,
            model.SourceKind,
            modelMarker);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string? ResolveFfmpegScriptPath(string? explicitPath)
    {
        if (TryResolveExistingPath(explicitPath, out var resolvedExplicitPath))
        {
            return resolvedExplicitPath;
        }

        if (TryResolveExistingPath(Environment.GetEnvironmentVariable("AMS_FFMPEG_SETUP_SCRIPT"), out var fromEnvironment))
        {
            return fromEnvironment;
        }

        if (TrySearchForScriptFrom(AppContext.BaseDirectory, out var fromAppBase))
        {
            return fromAppBase;
        }

        if (TrySearchForScriptFrom(Directory.GetCurrentDirectory(), out var fromCurrentDirectory))
        {
            return fromCurrentDirectory;
        }

        return null;
    }

    private static bool TrySearchForScriptFrom(string? startPath, out string? scriptPath)
    {
        scriptPath = null;
        if (string.IsNullOrWhiteSpace(startPath))
        {
            return false;
        }

        DirectoryInfo? current;
        try
        {
            current = new DirectoryInfo(startPath);
        }
        catch
        {
            return false;
        }

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "scripts", "setup_ffmpeg.py");
            if (File.Exists(candidate))
            {
                scriptPath = candidate;
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    private static bool TryResolveExistingPath(string? value, out string? normalizedPath)
    {
        normalizedPath = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            var normalized = AmsPathResolver.NormalizePath(value.Trim());
            if (!File.Exists(normalized))
            {
                return false;
            }

            normalizedPath = normalized;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed record ProcessExecutionResult(
        bool Started,
        bool TimedOut,
        int? ExitCode,
        string StdOut,
        string StdErr,
        string? StartFailure,
        long DurationMs)
    {
        public static ProcessExecutionResult StartFailed(string startFailure, long durationMs)
            => new(
                Started: false,
                TimedOut: false,
                ExitCode: null,
                StdOut: string.Empty,
                StdErr: string.Empty,
                StartFailure: startFailure,
                DurationMs: durationMs);

        public static ProcessExecutionResult TimeoutResult(string stdOut, string stdErr, long durationMs)
            => new(
                Started: true,
                TimedOut: true,
                ExitCode: null,
                StdOut: stdOut,
                StdErr: stdErr,
                StartFailure: null,
                DurationMs: durationMs);

        public static ProcessExecutionResult Completed(int exitCode, string stdOut, string stdErr, long durationMs)
            => new(
                Started: true,
                TimedOut: false,
                ExitCode: exitCode,
                StdOut: stdOut,
                StdErr: stdErr,
                StartFailure: null,
                DurationMs: durationMs);
    }
}
