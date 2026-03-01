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
# SentenceTiming::WithFragmentBacked
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/SentenceTiming.cs`

## Summary
**Create a copy of the timing record with an updated `FragmentBacked` flag.**

`WithFragmentBacked` is an immutable record updater that returns a new `SentenceTiming` via `this with { FragmentBacked = fragmentBacked }`. It preserves all other fields (`StartSec`, `EndSec`, `Confidence`) unchanged and does not mutate the current instance. This provides a concise way to toggle provenance state in fluent flows.


#### [[SentenceTiming.WithFragmentBacked]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public SentenceTiming WithFragmentBacked(bool fragmentBacked)
```

