---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/SentenceTiming.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# SentenceTiming::WithConfidence
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/SentenceTiming.cs`

## Summary
**Return a copy of the timing record with a new optional confidence value.**

`WithConfidence` uses record non-destructive mutation (`this with { Confidence = confidence }`) to produce a new `SentenceTiming` instance. It updates only the nullable `Confidence` field while retaining start/end and `FragmentBacked` values from the original object. The current instance remains unchanged.


#### [[SentenceTiming.WithConfidence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public SentenceTiming WithConfidence(double? confidence)
```

