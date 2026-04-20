using System.Linq;
using Ams.Core.Asr;
using Ams.Core.Common;

namespace Ams.Core.Application.Benchmark;

public sealed class BenchmarkDeterminismGate
{
    private static readonly BenchmarkDeterminismReasonCode[] StableReasonOrder =
    [
        BenchmarkDeterminismReasonCode.MissingExplicitModel,
        BenchmarkDeterminismReasonCode.AliasOnlyModel,
        BenchmarkDeterminismReasonCode.InvalidModelInput,
        BenchmarkDeterminismReasonCode.MissingModelFile,
        BenchmarkDeterminismReasonCode.DependencyProbeMalformed,
        BenchmarkDeterminismReasonCode.DependencyProbeTimeout,
        BenchmarkDeterminismReasonCode.DependencyProbeFailed,
        BenchmarkDeterminismReasonCode.FfmpegNotReady,
        BenchmarkDeterminismReasonCode.MfaNotReady
    ];

    private readonly IBenchmarkDependencyReadinessProbe _dependencyProbe;
    private readonly Func<string, bool> _fileExists;
    private readonly Func<DateTimeOffset> _utcNow;

    public BenchmarkDeterminismGate(
        IBenchmarkDependencyReadinessProbe dependencyProbe,
        Func<string, bool>? fileExists = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _dependencyProbe = dependencyProbe ?? throw new ArgumentNullException(nameof(dependencyProbe));
        _fileExists = fileExists ?? File.Exists;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<BenchmarkDeterminismContract> EvaluateAsync(
        BenchmarkDeterminismGateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reasons = new HashSet<BenchmarkDeterminismReasonCode>();
        var guidance = new List<string>();

        var modelProvenance = EvaluateModelProvenance(request.RequestedModel);
        AddModelReason(modelProvenance, reasons);
        guidance.Add(modelProvenance.Summary);
        guidance.Add(modelProvenance.Guidance);

        BenchmarkDependencyReadiness ffmpeg;
        BenchmarkDependencyReadiness mfa;

        try
        {
            var dependencySnapshot = await _dependencyProbe.CaptureAsync(cancellationToken).ConfigureAwait(false);

            if (!TryValidateDependencySnapshot(dependencySnapshot, out var malformedMessage))
            {
                reasons.Add(BenchmarkDeterminismReasonCode.DependencyProbeMalformed);
                ffmpeg = CreateUnknownDependency(
                    "FFmpeg",
                    "Dependency readiness probe returned malformed FFmpeg data.",
                    malformedMessage);
                mfa = CreateUnknownDependency(
                    "MFA",
                    "Dependency readiness probe returned malformed MFA data.",
                    malformedMessage);
                guidance.Add("Dependency readiness snapshot was malformed; rerun readiness probe before benchmark execution.");
            }
            else
            {
                ffmpeg = dependencySnapshot.Ffmpeg;
                mfa = dependencySnapshot.Mfa;

                if (!ffmpeg.IsReady)
                {
                    reasons.Add(BenchmarkDeterminismReasonCode.FfmpegNotReady);
                    guidance.Add(ffmpeg.Summary);
                    if (!string.IsNullOrWhiteSpace(ffmpeg.Detail))
                    {
                        guidance.Add(ffmpeg.Detail);
                    }
                }

                if (!mfa.IsReady)
                {
                    reasons.Add(BenchmarkDeterminismReasonCode.MfaNotReady);
                    guidance.Add(mfa.Summary);
                    if (!string.IsNullOrWhiteSpace(mfa.Detail))
                    {
                        guidance.Add(mfa.Detail);
                    }
                }

                if (dependencySnapshot.Notes.Count > 0)
                {
                    guidance.AddRange(dependencySnapshot.Notes);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TimeoutException ex)
        {
            reasons.Add(BenchmarkDeterminismReasonCode.DependencyProbeTimeout);
            ffmpeg = CreateUnknownDependency(
                "FFmpeg",
                "Dependency readiness probe timed out before FFmpeg state was confirmed.",
                ex.Message);
            mfa = CreateUnknownDependency(
                "MFA",
                "Dependency readiness probe timed out before MFA state was confirmed.",
                ex.Message);
            guidance.Add("Dependency readiness probe timed out; deterministic benchmark run rejected.");
        }
        catch (OperationCanceledException ex)
        {
            reasons.Add(BenchmarkDeterminismReasonCode.DependencyProbeTimeout);
            ffmpeg = CreateUnknownDependency(
                "FFmpeg",
                "Dependency readiness probe timed out before FFmpeg state was confirmed.",
                ex.Message);
            mfa = CreateUnknownDependency(
                "MFA",
                "Dependency readiness probe timed out before MFA state was confirmed.",
                ex.Message);
            guidance.Add("Dependency readiness probe timed out; deterministic benchmark run rejected.");
        }
        catch (Exception ex)
        {
            reasons.Add(BenchmarkDeterminismReasonCode.DependencyProbeFailed);
            ffmpeg = CreateUnknownDependency(
                "FFmpeg",
                "Dependency readiness probe failed before FFmpeg state was confirmed.",
                ex.Message);
            mfa = CreateUnknownDependency(
                "MFA",
                "Dependency readiness probe failed before MFA state was confirmed.",
                ex.Message);
            guidance.Add("Dependency readiness probe failed; deterministic benchmark run rejected.");
        }

        var orderedReasons = CanonicalizeReasons(reasons);
        var verdict = orderedReasons.Count == 0
            ? BenchmarkDeterminismVerdict.Valid
            : BenchmarkDeterminismVerdict.Invalid;

        if (verdict == BenchmarkDeterminismVerdict.Valid)
        {
            guidance.Add("Deterministic benchmark gate passed with pinned model provenance and ready dependencies.");
        }

        return new BenchmarkDeterminismContract(
            evaluatedAtUtc: _utcNow(),
            verdict: verdict,
            reasonCodes: orderedReasons,
            modelProvenance: modelProvenance,
            ffmpeg: ffmpeg,
            mfa: mfa,
            cachePolicy: request.CachePolicy,
            chunkPolicy: request.ChunkPolicy,
            guidance: guidance);
    }

    private static BenchmarkDependencyReadiness CreateUnknownDependency(
        string dependency,
        string summary,
        string? detail)
    {
        return new BenchmarkDependencyReadiness(
            dependency,
            BenchmarkReadinessState.Unknown,
            summary,
            detail);
    }

    private static IReadOnlyList<BenchmarkDeterminismReasonCode> CanonicalizeReasons(
        IEnumerable<BenchmarkDeterminismReasonCode> reasons)
    {
        var unique = new HashSet<BenchmarkDeterminismReasonCode>(reasons);
        var ordered = new List<BenchmarkDeterminismReasonCode>();

        foreach (var code in StableReasonOrder)
        {
            if (unique.Remove(code))
            {
                ordered.Add(code);
            }
        }

        if (unique.Count > 0)
        {
            ordered.AddRange(unique.OrderBy(code => (int)code));
        }

        return ordered;
    }

    private static bool TryValidateDependencySnapshot(
        BenchmarkDependencyReadinessSnapshot? snapshot,
        out string? message)
    {
        if (snapshot is null)
        {
            message = "Readiness probe returned null snapshot.";
            return false;
        }

        if (!TryValidateDependencyShape(snapshot.Ffmpeg, "FFmpeg", out message))
        {
            return false;
        }

        if (!TryValidateDependencyShape(snapshot.Mfa, "MFA", out message))
        {
            return false;
        }

        message = null;
        return true;
    }

    private static bool TryValidateDependencyShape(
        BenchmarkDependencyReadiness dependency,
        string expectedDependency,
        out string? message)
    {
        if (dependency is null)
        {
            message = $"{expectedDependency} readiness entry was null.";
            return false;
        }

        if (!string.Equals(dependency.Dependency, expectedDependency, StringComparison.OrdinalIgnoreCase))
        {
            message = $"Expected dependency '{expectedDependency}' but received '{dependency.Dependency}'.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dependency.Summary))
        {
            message = $"{expectedDependency} readiness summary was blank.";
            return false;
        }

        message = null;
        return true;
    }

    private void AddModelReason(
        BenchmarkModelProvenance model,
        ISet<BenchmarkDeterminismReasonCode> reasons)
    {
        switch (model.SourceKind)
        {
            case BenchmarkModelProvenanceKind.MissingExplicitModel:
                reasons.Add(BenchmarkDeterminismReasonCode.MissingExplicitModel);
                break;
            case BenchmarkModelProvenanceKind.AliasOnly:
                reasons.Add(BenchmarkDeterminismReasonCode.AliasOnlyModel);
                break;
            case BenchmarkModelProvenanceKind.InvalidModelInput:
                reasons.Add(BenchmarkDeterminismReasonCode.InvalidModelInput);
                break;
            case BenchmarkModelProvenanceKind.MissingModelFile:
                reasons.Add(BenchmarkDeterminismReasonCode.MissingModelFile);
                break;
        }
    }

    private BenchmarkModelProvenance EvaluateModelProvenance(string? requestedModel)
    {
        var normalizedInput = NormalizeOptionalText(requestedModel);
        if (string.IsNullOrWhiteSpace(normalizedInput))
        {
            return new BenchmarkModelProvenance(
                BenchmarkReadinessState.Warning,
                BenchmarkModelProvenanceKind.MissingExplicitModel,
                requestedModel,
                normalizedModelPath: null,
                isDeterministic: false,
                summary: "ASR model is not explicitly pinned.",
                guidance: "Provide explicit ASR model file path for deterministic benchmark runs.");
        }

        if (HasMalformedPathInput(normalizedInput))
        {
            return new BenchmarkModelProvenance(
                BenchmarkReadinessState.Warning,
                BenchmarkModelProvenanceKind.InvalidModelInput,
                normalizedInput,
                normalizedModelPath: null,
                isDeterministic: false,
                summary: "ASR model input contains invalid path characters.",
                guidance: "Use a valid filesystem path to a pinned Whisper model file.");
        }

        string normalizedPath;
        try
        {
            normalizedPath = AmsPathResolver.NormalizePath(normalizedInput);
        }
        catch (Exception ex)
        {
            return new BenchmarkModelProvenance(
                BenchmarkReadinessState.Warning,
                BenchmarkModelProvenanceKind.InvalidModelInput,
                normalizedInput,
                normalizedModelPath: null,
                isDeterministic: false,
                summary: "ASR model input could not be normalized as a filesystem path.",
                guidance: $"Model path normalization failed: {ex.Message}");
        }

        if (_fileExists(normalizedPath))
        {
            return new BenchmarkModelProvenance(
                BenchmarkReadinessState.Ready,
                BenchmarkModelProvenanceKind.PinnedPath,
                normalizedInput,
                normalizedPath,
                isDeterministic: true,
                summary: "ASR model is pinned to an existing file path.",
                guidance: "Pinned ASR model path verified.");
        }

        if (LooksLikePath(normalizedInput))
        {
            return new BenchmarkModelProvenance(
                BenchmarkReadinessState.Failed,
                BenchmarkModelProvenanceKind.MissingModelFile,
                normalizedInput,
                normalizedPath,
                isDeterministic: false,
                summary: "Pinned ASR model path does not exist.",
                guidance: "Verify model file path before running deterministic benchmarks.");
        }

        if (AsrEngineConfig.ParseModelAlias(normalizedInput).HasValue)
        {
            return new BenchmarkModelProvenance(
                BenchmarkReadinessState.Warning,
                BenchmarkModelProvenanceKind.AliasOnly,
                normalizedInput,
                normalizedModelPath: null,
                isDeterministic: false,
                summary: "ASR model input is alias-only and not pinned to a file path.",
                guidance: "Provide concrete model file path to avoid alias drift across machines.");
        }

        return new BenchmarkModelProvenance(
            BenchmarkReadinessState.Warning,
            BenchmarkModelProvenanceKind.InvalidModelInput,
            normalizedInput,
            normalizedPath,
            isDeterministic: false,
            summary: "ASR model input is neither known alias nor existing file path.",
            guidance: "Use known Whisper alias or explicit existing model path; deterministic mode requires pinned path.");
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

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
