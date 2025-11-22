namespace Ams.Core.Artifacts;

/// <summary>
/// Associates a timing range with its originating chunk fragment.
/// </summary>
public sealed record FragmentTiming(string ChunkId, int FragmentIndex, double StartSec, double EndSec)
    : TimingRange(StartSec, EndSec);