using Ams.Core.Audio;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services.Pickups.Edl;

/// <summary>
/// Pure EDL runtime rules: operation shaping, deterministic ordering, and state transitions.
/// </summary>
public sealed class PickupEdlEngine
{
    public PickupEdlOperation BuildOperation(
        StagedReplacement replacement,
        PickupEdlSourceReference source,
        PickupEdlOperationState state,
        IReadOnlySet<int>? knownSentenceIds = null,
        DateTime? updatedAtUtc = null,
        double? explicitReplacementDurationSec = null,
        PickupEdlFitMetadata? fitMetadata = null)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        ArgumentNullException.ThrowIfNull(source);

        if (replacement.SentenceId <= 0)
        {
            throw new InvalidOperationException(
                $"Pickup op '{replacement.Id}' in chapter '{replacement.ChapterStem}' has invalid sentence id '{replacement.SentenceId}'.");
        }

        if (knownSentenceIds is not null && !knownSentenceIds.Contains(replacement.SentenceId))
        {
            throw new InvalidOperationException(
                $"Pickup op '{replacement.Id}' in chapter '{replacement.ChapterStem}' references unknown sentence '{replacement.SentenceId}'.");
        }

        return new PickupEdlOperation(
            id: replacement.Id,
            chapterStem: replacement.ChapterStem,
            kind: PickupEdlOperationType.PickupReplace,
            state: state,
            baselineStartSec: replacement.OriginalStartSec,
            baselineEndSec: replacement.OriginalEndSec,
            sourceStartSec: replacement.PickupStartSec,
            sourceEndSec: replacement.PickupEndSec,
            sourceFingerprint: source.Fingerprint,
            sentenceId: replacement.SentenceId,
            errorNumber: null,
            pickupAssetId: null,
            crossfadeDurationSec: replacement.CrossfadeDurationSec,
            crossfadeCurve: replacement.CrossfadeCurve,
            updatedAtUtc: updatedAtUtc ?? DateTime.UtcNow,
            explicitReplacementDurationSec: explicitReplacementDurationSec,
            fitMetadata: fitMetadata);
    }

    public PickupEdlDocument UpsertOperation(PickupEdlDocument document, PickupEdlOperation operation)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(operation);

        EnsureOperationAffinity(document, operation);

        var merged = document.Operations
            .Where(existing => !string.Equals(existing.Id, operation.Id, StringComparison.Ordinal))
            .Append(operation)
            .OrderBy(op => op.BaselineStartSec)
            .ThenBy(op => op.BaselineEndSec)
            .ThenBy(op => op.Id, StringComparer.Ordinal)
            .ToArray();

        return new PickupEdlDocument(
            schemaVersion: document.SchemaVersion,
            chapterStem: document.ChapterStem,
            revision: document.Revision,
            source: document.Source,
            operations: merged);
    }

    public PickupEdlDocument TransitionOperationState(
        PickupEdlDocument document,
        string operationId,
        PickupEdlOperationState nextState,
        DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);

        if (!Enum.IsDefined(typeof(PickupEdlOperationState), nextState))
        {
            throw new InvalidOperationException(
                $"Pickup EDL chapter '{document.ChapterStem}' op '{operationId}' requested unknown transition target '{(int)nextState}'.");
        }

        var existing = TryGetOperation(document, operationId)
            ?? throw new InvalidOperationException(
                $"Pickup EDL chapter '{document.ChapterStem}' does not contain op '{operationId}'.");

        if (!IsLegalTransition(existing.State, nextState))
        {
            throw new InvalidOperationException(
                $"Pickup EDL illegal transition in chapter '{document.ChapterStem}' for op '{operationId}': " +
                $"'{existing.State}' -> '{nextState}'.");
        }

        var transitioned = new PickupEdlOperation(
            id: existing.Id,
            chapterStem: existing.ChapterStem,
            kind: existing.Kind,
            state: nextState,
            baselineStartSec: existing.BaselineStartSec,
            baselineEndSec: existing.BaselineEndSec,
            sourceStartSec: existing.SourceStartSec,
            sourceEndSec: existing.SourceEndSec,
            sourceFingerprint: existing.SourceFingerprint,
            sentenceId: existing.SentenceId,
            errorNumber: existing.ErrorNumber,
            pickupAssetId: existing.PickupAssetId,
            crossfadeDurationSec: existing.CrossfadeDurationSec,
            crossfadeCurve: existing.CrossfadeCurve,
            updatedAtUtc: updatedAtUtc ?? DateTime.UtcNow,
            explicitReplacementDurationSec: existing.ExplicitReplacementDurationSec,
            fitMetadata: existing.FitMetadata);

        return UpsertOperation(document, transitioned);
    }

    private static bool IsLegalTransition(PickupEdlOperationState current, PickupEdlOperationState next)
    {
        if (current == next)
        {
            return false;
        }

        return current switch
        {
            PickupEdlOperationState.Staged =>
                next is PickupEdlOperationState.Applied or PickupEdlOperationState.Failed,

            PickupEdlOperationState.Applied =>
                next is PickupEdlOperationState.Reverted or PickupEdlOperationState.Staged or PickupEdlOperationState.Failed,

            PickupEdlOperationState.Reverted =>
                next is PickupEdlOperationState.Failed,

            PickupEdlOperationState.Failed =>
                next is PickupEdlOperationState.Staged,

            _ => false
        };
    }

    public PickupEdlOperation? TryGetOperation(PickupEdlDocument document, string operationId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);

        return document.Operations.FirstOrDefault(op => string.Equals(op.Id, operationId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Deterministic rebuild order for runtime splice application (applied only, back-to-front).
    /// </summary>
    public IReadOnlyList<PickupEdlOperation> BuildDeterministicRebuildOrder(PickupEdlDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.Operations
            .Where(op => op.State == PickupEdlOperationState.Applied)
            .OrderByDescending(op => op.BaselineStartSec)
            .ThenByDescending(op => op.BaselineEndSec)
            .ThenBy(op => op.Id, StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Projection edits used by timeline mapping/rebuild surfaces (applied only).
    /// </summary>
    public IReadOnlyList<ChapterEdit> BuildAppliedProjectionEdits(PickupEdlDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.Operations
            .Where(op => op.State == PickupEdlOperationState.Applied)
            .OrderBy(op => op.BaselineStartSec)
            .ThenBy(op => op.BaselineEndSec)
            .ThenBy(op => op.Id, StringComparer.Ordinal)
            .Select(op => op.ToChapterEdit())
            .ToArray();
    }

    public string BuildDeterministicOrderingDiagnostics(PickupEdlDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var rebuild = string.Join(">", BuildDeterministicRebuildOrder(document).Select(op => op.Id));
        var projectionEdits = BuildAppliedProjectionEdits(document);
        var projection = string.Join(">", projectionEdits.Select(edit => edit.Id));
        var replacementDurations = string.Join(">", projectionEdits.Select(edit => $"{edit.Id}:{edit.ReplacementDurationSec:F6}s"));

        return $"chapter={document.ChapterStem}; revision={document.Revision}; rebuild=[{rebuild}]; projection=[{projection}]; replacementDurations=[{replacementDurations}]";
    }

    private static void EnsureOperationAffinity(PickupEdlDocument document, PickupEdlOperation operation)
    {
        if (!string.Equals(document.ChapterStem, operation.ChapterStem, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Pickup EDL chapter mismatch for op '{operation.Id}': " +
                $"document='{document.ChapterStem}', op='{operation.ChapterStem}'.");
        }

        if (!string.Equals(document.Source.Fingerprint, operation.SourceFingerprint, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Pickup EDL source fingerprint mismatch for op '{operation.Id}' in chapter '{document.ChapterStem}': " +
                $"document='{document.Source.Fingerprint}', op='{operation.SourceFingerprint}'.");
        }
    }
}
