namespace Ams.Core.Artifacts;

public sealed record WaveformPeaks
{
    public required int BucketCount { get; init; }
    public required double DurationSeconds { get; init; }
    public required float[] Data { get; init; }
}
