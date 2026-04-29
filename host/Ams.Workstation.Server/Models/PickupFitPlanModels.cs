using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Ams.Workstation.Server.Models;

/// <summary>
/// Operator-visible lifecycle for one chapter-scoped pickup Fit row.
/// </summary>
public enum PickupFitPlanItemStatus
{
    Draft = 0,
    Fitted = 1,
    Previewed = 2,
    Accepted = 3,
    CommitReady = 4,
    Committed = 5,
    Failed = 6
}

/// <summary>
/// Commit lifecycle persisted separately from item fit/preview lifecycle.
/// </summary>
public enum PickupFitCommitStatus
{
    NotReady = 0,
    Ready = 1,
    Committed = 2,
    Failed = 3
}

/// <summary>
/// Positive timing range used for outer chapter cuts, pickup inner cuts, and destination placement.
/// </summary>
public sealed record PickupFitPlanRange
{
    private const double RangeEpsilonSec = 0.000_001;

    [JsonConstructor]
    public PickupFitPlanRange(double startSec, double endSec)
    {
        EnsureRange(startSec, endSec, "fit-plan range");

        StartSec = startSec;
        EndSec = endSec;
    }

    public double StartSec { get; }

    public double EndSec { get; }

    [JsonIgnore]
    public double DurationSec => EndSec - StartSec;

    public bool Contains(PickupFitPlanRange inner)
    {
        ArgumentNullException.ThrowIfNull(inner);

        return inner.StartSec >= StartSec - RangeEpsilonSec &&
               inner.EndSec <= EndSec + RangeEpsilonSec;
    }

    public static void EnsureRange(double startSec, double endSec, string rangeName)
    {
        if (double.IsNaN(startSec) || double.IsInfinity(startSec) ||
            double.IsNaN(endSec) || double.IsInfinity(endSec))
        {
            throw new ArgumentOutOfRangeException(
                rangeName,
                $"Pickup fit-plan {rangeName} has non-finite timing values.");
        }

        if (startSec < 0)
        {
            throw new ArgumentOutOfRangeException(
                rangeName,
                startSec,
                $"Pickup fit-plan {rangeName} has negative start '{startSec}'.");
        }

        if (endSec <= startSec + RangeEpsilonSec)
        {
            throw new ArgumentOutOfRangeException(
                rangeName,
                $"Pickup fit-plan {rangeName} has invalid range [{startSec:F6}, {endSec:F6}] (end must be greater than start). ");
        }
    }
}

/// <summary>
/// Audio transition policy chosen during Fit before preview/commit.
/// </summary>
public sealed record PickupFitTransitionPolicy
{
    [JsonConstructor]
    public PickupFitTransitionPolicy(
        double roomtoneBeforeSec,
        double roomtoneAfterSec,
        double crossfadeDurationSec,
        string crossfadeCurve,
        string? roomtoneAssetId = null)
    {
        EnsureNonNegativeFinite(roomtoneBeforeSec, nameof(roomtoneBeforeSec));
        EnsureNonNegativeFinite(roomtoneAfterSec, nameof(roomtoneAfterSec));
        EnsureNonNegativeFinite(crossfadeDurationSec, nameof(crossfadeDurationSec));
        ArgumentException.ThrowIfNullOrWhiteSpace(crossfadeCurve);

        RoomtoneBeforeSec = roomtoneBeforeSec;
        RoomtoneAfterSec = roomtoneAfterSec;
        CrossfadeDurationSec = crossfadeDurationSec;
        CrossfadeCurve = crossfadeCurve.Trim();
        RoomtoneAssetId = string.IsNullOrWhiteSpace(roomtoneAssetId) ? null : roomtoneAssetId.Trim();
    }

    public double RoomtoneBeforeSec { get; }

    public double RoomtoneAfterSec { get; }

    public double CrossfadeDurationSec { get; }

    public string CrossfadeCurve { get; }

    public string? RoomtoneAssetId { get; }

    public static PickupFitTransitionPolicy Default { get; } = new(
        roomtoneBeforeSec: 0,
        roomtoneAfterSec: 0,
        crossfadeDurationSec: 0.025,
        crossfadeCurve: "equal-power");

