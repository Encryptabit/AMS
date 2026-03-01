---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs"
access_modifier: "internal"
complexity: 4
fan_in: 7
fan_out: 0
tags:
  - method
---
# AsrAudioPreparer::BuildMonoPanClause
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs`


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

