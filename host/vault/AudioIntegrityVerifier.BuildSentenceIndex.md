---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioIntegrityVerifier::BuildSentenceIndex
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Create a start-time-sorted sentence span index from treated sentence timing mappings.**

`BuildSentenceIndex` materializes sentence timing metadata into a sortable interval list for fast context lookup. It preallocates a list sized to `treatedById.Count`, appends `(StartSec, EndSec, SentenceId)` tuples for each dictionary entry, then sorts by start time (`Item1`). The output is a compact, ordered index consumed by mismatch-to-sentence overlap queries.


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

