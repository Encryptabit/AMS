using System.Globalization;
using Ams.Workstation.Server.Models;

namespace Ams.Tests.Services;

public class PickupEdlProjectionTests
{
    private const string ChapterStem = "chapter-02";
    private const string SourceFingerprint = "fp-source-002";
    private static readonly DateTime FixedUtc = new(2026, 01, 02, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Projection_ZeroOps_IsIdentity()
    {
        var doc = CreateDocument([]);

        var projection = doc.BuildProjectionEdits();

        Assert.Empty(projection);
        Assert.Equal(15.25, TimelineProjection.BaselineToCurrentTime(15.25, projection));
        Assert.Equal(80.0, TimelineProjection.ProjectedDuration(80.0, projection));
    }

    [Fact]
    public void Projection_OneOp_MapsInsideBoundary_AndAppliesDelta()
    {
        var doc = CreateDocument([
            MakeOperation("op-001", baselineStartSec: 10, baselineEndSec: 12, sourceStartSec: 4, sourceEndSec: 7)
        ]);

        var projection = doc.BuildProjectionEdits();

        Assert.Single(projection);
        Assert.Equal(9.0, TimelineProjection.BaselineToCurrentTime(9.0, projection));
        Assert.Equal(10.0, TimelineProjection.BaselineToCurrentTime(11.0, projection));
        Assert.Equal(31.0, TimelineProjection.ProjectedDuration(30.0, projection));
    }

    [Fact]
    public void Projection_CompositeExplicitReplacementDurations_OverrideSourceSliceDuration()
    {
        var exact = MakeOperation(
            "op-exact",
            baselineStartSec: 5,
            baselineEndSec: 6,
            sourceStartSec: 0,
            sourceEndSec: 1,
            state: PickupEdlOperationState.Applied,
            explicitReplacementDurationSec: 1.0);
        var longer = MakeOperation(
            "op-longer",
            baselineStartSec: 10,
            baselineEndSec: 12,
            sourceStartSec: 2,
            sourceEndSec: 3,
            state: PickupEdlOperationState.Applied,
            explicitReplacementDurationSec: 3.5);
        var shorter = MakeOperation(
            "op-shorter",
            baselineStartSec: 20,
            baselineEndSec: 23,
            sourceStartSec: 4,
            sourceEndSec: 8,
            state: PickupEdlOperationState.Applied,
            explicitReplacementDurationSec: 1.25);
        var doc = CreateDocument([shorter, longer, exact]);

        var projection = doc.BuildProjectionEdits();

        Assert.Equal(new[] { "op-exact", "op-longer", "op-shorter" }, projection.Select(edit => edit.Id).ToArray());
        Assert.Equal(new[] { 1.0, 3.5, 1.25 }, projection.Select(edit => edit.ReplacementDurationSec).ToArray());
        Assert.Equal(9.0, TimelineProjection.BaselineToCurrentTime(9.0, projection));
        Assert.Equal(21.5, TimelineProjection.BaselineToCurrentTime(21.0, projection));
        Assert.Equal(39.75, TimelineProjection.ProjectedDuration(40.0, projection), precision: 6);
    }

    [Fact]
    public void ProjectionRoundTrip_ReorderedInput_ProducesSameMapping()
    {
        var opA = MakeOperation("op-a", 5, 6, sourceStartSec: 0, sourceEndSec: 1.5);
        var opB = MakeOperation("op-b", 10, 12, sourceStartSec: 5, sourceEndSec: 6);
        var opC = MakeOperation("op-c", 20, 21, sourceStartSec: 1, sourceEndSec: 3);

        var canonical = CreateDocument([opA, opB, opC]);
        var reordered = CreateDocument([opC, opA, opB]);

        var sampleTimes = new[] { 0.0, 5.5, 8.0, 11.0, 15.0, 22.0, 30.0 };
        var canonicalMap = BuildMap(canonical, sampleTimes);
        var reorderedMap = BuildMap(reordered, sampleTimes);

        Assert.True(
            canonicalMap.SequenceEqual(reorderedMap),
            "Projection mismatch for same op set under different insertion order. " +
            $"chapter={canonical.ChapterStem}; canonicalOrder={FormatOrder(canonical)}; reorderedOrder={FormatOrder(reordered)}; " +
            $"canonicalMap={FormatMap(canonicalMap)}; reorderedMap={FormatMap(reorderedMap)}");
    }

    [Fact]
    public void DeterministicProjectionOrder_LargeOpList_RemainsStableAcrossPermutations()
    {
        const int opCount = 512;
        var ops = new List<PickupEdlOperation>(opCount);

        for (var i = 0; i < opCount; i++)
        {
            var start = i * 3.0;
            ops.Add(MakeOperation(
                id: $"op-{i:D4}",
                baselineStartSec: start,
                baselineEndSec: start + 1.0,
                sourceStartSec: 10 + i,
                sourceEndSec: 10 + i + 0.5,
                state: PickupEdlOperationState.Applied));
        }

        var docA = CreateDocument(ops);
        var docB = CreateDocument(ops.AsEnumerable().Reverse().ToArray());

        var orderA = docA.BuildProjectionEdits().Select(edit => edit.Id).ToArray();
        var orderB = docB.BuildProjectionEdits().Select(edit => edit.Id).ToArray();

        Assert.True(
            orderA.SequenceEqual(orderB),
            "Projection order unstable under large-op permutation. " +
            $"chapter={docA.ChapterStem}; orderA_head=[{string.Join(",", orderA.Take(8))}]; " +
            $"orderB_head=[{string.Join(",", orderB.Take(8))}]");

        var projectedDurationA = TimelineProjection.ProjectedDuration(2000.0, docA.BuildProjectionEdits());
        var projectedDurationB = TimelineProjection.ProjectedDuration(2000.0, docB.BuildProjectionEdits());

        Assert.Equal(projectedDurationA, projectedDurationB, precision: 6);
    }

    private static double[] BuildMap(PickupEdlDocument doc, IReadOnlyList<double> sampleTimes)
    {
        var projection = doc.BuildProjectionEdits();
        var result = new double[sampleTimes.Count];

        for (var i = 0; i < sampleTimes.Count; i++)
        {
            result[i] = TimelineProjection.BaselineToCurrentTime(sampleTimes[i], projection);
        }

        return result;
    }

    private static string FormatOrder(PickupEdlDocument doc)
        => string.Join(",", doc.BuildProjectionEdits().Select(op => op.Id));

    private static string FormatMap(IEnumerable<double> map)
        => string.Join(",", map.Select(value => value.ToString("F6", CultureInfo.InvariantCulture)));

    private static PickupEdlDocument CreateDocument(IReadOnlyList<PickupEdlOperation> operations)
    {
        var source = new PickupEdlSourceReference(
            path: "/tmp/pickups-02.wav",
            fingerprint: SourceFingerprint,
            fileSizeBytes: 2048,
            modifiedAtUtc: FixedUtc);

        return new PickupEdlDocument(
            schemaVersion: PickupEdlDocument.CurrentSchemaVersion,
            chapterStem: ChapterStem,
            revision: 2,
            source: source,
            operations: operations);
    }

    private static PickupEdlOperation MakeOperation(
        string id,
        double baselineStartSec,
        double baselineEndSec,
        double sourceStartSec,
        double sourceEndSec,
        PickupEdlOperationState state = PickupEdlOperationState.Staged,
        double? explicitReplacementDurationSec = null,
        PickupEdlFitMetadata? fitMetadata = null)
    {
        return new PickupEdlOperation(
            id: id,
            chapterStem: ChapterStem,
            kind: PickupEdlOperationType.PickupReplace,
            state: state,
            baselineStartSec: baselineStartSec,
            baselineEndSec: baselineEndSec,
            sourceStartSec: sourceStartSec,
            sourceEndSec: sourceEndSec,
            sourceFingerprint: SourceFingerprint,
            sentenceId: 77,
            errorNumber: 11,
            pickupAssetId: "asset-002",
            crossfadeDurationSec: 0.05,
            crossfadeCurve: "hsin",
            updatedAtUtc: FixedUtc,
            explicitReplacementDurationSec: explicitReplacementDurationSec,
            fitMetadata: fitMetadata);
    }
}
