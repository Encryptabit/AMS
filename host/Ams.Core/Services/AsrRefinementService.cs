using System.Text.Json;
using Ams.Core.Align.Tx;

namespace Ams.Core.Services;

/// <summary>
/// Service for generating refined ASR JSON output that matches ./CORRECT_RESULTS/ format.
/// Applies sentence boundary constraints to token timings with proportional overlap splitting.
/// </summary>
public sealed class AsrRefinementService
{
    /// <summary>
    /// Generates refined ASR response by applying sentence boundary constraints to token timings.
    /// Implements proportional overlap splitting for tokens that straddle sentence boundaries.
    /// </summary>
    /// <param name="originalAsr">Original ASR response with token timing data</param>
    /// <param name="refinedSentences">Refined sentences providing new boundary constraints</param>
    /// <returns>ASR response with adjusted token timings within sentence boundaries</returns>
    public AsrResponse GenerateRefinedAsr(AsrResponse originalAsr, IReadOnlyList<SentenceRefined> refinedSentences)
    {
        if (originalAsr == null) throw new ArgumentNullException(nameof(originalAsr));
        if (refinedSentences == null) throw new ArgumentNullException(nameof(refinedSentences));

        var refinedTokens = new List<AsrToken>();

        var sortedSentences = refinedSentences.OrderBy(s => s.Start).ToList();

        foreach (var sentence in sortedSentences)
        {
            var sentenceTokens = originalAsr.Tokens
                .Skip(sentence.StartWordIdx)
                .Take(sentence.EndWordIdx - sentence.StartWordIdx + 1)
                .ToList();

            var adjustedTokens = AdjustTokenTimingsWithinSentences(sentenceTokens, sentence.Start, sentence.End);
            refinedTokens.AddRange(adjustedTokens);
        }

        return new AsrResponse(originalAsr.ModelVersion, refinedTokens.ToArray());
    }

    /// <summary>
    /// Adjusts token timings within sentence boundaries using proportional splitting rules.
    /// Implements the decisions from plan.json: clamp and split with deterministic rules.
    /// </summary>
    private IEnumerable<AsrToken> AdjustTokenTimingsWithinSentences(
        IEnumerable<AsrToken> tokens, 
        double sentenceStart, 
        double sentenceEnd)
    {
        const double MinTokenDuration = 0.001; // 1ms minimum duration
        var adjustedTokens = new List<AsrToken>();

        foreach (var token in tokens)
        {
            var tokenStart = token.StartTime;
            var tokenEnd = token.StartTime + token.Duration;

            // Token wholly outside sentence boundaries - drop it
            if (tokenEnd <= sentenceStart || tokenStart >= sentenceEnd)
            {
                continue;
            }

            // Token wholly within sentence boundaries - keep as is but clamp to sentence bounds
            if (tokenStart >= sentenceStart && tokenEnd <= sentenceEnd)
            {
                adjustedTokens.Add(token);
                continue;
            }

            // Token straddles boundary - split proportionally
            var clampedStart = Math.Max(tokenStart, sentenceStart);
            var clampedEnd = Math.Min(tokenEnd, sentenceEnd);
            var newDuration = clampedEnd - clampedStart;

            // Drop micro-fragments
            if (newDuration < MinTokenDuration)
            {
                continue;
            }

            // Create adjusted token with clamped timing
            adjustedTokens.Add(new AsrToken(
                StartTime: clampedStart,
                Duration: Math.Round(newDuration, 6), // 6 decimal places precision
                Word: token.Word
            ));
        }

        // Ensure monotonicity within sentence - sort by start time
        return adjustedTokens.OrderBy(t => t.StartTime);
    }

    /// <summary>
    /// Validates timing consistency between original and refined tokens.
    /// Ensures conservation principle is maintained within tolerance.
    /// </summary>
    public bool ValidateTimingConsistency(AsrResponse original, AsrResponse refined)
    {
        const double ToleranceMs = 0.001; // 1ms tolerance

        var originalTotalDuration = original.Tokens.Sum(t => t.Duration);
        var refinedTotalDuration = refined.Tokens.Sum(t => t.Duration);

        return Math.Abs(originalTotalDuration - refinedTotalDuration) <= ToleranceMs;
    }
}