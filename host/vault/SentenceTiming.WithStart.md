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
# SentenceTiming::WithStart
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/SentenceTiming.cs`

## Summary
**Create a new `SentenceTiming` with an updated start time while preserving all other timing metadata.**

`WithStart` is an immutable updater that constructs a new `SentenceTiming` with the provided `startSec` and existing `EndSec`, `FragmentBacked`, and `Confidence` values (`new(startSec, EndSec, FragmentBacked, Confidence)`). It shadows the base `TimingRange.WithStart` with a strongly typed return (`SentenceTiming`). Any range validation is delegated to the constructor/base type.


#### [[SentenceTiming.WithStart]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public SentenceTiming WithStart(double startSec)
```

