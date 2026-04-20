using System.Globalization;
using System.Text.Json;
using Ams.Core.Application.Runs;

namespace Ams.Core.Application.Benchmark;

public sealed class BenchmarkCompareService
{
    private const string LoadStage = "compare-load";
    private const string ValidationStage = "compare-validate";
    private const string PersistStage = "compare-persist";
    private const double NumericTolerance = 0.0001d;

    private static readonly IReadOnlyList<MetricDefinition> MetricDefinitions =
    [
        new(
            "totalPipelineRuntimeMs",
            new BenchmarkCompareMetricThreshold(
                value: 25,
                rationale: "Ignore runtime noise under 25ms.",
                unit: "ms"),
            aggregate => aggregate.TotalPipelineRuntimeMs,
            true),
        new(
            "totalAnalysisRuntimeMs",
            new BenchmarkCompareMetricThreshold(
                value: 10,
                rationale: "Ignore analyzer jitter under 10ms.",
                unit: "ms"),
            aggregate => aggregate.TotalAnalysisRuntimeMs,
            true),
        new(
            "totalMismatchCount",
            new BenchmarkCompareMetricThreshold(
                value: 1,
                rationale: "Single mismatch deltas are considered noise.",
                unit: "count"),
            aggregate => aggregate.TotalMismatchCount,
            true),
        new(
            "totalMissingSpeechSec",
            new BenchmarkCompareMetricThreshold(
                value: 0.05,
                rationale: "Ignore missing-speech movement inside 50ms.",
                unit: "sec"),
            aggregate => aggregate.TotalMissingSpeechSec,
            true),
        new(
            "totalExtraSpeechSec",
            new BenchmarkCompareMetricThreshold(
                value: 0.05,
                rationale: "Ignore extra-speech movement inside 50ms.",
                unit: "sec"),
            aggregate => aggregate.TotalExtraSpeechSec,
            true),
        new(
            "totalQcFlags",
            new BenchmarkCompareMetricThreshold(
                value: 1,
                rationale: "Require at least one QC flag delta.",
                unit: "count"),
            aggregate => aggregate.TotalQcFlags,
            true)
    ];

    private readonly BenchmarkRunArtifactStore _artifactStore;
    private readonly BenchmarkRunManifestValidator _manifestValidator;
    private readonly Func<FileInfo, CancellationToken, Task<string>> _readArtifactAsync;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly Func<string> _compareIdFactory;

    public BenchmarkCompareService(
        BenchmarkRunArtifactStore artifactStore,
        BenchmarkRunManifestValidator manifestValidator,
        Func<DateTimeOffset>? utcNow = null,
        Func<string>? compareIdFactory = null)
        : this(
            artifactStore,
            manifestValidator,
            readArtifactAsync: static (artifact, cancellationToken) => File.ReadAllTextAsync(artifact.FullName, cancellationToken),
            utcNow,
            compareIdFactory)
    {
    }

    internal BenchmarkCompareService(
        BenchmarkRunArtifactStore artifactStore,
        BenchmarkRunManifestValidator manifestValidator,
        Func<FileInfo, CancellationToken, Task<string>> readArtifactAsync,
        Func<DateTimeOffset>? utcNow = null,
        Func<string>? compareIdFactory = null)
    {
        _artifactStore = artifactStore ?? throw new ArgumentNullException(nameof(artifactStore));
        _manifestValidator = manifestValidator ?? throw new ArgumentNullException(nameof(manifestValidator));
        _readArtifactAsync = readArtifactAsync ?? throw new ArgumentNullException(nameof(readArtifactAsync));
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _compareIdFactory = compareIdFactory
                           ?? GenerateDefaultCompareId;
    }

