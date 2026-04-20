namespace Ams.Core.Audio;

/// <summary>
/// Pure static service that maps positions between a chapter's baseline (original)
/// timeline and its current (post-edit) timeline using an ordered list of
/// immutable <see cref="ChapterEdit"/> records.
///
/// Each edit represents a region in the baseline timeline that was replaced
/// by content of a (possibly different) duration. The projection walks the
/// edit list front-to-back, accumulating a cumulative shift, to translate
/// any baseline time into its current equivalent.
///
/// Follows the same static/stateless pattern as <see cref="AudioSpliceService"/>.
/// </summary>
public static class TimelineProjection
{
    /// <summary>
    /// Maps a position in the baseline (original) timeline to the corresponding
    /// position in the current (post-edit) timeline.
    /// </summary>
    /// <param name="baselineTimeSec">A time position in the original, unedited audio.</param>
    /// <param name="appliedEdits">
    /// The ordered list of edits applied to this chapter. Edits should reference
    /// baseline timeline positions (not shifted positions).
    /// </param>
    /// <returns>
    /// The equivalent time position in the current audio after all edits are applied.
    /// Never negative — clamped to zero.
    /// </returns>
    /// <remarks>
    /// Algorithm: Walk edits front-to-back. For each edit whose baseline region
    /// is fully upstream of <paramref name="baselineTimeSec"/>, accumulate the
    /// duration delta. If the query time falls inside an edit's baseline region,
    /// clamp to the edit boundary (start of replacement). Edits downstream of
    /// the query time are skipped.
    /// </remarks>
    public static double BaselineToCurrentTime(
        double baselineTimeSec,
        IReadOnlyList<ChapterEdit> appliedEdits)
    {
        if (appliedEdits is null || appliedEdits.Count == 0)
            return Math.Max(0, baselineTimeSec);

        double cumulativeShift = 0;

        for (int i = 0; i < appliedEdits.Count; i++)
        {
            var edit = appliedEdits[i];
            double editBaselineSpan = edit.BaselineEndSec - edit.BaselineStartSec;
            double delta = edit.ReplacementDurationSec - editBaselineSpan;

            if (baselineTimeSec >= edit.BaselineEndSec)
            {
                // Edit is fully upstream — accumulate the delta
                cumulativeShift += delta;
            }
            else if (baselineTimeSec > edit.BaselineStartSec)
            {
                // Query time falls inside this edit's baseline region.
                // Clamp to the edit start boundary in the current timeline.
                return Math.Max(0, edit.BaselineStartSec + cumulativeShift);
            }
            // else: edit is downstream — skip it
        }

        return Math.Max(0, baselineTimeSec + cumulativeShift);
    }

    /// <summary>
    /// Computes the projected total duration of the chapter after all edits are applied.
    /// </summary>
    /// <param name="baselineDurationSec">The original chapter duration in seconds.</param>
    /// <param name="appliedEdits">The ordered list of edits applied to this chapter.</param>
    /// <returns>The projected total duration after edits.</returns>
    public static double ProjectedDuration(
        double baselineDurationSec,
        IReadOnlyList<ChapterEdit> appliedEdits)
    {
        return BaselineToCurrentTime(baselineDurationSec, appliedEdits);
    }
}
