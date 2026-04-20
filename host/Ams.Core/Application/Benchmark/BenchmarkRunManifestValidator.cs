using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Ams.Core.Application.Benchmark;

public sealed record BenchmarkRunManifestValidationDiagnostic
{
    public BenchmarkRunManifestValidationDiagnostic(
        string field,
        string expected,
        string actual,
        string rationale)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(expected);
        ArgumentException.ThrowIfNullOrWhiteSpace(actual);
        ArgumentException.ThrowIfNullOrWhiteSpace(rationale);

        Field = field.Trim();
        Expected = expected.Trim();
        Actual = actual.Trim();
        Rationale = rationale.Trim();
    }

    public string Field { get; }

    public string Expected { get; }

    public string Actual { get; }

    public string Rationale { get; }
}

public sealed record BenchmarkRunManifestValidationResult
{
    public BenchmarkRunManifestValidationResult(
        IReadOnlyList<BenchmarkRunManifestValidationDiagnostic>? diagnostics = null)
    {
        Diagnostics = diagnostics?
            .Where(diagnostic => diagnostic is not null)
            .Distinct()
            .ToArray()
            ?? [];
    }

    public IReadOnlyList<BenchmarkRunManifestValidationDiagnostic> Diagnostics { get; }

    public bool IsValid => Diagnostics.Count == 0;

    public string ToSummary()
    {
        if (IsValid)
        {
            return "ok";
        }

        return string.Join(
            "; ",
            Diagnostics.Select(diagnostic =>
                $"field={diagnostic.Field}, expected={diagnostic.Expected}, actual={diagnostic.Actual}, rationale={diagnostic.Rationale}"));
    }
}

public sealed class BenchmarkRunManifestValidator
{
    private const double FloatingTolerance = 0.0001d;

    public BenchmarkRunManifestValidationResult Validate(BenchmarkRunManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        return ValidateCore(
            chapterSet: manifest.ChapterSet,
            chapterSetFingerprint: manifest.ChapterSetFingerprint,
            chapterOutcomes: manifest.ChapterOutcomes,
            aggregateMetrics: manifest.AggregateMetrics,
            verifyChapterSetFingerprint: true);
    }

    public BenchmarkRunManifestValidationResult Validate(BenchmarkRunResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return ValidateCore(
            chapterSet: result.ChapterSet,
            chapterSetFingerprint: result.ChapterSetFingerprint,
            chapterOutcomes: result.ChapterOutcomes,
            aggregateMetrics: result.AggregateMetrics,
            verifyChapterSetFingerprint: false);
    }

