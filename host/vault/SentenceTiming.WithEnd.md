---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/SentenceTiming.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# SentenceTiming::WithEnd
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/SentenceTiming.cs`

## Summary
**Return a copy of the sentence timing with an updated end time and all other metadata preserved.**

`WithEnd` creates a new `SentenceTiming` instance with a replaced end timestamp while carrying forward `StartSec`, `FragmentBacked`, and `Confidence` (`new(StartSec, endSec, FragmentBacked, Confidence)`). It is a typed override-like helper (`new`) that returns `SentenceTiming` instead of a base `TimingRange`. The method is non-mutating and relies on constructor/base validation for temporal bounds.


#### [[SentenceTiming.WithEnd]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public SentenceTiming WithEnd(double endSec)
```

**Called-by <-**
- [[SentenceRefinementService.RefineAsync]]

