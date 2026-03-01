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
---
# SentenceTiming::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/SentenceTiming.cs`

## Summary
**Create a `SentenceTiming` from an existing `TimingRange` plus optional fragment provenance and confidence values.**

This overload is a convenience constructor that delegates to the primary constructor using constructor chaining: `this(range.StartSec, range.EndSec, fragmentBacked, confidence)`. It converts a generic `TimingRange` into a `SentenceTiming` while preserving the optional provenance/confidence arguments. The constructor itself adds no extra logic beyond extracting start/end from `range`.


#### [[SentenceTiming..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public SentenceTiming(TimingRange range, bool fragmentBacked = false, double? confidence = null)
```