    public async Task<BenchmarkCompareResult> ExecuteAsync(
        BenchmarkCompareRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var compareId = ResolveCompareId(request);

        var baselineLoad = await LoadArtifactAsync(request.BaselineArtifact, baseline: true, cancellationToken).ConfigureAwait(false);
        if (baselineLoad.Failure is not null)
        {
            return BuildFailureResult(compareId, request.ModuleId, baselineLoad.Reference, baselineLoad.Reason!, baselineLoad.Failure);
        }

        var candidateLoad = await LoadArtifactAsync(request.CandidateArtifact, baseline: false, cancellationToken).ConfigureAwait(false);
        if (candidateLoad.Failure is not null)
        {
            return BuildFailureResult(compareId, request.ModuleId, baselineLoad.Reference, candidateLoad.Reference, candidateLoad.Reason!, candidateLoad.Failure);
        }

        var reasons = new List<BenchmarkCompareCompatibilityReason>();

        if (baselineLoad.Reason is not null)
        {
            reasons.Add(baselineLoad.Reason);
        }

        if (candidateLoad.Reason is not null)
        {
            reasons.Add(candidateLoad.Reason);
        }

        if (baselineLoad.Manifest is not null)
        {
            AppendManifestValidationReasons(
                reasons,
                baselineLoad.Manifest,
                baseline: true,
                rawChapterSet: baselineLoad.RawChapterSet);
        }

        if (candidateLoad.Manifest is not null)
        {
            AppendManifestValidationReasons(
                reasons,
                candidateLoad.Manifest,
                baseline: false,
                rawChapterSet: candidateLoad.RawChapterSet);
        }

        if (baselineLoad.Manifest is not null && candidateLoad.Manifest is not null)
        {
            AppendCompatibilityReasons(reasons, baselineLoad.Manifest, candidateLoad.Manifest);
        }

        var compatibility = new BenchmarkCompareCompatibility(ResolveStatus(reasons), reasons);

        IReadOnlyList<BenchmarkCompareMetricVerdict> metricVerdicts = [];
        if (compatibility.IsCompatible && baselineLoad.Manifest is not null && candidateLoad.Manifest is not null)
        {
            metricVerdicts = EvaluateMetricVerdicts(
                baselineLoad.Manifest.AggregateMetrics,
                candidateLoad.Manifest.AggregateMetrics);
        }

        var compareArtifact = new BenchmarkCompareArtifact(
            compareId: compareId,
            comparedAtUtc: _utcNow(),
            baseline: baselineLoad.Reference,
            candidate: candidateLoad.Reference,
            compatibility: compatibility,
            metricVerdicts: metricVerdicts);

        try
        {
            var artifactFile = await _artifactStore
                .WriteCompareAsync(request.OutputRoot, compareArtifact, cancellationToken)
                .ConfigureAwait(false);

            return new BenchmarkCompareResult(
                compareId,
                request.ModuleId,
                compatibility,
                metricVerdicts,
                artifactFile,
                failure: null);
        }
        catch (Exception ex)
        {
            var failure = MapExceptionToFailure(ex, PersistStage, "Benchmark compare artifact write failed");
            return new BenchmarkCompareResult(
                compareId,
                request.ModuleId,
                compatibility,
                metricVerdicts: [],
                artifactFile: null,
                failure: failure);
        }
    }

    private BenchmarkCompareResult BuildFailureResult(
        string compareId,
        ModuleId moduleId,
        BenchmarkCompareManifestReference baselineReference,
        BenchmarkCompareCompatibilityReason reason,
        RunFailure failure)
    {
        var candidateReference = new BenchmarkCompareManifestReference(
            runId: "candidate-unavailable",
            chapterSetFingerprint: "candidate-unavailable",
            deterministic: false,
            phase: BenchmarkRunPhase.Failed,
            state: RunState.Failed);

        return BuildFailureResult(compareId, moduleId, baselineReference, candidateReference, reason, failure);
    }

    private static BenchmarkCompareResult BuildFailureResult(
        string compareId,
        ModuleId moduleId,
        BenchmarkCompareManifestReference baselineReference,
        BenchmarkCompareManifestReference candidateReference,
        BenchmarkCompareCompatibilityReason reason,
        RunFailure failure)
    {
        ArgumentNullException.ThrowIfNull(reason);
        ArgumentNullException.ThrowIfNull(failure);

        var compatibility = new BenchmarkCompareCompatibility(
            status: BenchmarkCompareCompatibilityStatus.Malformed,
            reasons: [reason]);

        return new BenchmarkCompareResult(
            compareId,
            moduleId,
            compatibility,
            metricVerdicts: [],
            artifactFile: null,
            failure: failure);
    }

