using System.Text.Json;
using Ams.Workstation.Server.Models;

namespace Ams.Tests.Services;

public class PickupEdlContractTests
{
    private const string ChapterStem = "chapter-01";
    private const string SourceFingerprint = "fp-source-001";
    private static readonly DateTime FixedUtc = new(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Document_DeclaresStableSchemaVersionMarker()
    {
        Assert.Equal("pickup-edl/v1", PickupEdlDocument.CurrentSchemaVersion);
    }

    [Fact]
    public void Document_RejectsMissingChapterStem()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            CreateDocument([], chapterStem: " "));

        Assert.Contains("chapter", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Document_RejectsDuplicateOpIds_WithChapterDiagnostics()
    {
        var ops = new[]
        {
            MakeOperation("op-001", 10, 12, state: PickupEdlOperationState.Staged),
            MakeOperation("op-001", 14, 16, state: PickupEdlOperationState.Applied)
        };

        var ex = Assert.Throws<InvalidOperationException>(() => CreateDocument(ops));

        Assert.Contains("duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("op-001", ex.Message, StringComparison.Ordinal);
        Assert.Contains(ChapterStem, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Document_RejectsActiveOverlap_WithOpAndChapterDiagnostics()
    {
        var ops = new[]
        {
            MakeOperation("op-left", 30, 34, state: PickupEdlOperationState.Staged),
            MakeOperation("op-right", 33.9, 37, state: PickupEdlOperationState.Applied)
        };

        var ex = Assert.Throws<InvalidOperationException>(() => CreateDocument(ops));

        Assert.Contains("overlap", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("op-left", ex.Message, StringComparison.Ordinal);
        Assert.Contains("op-right", ex.Message, StringComparison.Ordinal);
        Assert.Contains(ChapterStem, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Document_RejectsSourceFingerprintMismatch_WithOpAndChapterDiagnostics()
    {
        var ops = new[]
        {
            MakeOperation("op-001", 10, 12, sourceFingerprint: "fp-other")
        };

        var ex = Assert.Throws<InvalidOperationException>(() => CreateDocument(ops));

        Assert.Contains("fingerprint", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("op-001", ex.Message, StringComparison.Ordinal);
        Assert.Contains(ChapterStem, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Operation_RejectsUnknownState_BeforeEngineUse()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            MakeOperation("op-bad-state", 10, 12, state: (PickupEdlOperationState)999));

        Assert.Contains("state", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("op-bad-state", ex.Message, StringComparison.Ordinal);
        Assert.Contains(ChapterStem, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void JsonDeserialize_RejectsSchemaVersionMismatch()
    {
        var json =
            """
            {
              "schemaVersion": "pickup-edl/v999",
              "chapterStem": "chapter-01",
              "revision": 1,
              "source": {
                "path": "/tmp/pickups.wav",
                "fingerprint": "fp-source-001",
                "fileSizeBytes": 10,
                "modifiedAtUtc": "2026-01-01T00:00:00Z"
              },
              "operations": []
            }
            """;

        var ex = Assert.ThrowsAny<Exception>(() => JsonSerializer.Deserialize<PickupEdlDocument>(json));

        var errorText = ex.ToString();
        Assert.Contains("schema", errorText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("version", errorText, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(-0.1, 1.0, 0.0, 1.0)]
    [InlineData(1.0, 1.0, 0.0, 1.0)]
    [InlineData(2.0, 1.5, 0.0, 1.0)]
    [InlineData(1.0, 2.0, -0.2, 0.8)]
    [InlineData(1.0, 2.0, 0.7, 0.7)]
    public void Operation_RejectsMalformedRanges(
        double baselineStart,
        double baselineEnd,
        double sourceStart,
        double sourceEnd)
    {
        var ex = Assert.ThrowsAny<Exception>(() =>
            MakeOperation(
                "op-bad-range",
                baselineStart,
                baselineEnd,
                sourceStartSec: sourceStart,
                sourceEndSec: sourceEnd));

        Assert.Contains("op-bad-range", ex.ToString(), StringComparison.Ordinal);
        Assert.Contains(ChapterStem, ex.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Document_AllowsZeroOperations_AndProducesEmptyOrders()
    {
        var doc = CreateDocument([]);

        Assert.Empty(doc.Operations);
        Assert.Empty(doc.GetDeterministicOperationOrder());
        Assert.Empty(doc.GetDeterministicApplyOrder());
        Assert.Empty(doc.BuildProjectionEdits());
    }

    [Fact]
    public void DeterministicOrder_ManySameStart_UsesStableOpIdTieBreak()
    {
        var ops = Enumerable.Range(0, 32)
            .Select(i => MakeOperation(
                id: $"op-{31 - i:D2}",
                baselineStartSec: 40,
                baselineEndSec: 41,
                state: PickupEdlOperationState.Failed))
            .ToArray();

        var doc = CreateDocument(ops);
        var actualOrder = doc.GetDeterministicOperationOrder().Select(op => op.Id).ToArray();
        var expectedOrder = ops.Select(op => op.Id).OrderBy(id => id, StringComparer.Ordinal).ToArray();

        Assert.True(
            expectedOrder.SequenceEqual(actualOrder),
            $"Deterministic tie-break mismatch; chapter={doc.ChapterStem}; " +
            $"expected=[{string.Join(",", expectedOrder)}]; actual=[{string.Join(",", actualOrder)}]");
    }

    private static PickupEdlDocument CreateDocument(
        IReadOnlyList<PickupEdlOperation> operations,
        string? chapterStem = null,
        string? schemaVersion = null,
        string? sourceFingerprint = null)
    {
        var source = new PickupEdlSourceReference(
            path: "/tmp/pickups.wav",
            fingerprint: sourceFingerprint ?? SourceFingerprint,
            fileSizeBytes: 1024,
            modifiedAtUtc: FixedUtc);

        return new PickupEdlDocument(
            schemaVersion: schemaVersion ?? PickupEdlDocument.CurrentSchemaVersion,
            chapterStem: chapterStem ?? ChapterStem,
            revision: 1,
            source: source,
            operations: operations);
    }

    private static PickupEdlOperation MakeOperation(
        string id,
        double baselineStartSec,
        double baselineEndSec,
        PickupEdlOperationState state = PickupEdlOperationState.Staged,
        string? sourceFingerprint = null,
        string? chapterStem = null,
        double sourceStartSec = 5,
        double sourceEndSec = 6)
    {
        return new PickupEdlOperation(
            id: id,
            chapterStem: chapterStem ?? ChapterStem,
            kind: PickupEdlOperationType.PickupReplace,
            state: state,
            baselineStartSec: baselineStartSec,
            baselineEndSec: baselineEndSec,
            sourceStartSec: sourceStartSec,
            sourceEndSec: sourceEndSec,
            sourceFingerprint: sourceFingerprint ?? SourceFingerprint,
            sentenceId: 42,
            errorNumber: 7,
            pickupAssetId: "asset-001",
            crossfadeDurationSec: 0.07,
            crossfadeCurve: "hsin",
            updatedAtUtc: FixedUtc);
    }
}
