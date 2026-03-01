---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioIntegrityVerifier::CollectRuns
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Convert a boolean mismatch mask into duration-filtered contiguous segments tagged with mismatch type.**

`CollectRuns` scans a boolean frame mask and extracts contiguous `true` spans as typed `Segment` ranges. It converts the minimum duration constraint into a minimum frame length (`minLen = max(1, ceil((minDurSec - windowSec) / stepSec))`), then uses a single-pointer run-length traversal to find `[start, end]` intervals. Only runs meeting `minLen` are materialized as `new Segment(start, end, type)` and returned in encounter order.


#### [[AudioIntegrityVerifier.CollectRuns]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<AudioIntegrityVerifier.Segment> CollectRuns(bool[] mask, double stepSec, double windowSec, double minDurSec, AudioMismatchType type)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

