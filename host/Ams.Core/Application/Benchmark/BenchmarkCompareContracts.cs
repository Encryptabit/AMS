using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ams.Core.Application.Runs;

namespace Ams.Core.Application.Benchmark;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkCompareCompatibilityStatus
{
    Compatible,
    Incompatible,
    Malformed
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkCompareCompatibilityReasonCode
{
    ArtifactTypeMismatch,
    BaselineManifestMalformed,
    CandidateManifestMalformed,
    BaselineManifestInvalid,
    CandidateManifestInvalid,
    AggregateMetricsMismatch,
    ChapterSetMismatch,
    ChapterFingerprintMismatch,
    DeterminismMismatch,
    ModelProvenanceMismatch,
    CachePolicyMismatch,
    ChunkPolicyMismatch,
    MetricsPolicyMismatch
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkCompareVerdict
{
    Improved,
    Regressed,
    NoChange,
    Invalid
}

public sealed record BenchmarkCompareMetricThreshold
{
    [JsonConstructor]
    public BenchmarkCompareMetricThreshold(
        double value,
        string rationale,
        string? unit = null)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Threshold value must be finite and non-negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(rationale);

        Value = value;
        Rationale = rationale.Trim();
        Unit = NormalizeOptionalText(unit);
    }

    public double Value { get; }

    public string? Unit { get; }

    public string Rationale { get; }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

public sealed record BenchmarkCompareMetricVerdict
{
    [JsonConstructor]
    public BenchmarkCompareMetricVerdict(
        string metric,
        BenchmarkCompareVerdict verdict,
        double baseline,
        double candidate,
        double delta,
        BenchmarkCompareMetricThreshold threshold,
        string rationale)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);
        ArgumentNullException.ThrowIfNull(threshold);
        ArgumentException.ThrowIfNullOrWhiteSpace(rationale);

        ValidateFinite(baseline, nameof(baseline));
        ValidateFinite(candidate, nameof(candidate));
        ValidateFinite(delta, nameof(delta));

        Metric = metric.Trim();
        Verdict = verdict;
        Baseline = baseline;
        Candidate = candidate;
        Delta = delta;
        Threshold = threshold;
        Rationale = rationale.Trim();
    }

    public string Metric { get; }

    public BenchmarkCompareVerdict Verdict { get; }

    public double Baseline { get; }

    public double Candidate { get; }

    public double Delta { get; }

    public BenchmarkCompareMetricThreshold Threshold { get; }

    public string Rationale { get; }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }
    }
}

public sealed record BenchmarkCompareCompatibilityReason
{
    [JsonConstructor]
    public BenchmarkCompareCompatibilityReason(
        BenchmarkCompareCompatibilityReasonCode code,
        string message,
        string? field = null,
        string? expected = null,
        string? actual = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Code = code;
        Message = message.Trim();
        Field = NormalizeOptionalText(field);
        Expected = NormalizeOptionalText(expected);
        Actual = NormalizeOptionalText(actual);
    }

    public BenchmarkCompareCompatibilityReasonCode Code { get; }

    public string Message { get; }

    public string? Field { get; }

    public string? Expected { get; }

    public string? Actual { get; }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

public sealed record BenchmarkCompareCompatibility
{
    [JsonConstructor]
    public BenchmarkCompareCompatibility(
        BenchmarkCompareCompatibilityStatus status,
        IReadOnlyList<BenchmarkCompareCompatibilityReason>? reasons = null)
    {
        var normalizedReasons = reasons?
            .Where(reason => reason is not null)
            .Distinct()
            .ToArray()
            ?? [];

        if (status == BenchmarkCompareCompatibilityStatus.Compatible && normalizedReasons.Length > 0)
        {
            throw new ArgumentException("Compatible compare payloads cannot include incompatibility reasons.", nameof(reasons));
        }

        if (status != BenchmarkCompareCompatibilityStatus.Compatible && normalizedReasons.Length == 0)
        {
            throw new ArgumentException("Incompatible or malformed compare payloads must include explicit reasons.", nameof(reasons));
        }

        Status = status;
        Reasons = normalizedReasons;
    }

    public BenchmarkCompareCompatibilityStatus Status { get; }

    public IReadOnlyList<BenchmarkCompareCompatibilityReason> Reasons { get; }

