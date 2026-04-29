using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Processors;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services.Pickups.Fit;

/// <summary>
/// Renders the local replacement segment for a single accepted Fit item.
/// Strategy stays narrow: no chapter-wide roomtone bed, only a composite replacement segment
/// that Polish/EDL can commit through the existing rebuild and ledger runtime.
/// </summary>
public static class PickupFitAudioRenderer
{
    private const double TimingEpsilonSec = 0.000_001;

    public static PickupFitAudioRenderResult Render(
        PickupFitPlanItem item,
        AudioBuffer pickupSource,
        AudioBuffer? roomtoneSource = null)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(pickupSource);
        EnsureValidBuffer(pickupSource, "pickup source", item.FitItemId);

        var policy = item.TransitionPolicy;
        var crossfadeCurve = NormalizeCrossfadeCurve(policy.CrossfadeCurve, item.FitItemId);
        var pickupSlice = AudioSpliceService.SliceByTime(
            pickupSource,
            item.InnerRange.StartSec,
            item.InnerRange.EndSec,
            $"fit item '{item.FitItemId}' pickup inner range");

        var pickupDurationSec = AudioSpliceService.DurationSeconds(pickupSlice);
        if (pickupDurationSec <= TimingEpsilonSec)
        {
            throw new InvalidOperationException(
                $"Pickup fit renderer rejected item '{item.FitItemId}': pickup inner range produced an empty replacement segment.");
        }

        var placementOffsetSec = item.Placement.StartSec - item.OuterRange.StartSec;
        var outerTrailingSec = item.OuterRange.EndSec - item.Placement.EndSec;
        if (placementOffsetSec < -TimingEpsilonSec || outerTrailingSec < -TimingEpsilonSec)
        {
            throw new InvalidOperationException(
                $"Pickup fit renderer rejected item '{item.FitItemId}': placement [{item.Placement.StartSec:F6}, {item.Placement.EndSec:F6}] " +
                $"is outside outer range [{item.OuterRange.StartSec:F6}, {item.OuterRange.EndSec:F6}].");
        }

        var placementDurationSec = item.Placement.DurationSec;
        if (pickupDurationSec > placementDurationSec + TimingEpsilonSec)
        {
            throw new InvalidOperationException(
                $"Pickup fit renderer rejected item '{item.FitItemId}': pickup inner duration {pickupDurationSec:F6}s exceeds " +
                $"placement window {placementDurationSec:F6}s; no time-stretch policy is available.");
        }

        var placementTrailingFillSec = Math.Max(0, placementDurationSec - pickupDurationSec);
        var roomtoneBeforeSec = ClampTinyToZero(Math.Max(0, placementOffsetSec) + policy.RoomtoneBeforeSec);
        var roomtoneAfterSec = ClampTinyToZero(Math.Max(0, outerTrailingSec) + placementTrailingFillSec + policy.RoomtoneAfterSec);
        var requiresRoomtone = roomtoneBeforeSec > TimingEpsilonSec || roomtoneAfterSec > TimingEpsilonSec;

        var segments = new List<AudioBuffer>(capacity: 3);
        AudioBuffer? compatibleRoomtone = null;
        if (requiresRoomtone)
        {
            if (roomtoneSource is null)
            {
                throw new InvalidOperationException(
                    $"Pickup fit renderer rejected item '{item.FitItemId}': roomtone is required for placement/padding fill " +
                    $"(before={roomtoneBeforeSec:F6}s, after={roomtoneAfterSec:F6}s) but no roomtone buffer was provided.");
            }

            compatibleRoomtone = NormalizeRoomtone(roomtoneSource, pickupSlice, item.FitItemId);
        }

        if (roomtoneBeforeSec > TimingEpsilonSec)
        {
            segments.Add(AudioSpliceService.GenerateRoomtoneFill(compatibleRoomtone!, roomtoneBeforeSec));
        }

        segments.Add(pickupSlice);

        if (roomtoneAfterSec > TimingEpsilonSec)
        {
            segments.Add(AudioSpliceService.GenerateRoomtoneFill(compatibleRoomtone!, roomtoneAfterSec));
        }

        var rendered = segments.Count == 1
            ? pickupSlice
            : AudioSpliceService.ConcatenateSegments(segments);
        var renderedDurationSec = AudioSpliceService.DurationSeconds(rendered);
        var effectiveCrossfadeSec = AudioSpliceService.ClampCrossfadeDuration(
            policy.CrossfadeDurationSec,
            item.OuterRange.DurationSec,
            renderedDurationSec);

        return new PickupFitAudioRenderResult(
            Buffer: rendered,
            RenderedDurationSec: renderedDurationSec,
            PickupInnerDurationSec: pickupDurationSec,
            RoomtoneBeforeSec: roomtoneBeforeSec,
            RoomtoneAfterSec: roomtoneAfterSec,
            PlacementOffsetSec: Math.Max(0, placementOffsetSec),
            EffectiveCrossfadeDurationSec: effectiveCrossfadeSec,
            CrossfadeCurve: crossfadeCurve);
    }

    private static string NormalizeCrossfadeCurve(string curve, string fitItemId)
    {
        try
        {
            return AudioSpliceService.NormalizeCrossfadeCurve(curve);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new InvalidOperationException(
                $"Pickup fit renderer rejected item '{fitItemId}': crossfade curve '{curve}' is unsupported.",
                ex);
        }
    }

    private static AudioBuffer NormalizeRoomtone(AudioBuffer roomtoneSource, AudioBuffer reference, string fitItemId)
    {
        EnsureValidBuffer(roomtoneSource, "roomtone source", fitItemId);

        var normalized = roomtoneSource.SampleRate == reference.SampleRate
            ? roomtoneSource
            : AudioProcessor.Resample(roomtoneSource, (ulong)reference.SampleRate);

        if (normalized.Channels != reference.Channels)
        {
            throw new InvalidOperationException(
                $"Pickup fit renderer rejected item '{fitItemId}': roomtone channel count {normalized.Channels} does not match " +
                $"pickup channel count {reference.Channels}.");
        }

        return normalized;
    }

    private static void EnsureValidBuffer(AudioBuffer buffer, string bufferName, string fitItemId)
    {
        if (buffer.SampleRate <= 0 || buffer.Channels <= 0)
        {
            throw new InvalidOperationException(
                $"Pickup fit renderer rejected item '{fitItemId}': {bufferName} has invalid sample metadata " +
                $"(sampleRate={buffer.SampleRate}, channels={buffer.Channels}).");
        }
    }

    private static double ClampTinyToZero(double value)
        => value <= TimingEpsilonSec ? 0 : value;
}

public sealed record PickupFitAudioRenderResult(
    AudioBuffer Buffer,
    double RenderedDurationSec,
    double PickupInnerDurationSec,
    double RoomtoneBeforeSec,
    double RoomtoneAfterSec,
    double PlacementOffsetSec,
    double EffectiveCrossfadeDurationSec,
    string CrossfadeCurve);
