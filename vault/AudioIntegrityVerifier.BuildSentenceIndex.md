---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
---
# AudioIntegrityVerifier::BuildSentenceIndex
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`


#### [[AudioIntegrityVerifier.BuildSentenceIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<(double Start, double End, int SentenceId)> BuildSentenceIndex(IReadOnlyDictionary<int, SentenceTiming> treatedById)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

