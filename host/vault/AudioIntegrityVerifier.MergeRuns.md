---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioIntegrityVerifier::MergeRuns
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Merge adjacent/nearby mismatch runs by type to reduce fragmented segment output.**

`MergeRuns` coalesces nearby mismatch segments of the same `AudioMismatchType` when their frame gap is within `maxGapFrames`. It returns early for empty input, then groups runs by type, processes each group ordered by `StartIndex`, and either appends a new segment or extends the previous one (`new Segment(prev.StartIndex, max(prev.EndIndex, r.EndIndex), type)`). After type-local merging, it reorders the merged output globally by `StartIndex` before returning.


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

