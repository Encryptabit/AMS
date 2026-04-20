using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ams.Core.Application.Pipeline;
using Ams.Core.Services.Alignment;

namespace Ams.Core.Application.Benchmark;

public static class BenchmarkDeterminismJson
{
    public static JsonSerializerOptions SerializerOptions { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkDeterminismVerdict
{
    Valid,
    Invalid
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkReadinessState
{
    Ready,
    Warning,
    Failed,
    Unknown
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkModelProvenanceKind
{
    PinnedPath,
    MissingExplicitModel,
    AliasOnly,
    MissingModelFile,
    InvalidModelInput
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkDeterminismReasonCode
{
    MissingExplicitModel,
    AliasOnlyModel,
    InvalidModelInput,
    MissingModelFile,
    DependencyProbeMalformed,
    DependencyProbeTimeout,
    DependencyProbeFailed,
    FfmpegNotReady,
    MfaNotReady
}

public sealed record BenchmarkDependencyReadiness
{
    [JsonConstructor]
    public BenchmarkDependencyReadiness(
        string dependency,
        BenchmarkReadinessState state,
        string summary,
        string? detail = null,
        int? exitCode = null,
        long? durationMs = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dependency);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);

        Dependency = dependency;
        State = state;
        Summary = summary;
        Detail = detail;
        ExitCode = exitCode;
        DurationMs = durationMs;
    }

    public string Dependency { get; }

    public BenchmarkReadinessState State { get; }

    public string Summary { get; }

    public string? Detail { get; }

    public int? ExitCode { get; }

    public long? DurationMs { get; }

    public bool IsReady => State == BenchmarkReadinessState.Ready;
}

public sealed record BenchmarkDependencyReadinessSnapshot
{
    [JsonConstructor]
    public BenchmarkDependencyReadinessSnapshot(
        DateTimeOffset capturedAtUtc,
        BenchmarkDependencyReadiness ffmpeg,
        BenchmarkDependencyReadiness mfa,
        IReadOnlyList<string>? notes = null)
    {
        ArgumentNullException.ThrowIfNull(ffmpeg);
        ArgumentNullException.ThrowIfNull(mfa);

        CapturedAtUtc = capturedAtUtc;
        Ffmpeg = ffmpeg;
        Mfa = mfa;
        Notes = notes?
            .Where(note => !string.IsNullOrWhiteSpace(note))
            .Distinct(StringComparer.Ordinal)
            .ToArray()
            ?? [];
    }

    public DateTimeOffset CapturedAtUtc { get; }

    public BenchmarkDependencyReadiness Ffmpeg { get; }

    public BenchmarkDependencyReadiness Mfa { get; }

    public IReadOnlyList<string> Notes { get; }

    public bool IsReady => Ffmpeg.IsReady && Mfa.IsReady;
}

public interface IBenchmarkDependencyReadinessProbe
{
    Task<BenchmarkDependencyReadinessSnapshot> CaptureAsync(CancellationToken cancellationToken = default);
}

public sealed record BenchmarkModelProvenance
{
    [JsonConstructor]
    public BenchmarkModelProvenance(
        BenchmarkReadinessState state,
        BenchmarkModelProvenanceKind sourceKind,
        string? requestedModel,
        string? normalizedModelPath,
        bool isDeterministic,
        string summary,
        string guidance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(guidance);

        State = state;
        SourceKind = sourceKind;
        RequestedModel = requestedModel;
        NormalizedModelPath = normalizedModelPath;
        IsDeterministic = isDeterministic;
        Summary = summary;
        Guidance = guidance;
    }

    public BenchmarkReadinessState State { get; }

    public BenchmarkModelProvenanceKind SourceKind { get; }

    public string? RequestedModel { get; }

    public string? NormalizedModelPath { get; }

    public bool IsDeterministic { get; }

    public string Summary { get; }

    public string Guidance { get; }
}

public sealed record BenchmarkCachePolicy
{
    [JsonConstructor]
    public BenchmarkCachePolicy(bool forcePipelineRebuild, bool forceBookIndexRebuild)
    {
        ForcePipelineRebuild = forcePipelineRebuild;
        ForceBookIndexRebuild = forceBookIndexRebuild;
    }

    public bool ForcePipelineRebuild { get; }

    public bool ForceBookIndexRebuild { get; }

    public bool AllowsCachedPipelineArtifacts => !ForcePipelineRebuild;

    public bool AllowsCachedBookIndex => !ForcePipelineRebuild && !ForceBookIndexRebuild;

    public string Summary => ForcePipelineRebuild
        ? "Pipeline artifact cache bypass requested."
        : ForceBookIndexRebuild
            ? "Book-index cache bypass requested."
            : "Pipeline and book-index caches allowed.";

    public static BenchmarkCachePolicy Default { get; } = new(forcePipelineRebuild: false, forceBookIndexRebuild: false);

    public static BenchmarkCachePolicy FromPipelineOptions(PipelineRunOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new BenchmarkCachePolicy(options.Force, options.ForceIndex);
    }
}

public sealed record BenchmarkChunkPolicy
{
    [JsonConstructor]
    public BenchmarkChunkPolicy(
        bool disableChunkPlan,
        bool disableChunkedMfa,
        bool usedDefaultPlanningPolicy,
        double silenceThresholdDb,
        double minSilenceDurationMs,
        double minChunkDurationSec,
        double maxChunkDurationSec)
    {
        ValidateFinite(silenceThresholdDb, nameof(silenceThresholdDb));
        ValidateFinite(minSilenceDurationMs, nameof(minSilenceDurationMs));
        ValidateFinite(minChunkDurationSec, nameof(minChunkDurationSec));
        ValidateFinite(maxChunkDurationSec, nameof(maxChunkDurationSec));

        if (minSilenceDurationMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minSilenceDurationMs), "Minimum silence duration must be positive.");
        }

        if (minChunkDurationSec <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minChunkDurationSec), "Minimum chunk duration must be positive.");
        }

