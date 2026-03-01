---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::EnsureDefaultFormatClause
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Ensures the graph has a default float sample-format clause unless format selection has already been fixed or a custom graph is being used.**

`EnsureDefaultFormatClause` conditionally injects a default FFmpeg format filter into the graph builder state. It exits early when `_customGraphOverride` is active or `_formatPinned` is already true, preventing mutation in custom/locked modes. Otherwise it prepends `"aformat=sample_fmts=flt"` at index `0` of `_clauses` and marks `_formatPinned = true` to enforce one-time insertion.


#### [[FfFilterGraph.EnsureDefaultFormatClause]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureDefaultFormatClause()
```

**Called-by <-**
- [[FfFilterGraph.BuildSpec]]