    public bool IsCompatible => Status == BenchmarkCompareCompatibilityStatus.Compatible;
}

public sealed record BenchmarkCompareManifestReference
{
    [JsonConstructor]
    public BenchmarkCompareManifestReference(
        string runId,
        string chapterSetFingerprint,
        bool deterministic,
        BenchmarkRunPhase phase,
        RunState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterSetFingerprint);

        RunId = runId.Trim();
        ChapterSetFingerprint = chapterSetFingerprint.Trim();
        Deterministic = deterministic;
        Phase = phase;
        State = state;
    }

    public string RunId { get; }

    public string ChapterSetFingerprint { get; }

    public bool Deterministic { get; }

    public BenchmarkRunPhase Phase { get; }

    public RunState State { get; }
}

public sealed record BenchmarkCompareArtifact
{
    [JsonConstructor]
    public BenchmarkCompareArtifact(
        string compareId,
        DateTimeOffset comparedAtUtc,
        BenchmarkCompareManifestReference baseline,
        BenchmarkCompareManifestReference candidate,
        BenchmarkCompareCompatibility compatibility,
        IReadOnlyList<BenchmarkCompareMetricVerdict>? metricVerdicts = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compareId);
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(compatibility);

        CompareId = compareId.Trim();
        ComparedAtUtc = comparedAtUtc;
        Baseline = baseline;
        Candidate = candidate;
        Compatibility = compatibility;
        MetricVerdicts = metricVerdicts?
            .Where(verdict => verdict is not null)
            .ToArray()
            ?? [];
    }

    public string CompareId { get; }

    public DateTimeOffset ComparedAtUtc { get; }

    public BenchmarkCompareManifestReference Baseline { get; }

    public BenchmarkCompareManifestReference Candidate { get; }

    public BenchmarkCompareCompatibility Compatibility { get; }

    public IReadOnlyList<BenchmarkCompareMetricVerdict> MetricVerdicts { get; }

    public static string Serialize(BenchmarkCompareArtifact artifact, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        return JsonSerializer.Serialize(artifact, options ?? BenchmarkDeterminismJson.SerializerOptions);
    }

    public static BenchmarkCompareArtifact Deserialize(string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var artifact = JsonSerializer.Deserialize<BenchmarkCompareArtifact>(json, options ?? BenchmarkDeterminismJson.SerializerOptions);
        return artifact ?? throw new InvalidDataException("Benchmark compare payload deserialized to null.");
    }
}

public sealed record BenchmarkCompareRequest
{
    public BenchmarkCompareRequest(
        FileInfo baselineArtifact,
        FileInfo candidateArtifact,
        DirectoryInfo outputRoot,
        string? compareId = null,
        ModuleId? moduleId = null)
    {
        ArgumentNullException.ThrowIfNull(baselineArtifact);
        ArgumentNullException.ThrowIfNull(candidateArtifact);
        ArgumentNullException.ThrowIfNull(outputRoot);

        BaselineArtifact = baselineArtifact;
        CandidateArtifact = candidateArtifact;
        OutputRoot = outputRoot;
        CompareId = NormalizeOptionalText(compareId);
        ModuleId = moduleId ?? ModuleIds.BenchmarkCompare;
    }

    public FileInfo BaselineArtifact { get; }

    public FileInfo CandidateArtifact { get; }

    public DirectoryInfo OutputRoot { get; }

    public string? CompareId { get; }

    public ModuleId ModuleId { get; }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

public sealed record BenchmarkCompareResult
{
    public BenchmarkCompareResult(
        string compareId,
        ModuleId moduleId,
        BenchmarkCompareCompatibility compatibility,
        IReadOnlyList<BenchmarkCompareMetricVerdict>? metricVerdicts = null,
        FileInfo? artifactFile = null,
        RunFailure? failure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compareId);
        ArgumentNullException.ThrowIfNull(moduleId);
        ArgumentNullException.ThrowIfNull(compatibility);

        CompareId = compareId.Trim();
        ModuleId = moduleId;
        Compatibility = compatibility;
        MetricVerdicts = metricVerdicts?
            .Where(verdict => verdict is not null)
            .ToArray()
            ?? [];
        ArtifactFile = artifactFile;
        Failure = failure;
    }

    public string CompareId { get; }

    public ModuleId ModuleId { get; }

    public BenchmarkCompareCompatibility Compatibility { get; }

    public IReadOnlyList<BenchmarkCompareMetricVerdict> MetricVerdicts { get; }

    public FileInfo? ArtifactFile { get; }

    public RunFailure? Failure { get; }
}
