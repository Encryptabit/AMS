using Ams.Core.Artifacts;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services.Pickups.Fit;

namespace Ams.Tests.Services;

public sealed class PickupFitAudioRendererTests
{
    [Fact]
    public void Render_ExactFitNoRoomtone_UsesPickupInnerRangeAndClampsCrossfade()
    {
        var pickup = CreateRampBuffer(sampleRate: 100, length: 500);
        var item = CreateFitItem(
            outerStartSec: 10,
            outerEndSec: 12,
            innerStartSec: 1,
            innerEndSec: 3,
            placementStartSec: 10,
            placementEndSec: 12,
            policy: new PickupFitTransitionPolicy(0, 0, 5, "tri"));

        var result = PickupFitAudioRenderer.Render(item, pickup);

        Assert.Equal(200, result.Buffer.Length);
        Assert.Equal(2.0, result.RenderedDurationSec, precision: 6);
        Assert.Equal(100f, result.Buffer[0, 0]);
        Assert.Equal(299f, result.Buffer[0, result.Buffer.Length - 1]);
        Assert.Equal(0.6, result.EffectiveCrossfadeDurationSec, precision: 6);
    }

    [Fact]
    public void Render_PlacementOffsetAndRoomtonePadding_ComposesCompositeSegment()
    {
        var pickup = CreateRampBuffer(sampleRate: 10, length: 50);
        var roomtone = CreateConstantBuffer(sampleRate: 10, length: 4, value: -1f);
        var item = CreateFitItem(
            outerStartSec: 0,
            outerEndSec: 3,
            innerStartSec: 1,
            innerEndSec: 2,
            placementStartSec: 0.5,
            placementEndSec: 1.5,
            policy: new PickupFitTransitionPolicy(0.2, 0.3, 0.05, "hsin"));

        var result = PickupFitAudioRenderer.Render(item, pickup, roomtone);

        Assert.Equal(35, result.Buffer.Length);
        Assert.Equal(3.5, result.RenderedDurationSec, precision: 6);
        Assert.Equal(0.7, result.RoomtoneBeforeSec, precision: 6);
        Assert.Equal(1.8, result.RoomtoneAfterSec, precision: 6);
        Assert.Equal(0.5, result.PlacementOffsetSec, precision: 6);

        for (var i = 0; i < 7; i++)
        {
            Assert.Equal(-1f, result.Buffer[0, i]);
        }

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(10 + i, result.Buffer[0, 7 + i]);
        }

        for (var i = 17; i < result.Buffer.Length; i++)
        {
            Assert.Equal(-1f, result.Buffer[0, i]);
        }
    }

    [Fact]
    public void Render_MissingRoomtoneWhenFillRequired_FailsClosed()
    {
        var pickup = CreateRampBuffer(sampleRate: 10, length: 50);
        var item = CreateFitItem(
            outerStartSec: 0,
            outerEndSec: 2,
            innerStartSec: 1,
            innerEndSec: 2,
            placementStartSec: 0.5,
            placementEndSec: 1.5,
            policy: new PickupFitTransitionPolicy(0, 0, 0.05, "tri"));

        var ex = Assert.Throws<InvalidOperationException>(() => PickupFitAudioRenderer.Render(item, pickup));

        Assert.Contains("roomtone is required", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(item.FitItemId, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_PickupInnerLongerThanPlacement_FailsClosedWithoutStretching()
    {
        var pickup = CreateRampBuffer(sampleRate: 10, length: 50);
        var item = CreateFitItem(
            outerStartSec: 0,
            outerEndSec: 2,
            innerStartSec: 1,
            innerEndSec: 2,
            placementStartSec: 0,
            placementEndSec: 0.5,
            policy: new PickupFitTransitionPolicy(0, 0, 0.05, "tri"));

        var ex = Assert.Throws<InvalidOperationException>(() => PickupFitAudioRenderer.Render(item, pickup));

        Assert.Contains("no time-stretch policy", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(item.FitItemId, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_RoomtoneChannelMismatch_FailsClosed()
    {
        var pickup = CreateRampBuffer(sampleRate: 10, length: 50, channels: 1);
        var roomtone = CreateConstantBuffer(sampleRate: 10, length: 10, value: -1f, channels: 2);
        var item = CreateFitItem(
            outerStartSec: 0,
            outerEndSec: 2,
            innerStartSec: 1,
            innerEndSec: 2,
            placementStartSec: 0.5,
            placementEndSec: 1.5,
            policy: new PickupFitTransitionPolicy(0, 0, 0.05, "tri"));

        var ex = Assert.Throws<InvalidOperationException>(() => PickupFitAudioRenderer.Render(item, pickup, roomtone));

        Assert.Contains("channel count", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(item.FitItemId, ex.Message, StringComparison.Ordinal);
    }

    private static PickupFitPlanItem CreateFitItem(
        double outerStartSec,
        double outerEndSec,
        double innerStartSec,
        double innerEndSec,
        double placementStartSec,
        double placementEndSec,
        PickupFitTransitionPolicy policy)
    {
        var target = new PickupPickMapTargetReference(
            chapterStem: "chapter-01",
            chapterName: "Chapter 01",
            errorNumber: 7,
            sentenceId: 11,
            originalStartSec: outerStartSec,
            originalEndSec: outerEndSec);

        return new PickupFitPlanItem(
            fitItemId: "fit::assignment-001",
            replacementId: "replacement::assignment-001",
            pickAssignmentId: "assignment-001",
            pickupSegmentId: "segment-001",
            target: target,
            outerRange: new PickupFitPlanRange(outerStartSec, outerEndSec),
            innerRange: new PickupFitPlanRange(innerStartSec, innerEndSec),
            placement: new PickupFitPlanRange(placementStartSec, placementEndSec),
            transitionPolicy: policy,
            status: PickupFitPlanItemStatus.Draft,
            previewEvidence: null,
            acceptance: PickupFitAcceptanceState.None,
            commit: PickupFitCommitState.NotReady,
            validationError: null,
            commitError: null,
            updatedAtUtc: DateTime.UtcNow);
    }

    private static AudioBuffer CreateRampBuffer(int sampleRate, int length, int channels = 1)
    {
        var buffer = new AudioBuffer(channels: channels, sampleRate: sampleRate, length: length);
        for (var channel = 0; channel < channels; channel++)
        {
            for (var i = 0; i < length; i++)
            {
                buffer[channel, i] = i;
            }
        }

        return buffer;
    }

    private static AudioBuffer CreateConstantBuffer(int sampleRate, int length, float value, int channels = 1)
    {
        var buffer = new AudioBuffer(channels: channels, sampleRate: sampleRate, length: length);
        for (var channel = 0; channel < channels; channel++)
        {
            for (var i = 0; i < length; i++)
            {
                buffer[channel, i] = value;
            }
        }

        return buffer;
    }
}