        if (maxChunkDurationSec <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxChunkDurationSec), "Maximum chunk duration must be positive.");
        }

        if (maxChunkDurationSec < minChunkDurationSec)
        {
            throw new ArgumentException("Maximum chunk duration cannot be less than minimum chunk duration.", nameof(maxChunkDurationSec));
        }

        DisableChunkPlan = disableChunkPlan;
        DisableChunkedMfa = disableChunkedMfa;
        UsedDefaultPlanningPolicy = usedDefaultPlanningPolicy;
        SilenceThresholdDb = silenceThresholdDb;
        MinSilenceDurationMs = minSilenceDurationMs;
        MinChunkDurationSec = minChunkDurationSec;
        MaxChunkDurationSec = maxChunkDurationSec;
    }

    public bool DisableChunkPlan { get; }

    public bool DisableChunkedMfa { get; }

    public bool UsedDefaultPlanningPolicy { get; }

    public double SilenceThresholdDb { get; }

    public double MinSilenceDurationMs { get; }

    public double MinChunkDurationSec { get; }

    public double MaxChunkDurationSec { get; }

    public string Summary => DisableChunkPlan
        ? "Chunk planning disabled for ASR; deterministic chunk provenance is reduced."
        : UsedDefaultPlanningPolicy
            ? "Chunk planning enabled with shared default policy."
            : "Chunk planning enabled with explicit policy overrides.";

    public static BenchmarkChunkPolicy Default { get; } = FromChunkPlanningPolicy(
        ChunkPlanningPolicy.Default,
        disableChunkPlan: false,
        disableChunkedMfa: false,
        usedDefaultPlanningPolicy: true);

    public static BenchmarkChunkPolicy FromPipelineOptions(PipelineRunOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var policy = options.ChunkPlanningPolicy ?? ChunkPlanningPolicy.Default;
        var usingDefaultPolicy = options.ChunkPlanningPolicy is null;

        return FromChunkPlanningPolicy(
            policy,
            disableChunkPlan: options.DisableChunkPlan,
            disableChunkedMfa: options.DisableChunkedMfa,
            usedDefaultPlanningPolicy: usingDefaultPolicy);
    }

    private static BenchmarkChunkPolicy FromChunkPlanningPolicy(
        ChunkPlanningPolicy policy,
        bool disableChunkPlan,
        bool disableChunkedMfa,
        bool usedDefaultPlanningPolicy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return new BenchmarkChunkPolicy(
            disableChunkPlan,
            disableChunkedMfa,
            usedDefaultPlanningPolicy,
            silenceThresholdDb: policy.SilenceThresholdDb,
            minSilenceDurationMs: policy.MinSilenceDuration.TotalMilliseconds,
            minChunkDurationSec: policy.MinChunkDuration.TotalSeconds,
            maxChunkDurationSec: policy.MaxChunkDuration.TotalSeconds);
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }
    }
}