    private static void EnsureNonNegativeFinite(double value, string fieldName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(
                fieldName,
                value,
                $"Pickup fit-plan transition policy field '{fieldName}' must be non-negative and finite.");
        }
    }
}

/// <summary>
/// Redacted previous/current/next sentence context included in a Fit preview.
/// Carries ids and timings only; full transcript text stays out of durable diagnostics.
/// </summary>
public sealed record PickupFitPreviewContext
{
    [JsonConstructor]
    public PickupFitPreviewContext(
        string role,
        int sentenceId,
        double? startSec,
        double? endSec,
        string? fitItemId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        if (sentenceId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sentenceId),
                sentenceId,
                "Pickup fit preview context sentence id must be greater than zero.");
        }

        if (startSec.HasValue != endSec.HasValue)
        {
            throw new ArgumentException(
                "Pickup fit preview context must provide both start and end timing, or neither.",
                nameof(startSec));
        }

        if (startSec.HasValue)
        {
            PickupFitPlanRange.EnsureRange(startSec.Value, endSec!.Value, "preview context");
        }

        Role = role.Trim();
        SentenceId = sentenceId;
        StartSec = startSec;
        EndSec = endSec;
        FitItemId = string.IsNullOrWhiteSpace(fitItemId) ? null : fitItemId.Trim();
    }

    public string Role { get; }

    public int SentenceId { get; }

    public double? StartSec { get; }

    public double? EndSec { get; }

    public string? FitItemId { get; }
}

/// <summary>
/// Bounded evidence that a preview was produced from a specific Pick truth snapshot.
/// </summary>
public sealed record PickupFitPreviewEvidence
{
    [JsonConstructor]
    public PickupFitPreviewEvidence(
        int previewVersion,
        int pickMapRevision,
        string pickAssignmentsFingerprint,
        string previewArtifactRef,
        double renderedDurationSec,
        DateTime generatedAtUtc,
        string? previousFitItemId = null,
        string? nextFitItemId = null,
        string? fitStateFingerprint = null,
        IReadOnlyList<PickupFitPreviewContext>? sentenceContexts = null)
    {
        if (previewVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(previewVersion),
                previewVersion,
                "Pickup fit-plan preview version must be greater than zero.");
        }

        if (pickMapRevision < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pickMapRevision),
                pickMapRevision,
                "Pickup fit-plan preview pick-map revision must be non-negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(pickAssignmentsFingerprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(previewArtifactRef);

        if (double.IsNaN(renderedDurationSec) || double.IsInfinity(renderedDurationSec) || renderedDurationSec <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(renderedDurationSec),
                renderedDurationSec,
                "Pickup fit-plan preview rendered duration must be positive and finite.");
        }

        PreviewVersion = previewVersion;
        PickMapRevision = pickMapRevision;
        PickAssignmentsFingerprint = pickAssignmentsFingerprint.Trim();
        PreviewArtifactRef = NormalizeArtifactRef(previewArtifactRef, nameof(previewArtifactRef));
        RenderedDurationSec = renderedDurationSec;
        GeneratedAtUtc = generatedAtUtc.ToUniversalTime();
        PreviousFitItemId = string.IsNullOrWhiteSpace(previousFitItemId) ? null : previousFitItemId.Trim();
        NextFitItemId = string.IsNullOrWhiteSpace(nextFitItemId) ? null : nextFitItemId.Trim();
        FitStateFingerprint = string.IsNullOrWhiteSpace(fitStateFingerprint) ? null : fitStateFingerprint.Trim();
        SentenceContexts = (sentenceContexts ?? Array.Empty<PickupFitPreviewContext>()).ToArray();
    }

    public int PreviewVersion { get; }

    public int PickMapRevision { get; }

    public string PickAssignmentsFingerprint { get; }

    public string PreviewArtifactRef { get; }

    public double RenderedDurationSec { get; }

    public DateTime GeneratedAtUtc { get; }

    public string? PreviousFitItemId { get; }

    public string? NextFitItemId { get; }

    public string? FitStateFingerprint { get; }

    public IReadOnlyList<PickupFitPreviewContext> SentenceContexts { get; }

    public bool MatchesPickTruth(int pickMapRevision, string pickAssignmentsFingerprint)
        => PickMapRevision == pickMapRevision &&
           string.Equals(PickAssignmentsFingerprint, pickAssignmentsFingerprint, StringComparison.Ordinal);

    public bool MatchesFitState(string fitStateFingerprint)
        => !string.IsNullOrWhiteSpace(FitStateFingerprint) &&
           string.Equals(FitStateFingerprint, fitStateFingerprint, StringComparison.Ordinal);

    private static string NormalizeArtifactRef(string artifactRef, string paramName)
    {
        var normalized = artifactRef.Trim().Replace('\\', '/');
        if (Path.IsPathRooted(normalized))
        {
            throw new ArgumentException(
                $"Pickup fit-plan preview artifact ref '{normalized}' must be workspace-relative.",
                paramName);
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Any(segment => string.Equals(segment, "..", StringComparison.Ordinal)))
        {
            throw new ArgumentException(
                $"Pickup fit-plan preview artifact ref '{normalized}' cannot escape workspace root.",
                paramName);
        }

        return normalized;
    }
}

