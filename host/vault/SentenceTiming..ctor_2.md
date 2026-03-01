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
**Create a sentence-level timing object with start/end bounds plus provenance and optional confidence metadata.**

This constructor initializes `SentenceTiming` by delegating temporal bounds to `TimingRange` via `base(startSec, endSec)` and then setting provenance fields `FragmentBacked` and optional `Confidence`. It is marked with `[JsonConstructor]`, making it the primary deserialization path for persisted timing payloads. Aside from property assignment, it contains no additional normalization or validation logic beyond what `TimingRange` enforces.


#### [[SentenceTiming..ctor_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public SentenceTiming(double startSec, double endSec, bool fragmentBacked = false, double? confidence = null)
```

