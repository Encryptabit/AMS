using System;
using System.Collections.Generic;

namespace Ams.Core.Audio;

public static class RoomtonePlanVersion
{
    public const string Current = "v1";
}

public sealed record RoomtonePlan(
    string Version,
    string ChapterId,
    string AudioPath,
    string TranscriptIndexPath,
    string RoomtoneSeedPath,
    double AudioDurationSec,
    int TargetSampleRate,
    double TargetRmsDb,
    double AppliedGainDb,
    double RoomtoneSeedRmsDb,
    double FadeMs,
    DateTime GeneratedAtUtc,
    IReadOnlyList<RoomtonePlanSentence> Sentences,
    IReadOnlyList<RoomtonePlanGap> Gaps);

public sealed record RoomtonePlanSentence(
    int SentenceId,
    double StartSec,
    double EndSec,
    double WindowStartSec,
    double WindowEndSec,
    bool HasTiming,
    bool FragmentBacked,
    double? Confidence);

public sealed record RoomtonePlanGap(
    double StartSec,
    double EndSec,
    double DurationSec,
    int? PreviousSentenceId,
    int? NextSentenceId,
    double MinRmsDb,
    double MaxRmsDb,
    double MeanRmsDb,
    double SilenceFraction);