    private async Task<ArtifactLoadResult> LoadArtifactAsync(
        FileInfo artifactFile,
        bool baseline,
        CancellationToken cancellationToken)
    {
        var role = baseline ? "Baseline" : "Candidate";
        var malformedCode = baseline
            ? BenchmarkCompareCompatibilityReasonCode.BaselineManifestMalformed
            : BenchmarkCompareCompatibilityReasonCode.CandidateManifestMalformed;

        var fallbackReference = CreateFallbackReference(artifactFile, baseline);

        string payload;
        try
        {
            payload = await _readArtifactAsync(artifactFile, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var failure = MapExceptionToFailure(
                ex,
                LoadStage,
                $"{role} artifact read failed ({NormalizePath(artifactFile.FullName)})");

            var reason = new BenchmarkCompareCompatibilityReason(
                code: malformedCode,
                message: $"{role} artifact could not be read.",
                field: "artifactPath",
                expected: "readable JSON benchmark manifest",
                actual: NormalizePath(artifactFile.FullName));

            return new ArtifactLoadResult(
                Manifest: null,
                Reference: fallbackReference,
                RawChapterSet: [],
                Reason: reason,
                Failure: failure);
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            var reason = new BenchmarkCompareCompatibilityReason(
                code: malformedCode,
                message: $"{role} artifact payload was empty.",
                field: "artifactPayload",
                expected: "non-empty JSON payload",
                actual: "(empty)");

            return new ArtifactLoadResult(
                Manifest: null,
                Reference: fallbackReference,
                RawChapterSet: [],
                Reason: reason,
                Failure: null);
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(payload);
        }
        catch (JsonException ex)
        {
            var reason = new BenchmarkCompareCompatibilityReason(
                code: malformedCode,
                message: $"{role} artifact payload is not valid JSON: {ex.Message}",
                field: "artifactPayload",
                expected: "valid JSON object",
                actual: "invalid-json");

            return new ArtifactLoadResult(
                Manifest: null,
                Reference: fallbackReference,
                RawChapterSet: [],
                Reason: reason,
                Failure: null);
        }

        using (document)
        {
            var rawChapterSet = ExtractRawChapterSet(document.RootElement);
            var parsedReference = CreateReferenceFromJson(document.RootElement, fallbackReference);

            try
            {
                var manifest = BenchmarkRunManifest.Deserialize(payload);
                return new ArtifactLoadResult(
                    Manifest: manifest,
                    Reference: CreateReference(manifest),
                    RawChapterSet: rawChapterSet,
                    Reason: null,
                    Failure: null);
            }
            catch (Exception manifestException)
            {
                try
                {
                    var invalidRun = BenchmarkInvalidRunArtifact.Deserialize(payload);
                    var reason = new BenchmarkCompareCompatibilityReason(
                        code: BenchmarkCompareCompatibilityReasonCode.ArtifactTypeMismatch,
                        message: $"{role} artifact is invalid-run payload and cannot be compared.",
                        field: "phase",
                        expected: BenchmarkRunPhase.Completed.ToString(),
                        actual: invalidRun.Phase.ToString());

                    return new ArtifactLoadResult(
                        Manifest: null,
                        Reference: CreateReference(invalidRun),
                        RawChapterSet: rawChapterSet,
                        Reason: reason,
                        Failure: null);
                }
                catch
                {
                    var reason = new BenchmarkCompareCompatibilityReason(
                        code: malformedCode,
                        message: $"{role} artifact manifest parse failed: {manifestException.Message}",
                        field: "artifactPayload",
                        expected: "benchmark run manifest contract",
                        actual: "malformed-manifest");

                    return new ArtifactLoadResult(
                        Manifest: null,
                        Reference: parsedReference,
                        RawChapterSet: rawChapterSet,
                        Reason: reason,
                        Failure: null);
                }
            }
        }
    }

    private void AppendManifestValidationReasons(
        ICollection<BenchmarkCompareCompatibilityReason> reasons,
        BenchmarkRunManifest manifest,
        bool baseline,
        IReadOnlyList<string> rawChapterSet)
    {
        ArgumentNullException.ThrowIfNull(reasons);
        ArgumentNullException.ThrowIfNull(manifest);

        ValidateChapterSetShape(reasons, manifest, baseline, rawChapterSet);

        BenchmarkRunManifestValidationResult validation;
        try
        {
            validation = _manifestValidator.Validate(manifest);
        }
        catch (Exception ex)
        {
            var side = baseline ? "Baseline" : "Candidate";
            reasons.Add(new BenchmarkCompareCompatibilityReason(
                code: baseline
                    ? BenchmarkCompareCompatibilityReasonCode.BaselineManifestInvalid
                    : BenchmarkCompareCompatibilityReasonCode.CandidateManifestInvalid,
                message: $"{side} manifest validator failed: {ex.Message}",
                field: "manifestValidator",
                expected: "validator completes successfully",
                actual: ex.GetType().Name));
            return;
        }

        var sidePrefix = baseline ? "Baseline" : "Candidate";

        foreach (var diagnostic in validation.Diagnostics)
        {
            var code = diagnostic.Field.StartsWith("aggregateMetrics.", StringComparison.Ordinal)
                ? BenchmarkCompareCompatibilityReasonCode.AggregateMetricsMismatch
                : baseline
                    ? BenchmarkCompareCompatibilityReasonCode.BaselineManifestInvalid
                    : BenchmarkCompareCompatibilityReasonCode.CandidateManifestInvalid;

            reasons.Add(new BenchmarkCompareCompatibilityReason(
                code: code,
                message: $"{sidePrefix} manifest validation failed: {diagnostic.Rationale}",
                field: diagnostic.Field,
                expected: diagnostic.Expected,
                actual: diagnostic.Actual));
        }
    }

    private static void ValidateChapterSetShape(
        ICollection<BenchmarkCompareCompatibilityReason> reasons,
        BenchmarkRunManifest manifest,
        bool baseline,
        IReadOnlyList<string> rawChapterSet)
    {
        var code = baseline
            ? BenchmarkCompareCompatibilityReasonCode.BaselineManifestInvalid
            : BenchmarkCompareCompatibilityReasonCode.CandidateManifestInvalid;

        var side = baseline ? "Baseline" : "Candidate";

        if (rawChapterSet.Any(chapter => string.IsNullOrWhiteSpace(chapter)))
        {
            reasons.Add(new BenchmarkCompareCompatibilityReason(
                code,
                $"{side} chapterSet contains blank chapter identifiers.",
                field: "chapterSet",
                expected: "non-empty chapter identifiers",
                actual: "contains blank chapter id"));
        }

        var duplicateRaw = rawChapterSet
            .Where(chapter => !string.IsNullOrWhiteSpace(chapter))
            .Select(chapter => chapter.Trim())
            .GroupBy(chapter => chapter, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        if (duplicateRaw.Length > 0)
        {
            reasons.Add(new BenchmarkCompareCompatibilityReason(
                code,
                $"{side} chapterSet contains duplicate chapter identifiers.",
                field: "chapterSet",
                expected: "unique chapter identifiers",
                actual: string.Join(",", duplicateRaw)));
        }

        var duplicateManifest = manifest.ChapterSet
            .GroupBy(chapter => chapter, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        if (duplicateManifest.Length > 0)
        {
            reasons.Add(new BenchmarkCompareCompatibilityReason(
                code,
                $"{side} manifest chapterSet resolved to duplicate identifiers.",
                field: "chapterSet",
                expected: "unique chapter identifiers",
                actual: string.Join(",", duplicateManifest)));
        }
    }

    private static void AppendCompatibilityReasons(
        ICollection<BenchmarkCompareCompatibilityReason> reasons,
        BenchmarkRunManifest baseline,
        BenchmarkRunManifest candidate)
    {
        if (!SequenceEqualOrdinalIgnoreCase(baseline.ChapterSet, candidate.ChapterSet))
        {
            reasons.Add(new BenchmarkCompareCompatibilityReason(
                code: BenchmarkCompareCompatibilityReasonCode.ChapterSetMismatch,
                message: "Baseline and candidate chapter sets differ.",
                field: "chapterSet",
                expected: string.Join(",", baseline.ChapterSet),
                actual: string.Join(",", candidate.ChapterSet)));
        }

        if (!string.Equals(
                baseline.ChapterSetFingerprint,
                candidate.ChapterSetFingerprint,
                StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(new BenchmarkCompareCompatibilityReason(
                code: BenchmarkCompareCompatibilityReasonCode.ChapterFingerprintMismatch,
                message: "Baseline and candidate chapter set fingerprints differ.",
                field: "chapterSetFingerprint",
                expected: baseline.ChapterSetFingerprint,
                actual: candidate.ChapterSetFingerprint));
        }

        if (baseline.Deterministic != candidate.Deterministic)
        {
            reasons.Add(new BenchmarkCompareCompatibilityReason(
                code: BenchmarkCompareCompatibilityReasonCode.DeterminismMismatch,
                message: "Deterministic mode mismatch between baseline and candidate.",
                field: "deterministic",
                expected: baseline.Deterministic.ToString(),
                actual: candidate.Deterministic.ToString()));
        }

        if (baseline.Deterministic)
        {
            AppendDeterminismReasons(reasons, baseline, candidate);
        }

        AppendCachePolicyReasons(reasons, baseline.CachePolicy, candidate.CachePolicy);
        AppendChunkPolicyReasons(reasons, baseline.ChunkPolicy, candidate.ChunkPolicy);
        AppendMetricsPolicyReasons(reasons, baseline.MetricsPolicy, candidate.MetricsPolicy);
    }

    private static void AppendDeterminismReasons(
        ICollection<BenchmarkCompareCompatibilityReason> reasons,
        BenchmarkRunManifest baseline,
        BenchmarkRunManifest candidate)
    {
        if (baseline.Determinism is null || candidate.Determinism is null)
        {
            reasons.Add(new BenchmarkCompareCompatibilityReason(
                code: BenchmarkCompareCompatibilityReasonCode.DeterminismMismatch,
                message: "Deterministic compare requires determinism contracts on both artifacts.",
                field: "determinism",
                expected: "present on baseline and candidate",
                actual: baseline.Determinism is null && candidate.Determinism is null
                    ? "missing on both"
                    : baseline.Determinism is null
                        ? "missing baseline determinism"
                        : "missing candidate determinism"));

            return;
        }

        CompareProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.DeterminismMismatch,
            field: "determinism.verdict",
            message: "Determinism verdict mismatch.",
            baseline.Determinism.Verdict,
            candidate.Determinism.Verdict,
            value => value.ToString());

        CompareProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.ModelProvenanceMismatch,
            field: "determinism.modelProvenance.sourceKind",
            message: "Model provenance class mismatch.",
            baseline.Determinism.ModelProvenance.SourceKind,
            candidate.Determinism.ModelProvenance.SourceKind,
            value => value.ToString());

        CompareProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.ModelProvenanceMismatch,
            field: "determinism.modelProvenance.isDeterministic",
            message: "Model determinism classification mismatch.",
            baseline.Determinism.ModelProvenance.IsDeterministic,
            candidate.Determinism.ModelProvenance.IsDeterministic,
            value => value.ToString());
    }

    private static void AppendCachePolicyReasons(
        ICollection<BenchmarkCompareCompatibilityReason> reasons,
        BenchmarkCachePolicy baseline,
        BenchmarkCachePolicy candidate)
    {
        CompareProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.CachePolicyMismatch,
            field: "cachePolicy.forcePipelineRebuild",
            message: "Cache policy mismatch for pipeline rebuild flag.",
            baseline.ForcePipelineRebuild,
            candidate.ForcePipelineRebuild,
            value => value.ToString());

        CompareProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.CachePolicyMismatch,
            field: "cachePolicy.forceBookIndexRebuild",
            message: "Cache policy mismatch for book-index rebuild flag.",
            baseline.ForceBookIndexRebuild,
            candidate.ForceBookIndexRebuild,
            value => value.ToString());
    }

    private static void AppendChunkPolicyReasons(
        ICollection<BenchmarkCompareCompatibilityReason> reasons,
        BenchmarkChunkPolicy baseline,
        BenchmarkChunkPolicy candidate)
    {
        CompareProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.ChunkPolicyMismatch,
            field: "chunkPolicy.disableChunkPlan",
            message: "Chunk policy mismatch for disableChunkPlan.",
            baseline.DisableChunkPlan,
            candidate.DisableChunkPlan,
            value => value.ToString());

        CompareProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.ChunkPolicyMismatch,
            field: "chunkPolicy.disableChunkedMfa",
            message: "Chunk policy mismatch for disableChunkedMfa.",
            baseline.DisableChunkedMfa,
            candidate.DisableChunkedMfa,
            value => value.ToString());

        CompareProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.ChunkPolicyMismatch,
            field: "chunkPolicy.usedDefaultPlanningPolicy",
            message: "Chunk policy mismatch for usedDefaultPlanningPolicy.",
            baseline.UsedDefaultPlanningPolicy,
            candidate.UsedDefaultPlanningPolicy,
            value => value.ToString());

        CompareDoubleProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.ChunkPolicyMismatch,
            field: "chunkPolicy.silenceThresholdDb",
            message: "Chunk policy mismatch for silenceThresholdDb.",
            baseline.SilenceThresholdDb,
            candidate.SilenceThresholdDb);

        CompareDoubleProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.ChunkPolicyMismatch,
            field: "chunkPolicy.minSilenceDurationMs",
            message: "Chunk policy mismatch for minSilenceDurationMs.",
            baseline.MinSilenceDurationMs,
            candidate.MinSilenceDurationMs);

        CompareDoubleProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.ChunkPolicyMismatch,
            field: "chunkPolicy.minChunkDurationSec",
            message: "Chunk policy mismatch for minChunkDurationSec.",
            baseline.MinChunkDurationSec,
            candidate.MinChunkDurationSec);

        CompareDoubleProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.ChunkPolicyMismatch,
            field: "chunkPolicy.maxChunkDurationSec",
            message: "Chunk policy mismatch for maxChunkDurationSec.",
            baseline.MaxChunkDurationSec,
            candidate.MaxChunkDurationSec);
    }

    private static void AppendMetricsPolicyReasons(
        ICollection<BenchmarkCompareCompatibilityReason> reasons,
        BenchmarkMetricsPolicySnapshot baseline,
        BenchmarkMetricsPolicySnapshot candidate)
    {
        CompareProperty(
            reasons,
            BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch,
            field: "metricsPolicy.enabled",
            message: "Metrics policy mismatch for enabled flag.",
            baseline.Enabled,
            candidate.Enabled,
            value => value.ToString());

        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.integrityWindowMs", "Metrics policy mismatch for integrityWindowMs.", baseline.IntegrityWindowMs, candidate.IntegrityWindowMs);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.integrityStepMs", "Metrics policy mismatch for integrityStepMs.", baseline.IntegrityStepMs, candidate.IntegrityStepMs);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.integrityMinMismatchMs", "Metrics policy mismatch for integrityMinMismatchMs.", baseline.IntegrityMinMismatchMs, candidate.IntegrityMinMismatchMs);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.integrityMergeGapMs", "Metrics policy mismatch for integrityMergeGapMs.", baseline.IntegrityMergeGapMs, candidate.IntegrityMergeGapMs);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.integrityMinDeltaDb", "Metrics policy mismatch for integrityMinDeltaDb.", baseline.IntegrityMinDeltaDb, candidate.IntegrityMinDeltaDb);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.qcNoiseFloorDb", "Metrics policy mismatch for qcNoiseFloorDb.", baseline.QcNoiseFloorDb, candidate.QcNoiseFloorDb);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.qcMinSilenceDurationSec", "Metrics policy mismatch for qcMinSilenceDurationSec.", baseline.QcMinSilenceDurationSec, candidate.QcMinSilenceDurationSec);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.loudnessWindowSec", "Metrics policy mismatch for loudnessWindowSec.", baseline.LoudnessWindowSec, candidate.LoudnessWindowSec);

        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.qcThresholds.minHeadSilence", "Metrics policy mismatch for qcThresholds.minHeadSilence.", baseline.QcThresholds.MinHeadSilence, candidate.QcThresholds.MinHeadSilence);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.qcThresholds.maxHeadSilence", "Metrics policy mismatch for qcThresholds.maxHeadSilence.", baseline.QcThresholds.MaxHeadSilence, candidate.QcThresholds.MaxHeadSilence);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.qcThresholds.minTailSilence", "Metrics policy mismatch for qcThresholds.minTailSilence.", baseline.QcThresholds.MinTailSilence, candidate.QcThresholds.MinTailSilence);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.qcThresholds.maxTailSilence", "Metrics policy mismatch for qcThresholds.maxTailSilence.", baseline.QcThresholds.MaxTailSilence, candidate.QcThresholds.MaxTailSilence);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.qcThresholds.minTitleBodyGap", "Metrics policy mismatch for qcThresholds.minTitleBodyGap.", baseline.QcThresholds.MinTitleBodyGap, candidate.QcThresholds.MinTitleBodyGap);
        CompareDoubleProperty(reasons, BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch, "metricsPolicy.qcThresholds.maxTitleBodyGap", "Metrics policy mismatch for qcThresholds.maxTitleBodyGap.", baseline.QcThresholds.MaxTitleBodyGap, candidate.QcThresholds.MaxTitleBodyGap);
    }

    private static IReadOnlyList<BenchmarkCompareMetricVerdict> EvaluateMetricVerdicts(
        BenchmarkRunMetricsAggregate baseline,
        BenchmarkRunMetricsAggregate candidate)
    {
        var contaminationDetected = HasMetricsContamination(baseline) || HasMetricsContamination(candidate);

        var baselineStates = DescribeMetricsStates(baseline.MetricsStates);
        var candidateStates = DescribeMetricsStates(candidate.MetricsStates);

        return MetricDefinitions
            .Select(definition => BuildMetricVerdict(
                definition,
                baseline,
                candidate,
                contaminationDetected,
                baselineStates,
                candidateStates))
            .ToArray();
    }

    private static BenchmarkCompareMetricVerdict BuildMetricVerdict(
        MetricDefinition definition,
        BenchmarkRunMetricsAggregate baseline,
        BenchmarkRunMetricsAggregate candidate,
        bool contaminationDetected,
        string baselineStates,
        string candidateStates)
    {
        var baselineValue = definition.Selector(baseline);
        var candidateValue = definition.Selector(candidate);
        var delta = candidateValue - baselineValue;

        if (contaminationDetected)
        {
            var contaminationRationale = $"Metrics contamination detected (baseline: {baselineStates}; candidate: {candidateStates}); verdict forced invalid.";
            return new BenchmarkCompareMetricVerdict(
                metric: definition.Metric,
                verdict: BenchmarkCompareVerdict.Invalid,
                baseline: baselineValue,
                candidate: candidateValue,
                delta: delta,
                threshold: definition.Threshold,
                rationale: contaminationRationale);
        }

        var absoluteDelta = Math.Abs(delta);
        var thresholdValue = definition.Threshold.Value;

        if (absoluteDelta <= thresholdValue || AreClose(absoluteDelta, thresholdValue))
        {
            return new BenchmarkCompareMetricVerdict(
                metric: definition.Metric,
                verdict: BenchmarkCompareVerdict.NoChange,
                baseline: baselineValue,
                candidate: candidateValue,
                delta: delta,
                threshold: definition.Threshold,
                rationale: BuildNoChangeRationale(definition, absoluteDelta));
        }

        var improved = definition.LowerIsBetter
            ? delta < 0
            : delta > 0;

        var verdict = improved
            ? BenchmarkCompareVerdict.Improved
            : BenchmarkCompareVerdict.Regressed;

        return new BenchmarkCompareMetricVerdict(
            metric: definition.Metric,
            verdict: verdict,
            baseline: baselineValue,
            candidate: candidateValue,
            delta: delta,
            threshold: definition.Threshold,
            rationale: BuildDirectionalRationale(definition, delta, verdict));
    }

    private static bool HasMetricsContamination(BenchmarkRunMetricsAggregate aggregate)
    {
        return aggregate.MetricsStates.NotRun > 0
               || aggregate.MetricsStates.Partial > 0
               || aggregate.MetricsStates.Failed > 0;
    }

    private static string DescribeMetricsStates(BenchmarkMetricsStateCounts states)
    {
        return $"notRun={states.NotRun}, completed={states.Completed}, partial={states.Partial}, failed={states.Failed}";
    }

    private static string BuildNoChangeRationale(MetricDefinition definition, double absoluteDelta)
    {
        var unitSuffix = string.IsNullOrWhiteSpace(definition.Threshold.Unit)
            ? string.Empty
            : $" {definition.Threshold.Unit}";

        return $"{definition.Threshold.Rationale} | |delta|={absoluteDelta.ToString("0.###", CultureInfo.InvariantCulture)}{unitSuffix} <= threshold={definition.Threshold.Value.ToString("0.###", CultureInfo.InvariantCulture)}{unitSuffix}.";
    }

    private static string BuildDirectionalRationale(
        MetricDefinition definition,
        double delta,
        BenchmarkCompareVerdict verdict)
    {
        var unitSuffix = string.IsNullOrWhiteSpace(definition.Threshold.Unit)
            ? string.Empty
            : $" {definition.Threshold.Unit}";

        var direction = verdict == BenchmarkCompareVerdict.Improved
            ? "improved"
            : "regressed";

        return $"{definition.Threshold.Rationale} | delta={delta.ToString("0.###", CultureInfo.InvariantCulture)}{unitSuffix} crossed threshold={definition.Threshold.Value.ToString("0.###", CultureInfo.InvariantCulture)}{unitSuffix}; verdict={direction}.";
    }

    private static void CompareProperty<T>(
        ICollection<BenchmarkCompareCompatibilityReason> reasons,
        BenchmarkCompareCompatibilityReasonCode code,
        string field,
        string message,
        T baseline,
        T candidate,
        Func<T, string> formatter)
    {
        if (EqualityComparer<T>.Default.Equals(baseline, candidate))
        {
            return;
        }

        reasons.Add(new BenchmarkCompareCompatibilityReason(
            code: code,
            message: message,
            field: field,
            expected: formatter(baseline),
            actual: formatter(candidate)));
    }

    private static void CompareDoubleProperty(
        ICollection<BenchmarkCompareCompatibilityReason> reasons,
        BenchmarkCompareCompatibilityReasonCode code,
        string field,
        string message,
        double baseline,
        double candidate)
    {
        if (AreClose(baseline, candidate))
        {
            return;
        }

        reasons.Add(new BenchmarkCompareCompatibilityReason(
            code: code,
            message: message,
            field: field,
            expected: baseline.ToString("0.###", CultureInfo.InvariantCulture),
            actual: candidate.ToString("0.###", CultureInfo.InvariantCulture)));
    }

    private static bool SequenceEqualOrdinalIgnoreCase(
        IReadOnlyList<string> baseline,
        IReadOnlyList<string> candidate)
    {
        if (baseline.Count != candidate.Count)
        {
            return false;
        }

        for (var index = 0; index < baseline.Count; index++)
        {
            if (!string.Equals(baseline[index], candidate[index], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static BenchmarkCompareCompatibilityStatus ResolveStatus(
        IReadOnlyCollection<BenchmarkCompareCompatibilityReason> reasons)
    {
        if (reasons.Count == 0)
        {
            return BenchmarkCompareCompatibilityStatus.Compatible;
        }

        var hasMalformedReason = reasons.Any(reason =>
            reason.Code is BenchmarkCompareCompatibilityReasonCode.BaselineManifestMalformed
                or BenchmarkCompareCompatibilityReasonCode.CandidateManifestMalformed);

        return hasMalformedReason
            ? BenchmarkCompareCompatibilityStatus.Malformed
            : BenchmarkCompareCompatibilityStatus.Incompatible;
    }

    private static IReadOnlyList<string> ExtractRawChapterSet(JsonElement root)
    {
        if (!root.TryGetProperty("chapterSet", out var chapterSetNode) || chapterSetNode.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return chapterSetNode
            .EnumerateArray()
            .Select(node => node.ValueKind == JsonValueKind.String ? node.GetString() ?? string.Empty : string.Empty)
            .ToArray();
    }

    private static BenchmarkCompareManifestReference CreateReference(BenchmarkRunManifest manifest)
    {
        return new BenchmarkCompareManifestReference(
            runId: manifest.RunId,
            chapterSetFingerprint: manifest.ChapterSetFingerprint,
            deterministic: manifest.Deterministic,
            phase: manifest.Phase,
            state: manifest.State);
    }

    private static BenchmarkCompareManifestReference CreateReference(BenchmarkInvalidRunArtifact invalidRun)
    {
        return new BenchmarkCompareManifestReference(
            runId: invalidRun.RunId,
            chapterSetFingerprint: invalidRun.ChapterSetFingerprint,
            deterministic: invalidRun.Deterministic,
            phase: invalidRun.Phase,
            state: RunState.Completed);
    }

    private static BenchmarkCompareManifestReference CreateReferenceFromJson(
        JsonElement root,
        BenchmarkCompareManifestReference fallback)
    {
        var runId = ReadString(root, "runId") ?? fallback.RunId;
        var chapterSetFingerprint = ReadString(root, "chapterSetFingerprint") ?? fallback.ChapterSetFingerprint;
        var deterministic = ReadBoolean(root, "deterministic") ?? fallback.Deterministic;

        var phase = fallback.Phase;
        var phaseText = ReadString(root, "phase");
        if (!string.IsNullOrWhiteSpace(phaseText)
            && Enum.TryParse<BenchmarkRunPhase>(phaseText, ignoreCase: true, out var parsedPhase))
        {
            phase = parsedPhase;
        }

        var state = fallback.State;
        var stateText = ReadString(root, "state");
        if (!string.IsNullOrWhiteSpace(stateText)
            && Enum.TryParse<RunState>(stateText, ignoreCase: true, out var parsedState))
        {
            state = parsedState;
        }

        return new BenchmarkCompareManifestReference(
            runId,
            chapterSetFingerprint,
            deterministic,
            phase,
            state);
    }

    private static BenchmarkCompareManifestReference CreateFallbackReference(FileInfo artifactFile, bool baseline)
    {
        var prefix = baseline ? "baseline" : "candidate";
        var fileStem = Path.GetFileNameWithoutExtension(artifactFile.Name);

        var runId = string.IsNullOrWhiteSpace(fileStem)
            ? $"{prefix}-unknown-run"
            : $"{prefix}-{fileStem}";

        return new BenchmarkCompareManifestReference(
            runId: runId,
            chapterSetFingerprint: $"{prefix}-unknown-fingerprint",
            deterministic: false,
            phase: BenchmarkRunPhase.Failed,
            state: RunState.Failed);
    }

    private static string? ReadString(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var node) || node.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = node.GetString();
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static bool? ReadBoolean(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var node) || node.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return null;
        }

        return node.GetBoolean();
    }

    private static bool AreClose(double baseline, double candidate)
        => Math.Abs(baseline - candidate) <= NumericTolerance;

    private static string NormalizePath(string path)
        => path.Replace('\\', '/');

    private static RunFailure MapExceptionToFailure(Exception exception, string stage, string prefix)
    {
        var kind = exception switch
        {
            TimeoutException => RunFailureKind.Timeout,
            OperationCanceledException => RunFailureKind.Cancelled,
            FileNotFoundException => RunFailureKind.Validation,
            DirectoryNotFoundException => RunFailureKind.Validation,
            InvalidDataException => RunFailureKind.Validation,
            JsonException => RunFailureKind.Validation,
            ArgumentException => RunFailureKind.Validation,
            IOException => RunFailureKind.Dependency,
            UnauthorizedAccessException => RunFailureKind.Dependency,
            _ => RunFailureKind.Execution
        };

        return new RunFailure(
            kind,
            $"{prefix}: {exception.Message}",
            stage);
    }

    private static string GenerateDefaultCompareId()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"cmp-{timestamp}-{suffix}";
    }

    private string ResolveCompareId(BenchmarkCompareRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.CompareId))
        {
            return request.CompareId;
        }

        return _compareIdFactory();
    }

    private sealed record ArtifactLoadResult(
        BenchmarkRunManifest? Manifest,
        BenchmarkCompareManifestReference Reference,
        IReadOnlyList<string> RawChapterSet,
        BenchmarkCompareCompatibilityReason? Reason,
        RunFailure? Failure);

    private sealed record MetricDefinition(
        string Metric,
        BenchmarkCompareMetricThreshold Threshold,
        Func<BenchmarkRunMetricsAggregate, double> Selector,
        bool LowerIsBetter);
}
