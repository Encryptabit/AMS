using System.Globalization;
using System.Text;
using Ams.Core.Processors.Alignment.Mfa;

namespace Ams.Core.Application.Mfa;

/// <summary>
/// Aggregates per-chunk MFA TextGrid outputs into a single canonical chapter-level
/// TextGrid file. Applies chunk time offsets to all interval boundaries and
/// produces a merged, time-ordered result compatible with <see cref="TextGridParser"/>
/// and the downstream <c>MergeTimingsCommand</c>.
/// </summary>
internal static class TextGridAggregationService
{
    /// <summary>
    /// Aggregates per-chunk TextGrid files into a single chapter-level TextGrid.
    /// </summary>
    /// <param name="utterances">Chunk corpus utterance entries with offset metadata.</param>
    /// <param name="chunkTextGridDirectory">Directory containing per-chunk TextGrid files.</param>
    /// <param name="outputPath">Canonical chapter-level TextGrid output path.</param>
    /// <returns>The number of word intervals aggregated, or 0 if no intervals were found.</returns>
    internal static int Aggregate(
        IReadOnlyList<MfaChunkCorpusBuilder.UtteranceEntry> utterances,
        string chunkTextGridDirectory,
        string outputPath)
    {
        ArgumentNullException.ThrowIfNull(utterances);
        ArgumentException.ThrowIfNullOrWhiteSpace(chunkTextGridDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var allWordIntervals = new List<TextGridInterval>();
        var allPhoneIntervals = new List<TextGridInterval>();
        int chunksWithOutput = 0;
        int chunksEmpty = 0;

        foreach (var utterance in utterances)
        {
            var textGridPath = Path.Combine(chunkTextGridDirectory, utterance.UtteranceName + ".TextGrid");
            if (!File.Exists(textGridPath))
            {
                chunksEmpty++;
                Log.Debug(
                    "No TextGrid found for chunk {ChunkId} ({Utterance}); skipping",
                    utterance.ChunkId, utterance.UtteranceName);
                continue;
            }

            var chunkOffset = utterance.ChunkStartSec;

            var wordIntervals = TextGridParser.ParseWordIntervals(textGridPath);
            if (wordIntervals.Count == 0)
            {
                chunksEmpty++;
                Log.Debug(
                    "TextGrid for chunk {ChunkId} ({Utterance}) contains no word intervals",
                    utterance.ChunkId, utterance.UtteranceName);
            }
            else
            {
                chunksWithOutput++;
                foreach (var interval in wordIntervals)
                {
                    allWordIntervals.Add(new TextGridInterval(
                        interval.Start + chunkOffset,
                        interval.End + chunkOffset,
                        interval.Text));
                }
            }

            var phoneIntervals = TextGridParser.ParsePhoneIntervals(textGridPath);
            foreach (var interval in phoneIntervals)
            {
                allPhoneIntervals.Add(new TextGridInterval(
                    interval.Start + chunkOffset,
                    interval.End + chunkOffset,
                    interval.Text));
            }
        }

        if (allWordIntervals.Count == 0)
        {
            Log.Warn(
                "TextGrid aggregation produced no word intervals from {Total} chunks ({Empty} empty)",
                utterances.Count, chunksEmpty);
            return 0;
        }

        // Sort by start time for monotonic ordering
        allWordIntervals.Sort((a, b) => a.Start.CompareTo(b.Start));
        allPhoneIntervals.Sort((a, b) => a.Start.CompareTo(b.Start));

        // Determine overall time range
        var globalXmin = Math.Min(
            allWordIntervals.Count > 0 ? allWordIntervals[0].Start : 0.0,
            allPhoneIntervals.Count > 0 ? allPhoneIntervals[0].Start : double.MaxValue);
        var globalXmax = Math.Max(
            allWordIntervals.Count > 0 ? allWordIntervals[^1].End : 0.0,
            allPhoneIntervals.Count > 0 ? allPhoneIntervals[^1].End : 0.0);

        // Ensure output directory exists
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        WriteTextGrid(outputPath, globalXmin, globalXmax, allWordIntervals, allPhoneIntervals);

        Log.Info(
            "TextGrid aggregation complete: {Words} word intervals, {Phones} phone intervals " +
            "from {ChunksWithOutput}/{Total} chunks (range {Xmin:F3}s-{Xmax:F3}s)",
            allWordIntervals.Count, allPhoneIntervals.Count,
            chunksWithOutput, utterances.Count,
            globalXmin, globalXmax);

        return allWordIntervals.Count;
    }

    /// <summary>
    /// Writes a Praat TextGrid file with words and (optionally) phones tiers.
    /// The output format is the standard Praat full text format compatible with
    /// <see cref="TextGridParser"/>.
    /// </summary>
    private static void WriteTextGrid(
        string outputPath,
        double xmin,
        double xmax,
        IReadOnlyList<TextGridInterval> wordIntervals,
        IReadOnlyList<TextGridInterval> phoneIntervals)
    {
        var sb = new StringBuilder();
        var ci = CultureInfo.InvariantCulture;

        var tierCount = phoneIntervals.Count > 0 ? 2 : 1;

        sb.AppendLine("File type = \"ooTextFile\"");
        sb.AppendLine("Object class = \"TextGrid\"");
        sb.AppendLine();
        sb.AppendLine(string.Create(ci, $"xmin = {xmin}"));
        sb.AppendLine(string.Create(ci, $"xmax = {xmax}"));
        sb.AppendLine("tiers? <exists>");
        sb.AppendLine(string.Create(ci, $"size = {tierCount}"));
        sb.AppendLine("item []:");

        // Tier 1: words
        WriteTier(sb, ci, 1, "words", xmin, xmax, wordIntervals);

        // Tier 2: phones (if present)
        if (phoneIntervals.Count > 0)
        {
            WriteTier(sb, ci, 2, "phones", xmin, xmax, phoneIntervals);
        }

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    private static void WriteTier(
        StringBuilder sb,
        CultureInfo ci,
        int tierIndex,
        string tierName,
        double xmin,
        double xmax,
        IReadOnlyList<TextGridInterval> intervals)
    {
        sb.AppendLine(string.Create(ci, $"    item [{tierIndex}]:"));
        sb.AppendLine("        class = \"IntervalTier\"");
        sb.AppendLine(string.Create(ci, $"        name = \"{tierName}\""));
        sb.AppendLine(string.Create(ci, $"        xmin = {xmin}"));
        sb.AppendLine(string.Create(ci, $"        xmax = {xmax}"));
        sb.AppendLine(string.Create(ci, $"        intervals: size = {intervals.Count}"));

        for (int i = 0; i < intervals.Count; i++)
        {
            var interval = intervals[i];
            sb.AppendLine(string.Create(ci, $"        intervals [{i + 1}]:"));
            sb.AppendLine(string.Create(ci, $"            xmin = {interval.Start}"));
            sb.AppendLine(string.Create(ci, $"            xmax = {interval.End}"));
            sb.AppendLine($"            text = \"{EscapeTextGridString(interval.Text)}\"");
        }
    }

    /// <summary>
    /// Escapes special characters for TextGrid string literals.
    /// </summary>
    private static string EscapeTextGridString(string text)
    {
        return text.Replace("\"", "\"\"");
    }
}