/// <summary>
/// Operator acceptance proof tied to one preview version.
/// </summary>
public sealed record PickupFitAcceptanceState
{
    [JsonConstructor]
    public PickupFitAcceptanceState(
        bool isAccepted,
        int? acceptedPreviewVersion,
        DateTime? acceptedAtUtc,
        string? acceptedBy)
    {
        if (isAccepted)
        {
            if (acceptedPreviewVersion is null or <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(acceptedPreviewVersion),
                    acceptedPreviewVersion,
                    "Accepted pickup fit item must record the accepted preview version.");
            }

            if (acceptedAtUtc is null)
            {
                throw new ArgumentNullException(
                    nameof(acceptedAtUtc),
                    "Accepted pickup fit item must record acceptance timestamp.");
            }
        }

        IsAccepted = isAccepted;
        AcceptedPreviewVersion = acceptedPreviewVersion;
        AcceptedAtUtc = acceptedAtUtc?.ToUniversalTime();
        AcceptedBy = string.IsNullOrWhiteSpace(acceptedBy) ? null : acceptedBy.Trim();
    }

    public bool IsAccepted { get; }

    public int? AcceptedPreviewVersion { get; }

    public DateTime? AcceptedAtUtc { get; }

    public string? AcceptedBy { get; }

    public static PickupFitAcceptanceState None { get; } = new(
        isAccepted: false,
        acceptedPreviewVersion: null,
        acceptedAtUtc: null,
        acceptedBy: null);
}

/// <summary>
/// Commit metadata from the Fit-to-EDL/ledger boundary.
/// </summary>
public sealed record PickupFitCommitState
{
    [JsonConstructor]
    public PickupFitCommitState(
        PickupFitCommitStatus status,
        string? operationId,
        int? edlRevision,
        long? ledgerSequence,
        DateTime? committedAtUtc,
        string? error)
    {
        if (!Enum.IsDefined(typeof(PickupFitCommitStatus), status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                $"Pickup fit-plan commit state has unknown status value '{(int)status}'.");
        }

        if (edlRevision is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(edlRevision),
                edlRevision,
                "Pickup fit-plan commit EDL revision must be non-negative.");
        }

