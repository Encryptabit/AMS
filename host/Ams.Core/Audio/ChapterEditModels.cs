namespace Ams.Core.Audio;

/// <summary>
/// The type of edit operation applied to a chapter's audio timeline.
/// Each operation maps to a specific audio transformation that changes
/// the chapter duration relative to its baseline.
/// </summary>
public enum EditOperation
{
    /// <summary>Replace a region with a pickup recording (may change duration).</summary>
    PickupReplace,

    /// <summary>Insert roomtone at a point, pushing content apart (increases duration).</summary>
    RoomtoneInsert,

    /// <summary>Replace a region with looped roomtone (may change duration).</summary>
    RoomtoneReplace,

    /// <summary>Delete a region, pulling content together (decreases duration).</summary>
    RoomtoneDelete
}

/// <summary>
/// An immutable record of a single edit applied to a chapter's audio.
/// All time references are in the BASELINE (original, unedited) timeline.
/// The <see cref="TimelineProjection"/> service uses a list of these records
/// to map baseline positions to current (post-edit) positions.
/// </summary>
/// <remarks>
/// ChapterEdit records are append-only — once created they are never mutated.
/// Reverting an edit removes it from the list rather than modifying it.
/// </remarks>
public sealed record ChapterEdit(
    string Id,
    string ChapterStem,
    EditOperation Operation,
    double BaselineStartSec,
    double BaselineEndSec,
    double ReplacementDurationSec,
    int? SentenceId,
    int? ErrorNumber,
    string? PickupAssetId,
    double CrossfadeDurationSec,
    string CrossfadeCurve,
    DateTime AppliedAtUtc);