    private static BenchmarkRunManifestValidationResult ValidateCore(
        IReadOnlyList<string> chapterSet,
        string chapterSetFingerprint,
        IReadOnlyList<BenchmarkRunChapterOutcome> chapterOutcomes,
        BenchmarkRunMetricsAggregate aggregateMetrics,
        bool verifyChapterSetFingerprint)
    {
        ArgumentNullException.ThrowIfNull(chapterSet);
        ArgumentNullException.ThrowIfNull(chapterOutcomes);
        ArgumentNullException.ThrowIfNull(aggregateMetrics);

        var diagnostics = new List<BenchmarkRunManifestValidationDiagnostic>();

        if (chapterSet.Count == 0)
        {
            diagnostics.Add(new BenchmarkRunManifestValidationDiagnostic(
                field: "chapterSet",
                expected: "at least one chapter id",
                actual: "0 entries",
                rationale: "Compare compatibility checks require non-empty chapter sets."));
        }

        if (string.IsNullOrWhiteSpace(chapterSetFingerprint))
        {
            diagnostics.Add(new BenchmarkRunManifestValidationDiagnostic(
                field: "chapterSetFingerprint",
                expected: "non-empty sha256 fingerprint",
                actual: "(blank)",
                rationale: "Fingerprint must be present before compatibility scoring."));
        }
        else if (verifyChapterSetFingerprint && chapterSet.Count > 0)
        {
            var expectedFingerprint = ComputeChapterSetFingerprint(chapterSet);
            if (!string.Equals(expectedFingerprint, chapterSetFingerprint.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(new BenchmarkRunManifestValidationDiagnostic(
                    field: "chapterSetFingerprint",
                    expected: expectedFingerprint,
                    actual: chapterSetFingerprint.Trim(),
                    rationale: "Fingerprint must match canonical chapter-set hash."));
            }
        }

        var expectedAggregate = BenchmarkRunMetricsAggregate.FromChapterOutcomes(chapterOutcomes);

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.chapterStates.pending",
            expected: expectedAggregate.ChapterStates.Pending,
            actual: aggregateMetrics.ChapterStates.Pending,
            rationale: "Pending chapter count must equal chapter outcomes rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.chapterStates.running",
            expected: expectedAggregate.ChapterStates.Running,
            actual: aggregateMetrics.ChapterStates.Running,
            rationale: "Running chapter count must equal chapter outcomes rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.chapterStates.failed",
            expected: expectedAggregate.ChapterStates.Failed,
            actual: aggregateMetrics.ChapterStates.Failed,
            rationale: "Failed chapter count must equal chapter outcomes rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.chapterStates.completed",
            expected: expectedAggregate.ChapterStates.Completed,
            actual: aggregateMetrics.ChapterStates.Completed,
            rationale: "Completed chapter count must equal chapter outcomes rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.chapterStates.total",
            expected: expectedAggregate.ChapterStates.Total,
            actual: aggregateMetrics.ChapterStates.Total,
            rationale: "Chapter-state total must equal chapter outcomes rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.metricsStates.notRun",
            expected: expectedAggregate.MetricsStates.NotRun,
            actual: aggregateMetrics.MetricsStates.NotRun,
            rationale: "Not-run metrics count must equal chapter outcomes rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.metricsStates.completed",
            expected: expectedAggregate.MetricsStates.Completed,
            actual: aggregateMetrics.MetricsStates.Completed,
            rationale: "Completed metrics count must equal chapter outcomes rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.metricsStates.partial",
            expected: expectedAggregate.MetricsStates.Partial,
            actual: aggregateMetrics.MetricsStates.Partial,
            rationale: "Partial metrics count must equal chapter outcomes rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.metricsStates.failed",
            expected: expectedAggregate.MetricsStates.Failed,
            actual: aggregateMetrics.MetricsStates.Failed,
            rationale: "Failed metrics count must equal chapter outcomes rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.metricsStates.total",
            expected: expectedAggregate.MetricsStates.Total,
            actual: aggregateMetrics.MetricsStates.Total,
            rationale: "Metrics-state total must equal chapter outcomes rollup.");

        CompareLong(
            diagnostics,
            field: "aggregateMetrics.totalPipelineRuntimeMs",
            expected: expectedAggregate.TotalPipelineRuntimeMs,
            actual: aggregateMetrics.TotalPipelineRuntimeMs,
            rationale: "Pipeline runtime aggregate must equal chapter runtime rollup.");

        CompareLong(
            diagnostics,
            field: "aggregateMetrics.totalAnalysisRuntimeMs",
            expected: expectedAggregate.TotalAnalysisRuntimeMs,
            actual: aggregateMetrics.TotalAnalysisRuntimeMs,
            rationale: "Analysis runtime aggregate must equal chapter runtime rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.totalMismatchCount",
            expected: expectedAggregate.TotalMismatchCount,
            actual: aggregateMetrics.TotalMismatchCount,
            rationale: "Mismatch count aggregate must equal chapter quality rollup.");

        CompareDouble(
            diagnostics,
            field: "aggregateMetrics.totalMissingSpeechSec",
            expected: expectedAggregate.TotalMissingSpeechSec,
            actual: aggregateMetrics.TotalMissingSpeechSec,
            rationale: "Missing-speech aggregate must equal chapter quality rollup.");

        CompareDouble(
            diagnostics,
            field: "aggregateMetrics.totalExtraSpeechSec",
            expected: expectedAggregate.TotalExtraSpeechSec,
            actual: aggregateMetrics.TotalExtraSpeechSec,
            rationale: "Extra-speech aggregate must equal chapter quality rollup.");

        CompareInt(
            diagnostics,
            field: "aggregateMetrics.totalQcFlags",
            expected: expectedAggregate.TotalQcFlags,
            actual: aggregateMetrics.TotalQcFlags,
            rationale: "QC flag aggregate must equal chapter quality rollup.");

        return new BenchmarkRunManifestValidationResult(diagnostics);
    }

    private static void CompareInt(
        ICollection<BenchmarkRunManifestValidationDiagnostic> diagnostics,
        string field,
        int expected,
        int actual,
        string rationale)
    {
        if (expected == actual)
        {
            return;
        }

        diagnostics.Add(new BenchmarkRunManifestValidationDiagnostic(
            field,
            expected.ToString(CultureInfo.InvariantCulture),
            actual.ToString(CultureInfo.InvariantCulture),
            rationale));
    }

    private static void CompareLong(
        ICollection<BenchmarkRunManifestValidationDiagnostic> diagnostics,
        string field,
        long expected,
        long actual,
        string rationale)
    {
        if (expected == actual)
        {
            return;
        }

        diagnostics.Add(new BenchmarkRunManifestValidationDiagnostic(
            field,
            expected.ToString(CultureInfo.InvariantCulture),
            actual.ToString(CultureInfo.InvariantCulture),
            rationale));
    }

    private static void CompareDouble(
        ICollection<BenchmarkRunManifestValidationDiagnostic> diagnostics,
        string field,
        double expected,
        double actual,
        string rationale)
    {
        if (AreClose(expected, actual))
        {
            return;
        }

        diagnostics.Add(new BenchmarkRunManifestValidationDiagnostic(
            field,
            expected.ToString("0.###", CultureInfo.InvariantCulture),
            actual.ToString("0.###", CultureInfo.InvariantCulture),
            rationale));
    }

    private static bool AreClose(double expected, double actual)
        => Math.Abs(expected - actual) <= FloatingTolerance;

    private static string ComputeChapterSetFingerprint(IReadOnlyList<string> chapterSet)
    {
        var canonical = string.Join("\n", chapterSet.Select(chapter => (chapter ?? string.Empty).Trim()));
        var bytes = Encoding.UTF8.GetBytes(canonical);

        using var hash = SHA256.Create();
        var digest = hash.ComputeHash(bytes);
        return Convert.ToHexString(digest).ToLowerInvariant();
    }
}