        if (ledgerSequence is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ledgerSequence),
                ledgerSequence,
                "Pickup fit-plan commit ledger sequence must be greater than zero when present.");
        }

        if (status is PickupFitCommitStatus.Committed)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(operationId);
            if (edlRevision is null)
            {
                throw new ArgumentNullException(
                    nameof(edlRevision),
                    "Committed pickup fit item must record EDL revision.");
            }

            if (ledgerSequence is null)
            {
                throw new ArgumentNullException(
                    nameof(ledgerSequence),
                    "Committed pickup fit item must record artifact-ledger sequence.");
            }

            if (committedAtUtc is null)
            {
                throw new ArgumentNullException(
                    nameof(committedAtUtc),
                    "Committed pickup fit item must record commit timestamp.");
            }
        }

        Status = status;
        OperationId = string.IsNullOrWhiteSpace(operationId) ? null : operationId.Trim();
        EdlRevision = edlRevision;
        LedgerSequence = ledgerSequence;
        CommittedAtUtc = committedAtUtc?.ToUniversalTime();
        Error = string.IsNullOrWhiteSpace(error) ? null : error.Trim();
    }

    public PickupFitCommitStatus Status { get; }

    public string? OperationId { get; }

    public int? EdlRevision { get; }

    public long? LedgerSequence { get; }

    public DateTime? CommittedAtUtc { get; }

    public string? Error { get; }

    public static PickupFitCommitState NotReady { get; } = new(
        status: PickupFitCommitStatus.NotReady,
        operationId: null,
        edlRevision: null,
        ledgerSequence: null,
        committedAtUtc: null,
        error: null);
}

