---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioIntegrityVerifier::LookupSentenceContext
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Find treated-sentence intervals that overlap a given time window for mismatch context annotation.**

`LookupSentenceContext` returns sentence spans that overlap a mismatch interval `[startSec, endSec)` from a start-sorted sentence index. It finds the initial probe position with `LowerBound(index, startSec)`, then linearly scans forward while `index[i].Start < endSec`, skipping non-overlaps where `End <= startSec` and materializing overlaps as `SentenceSpan` records. The method is optimized for per-run usage (few calls) and returns a compact list of contextual sentence spans.


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

