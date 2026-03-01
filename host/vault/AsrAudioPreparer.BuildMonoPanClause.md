---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs"
access_modifier: "internal"
complexity: 4
fan_in: 7
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrAudioPreparer::BuildMonoPanClause
**Path**: `Projects/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs`

## Summary
**Construct a culture-invariant FFmpeg pan-filter clause that mixes N input channels into one mono output channel with equal weighting.**

`BuildMonoPanClause` generates an FFmpeg `pan` filter expression for mono downmix using equal channel weights. For `channels <= 1`, it returns the identity mapping `"pan=mono|c0=c0"`; otherwise it computes `weight = 1.0 / channels`, builds `weight*cN` terms for each channel with `F6` precision via `FormattableString.Invariant`, joins them with `+`, and prefixes with `"pan=mono|c0="`. This guarantees culture-invariant decimal formatting and deterministic clauses for testable multi-channel mixes.


#### [[AsrAudioPreparer.BuildMonoPanClause]]
##### What it does:
<member name="M:Ams.Core.Audio.AsrAudioPreparer.BuildMonoPanClause(System.Int32)">
    <summary>
    Builds an FFmpeg pan filter clause for equal-weighted mono downmix.
    </summary>
    <param name="channels">The number of source channels.</param>
    <returns>An FFmpeg pan filter clause string.</returns>
    <example>
    For 2 channels: "pan=mono|c0=0.500000*c0+0.500000*c1"
    For 6 channels: "pan=mono|c0=0.166667*c0+0.166667*c1+...+0.166667*c5"
    </example>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static string BuildMonoPanClause(int channels)
```

**Called-by <-**
- [[AsrAudioPreparer.DownmixToMono]]
- [[AsrAudioPreparerTests.BuildMonoPanClause_SingleChannel_ReturnsIdentity]]
- [[AsrAudioPreparerTests.BuildMonoPanClause_StereoChannels_ReturnsEqualWeights]]
- [[AsrAudioPreparerTests.BuildMonoPanClause_SurroundChannels_ReturnsCorrectWeights]]
- [[AsrAudioPreparerTests.BuildMonoPanClause_VariousChannels_MatchesExpected]]
- [[AsrAudioPreparerTests.BuildMonoPanClause_WeightsAreInvariantCulture]]
- [[AsrAudioPreparerTests.BuildMonoPanClause_ZeroChannels_ReturnsIdentity]]

