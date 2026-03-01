---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::AStats
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add an `astats` filter node from structured parameters by formatting them into raw FFmpeg arguments.**

This expression-bodied overload converts `AStatsFilterParams` into a raw FFmpeg option string and appends the `astats` filter via `AddRawFilter`. It maps `EmitMetadata` to `metadata=1|0` and uses `ResetInterval` for `reset=...`, composing `"metadata=...:reset=..."` directly. It returns the same `FfFilterGraph` instance to maintain fluent chaining.


#### [[FfFilterGraph.AStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph AStats(AStatsFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddRawFilter]]

