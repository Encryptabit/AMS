using System.Globalization;
using Ams.Workstation.Server.Models;

namespace Ams.Tests.Services;

public class PickupEdlCommutativityTests
{
    private const string ChapterStem = "chapter-03";
    private const string SourceFingerprint = "fp-source-003";
    private static readonly DateTime FixedUtc = new(2026, 01, 03, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ApplyOrder_ReversedInput_YieldsSameDeterministicPlan()
    {
        var op1 = MakeOperation("op-001", 5, 6, sourceStartSec: 0, sourceEndSec: 1.2);
        var op2 = MakeOperation("op-002", 12, 14, sourceStartSec: 2, sourceEndSec: 2.6);
        var op3 = MakeOperation("op-003", 20, 21, sourceStartSec: 3, sourceEndSec: 5);

        var left = CreateDocument([op1, op2, op3]);
        var right = CreateDocument([op3, op2, op1]);

        var leftPlan = left.GetDeterministicApplyOrder().Select(op => op.Id).ToArray();
        var rightPlan = right.GetDeterministicApplyOrder().Select(op => op.Id).ToArray();

        Assert.True(
            leftPlan.SequenceEqual(rightPlan),
            "Deterministic apply plan mismatch under reversed insertion order. " +
            $"chapter={left.ChapterStem}; left=[{string.Join(",", leftPlan)}]; right=[{string.Join(",", rightPlan)}]");
    }

    [Fact]
    public void ApplyOrderCommutativity_ReorderedInput_ProducesEquivalentRenderSignature()
    {
        var ops = new[]
        {
            MakeOperation("op-001", 2, 3, sourceStartSec: 0, sourceEndSec: 1.0),
            MakeOperation("op-002", 7, 9, sourceStartSec: 1.0, sourceEndSec: 1.8),
            MakeOperation("op-003", 12, 13, sourceStartSec: 2.0, sourceEndSec: 3.4),
            MakeOperation("op-004", 18, 19.5, sourceStartSec: 4.0, sourceEndSec: 4.4),
            MakeOperation("op-005", 24, 25, sourceStartSec: 5.0, sourceEndSec: 6.2)
        };

        var docA = CreateDocument(ops);
        var docB = CreateDocument([ops[3], ops[0], ops[4], ops[1], ops[2]]);
        var sampleTimes = new[] { 0.0, 2.5, 8.5, 12.5, 20.0, 30.0 };

        var signatureA = BuildRenderSignature(docA, baselineDurationSec: 32.0, sampleTimes);
        var signatureB = BuildRenderSignature(docB, baselineDurationSec: 32.0, sampleTimes);

        Assert.Equal(signatureA, signatureB);
    }

    [Fact]
    public void ApplyOrderCommutativity_MultiplePermutations_CollapseToSingleSignature()
    {
        var baseOps = new[]
        {
            MakeOperation("op-a", 1, 2, sourceStartSec: 0, sourceEndSec: 1),
            MakeOperation("op-b", 4, 5, sourceStartSec: 1, sourceEndSec: 2),
            MakeOperation("op-c", 8, 9, sourceStartSec: 2, sourceEndSec: 2.5),
            MakeOperation("op-d", 12, 13, sourceStartSec: 3, sourceEndSec: 4),
            MakeOperation("op-e", 16, 17, sourceStartSec: 4, sourceEndSec: 4.75),
            MakeOperation("op-f", 20, 21, sourceStartSec: 5, sourceEndSec: 5.5)
        };

        var rng = new Random(17);
        var signatures = new HashSet<string>(StringComparer.Ordinal);
        var orderDiagnostics = new List<string>();
        var sampleTimes = new[] { 0.0, 1.5, 4.5, 8.5, 12.5, 16.5, 21.0, 28.0 };

        for (var i = 0; i < 20; i++)
        {
            var shuffled = baseOps
                .OrderBy(_ => rng.Next())
                .ToArray();

            var doc = CreateDocument(shuffled);
            signatures.Add(BuildRenderSignature(doc, baselineDurationSec: 30.0, sampleTimes));
            orderDiagnostics.Add(string.Join(",", shuffled.Select(op => op.Id)));
        }

        Assert.True(
            signatures.Count == 1,
            "Commutativity regression: different insertion orders produced multiple render signatures. " +
            $"chapter={ChapterStem}; signatures={signatures.Count}; sampleOrders=[{string.Join(" | ", orderDiagnostics.Take(5))}]");
    }

    private static string BuildRenderSignature(
        PickupEdlDocument doc,
        double baselineDurationSec,
        IReadOnlyList<double> sampleTimes)
    {
        var applyOrder = string.Join(">", doc.GetDeterministicApplyOrder().Select(op => op.Id));
        var projection = doc.BuildProjectionEdits();
        var map = string.Join(",", sampleTimes.Select(time =>
            TimelineProjection.BaselineToCurrentTime(time, projection).ToString("F6", CultureInfo.InvariantCulture)));
        var duration = TimelineProjection
            .ProjectedDuration(baselineDurationSec, projection)
            .ToString("F6", CultureInfo.InvariantCulture);

        return $"chapter={doc.ChapterStem}|order={applyOrder}|duration={duration}|map={map}";
    }

    private static PickupEdlDocument CreateDocument(IReadOnlyList<PickupEdlOperation> operations)
    {
        var source = new PickupEdlSourceReference(
            path: "/tmp/pickups-03.wav",
            fingerprint: SourceFingerprint,
            fileSizeBytes: 4096,
            modifiedAtUtc: FixedUtc);

        return new PickupEdlDocument(
            schemaVersion: PickupEdlDocument.CurrentSchemaVersion,
            chapterStem: ChapterStem,
            revision: 3,
            source: source,
            operations: operations);
    }

    private static PickupEdlOperation MakeOperation(
        string id,
        double baselineStartSec,
        double baselineEndSec,
        double sourceStartSec,
        double sourceEndSec)
    {
        return new PickupEdlOperation(
            id: id,
            chapterStem: ChapterStem,
            kind: PickupEdlOperationType.PickupReplace,
            state: PickupEdlOperationState.Applied,
            baselineStartSec: baselineStartSec,
            baselineEndSec: baselineEndSec,
            sourceStartSec: sourceStartSec,
            sourceEndSec: sourceEndSec,
            sourceFingerprint: SourceFingerprint,
            sentenceId: 91,
            errorNumber: 12,
            pickupAssetId: "asset-003",
            crossfadeDurationSec: 0.05,
            crossfadeCurve: "hsin",
            updatedAtUtc: FixedUtc);
    }
}
