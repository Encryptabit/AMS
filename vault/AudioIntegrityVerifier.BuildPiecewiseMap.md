---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# AudioIntegrityVerifier::BuildPiecewiseMap
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`


#### [[AudioIntegrityVerifier.BuildPiecewiseMap]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<AudioIntegrityVerifier.MapSeg> BuildPiecewiseMap(IReadOnlyDictionary<int, SentenceTiming> rawById, IReadOnlyDictionary<int, SentenceTiming> treatedById)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

