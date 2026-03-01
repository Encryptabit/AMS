---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/TranscriptModels.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# SentenceAlign::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/TranscriptModels.cs`

## Summary
**Create a `SentenceAlign` instance from legacy/minimal inputs by supplying an implicit empty timing range.**

This `[JsonConstructor]` overload provides a 5-parameter construction path for `SentenceAlign` and delegates to the primary positional record constructor with `: this(id, bookRange, scriptRange, TimingRange.Empty, metrics, status)`. Its main implementation detail is defaulting the `Timing` component to `TimingRange.Empty` when timing is not supplied in payloads. The constructor performs no additional validation or transformation beyond that delegation.


#### [[SentenceAlign..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public SentenceAlign(int id, IntRange bookRange, ScriptRange scriptRange, SentenceMetrics metrics, string status)
```

