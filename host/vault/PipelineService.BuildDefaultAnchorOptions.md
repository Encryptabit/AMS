---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# PipelineService::BuildDefaultAnchorOptions
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Creates the default anchor computation settings used by the pipeline when caller-provided anchor options are absent.**

`BuildDefaultAnchorOptions` is a static factory that returns a new `AnchorComputationOptions` with fixed defaults (`DetectSection=true`, `AsrPrefixTokens=8`, `NGram=3`, `TargetPerTokens=50`, `MinSeparation=100`, `AllowBoundaryCross=false`, `UseDomainStopwords=true`). In `RunChapterAsync`, it is used as a null-fallback for `options.AnchorOptions` and then cloned via `with` to toggle `EmitWindows` for anchor vs transcript-index stages.


#### [[PipelineService.BuildDefaultAnchorOptions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AnchorComputationOptions BuildDefaultAnchorOptions()
```

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