public sealed record BenchmarkDeterminismGateRequest
{
    [JsonConstructor]
    public BenchmarkDeterminismGateRequest(
        string? requestedModel,
        BenchmarkCachePolicy cachePolicy,
        BenchmarkChunkPolicy chunkPolicy)
    {
        RequestedModel = requestedModel;
        CachePolicy = cachePolicy ?? throw new ArgumentNullException(nameof(cachePolicy));
        ChunkPolicy = chunkPolicy ?? throw new ArgumentNullException(nameof(chunkPolicy));
    }

    public string? RequestedModel { get; }

    public BenchmarkCachePolicy CachePolicy { get; }

    public BenchmarkChunkPolicy ChunkPolicy { get; }

    public static BenchmarkDeterminismGateRequest FromPipelineOptions(
        string? requestedModel,
        PipelineRunOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new BenchmarkDeterminismGateRequest(
            requestedModel,
            BenchmarkCachePolicy.FromPipelineOptions(options),
            BenchmarkChunkPolicy.FromPipelineOptions(options));
    }
}

public sealed record BenchmarkDeterminismContract
{
    [JsonConstructor]
    public BenchmarkDeterminismContract(
        DateTimeOffset evaluatedAtUtc,
        BenchmarkDeterminismVerdict verdict,
        IReadOnlyList<BenchmarkDeterminismReasonCode>? reasonCodes,
        BenchmarkModelProvenance modelProvenance,
        BenchmarkDependencyReadiness ffmpeg,
        BenchmarkDependencyReadiness mfa,
        BenchmarkCachePolicy cachePolicy,
        BenchmarkChunkPolicy chunkPolicy,
        IReadOnlyList<string>? guidance = null)
    {
        ArgumentNullException.ThrowIfNull(reasonCodes);
        ArgumentNullException.ThrowIfNull(modelProvenance);
        ArgumentNullException.ThrowIfNull(ffmpeg);
        ArgumentNullException.ThrowIfNull(mfa);
        ArgumentNullException.ThrowIfNull(cachePolicy);
        ArgumentNullException.ThrowIfNull(chunkPolicy);

        var normalizedReasons = reasonCodes
            .Distinct()
            .ToArray();

        if (verdict == BenchmarkDeterminismVerdict.Valid && normalizedReasons.Length > 0)
        {
            throw new ArgumentException("Valid deterministic contracts cannot include rejection reason codes.", nameof(reasonCodes));
        }

        if (verdict == BenchmarkDeterminismVerdict.Invalid && normalizedReasons.Length == 0)
        {
            throw new ArgumentException("Invalid deterministic contracts must include at least one rejection reason code.", nameof(reasonCodes));
        }

        EvaluatedAtUtc = evaluatedAtUtc;
        Verdict = verdict;
        ReasonCodes = normalizedReasons;
        ModelProvenance = modelProvenance;
        Ffmpeg = ffmpeg;
        Mfa = mfa;
        CachePolicy = cachePolicy;
        ChunkPolicy = chunkPolicy;
        Guidance = guidance?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray()
            ?? [];
    }

    public DateTimeOffset EvaluatedAtUtc { get; }

    public BenchmarkDeterminismVerdict Verdict { get; }

    public IReadOnlyList<BenchmarkDeterminismReasonCode> ReasonCodes { get; }

    public BenchmarkModelProvenance ModelProvenance { get; }

    public BenchmarkDependencyReadiness Ffmpeg { get; }

    public BenchmarkDependencyReadiness Mfa { get; }

    public BenchmarkCachePolicy CachePolicy { get; }

    public BenchmarkChunkPolicy ChunkPolicy { get; }

    public IReadOnlyList<string> Guidance { get; }

    public bool IsValid => Verdict == BenchmarkDeterminismVerdict.Valid;

    public static string Serialize(BenchmarkDeterminismContract contract, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contract);
        return JsonSerializer.Serialize(contract, options ?? BenchmarkDeterminismJson.SerializerOptions);
    }

    public static BenchmarkDeterminismContract Deserialize(string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var contract = JsonSerializer.Deserialize<BenchmarkDeterminismContract>(json, options ?? BenchmarkDeterminismJson.SerializerOptions);
        return contract ?? throw new InvalidDataException("Benchmark determinism contract payload deserialized to null.");
    }
}