/// <summary>
/// One durable Fit row derived from one canonical Pick-map assignment.
/// </summary>
public sealed record PickupFitPlanItem
{
    [JsonConstructor]
    public PickupFitPlanItem(
        string fitItemId,
        string replacementId,
        string pickAssignmentId,
        string pickupSegmentId,
        PickupPickMapTargetReference target,
        PickupFitPlanRange outerRange,
        PickupFitPlanRange innerRange,
        PickupFitPlanRange placement,
        PickupFitTransitionPolicy transitionPolicy,
        PickupFitPlanItemStatus status,
        PickupFitPreviewEvidence? previewEvidence,
        PickupFitAcceptanceState acceptance,
        PickupFitCommitState commit,
        string? validationError,
        string? commitError,
        DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fitItemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(pickAssignmentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupSegmentId);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(outerRange);
        ArgumentNullException.ThrowIfNull(innerRange);
        ArgumentNullException.ThrowIfNull(placement);
        ArgumentNullException.ThrowIfNull(transitionPolicy);
        ArgumentNullException.ThrowIfNull(acceptance);
        ArgumentNullException.ThrowIfNull(commit);

        if (!Enum.IsDefined(typeof(PickupFitPlanItemStatus), status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                $"Pickup fit-plan item '{fitItemId}' has unknown status value '{(int)status}'.");
        }

        if (!outerRange.Contains(placement))
        {
            throw new InvalidOperationException(
                $"Pickup fit-plan item '{fitItemId}' has placement [{placement.StartSec:F3}, {placement.EndSec:F3}] " +
                $"outside outer range [{outerRange.StartSec:F3}, {outerRange.EndSec:F3}].");
        }

        if (status is PickupFitPlanItemStatus.Previewed && previewEvidence is null)
        {
            throw new InvalidOperationException(
                $"Pickup fit-plan item '{fitItemId}' is Previewed but has no preview evidence.");
        }

        if (status is PickupFitPlanItemStatus.Accepted or PickupFitPlanItemStatus.CommitReady or PickupFitPlanItemStatus.Committed &&
            !acceptance.IsAccepted)
        {
            throw new InvalidOperationException(
                $"Pickup fit-plan item '{fitItemId}' has status '{status}' but is not accepted.");
        }

        if (status is PickupFitPlanItemStatus.CommitReady && commit.Status != PickupFitCommitStatus.Ready)
        {
            throw new InvalidOperationException(
                $"Pickup fit-plan item '{fitItemId}' is CommitReady but commit state is '{commit.Status}'.");
        }

        if (status is PickupFitPlanItemStatus.Committed && commit.Status != PickupFitCommitStatus.Committed)
        {
            throw new InvalidOperationException(
                $"Pickup fit-plan item '{fitItemId}' is Committed but commit state is '{commit.Status}'.");
        }

        if (commit.Status is PickupFitCommitStatus.Ready or PickupFitCommitStatus.Committed && !acceptance.IsAccepted)
        {
            throw new InvalidOperationException(
                $"Pickup fit-plan item '{fitItemId}' has commit state '{commit.Status}' but is not accepted.");
        }

        FitItemId = fitItemId.Trim();
        ReplacementId = replacementId.Trim();
        PickAssignmentId = pickAssignmentId.Trim();
        PickupSegmentId = pickupSegmentId.Trim();
        Target = target;
        OuterRange = outerRange;
        InnerRange = innerRange;
        Placement = placement;
        TransitionPolicy = transitionPolicy;
        Status = status;
        PreviewEvidence = previewEvidence;
        Acceptance = acceptance;
        Commit = commit;
        ValidationError = string.IsNullOrWhiteSpace(validationError) ? null : validationError.Trim();
        CommitError = string.IsNullOrWhiteSpace(commitError) ? null : commitError.Trim();
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public string FitItemId { get; }

    public string ReplacementId { get; }

    public string PickAssignmentId { get; }

    public string PickupSegmentId { get; }

    public PickupPickMapTargetReference Target { get; }

    public PickupFitPlanRange OuterRange { get; }

    public PickupFitPlanRange InnerRange { get; }

    public PickupFitPlanRange Placement { get; }

    public PickupFitTransitionPolicy TransitionPolicy { get; }

    public PickupFitPlanItemStatus Status { get; }

    public PickupFitPreviewEvidence? PreviewEvidence { get; }

    public PickupFitAcceptanceState Acceptance { get; }

    public PickupFitCommitState Commit { get; }

    public string? ValidationError { get; }

    public string? CommitError { get; }

    public DateTime UpdatedAtUtc { get; }

    internal static PickupFitPlanItem FromPickAssignment(PickupPickMapAssignment assignment, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        var target = assignment.EffectiveTarget
            ?? throw new InvalidOperationException(
                $"Pickup fit-plan cannot create item for pick assignment '{assignment.Id}' because it has no target.");

        var targetRange = new PickupFitPlanRange(target.OriginalStartSec, target.OriginalEndSec);

        return new PickupFitPlanItem(
            fitItemId: BuildStableId("fit", assignment.Id),
            replacementId: BuildStableId("replacement", assignment.Id),
            pickAssignmentId: assignment.Id,
            pickupSegmentId: assignment.PickupSegmentId,
            target: target,
            outerRange: targetRange,
            innerRange: new PickupFitPlanRange(assignment.SourceStartSec, assignment.SourceEndSec),
            placement: targetRange,
            transitionPolicy: PickupFitTransitionPolicy.Default,
            status: PickupFitPlanItemStatus.Draft,
            previewEvidence: null,
            acceptance: PickupFitAcceptanceState.None,
            commit: PickupFitCommitState.NotReady,
            validationError: assignment.ValidationError,
            commitError: null,
            updatedAtUtc: nowUtc);
    }

    private static string BuildStableId(string prefix, string assignmentId)
        => $"{prefix}::{assignmentId.Trim()}";
}

/// <summary>
/// Durable chapter-scoped Fit plan persisted under .polish/pickups/fit/{chapterStem}.fit-plan.json.
/// </summary>
public sealed record PickupFitPlanDocument
{
    public const string CurrentSchemaVersion = "pickup-fit-plan/v1";

    [JsonConstructor]
    public PickupFitPlanDocument(
        string schemaVersion,
        string chapterStem,
        int revision,
        PickupPickMapSourceReference source,
        int pickMapRevision,
        string pickAssignmentsFingerprint,
        IReadOnlyList<PickupFitPlanItem>? items,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        string? lastOperationId,
        string? lastValidationError,
        bool isDraft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(pickAssignmentsFingerprint);

        if (!string.Equals(schemaVersion, CurrentSchemaVersion, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Unsupported pickup fit-plan schema version '{schemaVersion}'. Expected '{CurrentSchemaVersion}'.",
                nameof(schemaVersion));
        }

        if (revision < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(revision),
                revision,
                $"Pickup fit-plan revision for chapter '{chapterStem}' must be non-negative.");
        }

