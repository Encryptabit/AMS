---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
---
# AudioIntegrityVerifier::MergeRuns
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`


#### [[AudioIntegrityVerifier.MergeRuns]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<AudioIntegrityVerifier.Segment> MergeRuns(IReadOnlyList<AudioIntegrityVerifier.Segment> runs, int maxGapFrames)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

