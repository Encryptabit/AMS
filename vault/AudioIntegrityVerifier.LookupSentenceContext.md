---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# AudioIntegrityVerifier::LookupSentenceContext
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`


#### [[AudioIntegrityVerifier.LookupSentenceContext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<SentenceSpan> LookupSentenceContext(List<(double Start, double End, int SentenceId)> index, double startSec, double endSec)
```

**Calls ->**
- [[AudioIntegrityVerifier.LowerBound]]

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