        if (pickMapRevision < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pickMapRevision),
                pickMapRevision,
                $"Pickup fit-plan pick-map revision for chapter '{chapterStem}' must be non-negative.");
        }

        var normalizedItems = OrderItems(items ?? Array.Empty<PickupFitPlanItem>());
        ValidateItems(chapterStem, pickMapRevision, pickAssignmentsFingerprint.Trim(), normalizedItems, isDraft);

        SchemaVersion = schemaVersion;
        ChapterStem = chapterStem.Trim();
        Revision = revision;
        Source = source;
        PickMapRevision = pickMapRevision;
        PickAssignmentsFingerprint = pickAssignmentsFingerprint.Trim();
        Items = normalizedItems;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
        LastOperationId = string.IsNullOrWhiteSpace(lastOperationId) ? null : lastOperationId.Trim();
        LastValidationError = string.IsNullOrWhiteSpace(lastValidationError) ? null : lastValidationError.Trim();
        IsDraft = isDraft;
    }

    public string SchemaVersion { get; }

    public string ChapterStem { get; }

    public int Revision { get; }

    public PickupPickMapSourceReference Source { get; }

    public int PickMapRevision { get; }

    public string PickAssignmentsFingerprint { get; }

    public IReadOnlyList<PickupFitPlanItem> Items { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; }

    public string? LastOperationId { get; }

    public string? LastValidationError { get; }

    public bool IsDraft { get; }

    public IReadOnlyList<PickupFitPlanItem> GetDeterministicItemOrder()
        => OrderItems(Items);

    public IReadOnlyDictionary<PickupFitPlanItemStatus, int> GetItemCountsByStatus()
        => Items
            .GroupBy(item => item.Status)
            .OrderBy(group => group.Key)
            .ToDictionary(group => group.Key, group => group.Count());

    public static PickupFitPlanDocument CreateInitial(
        string chapterStem,
        PickupPickMapDocument pickMap,
        string? lastOperationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(pickMap);

        var now = DateTime.UtcNow;
        var fingerprint = ComputePickAssignmentsFingerprint(chapterStem, pickMap);
        var items = GetCanonicalAssignments(chapterStem, pickMap)
            .Select(assignment => PickupFitPlanItem.FromPickAssignment(assignment, now))
            .ToArray();

        return new PickupFitPlanDocument(
            schemaVersion: CurrentSchemaVersion,
            chapterStem: chapterStem,
            revision: 0,
            source: pickMap.Source,
            pickMapRevision: pickMap.Revision,
            pickAssignmentsFingerprint: fingerprint,
            items: items,
            createdAtUtc: now,
            updatedAtUtc: now,
            lastOperationId: lastOperationId,
            lastValidationError: null,
            isDraft: true);
    }

    public static string ComputePickAssignmentsFingerprint(string chapterStem, PickupPickMapDocument pickMap)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(pickMap);

        var builder = new StringBuilder();
        builder.Append("pickup-fit-assignments/v1").Append('\n');
        builder.Append("chapter=").Append(chapterStem.Trim()).Append('\n');
        builder.Append("pickRevision=").Append(pickMap.Revision).Append('\n');
        builder.Append("source=").Append(pickMap.Source.Fingerprint).Append('\n');
        builder.Append("crx=").Append(pickMap.Source.CrxTargetsFingerprint).Append('\n');

        foreach (var assignment in GetCanonicalAssignments(chapterStem, pickMap))
        {
            var target = assignment.EffectiveTarget!;
            builder
                .Append(assignment.Id).Append('\t')
                .Append(assignment.PickupSegmentId).Append('\t')
                .Append(assignment.Status).Append('\t')
                .Append(assignment.SourceStartSec.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('\t')
                .Append(assignment.SourceEndSec.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('\t')
                .Append(target.TargetKey).Append('\t')
                .Append(target.OriginalStartSec.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('\t')
                .Append(target.OriginalEndSec.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('\n');
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static IReadOnlyList<PickupPickMapAssignment> GetCanonicalAssignments(
        string chapterStem,
        PickupPickMapDocument pickMap)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(pickMap);

        return pickMap.GetDeterministicAssignmentOrder()
            .Where(assignment => assignment.RequiresUniqueTarget)
            .Where(assignment => assignment.EffectiveTarget is not null)
            .Where(assignment => string.Equals(
                assignment.EffectiveTarget!.ChapterStem,
                chapterStem.Trim(),
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static PickupFitPlanItem[] OrderItems(IEnumerable<PickupFitPlanItem> items)
        => items
            .OrderBy(item => item.Target.ChapterStem, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Target.ErrorNumber)
            .ThenBy(item => item.Target.SentenceId)
            .ThenBy(item => item.Placement.StartSec)
            .ThenBy(item => item.Placement.EndSec)
            .ThenBy(item => item.FitItemId, StringComparer.Ordinal)
            .ToArray();

    private static void ValidateItems(
        string chapterStem,
        int pickMapRevision,
        string pickAssignmentsFingerprint,
        IReadOnlyList<PickupFitPlanItem> items,
        bool isDraft)
    {
        if (items.Count == 0)
        {
            if (!isDraft)
            {
                throw new InvalidOperationException(
                    $"Pickup fit-plan for chapter '{chapterStem}' has no items. Empty fit plans are allowed only when IsDraft is true.");
            }

            return;
        }

        var fitItemIds = new HashSet<string>(StringComparer.Ordinal);
        var replacementIds = new HashSet<string>(StringComparer.Ordinal);
        var pickAssignmentIds = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i] ?? throw new InvalidOperationException(
                $"Pickup fit-plan for chapter '{chapterStem}' contains null item at index {i}.");

            if (!fitItemIds.Add(item.FitItemId))
            {
                throw new InvalidOperationException(
                    $"Pickup fit-plan for chapter '{chapterStem}' contains duplicate fit item id '{item.FitItemId}'.");
            }

            if (!replacementIds.Add(item.ReplacementId))
            {
                throw new InvalidOperationException(
                    $"Pickup fit-plan for chapter '{chapterStem}' contains duplicate replacement id '{item.ReplacementId}'.");
            }

            if (!pickAssignmentIds.Add(item.PickAssignmentId))
            {
                throw new InvalidOperationException(
                    $"Pickup fit-plan for chapter '{chapterStem}' contains duplicate pick assignment id '{item.PickAssignmentId}'.");
            }

            if (!string.Equals(item.Target.ChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Pickup fit-plan item '{item.FitItemId}' chapter mismatch: document='{chapterStem}', target='{item.Target.ChapterStem}'.");
            }

            if (item.Acceptance.IsAccepted)
            {
                if (item.PreviewEvidence is null)
                {
                    throw new InvalidOperationException(
                        $"Pickup fit-plan item '{item.FitItemId}' is accepted but has no preview evidence.");
                }

                if (item.Acceptance.AcceptedPreviewVersion != item.PreviewEvidence.PreviewVersion)
                {
                    throw new InvalidOperationException(
                        $"Pickup fit-plan item '{item.FitItemId}' accepted preview version " +
                        $"'{item.Acceptance.AcceptedPreviewVersion}' does not match preview evidence '{item.PreviewEvidence.PreviewVersion}'.");
                }
            }

            if (item.Commit.Status is PickupFitCommitStatus.Ready or PickupFitCommitStatus.Committed)
            {
                if (item.PreviewEvidence is null)
                {
                    throw new InvalidOperationException(
                        $"Pickup fit-plan item '{item.FitItemId}' is commit-ready but has no preview evidence.");
                }

                if (!item.PreviewEvidence.MatchesPickTruth(pickMapRevision, pickAssignmentsFingerprint))
                {
                    throw new InvalidOperationException(
                        $"Pickup fit-plan item '{item.FitItemId}' has stale preview evidence: " +
                        $"previewRevision='{item.PreviewEvidence.PickMapRevision}', documentRevision='{pickMapRevision}', " +
                        $"previewFingerprint='{item.PreviewEvidence.PickAssignmentsFingerprint}', documentFingerprint='{pickAssignmentsFingerprint}'.");
                }
            }
        }
    }
}
